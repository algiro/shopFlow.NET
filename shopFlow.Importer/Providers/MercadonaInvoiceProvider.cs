using Microsoft.Extensions.Logging;
using NLog;
using shopFlow.Importer.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace shopFlow.Importer.Providers
{
    public class MercadonaInvoiceProvider : IInvoiceProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, int> SpanishMonths = new()
        {
            { "ene", 1 }, { "feb", 2 }, { "mar", 3 }, { "abr", 4 },
            { "may", 5 }, { "jun", 6 }, { "jul", 7 }, { "ago", 8 },
            { "sep", 9 }, { "oct", 10 }, { "nov", 11 }, { "dic", 12 }
        };

        private decimal ParseDecimal(string value)
        {
            // Remove euro symbol and any whitespace
            var numStr = value.Replace("€", "").Trim();
            // Replace comma with period for decimal point
            numStr = numStr.Replace(",", ".");
            decimal.TryParse(numStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
            return result;
        }

        public IInvoiceInfo GetInvoiceInfo(Stream stream)
        {
            var invoiceInfo = new InvoiceInfo();
            invoiceInfo.SupplierName = "Mercadona";
            var items = new List<IInvoiceItem>();
            
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            using (IPdfDocumentHelper pdfDocumentHelper = PdfDocumentHelper.Create(stream))
            {
                while (pdfDocumentHelper.NextPage())
                {
                    var text = pdfDocumentHelper.GetPageContent();
                    // Invoice
                    var invoiceRegExp = @"(N. Factura:)\s+(\S*)";
                    var invoiceMatch = Regex.Match(text, invoiceRegExp);
                    if (invoiceMatch.Success) {
                        invoiceInfo.InvoiceNumber = invoiceMatch.Groups[2].Value.Trim();
                        logger.Info($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, found invoice number: {invoiceInfo.InvoiceNumber}");
                    }
                    else
                    {
                        logger.Warn($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, no invoice number found in text: {text}");
                    }

                    // 

                    var invoiceDateRegExp = @"(Fecha Factura:)([\d/]*)";
                    var invoiceDateMatch = Regex.Match(text, invoiceDateRegExp);
                    if (invoiceDateMatch.Success)
                    {
                        var invoiceDateStr = invoiceDateMatch.Groups[2].Value.Trim();
                        DateTime.TryParseExact(invoiceDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var invoiceDate);
                        invoiceInfo.InvoiceDate = invoiceDate;
                        logger.Info($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, found invoice date: {invoiceInfo.InvoiceNumber}");
                    }
                    else
                    {
                        logger.Warn($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, no invoice date found in text: {text}");
                    }

                    //  

                    var invoiceTotRegExp = @"(Total Factura.)([\d,.]*)";
                    var invoiceTotMatch = Regex.Match(text, invoiceTotRegExp);
                    if (invoiceDateMatch.Success)
                    {
                        var invoiceTotStr = invoiceTotMatch.Groups[2].Value.Trim().Replace(',','.');
                        if (decimal.TryParse(invoiceTotStr,out var TotalAmount))
                        {
                            invoiceInfo.TotalAmount = TotalAmount;
                        }
                        logger.Info($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, found invoice TotalAmount: {invoiceInfo.TotalAmount}");
                    }
                    else
                    {
                        logger.Warn($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, no invoice TotalAmount found in text: {text}");
                    }
                }
            }

            invoiceInfo.Items = items;
            return invoiceInfo;
        }

    }
}
