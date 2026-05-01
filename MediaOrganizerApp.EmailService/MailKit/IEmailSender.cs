using System.Threading.Tasks;

namespace MediaOrganizerApp.EmailService.MailKit
{
    public interface IEmailSender
    {
        void SendEmail(Message message);
        Task SendEmailAsync(Message message);
    }
}
