using BambuMan.Interfaces;

namespace BambuMan.Implementations
{
    /// <summary>
    /// Placeholder so the iOS build resolves <see cref="ISoftKeyboard"/>. Does nothing yet; iOS keyboard handling comes with the platform services task.
    /// </summary>
    public class IosSoftKeyboard : ISoftKeyboard
    {
        public void Show() { }

        public void Hide() { }
    }
}
