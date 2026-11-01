namespace BambuMan.Interfaces
{
    /// <summary>
    /// Diagnostic only: runs one phone NFC session and reports what the platform says about the tag held to it (type,
    /// UID, NDEF status, errors). It never reads spool data. Registered only where it is needed (iOS), so its absence
    /// hides the probe.
    /// </summary>
    public interface ITagProbe
    {
        /// <summary>
        /// Runs one probe session, passing each finding to <paramref name="report"/> as it happens. Completes when the
        /// session ends: probe finished, cancelled or timed out.
        /// </summary>
        Task RunAsync(Action<string> report);
    }
}
