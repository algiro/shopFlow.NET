using System;
using System.Collections.Generic;
using shopFlow.Importer.Providers;
using shopFlow.Importer.Domain;

namespace shopFlow.Importer.Domain
{
    public class InvoiceInfo : IInvoiceInfo
    {
        public string SupplierName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public IEnumerable<IInvoiceItem> Items { get; set; } = new List<IInvoiceItem>();
    }

    public class InvoiceItem : IInvoiceItem
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }
}
