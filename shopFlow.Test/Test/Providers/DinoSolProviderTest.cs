using System;
using System.IO;
using System.Linq;
using shopFlow.Importer.Providers;
using shopFlow.Importer.Domain;
using Xunit;
using FluentAssertions;

namespace shopFlow.Test.Test.Providers
{
    public class DinoSolProviderTest
    {
        [Fact]
        public void GetInvoiceInfo_ParsesPdfCorrectly()
        {
            var provider = new DinoSolInvoiceProvider();
            using var stream = File.OpenRead("./Resources/InvoiceSamples/TICKET_PDF_NUM_250617.9370.0011.00053115.PDF");
            var info = provider.GetInvoiceInfo(stream);

            Assert.Equal("DinoSol", info.SupplierName);
            Assert.Equal("250617.9370.0011.00053115", info.InvoiceNumber);
            Assert.Equal(new DateTime(2025, 6, 17, 0, 0, 0), info.InvoiceDate);
            Assert.Equal(89.38m, info.TotalAmount);
            Assert.NotNull(info.Items);
            var items = info.Items.ToList();
            items.Count().Should().Be(30);
            var firstItem = items.First();
            firstItem.Name.Should().Be("DINO LECHE CONDESADA LATA ENTER");
            firstItem.Total.Should().Be(3.02m);
        }
    }
}
