using BambuMan.Interfaces;

namespace BambuMan.Implementations
{
    /// <summary>
    /// iOS opens the keyboard for an input focused from script without a tap only inside WKWebView's own rules, and
    /// there is no public API to ask for it, so <see cref="Show"/> does nothing. It is only needed after an NFC spool
    /// read, which the iPhone can't do with Bambu tags anyway.
    /// </summary>
    public class IosSoftKeyboard : ISoftKeyboard
    {
        public void Show() { }

        public void Hide() => Platform.GetCurrentUIViewController()?.View?.Window?.EndEditing(true);
    }
}
