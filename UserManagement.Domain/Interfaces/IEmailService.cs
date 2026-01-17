using System.Threading.Tasks;

namespace UserManagement.Domain.Interfaces
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string userName);
    }
}
