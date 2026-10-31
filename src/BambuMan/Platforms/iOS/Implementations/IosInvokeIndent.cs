using BambuMan.Interfaces;
using MessageUI;

namespace BambuMan.Implementations
{
    /// <summary>
    /// Opens the Mail composer with the logs. Without a Mail account (and on the simulator) there is no composer, so the
    /// logs go to the share sheet instead, where the user can pick another mail app, Notes, AirDrop or Copy.
    /// </summary>
    public class IosInvokeIndent : IInvokeIndent
    {
        public async Task SendEmail(string toEmail, string subject, string body)
        {
            if (MFMailComposeViewController.CanSendMail)
            {
                await Email.Default.ComposeAsync(new EmailMessage(subject, body, toEmail));
                return;
            }

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = subject,
                Subject = subject,
                Text = $"Send to {toEmail}\r\n\r\n{body}"
            });
        }
    }
}
