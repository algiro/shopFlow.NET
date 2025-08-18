using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using shopFlow.Importer.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace shopFlow.Importer.Providers
{
    public class GDriveInvoicesLookup : IInvoicesLookup, IDisposable
    {
        private readonly DriveService _driveService;
        private readonly (string pending, string imported) _foldersId;
        private readonly ILogger _logger;

        public GDriveInvoicesLookup(
            string credentialsPath,
            (string pending,string imported) folderIds,
            ILogger logger)
        {
            _foldersId = folderIds;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                var credential = GoogleCredential.FromFile(credentialsPath)
                    .CreateScoped(DriveService.ScopeConstants.Drive);

                _driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "ShopFlow"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Google Drive service");
                throw new InvalidOperationException("Failed to initialize Google Drive service", ex);
            }
        }

        public IEnumerable<string> GetInvoicesIds()
        {
            try
            {
                var request = _driveService.Files.List();
                request.Q = $"'{_foldersId.pending}' in parents and trashed = false";
                request.Fields = "files(id, name)";

                var result = request.Execute();
                return result.Files
                    .Where(f => IsInvoiceFile(f.Name))
                    .Select(f => f.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get invoice IDs from Google Drive");
                return Enumerable.Empty<string>();
            }
        }

        public Stream GetInvoiceById(string id)
        {
            try
            {
                var request = _driveService.Files.List();
                request.Q = $"'{_foldersId.pending}' in parents and name = '{id}' and trashed = false";
                request.Fields = "files(id)";

                var result = request.Execute();
                var file = result.Files.FirstOrDefault();
                
                if (file == null)
                {
                    _logger.LogWarning("Invoice with ID {InvoiceId} not found", id);
                    return null;
                }

                var stream = new MemoryStream();
                _driveService.Files.Get(file.Id).Download(stream);
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get invoice {InvoiceId} from Google Drive", id);
                return null;
            }
        }
        public async Task MoveInvoiceToImportedFolderAsync(string invoiceId, IInvoiceInfo invoiceInfo)
        {
            try
            {
                var request = _driveService.Files.List();
                request.Q = $"'{_foldersId.pending}' in parents and name = '{invoiceId}' and trashed = false";
                request.Fields = "files(id)";

                var result = await request.ExecuteAsync();
                var file = result.Files.FirstOrDefault();

                if (file == null)
                {
                    _logger.LogWarning("Invoice with ID {InvoiceId} not found", invoiceId);
                    return;
                }
                var folderName = invoiceInfo.InvoiceDate.ToString("yyyy-MM") ?? "UnknownDate";
                var folderId = await CreateSubFolderIfDoesntExist(folderName, _foldersId.imported);
                // Create a NEW, EMPTY File object for the update
                var emptyFile = new Google.Apis.Drive.v3.Data.File();

                var updateRequest = _driveService.Files.Update(emptyFile, file.Id);
                updateRequest.AddParents = folderId;
                updateRequest.RemoveParents = _foldersId.pending;

                await updateRequest.ExecuteAsync();

                _logger.LogInformation("Moved invoice {InvoiceId} to imported folder", invoiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move invoice {InvoiceId} to imported folder", invoiceId);
            }
        }
        public async Task<string> CreateSubFolderIfDoesntExist(string subFolderName, string parentFolderId)
        {
            try
            {
                var request = _driveService.Files.List();
                request.Q = $"'{parentFolderId}' in parents and name = '{subFolderName}' and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
                request.Fields = "files(id, name)";
                var result = await request.ExecuteAsync();
                if (result.Files.Any())
                {
                    _logger.LogInformation("Subfolder {SubFolderName} already exists in folder {ParentFolderId}", subFolderName, parentFolderId);
                    return result.Files.First().Id;
                }
                var folderMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = subFolderName,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = new List<string> { parentFolderId }
                };
                var createRequest = _driveService.Files.Create(folderMetadata);
                createRequest.Fields = "id";
                var folder = await createRequest.ExecuteAsync();
                _logger.LogInformation("Created subfolder {SubFolderName} with ID {SubFolderId} in folder {ParentFolderId}", subFolderName, folder.Id, parentFolderId);
                return folder.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create subfolder {SubFolderName} in folder {ParentFolderId}", subFolderName, parentFolderId);
            }
            return string.Empty;
        }
        private bool IsInvoiceFile(string fileName)
        {
            return InvoiceProviders.TryGetInvoiceProvider(fileName, out _);
        }

        public void Dispose()
        {
            _driveService?.Dispose();
        }
    }
}
