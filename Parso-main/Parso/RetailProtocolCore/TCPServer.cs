using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CCI.Globalcom.GlobalcomRetailProtocol;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    public class TcpServer
    {
        public TcpListener Server;
        public ArrayList SocketList;
        public Thread TcpThread;
        public byte[] BytesIn;
        public int BytesInCounter;
        public IPAddress ListeningIp ;
        public bool BStopServer ;
        public int ExpectedWaitTime;

        //Delegates to look for Exceptions in threads that were logged
        public delegate void TcpLogDebug(object sender, TcpLogMessageEventArgs dLogMessageEventArgs);
        public event TcpLogDebug TcpLogDebugEvent;


        public TcpServer(IPAddress ipAddress, int portNumber, int windowsize=1024)
        {
            BytesIn = new byte[1024*10]; //Create buffer
            ListeningIp = ipAddress;
            BStopServer = false;
            ExpectedWaitTime = 10;
            SocketList = new ArrayList();

            Server = new TcpListener(ipAddress,portNumber);

            Server.Start();
            TcpThread = new Thread(new ThreadStart(ServerThreadStart));
            TcpThread.Start();
        }

        ~TcpServer()
        {
            StopListener();
        }

        /// <summary>
        /// Stop all the listeners
        /// </summary>
        public void StopListener()
        {
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

        /// <summary>
        /// Start the Server
        /// </summary>
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

                    if (TcpLogDebugEvent != null)
                    {
                        TcpLogMessageEventArgs tcpDebugArgs = new TcpLogMessageEventArgs("Adding socket:" + socketListener.SocketAddress.ToString());
                        TcpLogDebugEvent(this, tcpDebugArgs);
                    }
                } 
                catch (SocketException se)
                {
                    // We were asked to stop but were busy waiting for another socket
                    if (BStopServer)
                        return;

                    BStopServer = true;
                    // se  swallow me
                    if (TcpLogDebugEvent != null)
                    {
                        TcpLogMessageEventArgs tcpLog = new TcpLogMessageEventArgs("TCP Socket Exception:" + se.Message + " - Removing dead socket:" + socketListener?.SocketAddress.ToString());
                        TcpLogDebugEvent(this, tcpLog);
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
        public Int32 SendData(byte[] dataToSend,string requestName)
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
                    if (TcpLogDebugEvent != null)
                    {
                        TcpLogMessageEventArgs tcpDebugArgs =
                            new TcpLogMessageEventArgs("TCP unable to write data to Socket. " + ex.Message);
                        TcpLogDebugEvent(this, tcpDebugArgs);

                        socketListener.StopSocketListener(); //stop the socket listener

                        deleteList.Add(socketListener); //Add to the termination list
                    }
                }
            }

            //nuke any dead sockets
            foreach (TCPSocketListener socketListener in deleteList)
            {
                SocketList.Remove(socketListener);
                if (TcpLogDebugEvent != null)
                {
                    TcpLogMessageEventArgs tcpDebugArgs = new TcpLogMessageEventArgs("Removing dead socket:"+socketListener.SocketAddress.ToString());
                    TcpLogDebugEvent(this, tcpDebugArgs);
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
                if (timeoutCounter++ > (10 * ExpectedWaitTime) )
                {
                    BytesInCounter = 0;
                    throw new Exception("TCP Listener - No response to:" + requestName);
                }

                Thread.Sleep(100);
            }

            return ret;
        }

        //Debug Event Args containing a message to log
        public class TcpLogMessageEventArgs : EventArgs
        {
            public string LogMessage;
            public bool GoBackToSerial;

            public TcpLogMessageEventArgs(string logMessage, bool bGoBackToSerial = false)
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
            private TcpServer _tcpServer;
            public SocketAddress SocketAddress;
            /// <summary>
            /// Working Variables.
            /// </summary>
            private DateTime _lastReceiveDateTime;
            private DateTime _currentReceiveDateTime;

            /// <summary>
            /// Client Socket Listener Constructor.
            /// </summary>
            /// <param name="tcpServer"></param>
            /// <param name="clientSocketParam"></param>
            public TCPSocketListener(TcpServer tcpServer, Socket clientSocketParam)
            {
                _clientSocket = clientSocketParam;
                _tcpServer = tcpServer;
                SocketAddress = _clientSocket.RemoteEndPoint.Serialize();
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
                        size = _clientSocket.Receive(byteBuffer);
                        _currentReceiveDateTime = DateTime.Now;
                        ParseReceiveBuffer(byteBuffer, size);
                    }
                    catch (SocketException se)
                    {
                        _stopClient = true;
                        _markedForDeletion = true;
                        if (_tcpServer.TcpLogDebugEvent != null)
                        {
                            TcpLogMessageEventArgs tcpDebugArgs =
                                new TcpLogMessageEventArgs("Socket exception:" + se.Message + " - Terminating socket:"+SocketAddress.ToString(), true);
                            _tcpServer.TcpLogDebugEvent(_tcpServer, tcpDebugArgs);
                            _tcpServer?.SocketList?.Remove(this);
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
                lock (_tcpServer)
                {
                    Buffer.BlockCopy(byteBuffer,0,_tcpServer.BytesIn,_tcpServer.BytesInCounter,size);
                    _tcpServer.BytesInCounter += size;
                }

            }

            /// <summary>
            /// Sends the byte array to the client socket
            /// </summary>
            /// <param name="data">data to send</param>
            /// <returns>Number of Sent bytes</returns>
            public Int32 SendData(byte[] data)
            {
                return _clientSocket.Send(data, SocketFlags.None);
            }



        }

    }
}
