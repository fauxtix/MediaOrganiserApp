using MediaOrganizerApp.Application.Interfaces.Services;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using System.Collections.Generic;
using System.IO;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Services.DocumentHelpers
{
    public class DocxHelper : IDocxHelper
    {
        private static DocxHelper _Instancia;

        public static DocxHelper GetInstancia()
        {
            if (_Instancia == null)
            {
                _Instancia = new DocxHelper();
            }
            return _Instancia;
        }

        public int ContaPaginasDocWord(string ficheiro)
        {

            using (WordDocument document = new WordDocument())
            {
                using (FileStream fileStreamPath = new FileStream(ficheiro, FileMode.Open, FileAccess.Read))
                {
                    document.Open(fileStreamPath, FormatType.Automatic);

                    var numberOfPages = document.BuiltinDocumentProperties.PageCount;
                    document.Close();
                    document.Dispose();
                    return numberOfPages;
                }
            }

        }

        public Dictionary<Metadados, string> GetDocxMetadata(FileStream fileStream)
        {
            Dictionary<Metadados, string> dictionary = new Dictionary<Metadados, string>();
            using (WordDocument document = new WordDocument(fileStream, FormatType.Automatic))
            {
                dictionary.Add(Metadados.Autor, document.BuiltinDocumentProperties.Author);
                dictionary.Add(Metadados.Assunto, !string.IsNullOrEmpty(document.BuiltinDocumentProperties.Subject)
                    ? document.BuiltinDocumentProperties.Subject
                    : $"{document.BuiltinDocumentProperties.Company} ({document.BuiltinDocumentProperties.ApplicationName})");
                dictionary.Add(Metadados.Keywords, document.BuiltinDocumentProperties.Keywords);
                dictionary.Add(Metadados.Paginas, document.BuiltinDocumentProperties.PageCount.ToString());
                dictionary.Add(Metadados.AnoEdicao, document.BuiltinDocumentProperties.CreateDate.ToString("yyyy"));
                document.Close();
            }
            return dictionary;
        }

        public string ConvertToPdf(string docxFile)
        {
            using (FileStream docStream = new FileStream(docxFile, FileMode.Open, FileAccess.Read))
            {
                WordDocument wordDocument = new WordDocument(docStream, FormatType.Automatic);
                DocIORenderer render = new DocIORenderer();
                using PdfDocument pdfDocument = render.ConvertToPDF(wordDocument);
                render.Dispose();
                wordDocument.Dispose();
                var output = Path.Combine("Files","Uploads", "Publications", docxFile.Replace("docx", "pdf"));
                using (FileStream outputFileStream = new FileStream(output, FileMode.Create, FileAccess.ReadWrite))
                {
                    pdfDocument.Save(outputFileStream);
                }

                pdfDocument.Close(true);
                return output;
            }
            //File.Delete(docxFile);


        }
    }
}


