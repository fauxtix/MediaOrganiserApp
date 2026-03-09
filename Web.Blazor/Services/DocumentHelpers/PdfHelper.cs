using MediaOrganizerApp.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Exporting;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Services.DocumentHelpers
{
    public class PdfHelper : IPdfHelper
    {
        private static PdfHelper _Instancia;
        private readonly ILogger<PdfHelper> _logger;

        public static PdfHelper GetInstancia()
        {
            if (_Instancia == null)
            {
                _Instancia = new PdfHelper();
            }
            return _Instancia;
        }
        public Dictionary<Metadados, string> GetPdfMetadata_Sync(string ficheiro)
        {
            Dictionary<Metadados, string> dictionary = new Dictionary<Metadados, string>();
            try
            {
                using (FileStream docStream = new FileStream(ficheiro, FileMode.Open, FileAccess.Read))
                {
                    using (PdfLoadedDocument pdfDoc = new PdfLoadedDocument(docStream))
                    {
                        var Info = pdfDoc.DocumentInformation;
                        string sAutor = Info.Author;
                        DateTime dCriacao = Info.CreationDate;
                        var assunto = Info.Subject;
                        string sTitulo = Info.Title;
                        int iPaginas = Info.XmpMetadata.PagedTextSchema.NPages > 0 ? Info.XmpMetadata.PagedTextSchema.NPages : pdfDoc.Pages.Count;
                        var PalavrasChave = Info.Keywords;
                        dictionary.Add(Metadados.Titulo, sTitulo);
                        dictionary.Add(Metadados.Autor, sAutor);
                        dictionary.Add(Metadados.Assunto, assunto);
                        dictionary.Add(Metadados.Keywords, PalavrasChave);
                        dictionary.Add(Metadados.Paginas, iPaginas.ToString());
                        dictionary.Add(Metadados.AnoEdicao, dCriacao.Year.ToString());
                    }
                }

                return dictionary;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro na leitura de Pdf ({ficheiro}) - {ex.Message}");
                dictionary.Add(Metadados.Titulo, "Erro na leitura do Pdf");
                dictionary.Add(Metadados.Autor, "N/D");
                dictionary.Add(Metadados.Keywords, "");
                dictionary.Add(Metadados.Paginas, "0");
                dictionary.Add(Metadados.AnoEdicao, "2020");
                dictionary.Add(Metadados.Assunto, ex.Message);
                return dictionary;
            }
        }

        public Image GetPdfCover_Sync(string pathImagensPubs, string ficheiro, out string ficheiroGravado)
        {
            Image img = null;
            string ImagePath2Save = "";
            ficheiroGravado = "";
            bool found = false;
            try
            {
                if (!Directory.Exists(pathImagensPubs))
                {
                    Directory.CreateDirectory(pathImagensPubs);
                }

                using (FileStream docStream = new FileStream(ficheiro, FileMode.Open, FileAccess.Read))
                {
                    string tmpFileName = Path.GetFileNameWithoutExtension(ficheiro);
                    string[] words = tmpFileName.Split("\\s+");
                    if (words.Length == 1)
                        words = tmpFileName.Split("_");
                    if (words.Length == 1)
                        words = tmpFileName.Split(" ");

                    using (PdfLoadedDocument document = new PdfLoadedDocument(docStream))
                    {
                        foreach (PdfPageBase page in document.Pages)
                        {
                            found = false;
                            foreach (Stream image in page.ExtractImages())
                            {
                                if (image is null)
                                    continue;
                                else
                                {
                                    ImagePath2Save = String.Format(@"{0}{1}.png", pathImagensPubs, tmpFileName);
                                    var xImagePath2Save = $"{Path.Combine(pathImagensPubs, tmpFileName)}.png";
                                }  // image is null?
                                   // out file (png)
                                ficheiroGravado = ImagePath2Save;
                                img = Image.FromStream(image);  // devolve 1ª imagem encontrada
                                found = true;
                                break;
                            }  //foreach (extracted images)
                            if (found)
                                return img;
                        } // foreach (pages)
                    }
                } // try

                return null;
            }
            catch
            {
                return null;
            }
        }

        public int getNumberOfPdfPages(string ficheiro)
        {
            return ContaPaginasPdf(ficheiro);
        }

        private int ContaPaginasPdf(string ficheiro)
        {

            try
            {
                using (FileStream fs = new FileStream(ficheiro, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader r = new StreamReader(fs))
                    {
                        string pdfText = r.ReadToEnd();

                        Regex rx1 = new Regex(@"/Type\s*/Page[^s]");
                        MatchCollection matches = rx1.Matches(pdfText);

                        return matches.Count;
                    }
                }

            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}