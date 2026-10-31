using AudioToolbox;
using BambuMan.Shared.Interfaces;

namespace BambuMan.Implementations
{
    /// <summary>
    /// iOS has no tone generator; built-in system sounds stand in for the Android tones. Like all system sounds they
    /// follow the ringer switch, so a muted phone stays quiet.
    /// </summary>
    public class IosToneGenerator : IToneGenerator
    {
        private const uint KeyPressClick = 1104;
        private const uint Tink = 1057;
        private const uint NegativeAck = 1053;
        private const uint Alarm = 1005;

        public Task PlayTone() => PlayAsync(KeyPressClick);

        public Task PlaySystemTone() => PlayAsync(NegativeAck);

        public Task PlayAlarmTone() => PlayAsync(Alarm);

        public Task PlayToneSuccess() => PlayAsync(Tink);

        private static Task PlayAsync(uint soundId)
        {
            new SystemSound(soundId).PlaySystemSound();
            return Task.CompletedTask;
        }
    }
}
