using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parso.Utils.Objects;
using Serilog;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Parso.Classes
{
    internal sealed class ServerListener
    {
        // Singleton instance
        private static readonly Lazy<ServerListener> _instance =
            new Lazy<ServerListener>(() => new ServerListener());

        public static ServerListener Instance => _instance.Value;

        private CommandProcessor commandProcessor = new CommandProcessor();
        private Socket? listeningSocket;
        private ConcurrentDictionary<Socket, ClientSession> activeSessions = new ConcurrentDictionary<Socket, ClientSession>();
        private volatile bool _isRunning;
        private Thread? _serverThread;

        // Private constructor for singleton
        private ServerListener()
        {
        }

        public void StartServer()
        {
            if (_isRunning) return;

            _isRunning = true;
            _serverThread = new Thread(Run);
            _serverThread.Start();
        }

        public void StopServer()
        {
            _isRunning = false;
            Stop();
            _serverThread?.Join(5000); // Wait up to 5 seconds for thread to finish
        }

        private void Run()
        {
            listeningSocket = CreateSocket();
            listeningSocket.Bind(new IPEndPoint(0, 1994));
            listeningSocket.Listen(10);

            Log.Information("Server started and listening on port 1994");

            while (_isRunning)
            {
                try
                {
                    // Use a timeout so we can periodically check if we should stop
                    if (listeningSocket.Poll(1000, SelectMode.SelectRead)) // 1 second timeout
                    {
                        Socket clientSocket = listeningSocket.Accept();
                        Log.Information("Client connected");

                        // Create session and start communication threads
                        var session = new ClientSession(clientSocket, commandProcessor);
                        if (activeSessions.TryAdd(clientSocket, session))
                        {
                            new Thread(() => HandleClientSession(session)).Start();
                        }
                        else
                        {
                            Log.Error("Failed to add session to active sessions");
                            session.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_isRunning) // Only log if we're supposed to be running
                    {
                        Log.Error($"Accept error: {ex.Message}");
                        // Reconnection logic here...
                    }
                }
            }

            Log.Information("Server stopped");
        }

        // Method to send message to all connected clients
        public void BroadcastMessage(string message)
        {
            if (!_isRunning)
            {
                Log.Warning("Server is not running, cannot broadcast message");
                return;
            }

            var sessions = activeSessions.Values.ToArray();
            if (sessions.Length == 0)
            {
                Log.Information("No connected clients to broadcast to");
                return;
            }

            Log.Information($"Broadcasting message to {sessions.Length} clients: {message}");

            foreach (var session in sessions)
            {
                try
                {
                    session.SendMessage(message);
                }
                catch (Exception ex)
                {
                    Log.Error($"Broadcast error for client: {ex.Message}");
                }
            }
        }

        // Method to send message to specific client
        public void SendToClient(Socket clientSocket, string message)
        {
            if (!_isRunning)
            {
                Log.Warning("Server is not running, cannot send message");
                return;
            }

            if (activeSessions.TryGetValue(clientSocket, out var session))
            {
                session.SendMessage(message);
            }
            else
            {
                Log.Warning("Client socket not found in active sessions");
            }
        }

        // Get all connected client sockets (useful for external classes)
        public IReadOnlyList<Socket> GetConnectedClients()
        {
            return activeSessions.Keys.ToList();
        }

        // Get number of connected clients
        public int GetClientCount()
        {
            return activeSessions.Count;
        }

        private void HandleClientSession(ClientSession session)
        {
            try
            {
                // This will now block until the client disconnects
                session.Run();
            }
            catch (Exception ex)
            {
                Log.Error($"Client session error: {ex.Message}");
            }
            finally
            {
                activeSessions.TryRemove(session.Socket, out _);
                session.Dispose();
                Log.Information("Client disconnected");
            }
        }

        private Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void Stop()
        {
            _isRunning = false;

            listeningSocket?.Close();
            foreach (var session in activeSessions.Values)
            {
                session.Dispose();
            }
            activeSessions.Clear();
        }
    }

    internal class ClientSession : IDisposable
    {
        private readonly Socket _socket;
        private readonly CommandProcessor _commandProcessor;
        private volatile bool _isRunning;
        private CancellationTokenSource _cancellationTokenSource;

        public Socket Socket => _socket;

        public ClientSession(Socket socket, CommandProcessor commandProcessor)
        {
            _socket = socket;
            _commandProcessor = commandProcessor;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Run()
        {
            _isRunning = true;
            ReceiveLoop(); // Run in the current thread (blocking)
        }

        private void ReceiveLoop()
        {
            try
            {
                while (_isRunning && _socket.Connected)
                {
                    // Read 4-byte length prefix
                    byte[] lengthBuffer = new byte[4];
                    if (!ReceiveExactly(lengthBuffer, 4))
                        break;

                    int msgLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

                    // Safety check
                    if (msgLength < 0 || msgLength > 10 * 1024 * 1024) // 10MB max
                    {
                        Log.Error($"Invalid message length: {msgLength}");
                        break;
                    }

                    // Read full message
                    byte[] buffer = new byte[msgLength];
                    if (!ReceiveExactly(buffer, msgLength))
                        break;

                    // Process command
                    string command = Encoding.Default.GetString(buffer);
                    Log.Information($"Received command: {command}");

                    // Process payment commands asynchronously, others synchronously
                    if (IsPaymentCommand(command))
                    {
                        // Start payment command in background thread
                        Task.Run(() => ProcessPaymentCommand(command));
                    }
                    else
                    {
                        // Process other commands synchronously (printer, picob, etc.)
                        string response = _commandProcessor.processCommand(command);
                        if (response != "") { SendMessage(response); }
                    }

                    if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Receive loop error: {ex.Message}");
            }
        }

        private bool IsPaymentCommand(string command)
        {
            try
            {
                var json = JObject.Parse(command);
                var operationProperty = json.Properties().First();
                return operationProperty.Name == "3" &&
                       operationProperty.Value is JObject innerObject &&
                       innerObject["cancelPayment"] == null; // Regular payment, not cancellation
            }
            catch
            {
                return false;
            }
        }

        private async Task ProcessPaymentCommand(string command)
        {
            try
            {
                string response = await Task.Run(() => _commandProcessor.processCommand(command));
                if (response != "") { SendMessage(response); }
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing payment command: {ex.Message}");
                string errorResponse = JsonConvert.SerializeObject(
                    new Response(false, 500, false, $"ERROR PROCESSING PAYMENT: {ex.Message}"),
                    Formatting.None);
                SendMessage(errorResponse);
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                byte[] messageBuffer = Encoding.Default.GetBytes(message);
                byte[] lengthBuffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageBuffer.Length));

                lock (_socket) // Ensure thread-safe sending
                {
                    _socket.Send(lengthBuffer);
                    Log.Information($"{BitConverter.ToString(lengthBuffer).Replace("-", " ")}");

                    _socket.Send(messageBuffer);
                    Log.Information($"{BitConverter.ToString(messageBuffer).Replace("-", " ")}");
                }
                Log.Information($"Sent message: {message}");
            }
            catch (Exception ex)
            {
                Log.Error($"Send message error: {ex.Message}");
                throw;
            }
        }

        private bool ReceiveExactly(byte[] buffer, int expectedLength)
        {
            int bytesRead = 0;
            while (bytesRead < expectedLength && _isRunning)
            {
                try
                {
                    int rec = _socket.Receive(buffer, bytesRead, expectedLength - bytesRead, SocketFlags.None);
                    if (rec == 0) return false; // Graceful disconnect
                    bytesRead += rec;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    // Timeout - check if we should continue
                    continue;
                }
                catch (ObjectDisposedException)
                {
                    return false; // Socket was disposed
                }
            }
            return bytesRead == expectedLength;
        }

        public void Dispose()
        {
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            try
            {
                _socket?.Close();
                _socket?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error($"Dispose error: {ex.Message}");
            }
        }
    }
}
