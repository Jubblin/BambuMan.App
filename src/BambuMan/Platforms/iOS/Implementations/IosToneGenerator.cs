using BambuMan.Shared.Interfaces;

namespace BambuMan.Implementations
{
    /// <summary>
    /// Placeholder so the iOS build resolves <see cref="IToneGenerator"/>. Plays nothing yet; the real iOS sounds come with the platform services task.
    /// </summary>
    public class IosToneGenerator : IToneGenerator
    {
        public Task PlayTone() => Task.CompletedTask;

        public Task PlaySystemTone() => Task.CompletedTask;

        public Task PlayAlarmTone() => Task.CompletedTask;

        public Task PlayToneSuccess() => Task.CompletedTask;
    }
}
