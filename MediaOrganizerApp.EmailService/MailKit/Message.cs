using MimeKit;

namespace MediaOrganizerApp.EmailService.MailKit

{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public byte[] Attachment { get; set; }

        public Message(IEnumerable<string> to, string subject, string content, byte[] attachment)
        {
            To = new List<MailboxAddress>();

            To.AddRange(to.Select(x => new MailboxAddress("ClinicaSoft", x)));
            Subject = subject;
            Content = content;
            Attachment = attachment;
        }
    }
}
