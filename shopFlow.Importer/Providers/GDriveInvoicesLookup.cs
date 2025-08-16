using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace shopFlow.Importer.Providers
{
    public class GDriveInvoicesLookup : IInvoicesLookup, IDisposable
    {
        private readonly DriveService _driveService;
        private readonly string _folderId;
        private readonly ILogger _logger;

        public GDriveInvoicesLookup(
            string credentialsPath,
            string folderId,
            ILogger logger)
        {
            _folderId = folderId ?? throw new ArgumentNullException(nameof(folderId));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                var credential = GoogleCredential.FromFile(credentialsPath)
                    .CreateScoped(DriveService.ScopeConstants.DriveReadonly);

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
                request.Q = $"'{_folderId}' in parents and trashed = false";
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
                request.Q = $"'{_folderId}' in parents and name = '{id}' and trashed = false";
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
