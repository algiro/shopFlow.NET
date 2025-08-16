using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shopFlow.Importer.Domain
{
    public interface IInvoiceItem
    {
        string Name { get; }
        int Quantity { get; }
        decimal Price { get; }
        decimal Total { get; }
    }
}
