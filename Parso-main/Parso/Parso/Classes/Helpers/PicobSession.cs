using Newtonsoft.Json.Linq;
using Parso.Utils;
using PicobControllers;
using PicobControllers.Controllers;
using Serilog;
using System.Diagnostics;

namespace Parso.Classes.Helpers
{
    internal readonly record struct PicobStatus(bool Connected, long? AgeMs, int? C);

    internal sealed class PicobSession
    {
        // Singleton instance
        private static readonly Lazy<PicobSession> _instance =
            new Lazy<PicobSession>(() => new PicobSession());

        public static PicobSession Instance => _instance.Value;

        private const int ReconnectInitialMs = 1000;
        private const int ReconnectMaxMs = 10000;

        private readonly ProjectConstants _projectConstants = ProjectConstants.Instance;
        private readonly IPicob _picobController = new PicobController();
        private readonly object _lock = new object();
        private readonly object _cacheLock = new object();
        private volatile bool _isOpen;
        private bool _hasWritten;
        private long _lastWriteTimestamp;
        private bool _openFailureLogged;

        private int _started;
        private int _waitingCommands;
        private int _consecutiveFailures;
        private bool _hasPresence;
        private int _lastPresence;
        private long _lastPresenceTimestamp;

        // Private constructor for singleton
        private PicobSession()
        {
        }

        public bool IsOpen => _isOpen;

        public bool Open()
        {
            lock (_lock)
            {
                if (_isOpen) return true;

                try
                {
                    _isOpen = _picobController.OpenPicobPort(_projectConstants.PICOB_COM_PORT, _projectConstants.PICOB_BAUD_RATE,
                        _projectConstants.PICOB_DTR_ENABLE, _projectConstants.PICOB_RTS_ENABLE, "\r\n");
                }
                catch (Exception ex)
                {
                    // Logged once, the reconnect loop would repeat it every few seconds
                    if (!_openFailureLogged) Log.Error($"Cannot open Picob port {_projectConstants.PICOB_COM_PORT}: {ex.Message}");
                    _openFailureLogged = true;
                    _isOpen = false;
                }

                if (_isOpen)
                {
                    _openFailureLogged = false;
                    Log.Information($"Picob port {_projectConstants.PICOB_COM_PORT} opened");
                }
                return _isOpen;
            }
        }

        public void Close()
        {
            lock (_lock)
            {
                try
                {
                    _picobController.ClosePort();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error closing Picob port: {ex.Message}");
                }
                _isOpen = false;
            }
        }

        // Only write path to the Picob. Throws if the port is closed or the device does not answer in time
        public string Send(string command, bool waitForReply)
        {
            Interlocked.Increment(ref _waitingCommands);
            lock (_lock)
            {
                Interlocked.Decrement(ref _waitingCommands);
                return Write(command, waitForReply);
            }
        }

        // Starts the thread that opens the port, polls C and reopens the port after a disconnection. Safe to call more than once
        public void Start()
        {
            if (Interlocked.Exchange(ref _started, 1) == 1) return;

            new Thread(PollLoop) { IsBackground = true, Name = "PicobPoller" }.Start();
        }

        public PicobStatus GetStatus()
        {
            lock (_cacheLock)
            {
                if (!_hasPresence) return new PicobStatus(_isOpen, null, null);

                long ageMs = (long)Stopwatch.GetElapsedTime(_lastPresenceTimestamp).TotalMilliseconds;
                return new PicobStatus(_isOpen, ageMs, _lastPresence);
            }
        }

        private string Write(string command, bool waitForReply)
        {
            if (!_isOpen) throw new InvalidOperationException("Picob port is not open");

            WaitForGate();
            _lastWriteTimestamp = Stopwatch.GetTimestamp();
            _hasWritten = true;

            if (!waitForReply)
            {
                _picobController.SendToPicob(command);
                return string.Empty;
            }

            return _picobController.SendToPicobAndReceive(command, _projectConstants.PICOB_REPONSE_TIMEOUT_MS).Trim();
        }

        // The firmware needs a minimum time between two commands, whatever they are
        private void WaitForGate()
        {
            if (!_hasWritten) return;

            double remainingMs = _projectConstants.PICOB_MIN_COMMAND_INTERVAL_MS - Stopwatch.GetElapsedTime(_lastWriteTimestamp).TotalMilliseconds;
            if (remainingMs > 0) Thread.Sleep((int)Math.Ceiling(remainingMs));
        }

        private void PollLoop()
        {
            int backoffMs = ReconnectInitialMs;

            while (true)
            {
                if (!_isOpen)
                {
                    if (!Open())
                    {
                        Thread.Sleep(backoffMs);
                        backoffMs = Math.Min(backoffMs * 2, ReconnectMaxMs);
                        continue;
                    }
                    backoffMs = ReconnectInitialMs;
                }

                // A command from a client goes first, the next poll will refresh the value
                if (Volatile.Read(ref _waitingCommands) == 0) PollPresence();

                Thread.Sleep(Math.Max(_projectConstants.PICOB_POLL_INTERVAL_MS, 0));
            }
        }

        private void PollPresence()
        {
            try
            {
                string reply;
                lock (_lock)
                {
                    reply = Write("C", true);
                }

                int? presence = ParsePresence(reply);
                if (presence is null) throw new FormatException($"Unexpected reply to C: '{reply}'");

                lock (_cacheLock)
                {
                    _lastPresence = presence.Value;
                    _lastPresenceTimestamp = Stopwatch.GetTimestamp();
                    _hasPresence = true;
                }
                _consecutiveFailures = 0;
            }
            catch (Exception ex)
            {
                _consecutiveFailures++;
                Log.Warning($"Picob poll failed ({_consecutiveFailures}/{_projectConstants.PICOB_DISCONNECT_AFTER_FAILURES}): {ex.Message}");
                if (_consecutiveFailures >= _projectConstants.PICOB_DISCONNECT_AFTER_FAILURES) Disconnect();
            }
        }

        private void Disconnect()
        {
            Log.Error($"Picob disconnected after {_consecutiveFailures} failed polls. Reconnecting");
            Close();
            _consecutiveFailures = 0;
            lock (_cacheLock)
            {
                _hasPresence = false;
            }
        }

        private static int? ParsePresence(string reply)
        {
            try
            {
                return (int?)JObject.Parse(reply)["C"];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
