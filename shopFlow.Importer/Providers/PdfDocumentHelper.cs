using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shopFlow.Importer.Providers
{
    public interface IPdfDocumentHelper : IDisposable
    {
        int CurrentPageNumber { get; }

        string GetPageContent();
        bool NextPage();

    }

    public static class PdfDocumentHelper
    {
        public static IPdfDocumentHelper Create(Stream stream) => new DefaultPdfDocumentHelper(stream);

        private class DefaultPdfDocumentHelper(Stream stream) : IPdfDocumentHelper
        {

            private readonly PdfDocument document = new PdfDocument(new PdfReader(stream));
            void IDisposable.Dispose() => document.Close();
            private int NumberOfPages => document.GetNumberOfPages();
            public int CurrentPageNumber { get; private set; } = 0;

            public string GetPageContent()
            {
                var page = document.GetPage(CurrentPageNumber);
                var strategy = new LocationTextExtractionStrategy();
                return PdfTextExtractor.GetTextFromPage(page, strategy);
            }

            public bool NextPage()
            {
                CurrentPageNumber++;
                return CurrentPageNumber <= NumberOfPages;
            }
        }
    }
}
