using log4net;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AA.Pango.Parso
{
    public class ParsoClient : IDisposable
    {
        private ILog log = LogManager.GetLogger("ParsoClient");

        private static string parsoHost = ConfigurationManager.AppSettings["ParsoHost"] ?? "127.0.0.1";
        private static int parsoPort = int.Parse(ConfigurationManager.AppSettings["ParsoPort"] ?? "1994");
        private const int ConnectTimeout = 3000;
        private const int MaxMessageLength = 10 * 1024 * 1024;

        private readonly object _lock = new object();
        private TcpClient _client = null;
        private NetworkStream _stream = null;
        private bool _connectErrorLogged = false;

        public event EventHandler<string> MessageReceived;

        public ParsoResponse Send(string request, int timeoutInMs)
        {
            lock (_lock)
            {
                try
                {
                    if (!IsConnected() && !Connect())
                        return null;

                    WriteMessage(request);

                    DateTime end = DateTime.UtcNow.AddMilliseconds(timeoutInMs);
                    while (true)
                    {
                        string message = ReadMessage(end);
                        ParsoResponse response = ParseResponse(message);
                        if (response != null)
                            return response;

                        log.Info("PARSO MESSAGE: " + message);
                        OnMessageReceived(message);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("PARSO REQUEST FAILED " + request + " - " + ex.Message);
                    Disconnect();
                    return null;
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                Disconnect();
            }
        }

        protected virtual void OnMessageReceived(string message)
        {
            try
            {
                MessageReceived?.Invoke(this, message);
            }
            catch (Exception ex)
            {
                log.Error("PARSO MessageReceived EX: " + ex);
            }
        }

        private bool IsConnected()
        {
            if (_client == null || !_client.Connected)
                return false;

            //Parso cerro la conexion (reinicio del servicio)
            if (_client.Client.Poll(0, SelectMode.SelectRead) && _client.Client.Available == 0)
            {
                log.Info("Parso closed the connection");
                Disconnect();
                return false;
            }

            return true;
        }

        private bool Connect()
        {
            Disconnect();
            try
            {
                _client = new TcpClient();
                _client.NoDelay = true;
                if (!_client.ConnectAsync(parsoHost, parsoPort).Wait(ConnectTimeout))
                    throw new TimeoutException("CONNECT TIMEOUT");

                _stream = _client.GetStream();
                _stream.WriteTimeout = ConnectTimeout;
                _connectErrorLogged = false;
                log.Info("Connected to Parso " + parsoHost + ":" + parsoPort);
                return true;
            }
            catch (Exception ex)
            {
                if (!_connectErrorLogged)
                {
                    log.Error("CANNOT CONNECT TO PARSO " + parsoHost + ":" + parsoPort + " - " + ex.GetBaseException().Message);
                    _connectErrorLogged = true;
                }
                Disconnect();
                return false;
            }
        }

        private void Disconnect()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch (Exception ex)
            {
                log.Error("PARSO Disconnect EX: " + ex.Message);
            }
            _stream = null;
            _client = null;
        }

        private void WriteMessage(string request)
        {
            byte[] message = Encoding.UTF8.GetBytes(request);
            byte[] buffer = new byte[4 + message.Length];
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder(message.Length)).CopyTo(buffer, 0);
            message.CopyTo(buffer, 4);
            _stream.Write(buffer, 0, buffer.Length);
        }

        private string ReadMessage(DateTime end)
        {
            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ReadExactly(4, end), 0));
            if (length < 0 || length > MaxMessageLength)
                throw new IOException("INVALID MESSAGE LENGTH " + length);

            return Encoding.UTF8.GetString(ReadExactly(length, end));
        }

        private byte[] ReadExactly(int count, DateTime end)
        {
            byte[] buffer = new byte[count];
            int read = 0;
            while (read < count)
            {
                int remaining = (int)(end - DateTime.UtcNow).TotalMilliseconds;
                if (remaining <= 0)
                    throw new TimeoutException("NO RESPONSE FROM PARSO");

                _stream.ReadTimeout = remaining;
                int n = _stream.Read(buffer, read, count - read);
                if (n == 0)
                    throw new IOException("PARSO CLOSED THE CONNECTION");
                read += n;
            }
            return buffer;
        }

        private ParsoResponse ParseResponse(string message)
        {
            if (!message.TrimStart().StartsWith("{"))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<ParsoResponse>(message);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}