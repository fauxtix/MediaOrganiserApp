using System.Collections.Generic;
using System.IO;
using System.Text;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Data
{
    public class OutlookService
    {
        public OutlookService()
        {

        }
        public Dictionary<Metadados, string> ReadMSGMessage(string msgMessage)
        {
            Dictionary<Metadados, string> dictionary = new Dictionary<Metadados, string>();

            try
            {
                using (var msg = new MsgReader.Outlook.Storage.Message(msgMessage))
                {
                    var from = msg.Sender;
                    var sentOn = msg.SentOn;
                    var recipientsTo = msg.GetEmailRecipients(MsgReader.Outlook.RecipientType.To, false, false);
                    var recipientsCc = msg.GetEmailRecipients(MsgReader.Outlook.RecipientType.Cc, false, false);
                    var subject = msg.Subject;
                    var htmlBody = msg.BodyText; // msg.BodyHtml;

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(sentOn.ToString());
                    sb.AppendLine(htmlBody);

                    dictionary.Add(Metadados.Titulo, subject);
                    dictionary.Add(Metadados.Autor, from.DisplayName);
                    dictionary.Add(Metadados.Assunto, subject);
                    dictionary.Add(Metadados.Keywords, sb.ToString());
                    dictionary.Add(Metadados.Paginas, "0");
                    dictionary.Add(Metadados.AnoEdicao, sentOn.Value.Year.ToString());
                }
                return dictionary;

            }
            catch
            {
                throw;
            }
        }

        public void ReadEMLMessage(string emlMessage)
        {
            var fileInfo = new FileInfo(emlMessage);
            var eml = MsgReader.Mime.Message.Load(fileInfo);

            if (eml.Headers != null)
            {
                if (eml.Headers.To != null)
                {
                    foreach (var recipient in eml.Headers.To)
                    {
                        var to = recipient.Address;
                    }
                }
            }

            var subject = eml.Headers.Subject;

            if (eml.TextBody != null)
            {
                var textBody = System.Text.Encoding.UTF8.GetString(eml.TextBody.Body);
            }

            if (eml.HtmlBody != null)
            {
                var htmlBody = System.Text.Encoding.UTF8.GetString(eml.HtmlBody.Body);
            }
        }
    }
}
