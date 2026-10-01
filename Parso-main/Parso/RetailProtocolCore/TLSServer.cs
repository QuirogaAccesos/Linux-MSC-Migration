using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading.Tasks;
using static CCI.Globalcom.GlobalcomRetailProtocol.TcpServer;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices.ComTypes;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// TLS Server for accepting retail protocol connections over TLS from a terminal.
    /// </summary>
    public class TlsServer
    {
        /// <summary>
        /// TCP Listener for TLS server
        /// </summary>
        public TcpListener Server;
        public ArrayList SocketList;
        public Thread TlsThread;
        public byte[] BytesIn;
        public int BytesInCounter;
        public IPAddress ListeningIp;
        public bool BStopServer;
        public int ExpectedWaitTime;

        public SslStream SSLStream;


        //Delegates to look for Exceptions in threads that were logged
        public delegate void TlsLogDebug(object sender, TlsLogMessageEventArgs dLogMessageEventArgs);
        public event TlsLogDebug TlsLogDebugEvent;

        /// <summary>
        /// TLS Server 
        /// </summary>
        /// <param name="ipAddress">IP address</param>
        /// <param name="port">port</param>
        /// <param name="windowsize">Retail Protocol window size (infolength max)</param>
        public TlsServer(IPAddress ipAddress, int port, int windowsize = 1024)
        {
            BytesIn = new byte[1024*10]; //Create buffer
            ListeningIp = ipAddress;
            BStopServer = false;
            ExpectedWaitTime = 10;
            SocketList = new ArrayList();

            Server = new TcpListener(ipAddress, port);

            Server.Start();

            TlsThread = new Thread(new ThreadStart(ServerThreadStart));
            TlsThread.Start();
        }

        ~TlsServer()
        {
            StopListener();
        }

        public void StopListener()
        {
            BStopServer = true;
            BStopServer = true;
            if (null != SocketList)
            {
                foreach (TCPSocketListener socketListener in SocketList)
                {
                    socketListener.StopSocketListener();
                }
                SocketList.Clear();
                SocketList = null;
            }
            Server.Stop();
        }

        private void ServerThreadStart()
        {
            TCPSocketListener socketListener = null;
            Socket clientSocket = null;
            while (!BStopServer)
            {
                try
                {
                    clientSocket = Server.AcceptSocket();
                    socketListener = new TCPSocketListener(this, clientSocket);

                    lock (SocketList)
                    {
                        SocketList.Add(socketListener);
                    }
                    socketListener.StartSocketListener();
                    if (TlsLogDebugEvent != null)
                    {
                        TlsLogMessageEventArgs tlsDebugArgs = new TlsLogMessageEventArgs("Adding socket:" + socketListener.SocketAddress.ToString());
                        TlsLogDebugEvent(this, tlsDebugArgs);
                    }
                }
                catch (Exception se)
                {
                    BStopServer = true;
                    // se  swallow me
                    if (TlsLogDebugEvent != null)
                    {
                        TlsLogMessageEventArgs tlsLog = new TlsLogMessageEventArgs("TLS Socket Exception:" + se.Message + " - Removing dead socket:" + socketListener?.SocketAddress.ToString());
                        TlsLogDebugEvent(this, tlsLog);
                        if (socketListener != null) SocketList?.Remove(socketListener);
                    }
                }
            }
        }




        /// <summary>
        /// Send data to the ETH port and wait for a response
        /// </summary>
        /// <param name="dataToSend">Byte data request</param>
        /// <param name="requestName">Retail Protocol Method we are sending</param>
        /// <returns> Number of Bytes sent </returns>
        public Int32 SendData(byte[] dataToSend, string requestName)
        {
            int ret = 0;

            //array list to store dead sockets
            ArrayList deleteList = new ArrayList();

            foreach (TCPSocketListener socketListener in SocketList)
            {
                try
                {
                    ret = socketListener.SendData(dataToSend);
                }
                catch (Exception ex)
                {
                    //socketListener.StopSocketListener();
                    if (TlsLogDebugEvent != null)
                    {
                        TlsLogMessageEventArgs tlsDebugArgs =
                            new TlsLogMessageEventArgs("TLS unable to write data to Socket. " + ex.Message);
                        TlsLogDebugEvent(this, tlsDebugArgs);

                        socketListener.StopSocketListener(); //stop the socket listener

                        deleteList.Add(socketListener); //Add to the termination list
                    }
                }
            }

            //nuke any dead sockets
            foreach (TCPSocketListener socketListener in deleteList)
            {
                SocketList.Remove(socketListener);
                if (TlsLogDebugEvent != null)
                {
                    TlsLogMessageEventArgs tlsDebugArgs = new TlsLogMessageEventArgs("Removing dead socket:" + socketListener.SocketAddress.ToString());
                    TlsLogDebugEvent(this, tlsDebugArgs);
                }
            }

            bool notFinished = true;
            int timeoutCounter = 0;
            int i;
            int infoLength = 0;
            while (notFinished)
            {
                if (BytesInCounter > 2)
                {
                    for (i = 0; i <= BytesInCounter; i++)
                    {
                        if (BytesIn[i] == 0x02) //STX
                        {
                            break;
                        }
                    }

                    //Debug.Assert(i == 0, "Garbage in Serial buffer. Data does not begin with 0x02 (STX)");
                    infoLength = EndianBitConverter.Big.ToInt16(BytesIn, 3 + i);

                    notFinished = ((i + 7 + infoLength) > BytesInCounter);

                }
                if (timeoutCounter++ > (10 * ExpectedWaitTime))
                {
                    BytesInCounter = 0;
                    throw new Exception("TLS Listener - No response to:" + requestName);
                }

                Thread.Sleep(100);
            }

            return ret;
        }

        //Debug Event Args containing a message to log
        public class TlsLogMessageEventArgs : EventArgs
        {
            public string LogMessage;
            public bool GoBackToSerial;

            public TlsLogMessageEventArgs(string logMessage, bool bGoBackToSerial = false)
            {
                LogMessage = logMessage;
                GoBackToSerial = bGoBackToSerial;
            }
        }


        public class TCPSocketListener
        {
            /// <summary>
            /// Variables that are accessed by other classes indirectly.
            /// </summary>
            private Socket _clientSocket = null;
            private bool _stopClient = false;
            private Thread _clientListenerThread = null;
            private bool _markedForDeletion = false;
            private TlsServer _tlsServer;
            public SocketAddress SocketAddress;
            private SslStream _sslStream;

            /// <summary>
            /// Working Variables.
            /// </summary>
            private DateTime _lastReceiveDateTime;
            private DateTime _currentReceiveDateTime;

            /// <summary>
            /// Client Socket Listener Constructor.
            /// </summary>
            /// <param name="tlsServer"></param>
            /// <param name="clientSocketParam"></param>
            public TCPSocketListener(TlsServer tlsServer, Socket clientSocketParam)
            {
                _clientSocket = clientSocketParam;
                _tlsServer = tlsServer;
                SocketAddress = _clientSocket.RemoteEndPoint.Serialize();

                var networkStream = new NetworkStream(_clientSocket);
                _sslStream = new SslStream(networkStream, true);

                X509Certificate2 certificate = new X509Certificate2("ControlPanelCert.pfx", "MDYdH#2o83#rT44");
                _sslStream.AuthenticateAsServer(certificate);
            }

            /// <summary>
            /// Client SocketListener Destructor.
            /// </summary>
            ~TCPSocketListener()
            {
                StopSocketListener();
            }


            /// <summary>
            /// Method that starts SocketListener Thread.
            /// </summary>
            public void StartSocketListener()
            {
                if (_clientSocket != null)
                {
                    _clientListenerThread =
                        new Thread(new ThreadStart(SocketListenerThreadStart));
                    _clientListenerThread.Start();
                }
            }

            /// <summary>
            /// Thread method that does the communication to the client. This 
            /// thread tries to receive from client and if client sends any data
            /// then parses it and again wait for the client data to come in a
            /// loop. The recieve is an indefinite time receive.
            /// </summary>
            private void SocketListenerThreadStart()
            {
                int size = 0;
                Byte[] byteBuffer = new Byte[1024*10];

                _lastReceiveDateTime = DateTime.Now;
                _currentReceiveDateTime = DateTime.Now;

                while (!_stopClient)
                {
                    try
                    {
                        //size = _clientSocket.Receive(byteBuffer);
                        //_currentReceiveDateTime = DateTime.Now;
                        //ParseReceiveBuffer(byteBuffer, size);

                        // Use _sslStream.Read and _sslStream.Write for data transfer
                        size = _sslStream.Read(byteBuffer, 0, byteBuffer.Length);
                        _currentReceiveDateTime = DateTime.Now;
                        ParseReceiveBuffer(byteBuffer, size);
                    }
                    catch (SocketException se)
                    {
                        _stopClient = true;
                        _markedForDeletion = true;
                        if (_tlsServer.TlsLogDebugEvent != null)
                        {
                            TlsLogMessageEventArgs tlsDebugArgs =
                                new TlsLogMessageEventArgs("Socket exception:" + se.Message + " - Terminating socket:" + SocketAddress.ToString(), true);
                            _tlsServer.TlsLogDebugEvent(_tlsServer, tlsDebugArgs);
                            _tlsServer?.SocketList?.Remove(this);
                        }
                    }
                    catch(Exception e)
                    {
                        _stopClient = true;
                        _markedForDeletion = true;
                        if (_tlsServer.TlsLogDebugEvent != null)
                        {
                            TlsLogMessageEventArgs tlsDebugArgs =
                                new TlsLogMessageEventArgs("Exception:" + e.Message, true);
                            _tlsServer.TlsLogDebugEvent(_tlsServer, tlsDebugArgs);
                            _tlsServer?.SocketList?.Remove(this);
                        }

                    }
                }
            }

            /// <summary>
            /// Method that stops Client SocketListening Thread.
            /// </summary>
            public void StopSocketListener()
            {
                if (_clientSocket != null)
                {
                    _stopClient = true;
                    _clientSocket.Close();

                    // Wait for one second for the the thread to stop.
                    _clientListenerThread.Join(1000);

                    // If still alive; Get rid of the thread.
                    if (_clientListenerThread.IsAlive)
                    {
                        _clientListenerThread.Abort();
                    }
                    _clientListenerThread = null;
                    _clientSocket = null;
                    _markedForDeletion = true;
                }
            }

            /// <summary>
            /// Method that returns the state of this object i.e. whether this
            /// object is marked for deletion or not.
            /// </summary>
            /// <returns></returns>
            public bool IsMarkedForDeletion()
            {
                return _markedForDeletion;
            }

            /// <summary>
            /// This method appends data that is sent by a client using TCP/IP to the buffer.
            /// </summary>
            /// <param name="byteBuffer">Data </param>
            /// <param name="size">Data Size</param>
            private void ParseReceiveBuffer(Byte[] byteBuffer, int size)
            {
                lock (_tlsServer)
                {
                    Buffer.BlockCopy(byteBuffer, 0, _tlsServer.BytesIn, _tlsServer.BytesInCounter, size);
                    _tlsServer.BytesInCounter += size;
                }

            }

            /// <summary>
            /// Sends the byte array to the client socket
            /// </summary>
            /// <param name="data">data to send</param>
            /// <returns>Number of Sent bytes</returns>
            public Int32 SendData(byte[] data)
            {
                //return _clientSocket.Send(data, SocketFlags.None);
                _sslStream.Write(data, 0, data.Length);
                _sslStream.Flush();
                return data.Length;
            }
        }
    }
}




