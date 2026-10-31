using BambuMan.Interfaces;

namespace BambuMan.Implementations
{
    /// <summary>
    /// Placeholder so the iOS build resolves <see cref="IInvokeIndent"/>. Does nothing yet; the iOS mail composer comes with the platform services task.
    /// </summary>
    public class IosInvokeIndent : IInvokeIndent
    {
        public Task SendEmail(string toEmail, string subject, string body) => Task.CompletedTask;
    }
}
