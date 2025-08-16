using shopFlow.Importer.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shopFlow.Importer.Providers
{
    public interface IInvoiceProvider
    {
        IInvoiceInfo GetInvoiceInfo(Stream stream);
    }
}
