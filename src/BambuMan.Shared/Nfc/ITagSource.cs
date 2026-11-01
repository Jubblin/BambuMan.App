using BambuMan.Shared.Models;

namespace BambuMan.Shared.Nfc;

/// <summary>
/// A tag reader other than the phone's own NFC, e.g. an external Bluetooth reader for iPhones, whose NFC can't unlock
/// Bambu Lab (MIFARE Classic) tags. Register implementations in DI; the scanner inventories every tag they read.
/// </summary>
public interface ITagSource
{
    /// <summary>Shown to the user as the reader's name, e.g. "Bluetooth reader".</summary>
    string DisplayName { get; }

    /// <summary>True while the reader can read a tag (connected and ready).</summary>
    bool IsAvailable { get; }

    /// <summary>Raised with the source when <see cref="IsAvailable"/> changes.</summary>
    event Action<ITagSource>? OnAvailabilityChanged;

    /// <summary>Raised with the source and the parsed tag each time a Bambu Lab tag is read.</summary>
    event Action<ITagSource, BambuFilamentInfo>? OnTagRead;
}
