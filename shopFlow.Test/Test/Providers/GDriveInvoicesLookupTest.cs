using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using shopFlow.Importer.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shopFlow.Test.Test.Providers
{
    public class GDriveInvoicesLookupTest
    {
        [Test]
        public void GetInvoicesIds_ReturnsNonEmptyList()
        {
            // Arrange
            var credentialsPath = "./Resources/client_secret.json"; // Update with actual path
            var folderId = "1KH67uMcVSozSMls3Rxka_6qqaJS8N2XY"; // Update with actual folder ID
            var logger = new Mock<ILogger<GDriveInvoicesLookup>>().Object;
            var provider = new GDriveInvoicesLookup(credentialsPath, (folderId,""), logger);
            // Act
            var ids = provider.GetInvoicesIds();
            // Assert
            ids.Should().NotBeEmpty();
        }

        [Test]
        public void TryGetInvoiceProvider_ReturnsValidStream()
        {
            // Arrange
            var credentialsPath = "./Resources/client_secret.json"; // Update with actual path
            var folderId = "1KH67uMcVSozSMls3Rxka_6qqaJS8N2XY"; // Update with actual folder ID
            var logger = new Mock<ILogger<GDriveInvoicesLookup>>().Object;
            var provider = new GDriveInvoicesLookup(credentialsPath, (folderId, ""), logger);
            // Act
            var ids = provider.GetInvoicesIds();
            var firstId = ids.FirstOrDefault();
            var stream = provider.GetInvoiceById(firstId);
            // Assert
            InvoiceProviders.TryGetInvoiceProvider(firstId, out var invoiceProvider).Should().BeTrue();
            var invoiceInfo = invoiceProvider.GetInvoiceInfo(stream);

            invoiceInfo.Should().NotBeNull();
            invoiceInfo.SupplierName.Should().NotBeNullOrEmpty();


        }

        [Test]
        public async Task MoveInvoiceToImportedFolderAsync_ValidInvoice_MovesSuccessfully()
        {
            // Arrange
            var credentialsPath = "./Resources/client_secret.json"; // Update with actual path
            var pendingFolderId = "1KH67uMcVSozSMls3Rxka_6qqaJS8N2XY"; // Update with actual folder ID
            var importedFolderId = "1RShSzOUmLAx37yN8iO4CSKDJtjRaRinX"; // Update with actual folder ID
            var logger = new Mock<ILogger<GDriveInvoicesLookup>>().Object;
            var provider = new GDriveInvoicesLookup(credentialsPath, (pendingFolderId, importedFolderId), logger);
            var ids = provider.GetInvoicesIds();
            var firstId = ids.FirstOrDefault();
            var stream = provider.GetInvoiceById(firstId);
            InvoiceProviders.TryGetInvoiceProvider(firstId, out var invoiceProvider);
            var invoiceInfo = invoiceProvider.GetInvoiceInfo(stream);
            // Act
            await provider.MoveInvoiceToImportedFolderAsync(firstId, invoiceInfo);
        }
    }
}
