using System;
using System.IO;
using System.Linq;
using shopFlow.Importer.Providers;
using shopFlow.Importer.Domain;
using Xunit;
using FluentAssertions;

namespace shopFlow.Test.Test.Providers
{
    public class MacroInvoiceProviderTest
    {
        [Fact]
        public void GetInvoiceInfo_ParsesPdfCorrectly()
        {
            var provider = new MacroInvoiceProvider();
            using var stream = File.OpenRead("./Resources/InvoiceSamples/00(021)0205(2025)002727.pdf");
            var info = provider.GetInvoiceInfo(stream);

            Assert.Equal("Makro", info.SupplierName);
            Assert.Equal("0/0(021)0205/(2025)002727", info.InvoiceNumber);
            Assert.Equal(new DateTime(2025, 8, 8, 14, 44, 0), info.InvoiceDate);
            Assert.Equal(141.72m, info.TotalAmount);
            Assert.NotNull(info.Items);
            var items = info.Items.ToList();
            items.Count().Should().Be(10);
            var firstItem = items.First();
            firstItem.Name.Should().Be("ARANDANOS MC 1KG BL");
            firstItem.Total.Should().Be(5.89m);
        }
    }
}
