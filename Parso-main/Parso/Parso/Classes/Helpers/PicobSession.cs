using Parso.Utils;
using PicobControllers;
using PicobControllers.Controllers;
using Serilog;
using System.Diagnostics;

namespace Parso.Classes.Helpers
{
    internal sealed class PicobSession
    {
        // Singleton instance
        private static readonly Lazy<PicobSession> _instance =
            new Lazy<PicobSession>(() => new PicobSession());

        public static PicobSession Instance => _instance.Value;

        private readonly ProjectConstants _projectConstants = ProjectConstants.Instance;
        private readonly IPicob _picobController = new PicobController();
        private readonly object _lock = new object();
        private volatile bool _isOpen;
        private bool _hasWritten;
        private long _lastWriteTimestamp;

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
                    Log.Error($"Cannot open Picob port {_projectConstants.PICOB_COM_PORT}: {ex.Message}");
                    _isOpen = false;
                }

                if (_isOpen) Log.Information($"Picob port {_projectConstants.PICOB_COM_PORT} opened");
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
            lock (_lock)
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
        }

        // The firmware needs a minimum time between two commands, whatever they are
        private void WaitForGate()
        {
            if (!_hasWritten) return;

            double remainingMs = _projectConstants.PICOB_MIN_COMMAND_INTERVAL_MS - Stopwatch.GetElapsedTime(_lastWriteTimestamp).TotalMilliseconds;
            if (remainingMs > 0) Thread.Sleep((int)Math.Ceiling(remainingMs));
        }
    }
}
