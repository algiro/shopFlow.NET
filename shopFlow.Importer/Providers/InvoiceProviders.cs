using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace shopFlow.Importer.Providers
{
    public static class InvoiceProviders
    {
        public static string MACRO_Reg_Exp = @"\d{2}\(\d{3}\)\d{4}\(\d{4}\)\d*.pdf";
        public static string DINO_Reg_Exp = @"^TICKET_PDF_NUM_\d{6}.\d{4}.\d{4}.\d*.PDF";
        public static string MERCADONA_Reg_Exp = @"\S-\S{5}-\S{5,}.pdf";
        public static bool TryGetInvoiceProvider(string invoiceFileName, [NotNullWhen(true)] out IInvoiceProvider provider)
        {
            provider = null;
            if (Regex.IsMatch(invoiceFileName, MACRO_Reg_Exp, RegexOptions.IgnoreCase))
            {
                provider = new MacroInvoiceProvider();
                return true;
            }
            else if (Regex.IsMatch(invoiceFileName, DINO_Reg_Exp, RegexOptions.IgnoreCase))
            {
                provider = new DinoSolInvoiceProvider();
                return true;
            }
            else if (Regex.IsMatch(invoiceFileName, MERCADONA_Reg_Exp, RegexOptions.IgnoreCase))
            {
                provider = new MercadonaInvoiceProvider();
                return true;
            }
            else
            {
                provider = null;
                return false;
            }
        }
    }
}
