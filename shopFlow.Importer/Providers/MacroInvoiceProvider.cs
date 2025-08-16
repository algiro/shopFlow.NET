using iText.Kernel.Pdf;
using shopFlow.Importer.Domain;
using shopFlow.Importer.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace shopFlow.Importer.Providers
{
    public class MacroInvoiceProvider : IInvoiceProvider
    {
        public IInvoiceInfo GetInvoiceInfo(Stream stream)
        {
            var invoiceInfo = new InvoiceInfo();
            invoiceInfo.SupplierName = "Makro";
            var items = new List<IInvoiceItem>();
            string invoiceNumber = string.Empty;
            DateTime invoiceDate = default;
            decimal totalAmount = 0;

            using (IPdfDocumentHelper pdfDocumentHelper = PdfDocumentHelper.Create(stream))
            {
                while (pdfDocumentHelper.NextPage())
                {
                    var text = pdfDocumentHelper.GetPageContent();
                    // Invoice Number
                    if (string.IsNullOrEmpty(invoiceNumber))
                    {
                        var idx = text.IndexOf("Factura:");
                        if (idx >= 0)
                        {
                            var after = text.Substring(idx + "Factura:".Length);
                            var lines = after.Split('\n', '\r').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
                            if (lines.Count > 0)
                            {
                                var match = System.Text.RegularExpressions.Regex.Match(lines[0], @"[0-9]+/[0-9]+\([0-9]+\)[0-9]+/\([0-9]+\)[0-9]+|");
                                if (match.Success)
                                    invoiceNumber = match.Value;
                                else
                                    invoiceNumber = lines[0].Split(' ')[0].Trim(); // fallback: first token
                            }
                        }
                    }
                    // Invoice Date
                    if (invoiceDate == default)
                    {
                        var idx = text.IndexOf("Fecha de venta:");
                        if (idx >= 0)
                        {
                            var after = text.Substring(idx + "Fecha de venta:".Length);
                            var lines = after.Split('\n', '\r').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
                            if (lines.Count > 0)
                            {
                                var match = System.Text.RegularExpressions.Regex.Match(lines[0], @"(\d{2}/\d{2}/\d{4} \d{2}:\d{2})");
                                if (match.Success)
                                {
                                    DateTime.TryParseExact(match.Value, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out invoiceDate);
                                }
                                else
                                {
                                    DateTime.TryParse(lines[0], out invoiceDate);
                                }
                            }
                        }
                    }
                    // Total Amount
                    if (totalAmount == 0)
                    {
                        var idx = text.LastIndexOf("Total a pagar");
                        if (idx >= 0)
                        {
                            var after = text.Substring(idx + "Total a pagar".Length);
                            var lines = after.Split('\n', '\r').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
                            if (lines.Count > 0)
                            {
                                var match = System.Text.RegularExpressions.Regex.Match(lines[0], @"\d+[,.]\d{2}");
                                if (match.Success)
                                {
                                    decimal.TryParse(match.Value.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out totalAmount);
                                }
                                else
                                {
                                    decimal.TryParse(lines[0].Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out totalAmount);
                                }
                            }
                        }
                    }
                    // Items extraction: process all lines in the PDF text
                    var allLines = text.Split('\n', '\r').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
                    foreach (var line in allLines)
                    {
                        // Match lines that start with a GTIN (at least 10 digits)
                        if (System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d{10,} "))
                        {
                            var parts = line.Split(' ').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                            // Find the index of the price (first decimal value after GTIN)
                            int gtinIndex = 0;
                            int priceIndex = -1;
                            for (int i = 1; i < parts.Length; i++)
                            {
                                if (System.Text.RegularExpressions.Regex.IsMatch(parts[i], @"^\d+[,.]\d{2}$"))
                                {
                                    priceIndex = i;
                                    break;
                                }
                            }
                            if (priceIndex > 1 && priceIndex + 2 < parts.Length)
                            {
                                // Fix: Only join columns for name up to priceIndex, so name does not include price/quantity columns
                                var name = string.Join(" ", parts.Skip(1).Take(priceIndex - 1));
                                // Remove trailing price/quantity tokens from name if present
                                name = System.Text.RegularExpressions.Regex.Replace(name, @"( [\d,.]+)+$", "");
                                if (!int.TryParse(parts[priceIndex + 1], out var quantity)) continue;
                                if (!decimal.TryParse(parts[priceIndex].Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price)) continue;
                                if (!decimal.TryParse(parts[priceIndex + 2].Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var total)) continue;
                                items.Add(new InvoiceItem
                                {
                                    Name = name,
                                    Quantity = quantity,
                                    Price = price,
                                    Total = total
                                });
                            }
                        }
                    }
                }
            }
            invoiceInfo.InvoiceNumber = invoiceNumber;
            invoiceInfo.InvoiceDate = invoiceDate;
            invoiceInfo.TotalAmount = totalAmount;
            invoiceInfo.Items = items;
            return invoiceInfo;
        }
    }
}
