using BoldReports.RDL.DOM;
using MediaOrganizerApp.Application.Interfaces.Services;
using MsgReader.Outlook;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System.IO;
using System.Threading.Tasks;

namespace Web.Blazor.Services.DocumentHelpers
{
    public class OutlookMsgHelper : IOutlookMsgHelper
    {
        private static OutlookMsgHelper _Instancia;

        public static OutlookMsgHelper GetInstancia()
        {
            if (_Instancia == null)
            {
                _Instancia = new OutlookMsgHelper();
            }
            return _Instancia;
        }


        public async Task<string> ConvertMsgToPdfAsync(string msgFilePath)
        {
            using (var msgReader = new Storage.Message(msgFilePath))
            {
                // Extrair o corpo do e-mail como HTML
                string emailBodyHtml = msgReader.BodyHtml ?? msgReader.BodyText;

                // Caminho para salvar o HTML temporário
                string tempHtmlFilePath = Path.Combine(Path.GetTempPath(), "email.html");

                // Diretório para salvar imagens embutidas
                var imageFolderPath = Path.Combine(Path.GetTempPath(), "emailImages");
                Directory.CreateDirectory(imageFolderPath);

                // Salvar imagens embutidas e atualizar referências no HTML
                foreach (var attachment in msgReader.Attachments)
                {
                    if (attachment is Storage.Attachment att && att.IsInline && !string.IsNullOrEmpty(att.ContentId))
                    {
                        var imagePath = Path.Combine(imageFolderPath, att.FileName);
                        await File.WriteAllBytesAsync(imagePath, att.Data);

                        string cid = $"cid:{att.ContentId}";
                        emailBodyHtml = emailBodyHtml.Replace(cid, imagePath);
                    }
                }

                // Salvar o corpo do e-mail como HTML temporário
                await File.WriteAllTextAsync(tempHtmlFilePath, emailBodyHtml);

                // Configurar o conversor HTML para PDF
                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
                BlinkConverterSettings settings = new BlinkConverterSettings();
                htmlConverter.ConverterSettings = settings;

                // Converter HTML para PDF
                PdfDocument document = htmlConverter.Convert(tempHtmlFilePath);

                // Caminho para salvar o PDF com o mesmo nome do arquivo .msg, mas extensão .pdf
                string pdfFilePath = Path.Combine(Path.GetDirectoryName(msgFilePath), $"{Path.GetFileNameWithoutExtension(msgFilePath)}.pdf");

                // Salvar o documento PDF
                using (FileStream outputStream = new FileStream(pdfFilePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    document.Save(outputStream);
                }

                // Fechar o documento PDF
                document.Close(true);

                // Limpar arquivos temporários (HTML e imagens)
                File.Delete(tempHtmlFilePath);
                Directory.Delete(imageFolderPath, true);

                return pdfFilePath; // Retorna o caminho completo para o PDF gerado
            }
        }
    }
}


