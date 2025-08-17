using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shopFlow.Importer.Providers
{
    public interface IInvoicesLookup
    {
        IEnumerable<string> GetInvoicesIds();
        Stream GetInvoiceById(string id);
        Task MoveInvoiceToImportedFolderAsync(string invoiceId);
    }
}
