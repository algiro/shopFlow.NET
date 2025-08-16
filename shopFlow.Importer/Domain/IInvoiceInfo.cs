using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shopFlow.Importer.Domain
{
    public interface IInvoiceInfo
    {
        string SupplierName { get; }
        string InvoiceNumber { get; }
        DateTime InvoiceDate { get; }
        decimal TotalAmount { get; }
        IEnumerable<IInvoiceItem> Items { get; }
    }
}
