using System;

namespace MediaOrganizerApp.Domain.Entities
{
    public class MailMessage
    {
        public int Id { get; set; }
        public DateTime DataMovimento { get; set; }
        public string Assunto { get; set; }
        public string Body { get; set; }
        public bool TemAnexo { get; set; }
        public int Rating { get; set; }
        public string Notas { get; set; }
    }
}
