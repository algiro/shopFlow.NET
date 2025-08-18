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
    public class DinoSolInvoiceProvider : IInvoiceProvider
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
            invoiceInfo.SupplierName = "Hiperdino";
            var items = new List<IInvoiceItem>();
            
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            using (IPdfDocumentHelper pdfDocumentHelper = PdfDocumentHelper.Create(stream))
            {
                while (pdfDocumentHelper.NextPage())
                {
                    var text = pdfDocumentHelper.GetPageContent();
                    // Invoice
                    var invoiceRegExp = @"(FACTURA N.:)\s+([\d\.]*)";
                    var invoiceMatch = Regex.Match(text, invoiceRegExp);
                    if (invoiceMatch.Success) {
                        invoiceInfo.InvoiceNumber = invoiceMatch.Groups[2].Value.Trim();
                        logger.Info($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, found invoice number: {invoiceInfo.InvoiceNumber}");
                    }
                    else
                    {
                        logger.Warn($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, no invoice number found in text: {text}");
                    }
                    

                    // Invoice Date
                    var dateLabel = "Fecha factura";
                    var idx = text.IndexOf(dateLabel);
                    logger.Info($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, text length: {text.Length}, dateIdx:{idx}");

                    if (idx >= 0)
                    {
                        var after = text.Substring(idx + dateLabel.Length).Trim();
                        var dateStr = after.Split('\n', '\r')[0].Trim();
                        var dateParts = dateStr.Split('-');
                        if (dateParts.Length == 3)
                        {
                            var day = int.Parse(dateParts[0]);
                            var month = SpanishMonths[dateParts[1].ToLower()];
                            var year = int.Parse("20" + dateParts[2]);
                            invoiceInfo.InvoiceDate = new DateTime(year, month, day);
                        }
                    }

                    // Total Amount
                    var lastTotal = text.LastIndexOf("TOTAL");
                    logger.Info($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, text length: {text.Length}, lastTotalIdx:{lastTotal}");
                    var totalInvoiceRegExp = @"IMP TOTAL IMPORTE(.*\n)*TOTAL\s*([\d\,.]*)";

                    var totInvoiceMatch = Regex.Match(text, totalInvoiceRegExp);
                    if (totInvoiceMatch.Success)
                    {
                        invoiceInfo.TotalAmount = decimal.Parse(totInvoiceMatch.Groups[2].Value.Trim().Replace(',','.'));
                        logger.Info($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, found invoice total: {invoiceInfo.TotalAmount}");
                    }
                    else
                    {
                        logger.Warn($"Processing page: {pdfDocumentHelper.CurrentPageNumber}, no invoice total found in text: {text}");
                    }

                    // Items
                    var header = "ID Artículo EAN Descripción Cantidad Precio Unitario TOTAL";
                    var headerIdx = text.IndexOf(header);
                    if (headerIdx >= 0)
                    {
                        var afterHeader = text.Substring(headerIdx + header.Length);
                        var lines = afterHeader.Split('\n', '\r')
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrEmpty(l) && char.IsDigit(l[0]))
                            .ToList();

                        foreach (var line in lines)
                        {
                            var parts = line.Split(' ')
                                .Where(p => !string.IsNullOrWhiteSpace(p))
                                .ToArray();

                            if (parts.Length >= 6)
                            {
                                // Find indices of numeric values (quantity, price, total)
                                var numericIndices = new List<int>();
                                for (int i = parts.Length - 1; i >= 0 && numericIndices.Count < 3; i--)
                                {
                                    if (Regex.IsMatch(parts[i].Replace("€", "").Replace(",", "."), @"^\d+\.?\d*$"))
                                    {
                                        numericIndices.Insert(0, i);
                                    }
                                }

                                if (numericIndices.Count == 3)
                                {
                                    var firstNumericIndex = numericIndices[0];
                                    var name = string.Join(" ", parts.Skip(2).Take(firstNumericIndex - 2));
                                    var quantity = ParseDecimal(parts[numericIndices[0]]);
                                    var price = ParseDecimal(parts[numericIndices[1]]);
                                    var total = ParseDecimal(parts[numericIndices[2]]);

                                    items.Add(new InvoiceItem
                                    {
                                        Name = name,
                                        Quantity = (int)quantity,
                                        Price = price,
                                        Total = total
                                    });
                                }
                            }
                        }
                    }
                }
            }

            invoiceInfo.Items = items;
            return invoiceInfo;
        }

    }
}
