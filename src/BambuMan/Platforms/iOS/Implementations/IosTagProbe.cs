using BambuMan.Interfaces;
using CoreFoundation;
using CoreNFC;
using Foundation;

namespace BambuMan.Implementations
{
    /// <summary>
    /// Reports what Core NFC sees on a tag: type, MIFARE family, UID, historical bytes, NDEF status, and the answer to
    /// one unauthenticated MIFARE READ of block 0. Bambu spool tags are MIFARE Classic, whose data needs Crypto1 key-A
    /// authentication that Core NFC can't do, so this settles only whether iOS detects the tag and exposes its UID.
    /// Results go to the caller only; nothing is sent anywhere.
    /// </summary>
    public class IosTagProbe : NFCTagReaderSessionDelegate, ITagProbe
    {
        private const byte MifareReadCommand = 0x30;

        private NFCTagReaderSession? session;
        private TaskCompletionSource? completion;
        private Action<string>? report;
        private bool detected;
        private bool finished;

        public Task RunAsync(Action<string> report)
        {
            if (completion != null) return completion.Task;

            if (!NFCReaderSession.ReadingAvailable)
            {
                report("NFC reading is not available on this device.");
                return Task.CompletedTask;
            }

            this.report = report;
            detected = false;
            finished = false;
            completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            Report($"Probe started on {DeviceInfo.Current.Model}, iOS {DeviceInfo.Current.VersionString}. Polling ISO 14443 and ISO 15693.");

            var started = completion;

            try
            {
                // Kept in a field so the session lives until DidInvalidate.
                session = new NFCTagReaderSession(NFCPollingOption.Iso14443 | NFCPollingOption.Iso15693, this, DispatchQueue.MainQueue)
                {
                    AlertMessage = "Hold the spool tag to the top of the iPhone."
                };
                session.BeginSession();
            }
            catch (Exception ex)
            {
                Report($"Probe could not start: {ex.Message}");
                Complete();
            }

            return started.Task;
        }

        public override void DidDetectTags(NFCTagReaderSession session, INFCTag[] tags)
        {
            try
            {
                detected = true;
                Report($"Detected {tags.Length} tag(s).");

                for (var i = 0; i < tags.Length; i++) Report($"Tag {i + 1}: {Describe(tags[i])}");

                var tag = tags[0];

                session.ConnectTo(tag, error =>
                {
                    try
                    {
                        if (error != null)
                        {
                            Report($"Connect failed: {FormatError(error)}");
                            Finish(session, "Connect failed.");
                            return;
                        }

                        Report("Connected.");
                        QueryNdef(session, tag);
                    }
                    catch (Exception ex)
                    {
                        Fail(session, ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Fail(session, ex);
            }
        }

        public override void DidInvalidate(NFCTagReaderSession session, NSError error)
        {
            try
            {
                var readerError = (NFCReaderError)error.Code;

                if (finished) Report("Session closed.");
                else if (readerError == NFCReaderError.ReaderSessionInvalidationErrorSessionTimeout && !detected) Report("Session timed out: no tag was detected.");
                else if (readerError == NFCReaderError.ReaderSessionInvalidationErrorUserCanceled && !detected) Report("Cancelled before a tag was detected.");
                else Report($"Session ended before the probe finished: {FormatError(error)}");
            }
            catch (Exception ex)
            {
                Report($"Probe error: {ex.Message}");
            }
            finally
            {
                Complete();
            }
        }

        /// <summary>Ends the run so the next probe can start a new session.</summary>
        private void Complete()
        {
            var done = completion;
            completion = null;
            session = null;
            report = null;
            done?.TrySetResult();
        }

        private void QueryNdef(NFCTagReaderSession session, INFCTag tag)
        {
            var ndefTag = NfcNdefTagExtensions.GetNdefTag(tag);

            if (ndefTag == null)
            {
                Report("NDEF status: not available for this tag type.");
                ReadMifareBlockZero(session, tag);
                return;
            }

            ndefTag.QueryNdefStatus((status, capacity, error) =>
            {
                try
                {
                    if (error != null) Report($"NDEF status failed: {FormatError(error)}");
                    else Report($"NDEF status: {status}, capacity {capacity} bytes.");

                    ReadMifareBlockZero(session, tag);
                }
                catch (Exception ex)
                {
                    Fail(session, ex);
                }
            });
        }

        /// <summary>A read, never a write. MIFARE Classic wants authentication first, so a failure here is expected.</summary>
        private void ReadMifareBlockZero(NFCTagReaderSession session, INFCTag tag)
        {
            var mifareTag = tag.Type == NFCTagType.MiFare ? tag.AsNFCMiFareTag : null;

            if (mifareTag == null)
            {
                Finish(session, "Probe finished.");
                return;
            }

            Report("Sending MIFARE READ block 0 (30 00), without authentication.");

            mifareTag.SendMiFareCommand(NSData.FromArray(new byte[] { MifareReadCommand, 0x00 }), (response, error) =>
            {
                try
                {
                    if (error != null) Report($"READ block 0 failed: {FormatError(error)}");
                    else Report($"READ block 0 answered: {Hex(response)}");

                    Finish(session, "Probe finished.");
                }
                catch (Exception ex)
                {
                    Fail(session, ex);
                }
            });
        }

        private void Finish(NFCTagReaderSession session, string alertMessage)
        {
            finished = true;
            session.AlertMessage = alertMessage;
            session.InvalidateSession();
        }

        private void Fail(NFCTagReaderSession session, Exception ex)
        {
            Report($"Probe error: {ex.Message}");
            finished = true;
            session.InvalidateSession("Probe error.");
        }

        private void Report(string line) => report?.Invoke(line);

        private static string Describe(INFCTag tag)
        {
            var text = $"type {tag.Type}";

            switch (tag.Type)
            {
                case NFCTagType.MiFare:
                    var mifareTag = tag.AsNFCMiFareTag;
                    if (mifareTag != null) text += $", MIFARE family {mifareTag.MifareFamily}, UID {Hex(mifareTag.Identifier)}, historical bytes {Hex(mifareTag.HistoricalBytes)}";
                    break;
                case NFCTagType.Iso7816Compatible:
                    var iso7816Tag = tag.AsNFCIso7816Tag;
                    if (iso7816Tag != null) text += $", UID {Hex(iso7816Tag.Identifier)}, initial AID {iso7816Tag.InitialSelectedAid}, historical bytes {Hex(iso7816Tag.HistoricalBytes)}, application data {Hex(iso7816Tag.ApplicationData)}";
                    break;
                case NFCTagType.Iso15693:
                    var iso15693Tag = tag.AsNFCIso15693Tag;
                    if (iso15693Tag != null) text += $", UID {Hex(iso15693Tag.Identifier)}, IC manufacturer {iso15693Tag.IcManufacturerCode}";
                    break;
                case NFCTagType.FeliCa:
                    var feliCaTag = tag.AsNFCFeliCaTag;
                    if (feliCaTag != null) text += $", IDm {Hex(feliCaTag.CurrentIdm)}";
                    break;
            }

            return $"{text}. ATQA/SAK: not exposed by iOS.";
        }

        private static string Hex(NSData? data)
        {
            if (data == null) return "none";
            if (data.Length == 0) return "empty";

            return string.Join(":", data.ToByteArray().Select(b => b.ToString("X2")));
        }

        private static string FormatError(NSError error) => $"{error.LocalizedDescription} ({error.Domain} {error.Code})";
    }
}
