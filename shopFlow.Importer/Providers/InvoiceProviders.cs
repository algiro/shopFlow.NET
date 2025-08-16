using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shopFlow.Importer.Providers
{
    public static class InvoiceProviders
    {
        public static string MACRO_Reg_Exp = @"\d{2}\(\d{3}\)\d{4}\(\d{4}\)\d*.pdf";
        public static string DINO_Reg_Exp = @"^TICKET_PDF_NUM_\d{6}.\d{4}.\d{4}.\d*.PDF";
        public static bool TryGetInvoiceProvider(string invoiceFileName, [NotNullWhen(true)] out IInvoiceProvider provider)
        {
            provider = null;
            if (System.Text.RegularExpressions.Regex.IsMatch(invoiceFileName, MACRO_Reg_Exp, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                provider = new MacroInvoiceProvider();
                return true;
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(invoiceFileName, DINO_Reg_Exp, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                provider = new DinoSolInvoiceProvider();
                return true;
            }
            return false;
        }
    }
}
