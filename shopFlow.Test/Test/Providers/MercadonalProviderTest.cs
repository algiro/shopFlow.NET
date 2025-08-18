using System;
using System.IO;
using System.Linq;
using shopFlow.Importer.Providers;
using shopFlow.Importer.Domain;
using Xunit;
using FluentAssertions;

namespace shopFlow.Test.Test.Providers
{
    public class MercadonaProviderTest
    {
        [Fact]
        public void GetInvoiceInfo_ParsesPdfCorrectly()
        {
            var provider = new MercadonaInvoiceProvider();
            using var stream = File.OpenRead("./Resources/InvoiceSamples/A-G2025-369554.pdf");
            var info = provider.GetInvoiceInfo(stream);

            Assert.Equal("Mercadona", info.SupplierName);
            Assert.Equal("A-G2025-00000369554", info.InvoiceNumber);
            Assert.Equal(new DateTime(2025, 7, 29, 0, 0, 0), info.InvoiceDate);
            Assert.Equal(66.27m, info.TotalAmount);
            Assert.NotNull(info.Items);
            var items = info.Items.ToList();
            
        }
    }
}
