using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Mux Address - ie, Mux Logical Channel 
    /// </summary>
    public enum EMuxAddress
    {
        /// <summary>
        /// Logical Channel for Ack and Nak Packets
        /// </summary>
        AckNak = 0x00,
        /// <summary>
        /// Logical Channel for Retail Protocol packets
        /// </summary>
        RetailProtocol = 0x04,
        /// <summary>
        /// Logical Channel for Banking Packets
        /// </summary>
        Banking = 0x02,
        /// <summary>
        /// Logical Channel for TLS Control/commands
        /// </summary>
        Control = 0x03,
        /// <summary>
        /// Logical Channel for debug log data
        /// </summary>
        Syslog = 0x05
    }

    /// <summary>
    /// Mux Commands
    /// </summary>
    public enum ECmd
    {
        /// <summary>
        /// Application Message
        /// </summary>
        Application = 0x00,
        /// <summary>
        /// Acknowledgement Message
        /// </summary>
        Ack = 0x01,
        /// <summary>
        /// Bad Acknowledgement Message
        /// </summary>
        Nak = 0x02,
        /// <summary>
        /// Acknowledgement and Application message
        /// </summary>
        AckAndApplication = 0x03
    }
    /// <summary>
    /// Mux Packet Object
    /// </summary>
    public class MuxPacket 
    {
        /// <summary>
        /// Enumerate the direction of the packet
        /// </summary>
        public enum MuxDirection
        {
            /// <summary>
            /// Packet is flowing towards the terminal
            /// </summary>
            ToTerminal = 0x00,
            /// <summary>
            /// Packet is flowing from teh terminal
            /// </summary>
            FromTerminal = 0x01
        }

        //CRC16 table
        private readonly ushort[] _crc16Tab;
        
        /// <summary>
        /// Starting header of Packet
        /// </summary>
        public byte Soh;
        /// <summary>
        /// Logical Channel of Packet
        /// </summary>
        public EMuxAddress Address;
        /// <summary>
        /// Packet command
        /// </summary>
        public ECmd Cmd;
        /// <summary>
        /// Packet sequence number
        /// </summary>
        public byte Sequence;
        /// <summary>
        /// Packet Length
        /// </summary>
        public int Length;
        /// <summary>
        /// Packet Data
        /// </summary>
        public byte[] Data;
        /// <summary>
        /// Packet CRC
        /// </summary>
        public ushort Crc;
        /// <summary>
        /// CRC is valid?
        /// </summary>
        public bool ValidCrc;
        /// <summary>
        /// Packet direction
        /// </summary>
        public MuxDirection MuxPacketDirection;

        /// <summary>
        /// unreplied Mux packet?
        /// </summary>
        public bool stale;

        /// <summary>
        /// MUX Packet
        /// </summary>
        public MuxPacket()
        {
            ValidCrc = false;
            Data = new byte[1024];
            Soh = 0x02;

            _crc16Tab = new ushort[]{
                0x0000,0x1021,0x2042,0x3063,0x4084,0x50a5,0x60c6,0x70e7,
                0x8108,0x9129,0xa14a,0xb16b,0xc18c,0xd1ad,0xe1ce,0xf1ef,
                0x1231,0x0210,0x3273,0x2252,0x52b5,0x4294,0x72f7,0x62d6,
                0x9339,0x8318,0xb37b,0xa35a,0xd3bd,0xc39c,0xf3ff,0xe3de,
                0x2462,0x3443,0x0420,0x1401,0x64e6,0x74c7,0x44a4,0x5485,
                0xa56a,0xb54b,0x8528,0x9509,0xe5ee,0xf5cf,0xc5ac,0xd58d,
                0x3653,0x2672,0x1611,0x0630,0x76d7,0x66f6,0x5695,0x46b4,
                0xb75b,0xa77a,0x9719,0x8738,0xf7df,0xe7fe,0xd79d,0xc7bc,
                0x48c4,0x58e5,0x6886,0x78a7,0x0840,0x1861,0x2802,0x3823,
                0xc9cc,0xd9ed,0xe98e,0xf9af,0x8948,0x9969,0xa90a,0xb92b,
                0x5af5,0x4ad4,0x7ab7,0x6a96,0x1a71,0x0a50,0x3a33,0x2a12,
                0xdbfd,0xcbdc,0xfbbf,0xeb9e,0x9b79,0x8b58,0xbb3b,0xab1a,
                0x6ca6,0x7c87,0x4ce4,0x5cc5,0x2c22,0x3c03,0x0c60,0x1c41,
                0xedae,0xfd8f,0xcdec,0xddcd,0xad2a,0xbd0b,0x8d68,0x9d49,
                0x7e97,0x6eb6,0x5ed5,0x4ef4,0x3e13,0x2e32,0x1e51,0x0e70,
                0xff9f,0xefbe,0xdfdd,0xcffc,0xbf1b,0xaf3a,0x9f59,0x8f78,
                0x9188,0x81a9,0xb1ca,0xa1eb,0xd10c,0xc12d,0xf14e,0xe16f,
                0x1080,0x00a1,0x30c2,0x20e3,0x5004,0x4025,0x7046,0x6067,
                0x83b9,0x9398,0xa3fb,0xb3da,0xc33d,0xd31c,0xe37f,0xf35e,
                0x02b1,0x1290,0x22f3,0x32d2,0x4235,0x5214,0x6277,0x7256,
                0xb5ea,0xa5cb,0x95a8,0x8589,0xf56e,0xe54f,0xd52c,0xc50d,
                0x34e2,0x24c3,0x14a0,0x0481,0x7466,0x6447,0x5424,0x4405,
                0xa7db,0xb7fa,0x8799,0x97b8,0xe75f,0xf77e,0xc71d,0xd73c,
                0x26d3,0x36f2,0x0691,0x16b0,0x6657,0x7676,0x4615,0x5634,
                0xd94c,0xc96d,0xf90e,0xe92f,0x99c8,0x89e9,0xb98a,0xa9ab,
                0x5844,0x4865,0x7806,0x6827,0x18c0,0x08e1,0x3882,0x28a3,
                0xcb7d,0xdb5c,0xeb3f,0xfb1e,0x8bf9,0x9bd8,0xabbb,0xbb9a,
                0x4a75,0x5a54,0x6a37,0x7a16,0x0af1,0x1ad0,0x2ab3,0x3a92,
                0xfd2e,0xed0f,0xdd6c,0xcd4d,0xbdaa,0xad8b,0x9de8,0x8dc9,
                0x7c26,0x6c07,0x5c64,0x4c45,0x3ca2,0x2c83,0x1ce0,0x0cc1,
                0xef1f,0xff3e,0xcf5d,0xdf7c,0xaf9b,0xbfba,0x8fd9,0x9ff8,
                0x6e17,0x7e36,0x4e55,0x5e74,0x2e93,0x3eb2,0x0ed1,0x1ef0
                };
        }

        /// <summary>
        /// Check the CRC of the muxpacket        
        /// </summary>
        /// <returns> bool = CRC valid?</returns>
        public bool CheckCrc()
        {
            byte[] checkBuffer = new byte[1024 + 8];

            checkBuffer[0] = Soh;
            checkBuffer[1] = (byte)Address;
            checkBuffer[2] = (byte)Cmd;
            checkBuffer[3] = Sequence;
            checkBuffer[4] = (byte)(Length >> 8);
            checkBuffer[5] = (byte)(Length);
            Buffer.BlockCopy(Data, 0, checkBuffer, 6, Length);

            ushort computedCrc = ComputeChecksum(checkBuffer, (6 + Length));

            ValidCrc = (computedCrc == Crc);

            return ValidCrc;
        }

        /// <summary>
        /// Set the CRC of the muxpacket (for outbound)
        /// </summary>
        public void SetCrc()
        {
            byte[] checkBuffer = new byte[1024 + 8];

            checkBuffer[0] = Soh;
            checkBuffer[1] = (byte)Address;
            checkBuffer[2] = (byte)Cmd;
            checkBuffer[3] = Sequence;
            checkBuffer[4] = (byte)(Length >> 8);
            checkBuffer[5] = (byte)(Length);
            Buffer.BlockCopy(Data, 0, checkBuffer, 6, Length);

            Crc = ComputeChecksum(checkBuffer, (6 + Length));
            ValidCrc = true;
        }

        //Compute the checksum
        private ushort ComputeChecksum(byte[] data, int? length)
        {
            ushort crc = 0;
            if (!(length.HasValue)) length = data.Length;

            for (int i = 0; i < length; ++i)
            {
                crc = (ushort)((crc << 8) ^ _crc16Tab[((crc >> 8) ^ data[i]) & 0x00ff]);
            }
            return crc;
        }

        //Format the MUX packet for Human Readable consumption
        public string[] PrettyPrint()
        {
            string [] ret = new string[10];
            
            ret[0] =  Sequence.ToString("X2").PadRight(4); //seq
            ret[1] = ((MuxPacketDirection == MuxDirection.FromTerminal)? "BV-->CP":"BV<--CP").PadRight(9); 
            ret[2] = Address.ToString().PadRight(20); 
            ret[3] = Cmd.ToString().PadRight(20); 
            ret[4] = ValidCrc.ToString();
            ret[5] = Length.ToString().PadRight(4);
            ret[6] = DateTime.Now.ToString("hh:mm:ss.fff");
            ret[7] = ((Cmd == ECmd.AckAndApplication || Cmd == ECmd.Application)&& Address == EMuxAddress.RetailProtocol) ? "RP_"+((ERetailProtocolCommands) Data[2]).ToString() + ((MuxPacketDirection == MuxDirection.FromTerminal) ? " Resp" : " Req") : "";

            return ret;
        }
    }


    /// <summary>
    /// Mux Protocol Class
    /// </summary>
    public class MuxProtocol
    {
        //Mux input Buffer
        private byte[] _MuxBufferIn;
        private int _muxBytesInCounter;

        //TCP Client for outbound TLS connections
        private TcpClient _tcpClient;
        private SslStream _sslStream;
        private string _clientCertificateName;
        private string _TLSName;
        private int _TLSPortNumber;

        
        //Inbound MUX packets from the RS232 interface
        private List<MuxPacket> _ReceivedMuxPackets;
        
        //Sent Packet MUX Buffer to check ACK/NAK against
        //private ArrayList _PendingMuxPackets;
        private List<MuxPacket> _PendingMuxPackets;

        /// <summary>
        /// Buffer of Retail protocol messages
        /// </summary>
        public ArrayList RetailProtocolMessages;

        /// <summary>
        /// RS232 port the MUX is using
        /// </summary>
        private SerialPort _SerialPort;

        /// <summary>
        /// Sequence counter for outbound MUX packets
        /// </summary>
        public byte SequenceCounter;

        /// <summary>
        /// Buffer for Banking Data
        /// </summary>
        public byte[] BankingBytesIn;
        /// <summary>
        /// Banking Data buffer position counter
        /// </summary>
        public int BankingBytesInCounter;

        //Worker Thread to direct inbound MUX packets
        private MuxDirector _muxDirector;
        private Thread _muxDirectorThread;
        
        //Worker thread to manage data on the MUX Banking Channel
        private BankingProtocolWorker _bankingProtocolWorker;
        private Thread _bankingProtocolWorkerThread;

        /// <summary>
        /// Buffer for the retail protocol inbound data
        /// </summary>
        public byte[] RetailProtocolBytesIn;
        /// <summary>
        /// Buffer for the retail protocol inbound data position counter
        /// </summary>
        public int RetailProtocolBytesInCounter;

        //Worker thread to handle the Retail Protocol Channel
        private RetailProtocolWorker _retailProtocolWorker;
        private Thread _retailProtocolWorkThread;

        //Worker thread to handle the MUX Control Channel
        private MuxControlWorker _muxControlWorker;
        private Thread _muxControlWorkerThread;

        //Buffer for outbound Mux Packets
        private ArrayList _MuxSendPackets;

        //Worker Thread to send outbound Mux packets
        private MuxSendWorker _muxSendWorker;
        private Thread _muxSendWorkerThread;

        //Worker Thread to send debug messages back to the UI (This is optional)
        private DebugLogWorker _debugLogWorker;
        private Thread _debugLogWorkerThread;

        /// <summary>
        /// Delegates for the UI to hook for debug messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dArgs"></param>
        public delegate void MuxDebug(object sender, MuxPacketEventArgs dArgs);
        /// <summary>
        /// Event Delegates for the UI to hook for debug messages
        /// </summary>
        public event MuxDebug MuxDebugEvent;

        /// <summary>
        /// Delegates to look for Exceptions in threads that were logged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dLogMessageEventArgs"></param>
        public delegate void MuxLogDebug(object sender, MuxLogMessageEventArgs dLogMessageEventArgs);
        /// <summary>
        /// Event Delegates to look for Exceptions in threads that were logged
        /// </summary>
        public event MuxLogDebug MuxLogDebugEvent;

        /// <summary>
        /// Delegates for the UI to receive syslog messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dsSyslogEventArgs"></param>
        public delegate void SyslogDebug(object sender, SyslogEventArgs dsSyslogEventArgs);
        /// <summary>
        /// Event Delegates for the UI to receive syslog messages
        /// </summary>
        public event SyslogDebug SysLogDebugEvent;

        /// <summary>
        /// Delegate for returning Banking data to UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dMuxLog"></param>
        public delegate void MuxBankingDataLog(object sender, MuxBankingDataLogMessageEventArgs dMuxLog);
        /// <summary>
        /// Event Delegate for returning Banking data to UI
        /// </summary>
        public event MuxBankingDataLog MuxBankingDataLogEvent;

        /// <summary>
        /// flag to simulate a proxy timeout
        /// this is only used to show what happens when MUX fails to pass data via proxy to the host (and back)
        /// </summary>
        public bool bSimulateMuxProxyTimeout;

        /// <summary>
        /// flag to simulate a proxy timeout receiving a response (Timeout Reversal will occur on BV)
        /// this is only used to simulate what happens when MUX fails to get a response from the host
        /// </summary>
        public bool _bSimulateMuxResponseProxyTimeout;

        /// <summary>
        /// flag to simulate a proxy corruption of the received response 
        /// </summary>
        public bool _bSimulateMuxSaleOrAuthResponseCorruption;

        /// <summary>
        /// flag to simulate a proxy corruption of the received response 
        /// </summary>
        public bool _bSimulateMuxConfirmResponseCorruption;

        /// <summary>
        /// Flag to simulate a confirmation timeout
        /// </summary>
        public bool _bSimulateMuxConfirmTimeout;

        /// <summary>
        /// Debug Event Args containing a message to log
        /// </summary>
        public class MuxLogMessageEventArgs : EventArgs
        {
            public string LogMessage;

            /// <summary>
            /// Debug Event Args containing a message to log
            /// </summary>
            /// <param name="logMessage"></param>
            public MuxLogMessageEventArgs(string logMessage)
            {
                LogMessage = logMessage;
            }
        }

        /// <summary>
        /// Banking Data Log
        /// </summary>
        public class MuxBankingDataLogMessageEventArgs : EventArgs
        {
            /// <summary>
            /// Message to Log
            /// </summary>
            public string LogMessage;
            /// <summary>
            /// Output Message or input message?
            /// </summary>
            public bool Outputflag;

            /// <summary>
            /// Banking Data log message
            /// </summary>
            /// <param name="logMessage">Log Message</param>
            /// <param name="outputflag">Output Flag</param>
            public MuxBankingDataLogMessageEventArgs(string logMessage, bool outputflag)
            {
                LogMessage = logMessage;
                Outputflag = outputflag;
            }
        }

        /// <summary>
        /// Refresh the UI during long runnning communication
        /// </summary>
        /// <param name="sender"></param>
        public delegate void MUXRefreshUI(object sender);

        /// <summary>
        /// Event to Refresh the UI
        /// </summary>
        public event MUXRefreshUI MUXRefreshUIEvent;

        /// <summary>
        /// Debug Event Args containing a MuxPacket object
        /// </summary>
        public class MuxPacketEventArgs : EventArgs
        {
            /// <summary>
            /// Mux Packet
            /// </summary>
            public MuxPacket MuxPacket;

            /// <summary>
            /// Debug Event Args containing a MuxPacket object
            /// </summary>
            /// <param name="muxPacket">Muxpacket to Log</param>
            public MuxPacketEventArgs(MuxPacket muxPacket)
            {
                MuxPacket = muxPacket;
            }
        }

        /// <summary>
        /// Debug Event Args containing a Syslog Message
        /// </summary>
        public class SyslogEventArgs : EventArgs
        {
            /// <summary>
            /// Syslog Message
            /// </summary>
            public string SyslogMessage;

            /// <summary>
            /// Debug Event Args containing a Syslog Message
            /// </summary>
            /// <param name="syslogMessage"></param>
            public SyslogEventArgs(string syslogMessage)
            {
                SyslogMessage = syslogMessage;
            }
        }

        /// <summary>
        /// Update TLS overrides from the UI
        /// </summary>
        /// <param name="cert">Cert Name to search in Cert store</param>
        /// <param name="tlsName">TLS host Name</param>
        /// <param name="tlsPort">TLS host Port</param>
        /// <param name="simulateProxyTimeout">Simulate timeout flag</param>
        public void SetTlsOverride(string cert, string tlsName, int? tlsPort, bool? simulateProxyTimeout)
        {
            if (cert.Length > 0) _clientCertificateName = cert;
            if (tlsName.Length > 0) _TLSName = tlsName;
            if ((null != tlsPort) && (tlsPort.Value > 0) && (tlsPort.Value < 65535)) _TLSPortNumber = tlsPort.Value;
            if (null != simulateProxyTimeout) bSimulateMuxProxyTimeout = (bool)simulateProxyTimeout;
        }

        /// <summary>
        ///   Gobble up the response other than reversals.
        /// </summary>
        /// <param name="simulateResponseTimeout"></param>
        public void SetSimulateResponseTimeout(bool? simulateResponseTimeout)
        {
            if (null != simulateResponseTimeout) _bSimulateMuxResponseProxyTimeout = (bool)simulateResponseTimeout;
        }

        public void SetSimulateConfirmTimeout(bool? simulateConfirmTimeout)
        {
            if (null != simulateConfirmTimeout) _bSimulateMuxConfirmTimeout = (bool)simulateConfirmTimeout;
        }

        /// <summary>
        ///   Currupt the reponse
        /// </summary>
        /// <param name="simulateResponseCorruption"></param>
        public void SetSimulateResponseSaleOrAuthCorruption(bool? simulateResponseCorruption)
        {
            if (null != simulateResponseCorruption) _bSimulateMuxSaleOrAuthResponseCorruption = (bool)simulateResponseCorruption;
        }


        /// <summary>
        ///   Currupt the reponse
        /// </summary>
        /// <param name="simulateResponseCorruption"></param>
        public void SetSimulateResponseConfirmCorruption(bool? simulateResponseCorruption)
        {
            if (null != simulateResponseCorruption) _bSimulateMuxConfirmResponseCorruption = (bool)simulateResponseCorruption;
        }

        /// <summary>
        /// Returns the pending mux packet count
        /// </summary>
        /// <returns>number of mux packets pending</returns>
        public int GetPendingOutboundMuxPacketCount()
        {
            return _PendingMuxPackets.Count;
        }

        private readonly System.Timers.Timer _muxInactivityTimer;
        private DateTime _lastSerialDataTimestamp = DateTime.UtcNow;
        private readonly object _muxLock = new object();

        /// <summary>
        /// Mux Protocol Class
        /// </summary>
        /// <param name="comPort">RS232 Port</param>
        /// <param name="baudRate">Speed of the RS232 Port</param>
        /// <param name="hwFlowControl">Use hardware flow control?</param>
        public MuxProtocol(string comPort, Int32 baudRate, bool hwFlowControl)
        {
            _MuxBufferIn = new byte[3048];
            _muxBytesInCounter = 0;

            BankingBytesIn = new byte[32000];
            BankingBytesInCounter = 0;

            RetailProtocolBytesIn = new byte[4096];
            RetailProtocolBytesInCounter = 0;

            _clientCertificateName = "";
            _TLSName = "";

            RetailProtocolMessages = new ArrayList();

            _SerialPort = new SerialPort(comPort, baudRate, Parity.None, 8, StopBits.One);

            _SerialPort.Handshake = (hwFlowControl) ? Handshake.RequestToSend : Handshake.None;
            _SerialPort.ReadTimeout = 1500;
            _SerialPort.WriteTimeout = 1500;
            _SerialPort.DataReceived += SerialDataReader;

            _SerialPort.Open();

            SequenceCounter = 0;

            _bSimulateMuxResponseProxyTimeout = false;
            _bSimulateMuxSaleOrAuthResponseCorruption = false;
            _bSimulateMuxConfirmResponseCorruption = false;

            // "Queues"
            _PendingMuxPackets = new List<MuxPacket>();
            _ReceivedMuxPackets = new List<MuxPacket>();


            //Mux Director
            _muxDirector = new MuxDirector(this);
            _muxDirectorThread = new Thread(_muxDirector.Process);
            _muxDirectorThread.Start();

            //Retail Protocol Worker
            _retailProtocolWorker = new RetailProtocolWorker(this);
            _retailProtocolWorkThread = new Thread(_retailProtocolWorker.Process);
            _retailProtocolWorkThread.Start();

            //Banking Protocol Worker
            _bankingProtocolWorker = new BankingProtocolWorker(this);
            _bankingProtocolWorkerThread = new Thread(_bankingProtocolWorker.Process);
            _bankingProtocolWorkerThread.Start();

            //Service/Control channel worker
            _muxControlWorker = new MuxControlWorker(this);
            _muxControlWorkerThread = new Thread(_muxControlWorker.Process);
            _muxControlWorkerThread.Start();

            //Mux Send Packets
            _MuxSendPackets = new ArrayList();
            _muxSendWorker = new MuxSendWorker(this);
            _muxSendWorkerThread = new Thread(_muxSendWorker.Process);
            _muxSendWorkerThread.Start();

            //Optional Debug log worker
            _debugLogWorker = new DebugLogWorker(this);
            _debugLogWorkerThread = new Thread(_debugLogWorker.Process);
            _debugLogWorkerThread.IsBackground = true;
            _debugLogWorkerThread.Start();

            //Set serial port inactivity thread to clean up pending serial data the event dropped

            _muxInactivityTimer = new System.Timers.Timer(500); // check twice per second
            _muxInactivityTimer.Elapsed += CheckMuxTimeout;
            _muxInactivityTimer.AutoReset = true;
            _muxInactivityTimer.Start();
        }

        /// <summary>
        /// The RS232 port loses events on receipt and so decode mux doesn't run sometimes.
        /// This thead will kick off every second to clear out the mux buffer if required.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckMuxTimeout(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_muxLock)
            {
                if ((DateTime.UtcNow - _lastSerialDataTimestamp).TotalMilliseconds > 1000)
                {
                    if(_muxBytesInCounter > 8) DecodeMux();
                    _lastSerialDataTimestamp = DateTime.UtcNow; // reset so it doesn't fire repeatedly
                }
            }
        }


        ~MuxProtocol()
        {
            Close();
        }






        /// <summary>
        /// Debug Worker Class
        /// </summary>
        public class DebugLogWorker
        {
            private volatile bool _shouldStop;
            private MuxProtocol _mux;
            public ArrayList DebugMessages;
            public ArrayList LogMessages;
            public ArrayList SysLogMessages; // this is the arraylist for the terminal's log messages available on MUX Channel 5
            public ArrayList MuxBankingDataMessages;


            public DebugLogWorker(MuxProtocol mux)
            {
                _mux = mux;
                _shouldStop = false;
                DebugMessages = new ArrayList();
                LogMessages = new ArrayList();
                SysLogMessages = new ArrayList();
                MuxBankingDataMessages = new ArrayList();
            }

            public void Process()
            {
                while (!(_shouldStop))
                {
                    //Mux Packets
                    MuxPacket mux2Display = null;
                    lock (DebugMessages)
                    {
                        if (DebugMessages.Count > 0)
                        {
                            mux2Display = (MuxPacket) DebugMessages[0];
                            DebugMessages.RemoveAt(0);
                        }
                    }

                    if (null != mux2Display && null != _mux.MuxDebugEvent)
                    {
                        MuxPacketEventArgs muxDebugArgs = new MuxPacketEventArgs(mux2Display);
                        _mux.MuxDebugEvent(_mux, muxDebugArgs);
                    }

                    //UI Log messages
                    string logMessage = "";
                    lock (LogMessages)
                    {
                        if (LogMessages.Count > 0)
                        {
                            logMessage = (string) LogMessages[0];
                            LogMessages.RemoveAt(0);
                        }
                    }

                    if (logMessage.Length > 0 && null != _mux.MuxLogDebugEvent)
                    {
                        MuxLogMessageEventArgs muxLog = new MuxLogMessageEventArgs(logMessage);
                        _mux.MuxLogDebugEvent(_mux, muxLog);
                    }

                    //Syslog messages
                    string syslogMessage = "";
                    lock (SysLogMessages)
                    {
                        if (SysLogMessages.Count > 0)
                        {
                            syslogMessage = (string) SysLogMessages[0];
                            SysLogMessages.RemoveAt(0);
                        }
                    }

                    if (syslogMessage.Length > 0 && _mux.SysLogDebugEvent != null)
                    {
                        SyslogEventArgs sysLog = new SyslogEventArgs(syslogMessage);
                        _mux.SysLogDebugEvent(_mux, sysLog);
                    }


                    // Mux Banking Data Messages
                    string MuxBankingMsg = "";
                    bool MuxOutputFlag = false;

                    lock (MuxBankingDataMessages)
                    {
                        if (MuxBankingDataMessages.Count > 0)
                        {
                            MuxBankingDataLogMessageEventArgs muxBankingDataLogMessage = (MuxBankingDataLogMessageEventArgs)MuxBankingDataMessages[0];
                            MuxBankingMsg = muxBankingDataLogMessage.LogMessage;
                            MuxOutputFlag = muxBankingDataLogMessage.Outputflag;
                            MuxBankingDataMessages.RemoveAt(0);
                        }
                    }

                    if (MuxBankingMsg.Length > 0)
                    {
                        MuxBankingDataLogMessageEventArgs mArgs = new MuxBankingDataLogMessageEventArgs(MuxBankingMsg,MuxOutputFlag);
                        _mux.MuxBankingDataLogEvent(_mux, mArgs);
                    }

                    Thread.Sleep(100);
                }
            }



            public void AddItem(MuxPacket mp)
            {
                lock (DebugMessages)
                {
                    DebugMessages.Add(mp);
                }
            }

            public void AddLogMessage(string msg)
            {
                lock (LogMessages)
                {
                    LogMessages.Add(msg);
                }
            }

            public void AddSysLogMessage(string msg)
            {
                lock (SysLogMessages)
                {
                    SysLogMessages.Add(msg);
                }
            }

            public void AddMuxDataBankingMessage(MuxBankingDataLogMessageEventArgs msg)
            {
                lock (MuxBankingDataMessages)
                {
                    MuxBankingDataMessages.Add(msg);
                }
            }


            public void RequestStop()
            {
                _shouldStop = true;
            }
        }

        /// <summary>
        /// MUX Director
        /// This is the worker that Routes and processes inbound MUX packets
        /// </summary>
        public class MuxDirector
        {
            private volatile bool _shouldStop;
            private MuxProtocol _mux;
            

            /// <summary>
            /// MuxDirector worker class initializer
            /// </summary>
            /// <param name="mux"></param>
            public MuxDirector(MuxProtocol mux)
            {
                _mux = mux;
                _shouldStop = false;
            }

            /// <summary>
            /// Worker Thread
            /// </summary>
            public void Process()
            {
                while (!(_shouldStop))
                {
                    try
                    {
                        //remove pending stale mux packets
                        lock (_mux._PendingMuxPackets) _mux._PendingMuxPackets.RemoveAll(m => m.stale);

                        if (_mux._ReceivedMuxPackets.Count > 0)
                        {

                            MuxPacket muxPacket;

                            lock (_mux._ReceivedMuxPackets)
                            {
                                muxPacket = _mux._ReceivedMuxPackets[0];
                                _mux._ReceivedMuxPackets.RemoveAt(0);
                            }

                            //Check message integrity
                            if (!(muxPacket.CheckCrc()))
                            {
                                //Bad CRC
                                _mux.SendNak(muxPacket.Sequence);
                                throw new Exception("Bad CRC for MUX Packet:" + muxPacket.Sequence.ToString() + " sending NAK.");
                            }



                            //Direct message
                            switch (muxPacket.Address)
                            {
                                case EMuxAddress.AckNak:
                                    foreach (MuxPacket m in _mux._PendingMuxPackets)
                                    {
                                        if (m.Sequence == muxPacket.Sequence) //found a packet
                                        {
                                            if (muxPacket.Cmd == ECmd.Nak)
                                            {
                                                _mux.ResendMuxPacket(m.Sequence);
                                            }
                                            else
                                            {
                                                _mux.ClearPending(muxPacket.Sequence);
                                            }

                                            break;
                                        }

                                        //Remove old because we have an ack from new
                                        if (m.Sequence < muxPacket.Sequence)
                                        {
                                            _mux.ClearPending(m.Sequence);
                                            _mux._debugLogWorker.AddLogMessage("No answer received for Mux packet sequence:" +  m.Sequence);
                                            //ResendMuxPacket(muxPacket.Sequence);
                                            // _debugLogWorker.AddLogMessage("Response received out of sequence. Old packet being resent!"+m.Sequence);
                                        }
                                    }

                                    break;

                                case EMuxAddress.Banking:
                                    switch (muxPacket.Cmd)
                                    {
                                        case ECmd.Application:
                                            //Ack the good Mux packet
                                            _mux.SendAck(muxPacket.Sequence);
                                            _mux.SendPacketToBanking(muxPacket);
                                            break;
                                        case ECmd.AckAndApplication:
                                            _mux.SendPacketToBanking(muxPacket);
                                            _mux.ClearPending(muxPacket.Sequence);
                                            break;
                                        case ECmd.Nak:
                                            _mux.ResendMuxPacket(muxPacket.Sequence);
                                            break;
                                        case ECmd.Ack:
                                            _mux.ClearPending(muxPacket.Sequence);
                                            break;
                                        default:
                                            throw new Exception("Invalid MUX packet with Banking protocol address. Command not recognized:" + Environment.NewLine + muxPacket);
                                    }
                                    break;

                                case EMuxAddress.Control:

                                    _mux.MuxControl(muxPacket);
                                    break;

                                case EMuxAddress.RetailProtocol:

                                    switch (muxPacket.Cmd)
                                    {
                                        case ECmd.Application:
                                            //Ack the good Mux packet
                                            _mux.SendAck(muxPacket.Sequence);
                                            _mux.SendPacketToRetailProtocol(muxPacket);
                                            break;
                                        case ECmd.AckAndApplication:
                                            _mux.SendPacketToRetailProtocol(muxPacket);
                                            _mux.ClearPending(muxPacket.Sequence);
                                            break; 
                                        case ECmd.Nak:
                                            _mux.ResendMuxPacket(muxPacket.Sequence);
                                            break;
                                        case ECmd.Ack:
                                            _mux.ClearPending(muxPacket.Sequence);
                                            break;
                                        default:
                                            throw new Exception(
                                                "Invalid Mux protocol Packet with Retail protocol address. Command not recognized:" +
                                                Environment.NewLine + muxPacket);
                                    }
                                    break;

                                case EMuxAddress.Syslog:
                                    switch (muxPacket.Cmd)
                                    {
                                        case ECmd.Application:
                                            //Ack the good Mux packet
                                            _mux.SendAck(muxPacket.Sequence);
                                            _mux.SendPacketToSyslog(muxPacket);
                                            break;
                                        case ECmd.AckAndApplication:
                                            _mux.SendPacketToSyslog(muxPacket);
                                            _mux.ClearPending(muxPacket.Sequence);
                                            break;
                                        case ECmd.Nak:
                                            _mux.ResendMuxPacket(muxPacket.Sequence);
                                            break;
                                        case ECmd.Ack:
                                            _mux.ClearPending(muxPacket.Sequence);
                                            break;
                                        default:
                                            throw new Exception("Invalid Mux protocol Packet with SysLog address. Command not recognized:" + Environment.NewLine + muxPacket);
                                    }
                                    break;

                                default:
                                    throw new Exception("MUX address illegal:" + Environment.NewLine + muxPacket);
                                    

                            } //switch address

                        } //count > 0

                    } //try

                    catch (Exception e)
                    {
                        _mux._debugLogWorker.AddLogMessage(e.Message + Environment.NewLine + e.StackTrace);
                    }

                    Thread.Sleep(100);

                } //While
            }

            /// <summary>
            /// Method to request the close of this thread
            /// </summary>
            public void RequestStop()
            {
                _shouldStop = true;
            }

            /// <summary>
            /// Add a newly Received MuxPacket to the "queue"
            /// </summary>
            /// <param name="muxPacket"></param>
            public void AddNewMuxPacket(MuxPacket muxPacket)
            {
                lock (_mux._ReceivedMuxPackets)
                {
                    _mux._ReceivedMuxPackets.Add(muxPacket);
                }
            }
        }




        /// <summary>
        /// Send Worker Class
        /// This is the worker that transmits MUX packets over RS232
        /// </summary>
        public class MuxSendWorker
        {
            private volatile bool _shouldStop;
            private MuxProtocol _mux;

            public MuxSendWorker(MuxProtocol mux)
            {
                _mux = mux;
                _shouldStop = false;
            }
            public void Process()
            {
                while (!(_shouldStop))
                {
                    try
                    {
                        if (_mux._MuxSendPackets.Count > 0)
                        {
                            ArrayList localSendPacketList;

                            lock (_mux._MuxSendPackets)
                            {
                                localSendPacketList = new ArrayList(_mux._MuxSendPackets); //Make a current copy of the array list
                            }

                            foreach (MuxPacket muxPacket in localSendPacketList)
                            {
                                byte[] output = new byte[muxPacket.Length + 8];
                                output[0] = muxPacket.Soh;
                                output[1] = (byte) muxPacket.Address;
                                output[2] = (byte) muxPacket.Cmd;
                                output[3] = muxPacket.Sequence;
                                output[4] = (byte) (muxPacket.Length >> 8);
                                output[5] = (byte) (muxPacket.Length);
                                Buffer.BlockCopy(muxPacket.Data, 0, output, 6, muxPacket.Length);
                                output[(6 + muxPacket.Length)] = (byte) (muxPacket.Crc >> 8);
                                output[(7 + muxPacket.Length)] = (byte) (muxPacket.Crc);

                                _mux._debugLogWorker.AddItem(muxPacket);

                                _mux._SerialPort.Write(output, 0, (muxPacket.Length + 8));

                                //make sure the packet is ACKed before sending the next
                                //while the sequence is in pending mux packets wait
                                int waiting = 0;
                                int retries = 1;

                                //while (_mux._PendingMuxPackets.Contains(muxPacket))
                                while (_mux._PendingMuxPackets.Where(i=>i.Sequence == muxPacket.Sequence && i.stale==false).Count() > 0)
                                //while (_mux._PendingMuxPackets.Any(i => i.stale == false && i.Sequence < muxPacket.Sequence))
                                {
                                    Thread.Sleep(100);

                                    //_mux.MUXRefreshUIEvent?.Invoke(this);
                                    if (_mux.MUXRefreshUIEvent != null) _mux.MUXRefreshUIEvent.Invoke(this);

                                    if (++retries % 40 == 0) //4 seconds
                                    {
                                       // _mux._SerialPort.Write(output, 0, (muxPacket.Length + 8));
                                        _mux._debugLogWorker.AddLogMessage("ACK late for Mux Packet Sequence:"+ muxPacket.Sequence.ToString("X2"));
                                    }

                                    if (++waiting > 90) //9 seconds?
                                    {
                                        _mux._debugLogWorker.AddLogMessage(
                                            "ACK/NAK late for MUX packet sequence:" +
                                            muxPacket.Sequence.ToString("X2"));
                                        //_mux.ClearPending(muxPacket.Sequence);
                                        break; 
                                    }
                                }

                                lock (_mux._MuxSendPackets)
                                {
                                    _mux._MuxSendPackets.Remove(muxPacket);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _mux._debugLogWorker.AddLogMessage(e.Message + Environment.NewLine + e.StackTrace  );
                    }
                    Thread.Sleep(100);
                }
            }

            public void RequestStop()
            {
                _shouldStop = true;
            }
        }


        /// <summary>
        /// Worker Class to manage the MUX Control Channel
        /// </summary>
        public class MuxControlWorker
        {
            private volatile bool _shouldStop;
            private MuxProtocol _mux;
            
            private ArrayList _muxControlPackets;

            public enum EConnectionStatus
            {
                Disconnected = 0x00,
                Connecting = 0x01,
                Connected = 0x02   
            }
            

            public MuxControlWorker(MuxProtocol mux)
            {
                _shouldStop = false;
                _mux = mux;
                _muxControlPackets = new ArrayList();
            }

            public void Process()
            {
                while (!(_shouldStop))
                {
                    try
                    {
                        lock (_muxControlPackets)
                        {
                            foreach (MuxPacket muxPacket in _muxControlPackets)
                            {
                                byte code = muxPacket.Data[0];

                                byte protocol = muxPacket.Data[1];
                                string hostname;
                                int TLSPort = 0;
                                byte[] IPAddress = new byte[4];


                                switch (code)
                                {

                                    case 0x01: //Connection status Request

                                        if (null == _mux._tcpClient)
                                        {
                                            AckMuxServicePacket((byte) EConnectionStatus.Disconnected,
                                                muxPacket.Sequence);
                                        }
                                        else
                                        {
                                            if (_mux._tcpClient.Connected)
                                            {
                                                AckMuxServicePacket((byte) EConnectionStatus.Connected,
                                                    muxPacket.Sequence);
                                            }
                                            else
                                            {
                                                AckMuxServicePacket((byte) EConnectionStatus.Connecting,
                                                    muxPacket.Sequence);
                                            }
                                        }
                                        break;
                                    case 0x02: //Disconnect Request

                                        //wait up to 10 seconds for the banking channel to clear before disconnecting
                                        int counter = 0;
                                        while (_mux.BankingBytesInCounter != 0)
                                        {
                                            Thread.Sleep(100);
                                            if (++counter > 100)
                                            {
                                                break;
                                            }
                                        }

                                        _mux._tcpClient?.Close();
                                        _mux._debugLogWorker.AddLogMessage(@"Closed TLS");
                                        AckMuxServicePacket((byte) EConnectionStatus.Disconnected, muxPacket.Sequence);
                                        
                                        break;
                                    case 0x03: //CONNECT command using IP address

                                        TLSPort = (0 == _mux._TLSPortNumber) ? EndianBitConverter.Big.ToInt16(muxPacket.Data, 6) : _mux._TLSPortNumber;


                                        if (_mux._TLSName.Length == 0) //if override exists from UI use it
                                        {
                                            //get from MUX
                                            Buffer.BlockCopy(muxPacket.Data, 2, IPAddress, 0, 4);
                                            IPAddress ip = new IPAddress(IPAddress);
                                            _mux._tcpClient = new TcpClient(ip.ToString(), TLSPort);
                                            hostname = ip.ToString();
                                        }
                                        else
                                        {
                                            hostname = _mux._TLSName;
                                            _mux._tcpClient = new TcpClient(hostname, TLSPort);
                                        }


                                        ProcessMuxControl(protocol, hostname, muxPacket);

                                        break;
                                    case 0x04: //using hostname.
                                        byte[] hostnameBytes = new byte[(muxPacket.Length - 4)];


                                        //If override exists from UI use it
                                        TLSPort = (0 == _mux._TLSPortNumber) ? EndianBitConverter.Big.ToInt16(muxPacket.Data, 2) :  _mux._TLSPortNumber;

                                        if (_mux._TLSName.Length == 0) //if override exists from UI use it
                                        {
                                            //get from MUX                                         
                                            Buffer.BlockCopy(muxPacket.Data, 4, hostnameBytes, 0, (muxPacket.Length - 4));
                                            hostname = Encoding.ASCII.GetString(hostnameBytes);
                                        }
                                        else
                                        {
                                            hostname = _mux._TLSName;
                                        }

                                        _mux._tcpClient = new TcpClient(hostname, TLSPort);

                                        ProcessMuxControl(protocol, hostname, muxPacket);

                                        break;
                                }
                            }
                            _muxControlPackets.Clear();
                        }
                    }
                    catch (Exception e)
                    {
                        _mux._debugLogWorker.AddLogMessage(@"MUX Thread Exception"+e.Message + Environment.NewLine + e.StackTrace);
                    }

                    Thread.Sleep(100);
                }
            }


            public void ProcessMuxControl(byte protocol, string hostname, MuxPacket muxPacket)
            {
                // The protocol switch indicates how the TLS session should be established. If NO Auth is specified, then the server's cert is not validated against the CA.
                // In the payment terminal the cacert must be loaded internally and the connection initiated as TLS AUTH
                // If TLS NO AUTH is specified, the validation is not performed.
                // The following code follows this for TLS sessions opened in .NET. Additionally, if a client side certificate name is present, then that certificate
                // will be presented to the server for validation.
                switch (protocol)
                {
                    case 0x00: //TCP
                        _mux._debugLogWorker.AddLogMessage(@"No TLS protocol specified by BV1000. This configuration should not be supported.");
                        break;
                    case 0x01: //SSL without Auth
                    case 0x03: //TLS without Auth and no Client Certificates
                        if (_mux._clientCertificateName.Length == 0)
                        {
                            try
                            {
                                _mux._sslStream = new SslStream(_mux._tcpClient.GetStream(), true, ValidateCertNoAuth,
                                    null);
                                _mux._sslStream.AuthenticateAsClient(hostname, null, SslProtocols.Tls12,
                                    true);

                                AckMuxServicePacket((byte) EConnectionStatus.Connected,
                                    muxPacket.Sequence);
                                _mux._debugLogWorker.AddLogMessage(@"Opened TLS (No Client Cert) to:" + hostname + " using:" + _mux._sslStream.CipherAlgorithm);
                            }
                            catch (Exception e)
                            {
                                AckMuxServicePacket((byte) EConnectionStatus.Disconnected, muxPacket.Sequence);
                                _mux._debugLogWorker.AddLogMessage(@"Mux Thread Exception:" + e.Message +
                                                                   Environment.NewLine + e.StackTrace);
                            }
                        }
                        else
                        {
                            _mux._sslStream = new SslStream(_mux._tcpClient.GetStream(), false,
                                ValidateCertNoAuth, null);
                            try
                            {
                                //X509Certificate2 cert;
                                X509Store store = new X509Store(StoreLocation.CurrentUser);

                                store.Open(OpenFlags.ReadOnly);
                                X509Certificate2Collection cers =
                                    store.Certificates.Find(X509FindType.FindBySubjectName,
                                        _mux._clientCertificateName, false);

                                store.Close();

                                _mux._debugLogWorker.AddLogMessage(@"Found " + cers.Count.ToString() + " TLS Client Certificates to present to the server!");
                                _mux._sslStream.AuthenticateAsClient(hostname, cers, SslProtocols.Tls12, false);
                                AckMuxServicePacket((byte)EConnectionStatus.Connected,
                                    muxPacket.Sequence);
                                _mux._debugLogWorker.AddLogMessage(@"Opened TLS w/client cert:" + (String.IsNullOrEmpty(_mux._sslStream.LocalCertificate?.Subject)  ? "<none>" : _mux._sslStream.LocalCertificate?.Subject + " Expiring on:" + _mux._sslStream.LocalCertificate?.GetExpirationDateString()) + " to:" + hostname);
                            }
                            catch (Exception e)
                            {
                                AckMuxServicePacket((byte)EConnectionStatus.Disconnected, muxPacket.Sequence);
                                _mux._debugLogWorker.AddLogMessage(@"Mux Thread Exception:" + e.Message +
                                                                   Environment.NewLine + e.StackTrace);
                            }
                        }
                        break;
                    case 0x02: //SSL with Auth
                    case 0x04: //TLS with Auth and Client Certificates
                        if (_mux._clientCertificateName.Length == 0)
                        {
                            try
                            {
                                _mux._sslStream = new SslStream(_mux._tcpClient.GetStream(), true, ValidateCert,
                                    null);
                                _mux._sslStream.AuthenticateAsClient(hostname, null, SslProtocols.Tls12,
                                    true);

                                AckMuxServicePacket((byte) EConnectionStatus.Connected,
                                    muxPacket.Sequence);
                                _mux._debugLogWorker.AddLogMessage(@"Opened TLS (No Client Cert) to:" + hostname + " using:" + _mux._sslStream.CipherAlgorithm );
                            }
                            catch (Exception e)
                            {
                                AckMuxServicePacket((byte) EConnectionStatus.Disconnected, muxPacket.Sequence);
                                _mux._debugLogWorker.AddLogMessage(@"Mux Thread Exception:" + e.Message +
                                                                   Environment.NewLine + e.StackTrace);
                            }
                        }
                        else
                        {
                            _mux._sslStream = new SslStream(_mux._tcpClient.GetStream(), false,
                                ValidateCert, null);
                            try
                            {
                                //X509Certificate2 cert;
                                X509Store store = new X509Store(StoreLocation.CurrentUser);

                                store.Open(OpenFlags.ReadOnly);
                                X509Certificate2Collection cers =
                                    store.Certificates.Find(X509FindType.FindBySubjectName,
                                        _mux._clientCertificateName, false);

                                store.Close();

                                _mux._debugLogWorker.AddLogMessage(@"Found " + cers.Count.ToString() + " TLS Client Certificates to present to the server!");

                                _mux._sslStream.AuthenticateAsClient(hostname, cers, SslProtocols.Tls12, false);
                                AckMuxServicePacket((byte) EConnectionStatus.Connected,
                                    muxPacket.Sequence);
                                
                                //_mux._debugLogWorker.AddLogMessage(@"Opened TLS w/client cert:"+_mux._sslStream.LocalCertificate?.Subject +" Expiring on:"+ _mux._sslStream.LocalCertificate?.GetExpirationDateString()+ " to:" + hostname );
                                _mux._debugLogWorker.AddLogMessage(@"Opened TLS w/client cert:" + (String.IsNullOrEmpty(_mux._sslStream.LocalCertificate?.Subject) ? "<none>" : _mux._sslStream.LocalCertificate?.Subject + " Expiring on:" + _mux._sslStream.LocalCertificate?.GetExpirationDateString()) + " to:" + hostname);
                            }
                            catch (Exception e)
                            {
                                AckMuxServicePacket((byte) EConnectionStatus.Disconnected, muxPacket.Sequence);
                                _mux._debugLogWorker.AddLogMessage(@"Mux Thread Exception:" + e.Message +
                                                                   Environment.NewLine + e.StackTrace);
                            }
                        }
                        break;
                }
            }
            public void AckMuxServicePacket(byte status, byte sequence)
            {
                MuxPacket statusPacket = new MuxPacket();
                statusPacket.Address = EMuxAddress.Control;
                statusPacket.Cmd = ECmd.AckAndApplication;
                statusPacket.Sequence = sequence;
                statusPacket.Length = 2;
                statusPacket.Data[0] = 0x01; //Connection status
                statusPacket.Data[1] = status;
                statusPacket.MuxPacketDirection = MuxPacket.MuxDirection.ToTerminal;
                statusPacket.SetCrc();

                _mux.MuxPacketSend(statusPacket);
                lock (_mux._PendingMuxPackets)
                {
                    _mux._PendingMuxPackets.Add(statusPacket);
                }
            }


            public void AddControlPacket(MuxPacket mp)
            {
                lock (_muxControlPackets)
                {
                    _muxControlPackets.Add(mp);
                }
            }
            public void RequestStop()
            {
                _shouldStop = true;
            }
            public bool ValidateCert(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                    return true;

                //("SSL/TLS Policy Exception. SSLPolicyError:" + sslPolicyErrors + Environment.NewLine + @"Chain Status:" + chain.ChainStatus[0].StatusInformation);
                if (chain?.ChainStatus.Length > 0)
                    _mux._debugLogWorker.AddLogMessage("SSL/TLS Policy Exception. SSLPolicyError:" + sslPolicyErrors + Environment.NewLine + @"Chain Status:" + chain.ChainStatus[0].StatusInformation);
                else
                {
                    _mux._debugLogWorker.AddLogMessage("SSL/TLS Policy Exception. SSLPolicyError:" + sslPolicyErrors);
                }

                // We are logging policy exceptions.
                // In production you should consider which policy errors are acceptable.

                return true; 
            }

            public bool ValidateCertNoAuth(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                if (chain?.ChainStatus.Length > 0)
                    _mux._debugLogWorker.AddLogMessage("IGNORED by Config= TLS No Auth: SSL/TLS Policy Exception. SSLPolicyError:" + sslPolicyErrors + Environment.NewLine + @"Chain Status:" + chain.ChainStatus[0].StatusInformation);
                else
                {
                    _mux._debugLogWorker.AddLogMessage("IGNORED by Config=TLS No Auth: SSL/TLS Policy Exception. SSLPolicyError:" + sslPolicyErrors);
                }

                // We are logging policy exceptions.
                // In production you should consider which policy errors are acceptable.

                return true;
            }
        }

        /// <summary>
        /// Worker Class to manage the Banking Protocol channel
        /// </summary>
        public class BankingProtocolWorker
        {
            private volatile bool _shouldStop;
            private MuxProtocol _mux;

            public BankingProtocolWorker(MuxProtocol mux)
            {
                _mux = mux;
                _shouldStop = false;
            }

            public void Process()
            {
                int bankingbufferwaitcounter = 0;

                while (!(_shouldStop))
                {
                    try
                    {
                        if (_mux.BankingBytesInCounter > 0)
                        {
                            if ((_mux.BankingBytesInCounter%1024) == 0)
                            {
                                //if the buffer has exactly 1024 bytes, we are probalby waiting for the next packet of data
                                //we want to gather the entire banking message to send via TLS before doing the session write
                                if (bankingbufferwaitcounter++ > 20)
                                {
                                    //if we have waiting for 20 * 100 = 2000ms = 2 seconds and we still have exactly mod-1024 bytes - process them.
                                    CheckBankingPacket();
                                    bankingbufferwaitcounter = 0;
                                }
                            }
                            else
                            {
                                bankingbufferwaitcounter = 0;
                                CheckBankingPacket();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _mux._debugLogWorker.AddLogMessage(e.Message + Environment.NewLine + e.StackTrace);
                        _mux.BankingBytesInCounter = 0; //
                    }
                    Thread.Sleep(100);
                }
            }
            public void RequestStop()
            {
                _shouldStop = true;
            }

            public void CheckBankingPacket()
            {
                if (null == _mux._tcpClient)
                {
                    throw new Exception("Banking data received from the terminal but no TLS session is available.");
                }

                if (!(_mux._tcpClient.Connected))
                {
                    throw new Exception("Banking Data Received from the terminal but no TLS session is connected.");
                }

                if (null == _mux._sslStream)
                {
                    throw new Exception("Banking Data Received from the terminal but TLS stream is not open.");
                }

                string msg = "";
                lock (_mux.BankingBytesIn)
                {
                    msg = Encoding.UTF8.GetString(_mux.BankingBytesIn,0,_mux.BankingBytesInCounter);
                    if (!_mux.bSimulateMuxProxyTimeout) _mux._sslStream.Write(_mux.BankingBytesIn, 0, _mux.BankingBytesInCounter);
                    _mux._sslStream.Flush();

                    _mux.BankingBytesInCounter = 0;
                    if (_mux.bSimulateMuxProxyTimeout) msg = "DATA NOT SENT:" + msg;
                    _mux._debugLogWorker.AddMuxDataBankingMessage(new MuxBankingDataLogMessageEventArgs(msg, true));
                }
                
                
                byte[] readbuffer = new byte[16000];


                int receiveByteCount = -1;

                //The SSL stream remains open. If we set a readtimeout the session closes. This is a problem for gateways that require two XML
                //messages (Auth-Confirm). We can't close the TLS session prior to the confirm.
                //
                //This routine reads the stream and if the last character is >, it moves on. This assumes the payload is XML, and the first read didn't end on an XML boundary.
                //For practical deployments, the gateway's closing XML tag, Json tag, etc should be used to break from this while loop.
                msg = "";
                int timeoutCounter = 0;
                while (receiveByteCount != 0)
                {
                    receiveByteCount = _mux._sslStream.Read(readbuffer, 0, readbuffer.Length);
                    if (receiveByteCount == 0) continue;

                    msg += Encoding.UTF8.GetString(readbuffer, 0, receiveByteCount);

                    if (readbuffer[receiveByteCount-1] == '>') break;
                    if (timeoutCounter++ > 300) break; //100 * 300 = 30000 = 30 seconds.
                    Thread.Sleep(100);
                }

                // Track the fact the data was received but not being sent back.  Let a reversal through.
                if (_mux._bSimulateMuxResponseProxyTimeout && !msg.Contains("Reversal"))
                {
                    msg = "DATA NOT SENT TO DEVICE:" +Environment.NewLine + msg;
                    _mux._debugLogWorker.AddMuxDataBankingMessage(new MuxBankingDataLogMessageEventArgs(msg, false));
                    return;
                }
                // Manipulate the packet and continue.
                else if (_mux._bSimulateMuxConfirmResponseCorruption && msg.Contains("Response type=\"CardEaseXML") && !msg.Contains("AcquirerResponseCode"))
                {
                    // This is to corupt the response
                    int i = msg.IndexOf("<Response");
                    readbuffer[i + 2] = 0x21; // Add * to corrupt response packet
                    msg = Encoding.UTF8.GetString(readbuffer, 0, receiveByteCount);
                }
                else if (_mux._bSimulateMuxSaleOrAuthResponseCorruption && msg.Contains("AcquirerResponseCode"))
                {
                    // This is to corupt the response
                    int i = msg.IndexOf("<Response");
                    readbuffer[i + 2] = 0x21; // Add * to corrupt response packet
                    msg = Encoding.UTF8.GetString(readbuffer, 0, receiveByteCount);
                }
                else if (_mux._bSimulateMuxConfirmTimeout && msg.Contains("<Result>") && !msg.Contains("<CardDetails>"))
                {
                    msg = "CONFIRMATION NOT SENT TO DEVICE:" +Environment.NewLine + msg;
                    _mux._debugLogWorker.AddMuxDataBankingMessage(new MuxBankingDataLogMessageEventArgs(msg, false));
                    return;
                }

                _mux._debugLogWorker.AddMuxDataBankingMessage(new MuxBankingDataLogMessageEventArgs(msg, false));

                //We can only transmit a max of 1024 per muxpacket
                int bytesSent = 0;
                int packetSize = 0;
                while (bytesSent < receiveByteCount)
                {
                    MuxPacket bankResponsePacket = new MuxPacket();
                    bankResponsePacket.Address = EMuxAddress.Banking;
                    bankResponsePacket.Cmd = ECmd.Application;
                    bankResponsePacket.Sequence = ++_mux.SequenceCounter;
                    bankResponsePacket.MuxPacketDirection = MuxPacket.MuxDirection.ToTerminal;

                    if ((receiveByteCount - bytesSent) > 1024)
                    {
                        packetSize = 1024;
                    }
                    else
                    {
                        packetSize = (receiveByteCount - bytesSent);
                    }

                    Buffer.BlockCopy(readbuffer, bytesSent, bankResponsePacket.Data, 0, packetSize);
                    bankResponsePacket.Length = packetSize;
                    

                    bankResponsePacket.SetCrc();

                    _mux.MuxPacketSend(bankResponsePacket);
                    lock (_mux._PendingMuxPackets)
                    {
                        _mux._PendingMuxPackets.Add(bankResponsePacket);
                    }

                    bytesSent += packetSize;
                }
            }
        }


        /// <summary>
        /// Worker class for the Retail Protocol Channel
        /// This Class manages data inbound on the retail protocol channel
        /// </summary>
        public class RetailProtocolWorker
        {
            private volatile bool _shouldStop;
            private MuxProtocol _mux;
            public RetailProtocolWorker(MuxProtocol mux)
            {
                _mux = mux;
                _shouldStop = false;
            }

            public void Process()
            {
                while (!(_shouldStop))
                {
                    //Process retail protocol messages
                    try
                    {
                        CheckforRpPacket();
                    }
                    catch (Exception e)
                    {
                        _mux._debugLogWorker.AddLogMessage(e.Message + Environment.NewLine + e.StackTrace);
                    }
                    //sleep for 100ms
                    Thread.Sleep(100);
                }
            }

            void CheckforRpPacket()
            {
                if (_mux.RetailProtocolBytesInCounter < 5) return;

                int iStart;

                for (iStart = 0; iStart <= _mux.RetailProtocolBytesInCounter; iStart++)
                {
                    if (0x02 == _mux.RetailProtocolBytesIn[iStart]) //discard trash
                    {
                        break;
                    }
                }

                if (iStart == _mux.RetailProtocolBytesInCounter) return; // no SOH
                if ((iStart + 7) > _mux.RetailProtocolBytesInCounter) return; //packet too small

                int infoLength = EndianBitConverter.Big.ToInt16(_mux.RetailProtocolBytesIn, iStart + 3);

                if ((iStart + infoLength + 6) > _mux.RetailProtocolBytesInCounter) return; //packet with info length is too small

                if (_mux.RetailProtocolBytesIn[(iStart + infoLength + 5)] != 0x03) return; //no ETX

                byte[] retailProtocolPacket = new byte[(infoLength + 7)];

                lock (_mux.RetailProtocolBytesIn)
                {
                    Buffer.BlockCopy(_mux.RetailProtocolBytesIn, iStart, retailProtocolPacket, 0, (infoLength + 7));
                    _mux.RetailProtocolMessages.Add(retailProtocolPacket);

                    _mux.RetailProtocolBytesInCounter -= (iStart + infoLength + 7);
                    Buffer.BlockCopy(_mux.RetailProtocolBytesIn, (iStart + infoLength + 7), _mux.RetailProtocolBytesIn, 0, _mux.RetailProtocolBytesInCounter);
                }
            }




            public void RequestStop()
            {
                _shouldStop = true;
            }
        }



        /// <summary>
        /// This method returns the next retail protocol packet.
        /// </summary>
        /// <returns></returns>
        //public byte[] PullNextRpPacket(byte targetCommandID)
        //{
        //    byte[] ret = null;
        //    if (RetailProtocolMessages.Count > 0)
        //    {
        //        lock (RetailProtocolMessages)
        //        {
        //            for (int packetIndex = 0; packetIndex < RetailProtocolMessages.Count; packetIndex++)
        //            {
        //                ret = (byte[]) RetailProtocolMessages[packetIndex];
        //                if (ret[2] == targetCommandID)
        //                {
        //                    RetailProtocolMessages.RemoveAt(packetIndex);
        //                    break;
        //                }
        //                // the current packet doesn't match the target, so make sure we return null if we completed our pass through the arraylist without finding our target.
        //                ret = null;
        //            }
        //        }

        //    }
        //    return ret;
        //}

        public byte[] PullNextRpPacket(byte targetCommandID)
        {
            lock (RetailProtocolMessages)
            {
                for (int i = 0; i < RetailProtocolMessages.Count; i++)
                {
                    if (RetailProtocolMessages[i] is byte[] packet && packet.Length > 2 && packet[2] == targetCommandID)
                    {
                        RetailProtocolMessages.RemoveAt(i);
                        return packet; // Return the first matching packet (i.e., the oldest)
                    }
                }
            }

            return null; // No matching packet found
        }



        public bool RemoveRetailProtocolPacketsByID(byte targetCommandID)
        {
            // List to store indices of messages to remove
            List<int> indicesToRemove = new List<int>();

            // Brief lock to read and validate messages
            lock (RetailProtocolMessages)
            {
                if (RetailProtocolMessages == null || RetailProtocolMessages.Count == 0)
                    return false;

                // Iterate to find messages with matching command ID
                for (int i = 0; i < RetailProtocolMessages.Count; i++)
                {
                    object item = RetailProtocolMessages[i];
                    if (item is byte[] message && message != null && message.Length >= 3)
                    {
                        byte commandId = message[2]; // Command ID at offset 2 (third byte)
                        if (commandId == targetCommandID)
                        {
                            indicesToRemove.Add(i);
                        }
                    }
                    else
                    {
                        // Invalid message (null or too short)
                        return false;
                    }
                }
            }

            // If no messages to remove, return true (no need for second lock)
            if (indicesToRemove.Count == 0)
                return true;

            // Brief lock to remove messages at collected indices
            lock (RetailProtocolMessages)
            {
                // Remove messages in reverse order to avoid index shifting
                for (int i = indicesToRemove.Count - 1; i >= 0; i--)
                {
                    int index = indicesToRemove[i];
                    if (index < RetailProtocolMessages.Count) // Safety check
                    {
                        RetailProtocolMessages.RemoveAt(index);
                    }
                }
            }

            return true;
        }


        public void Close()
        {
            //Shutdown all the workers

            // stop the mux clean up timer
            _muxInactivityTimer?.Stop();

            _retailProtocolWorker?.RequestStop();
            _retailProtocolWorkThread?.Join();

            _muxDirector?.RequestStop();
            _muxDirectorThread?.Join();

            _bankingProtocolWorker?.RequestStop();
            _bankingProtocolWorkerThread?.Join();

            _muxControlWorker?.RequestStop();
            _muxControlWorkerThread?.Join();

            _muxSendWorker?.RequestStop();
            _muxSendWorkerThread?.Join();

            _debugLogWorker?.RequestStop();
            _debugLogWorkerThread?.Join(2000);
            if (_debugLogWorkerThread?.IsAlive == true) _debugLogWorkerThread.Abort(); //sometimes this doesn't close - so nuke it if necessary.

            //close the RS232 port
            if (null != _SerialPort)
            {
                if (_SerialPort.IsOpen)
                {
                    _SerialPort.Close();
                }
            }  
        }

        public bool MuxSerialStatus()
        {
            return _SerialPort.IsOpen;
        }
        
        /// <summary>
        /// Receive Serial Data Method
        /// 
        /// This will execute on the Receive Data Thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialDataReader(object sender, SerialDataReceivedEventArgs e)
        {
            lock (_MuxBufferIn)
            {
                if (_SerialPort.BytesToRead+_muxBytesInCounter <= _MuxBufferIn.Length) // If not Garbage Restart likely ??
                    _muxBytesInCounter += _SerialPort.Read(_MuxBufferIn, _muxBytesInCounter, _SerialPort.BytesToRead);
            }

            //set cleanup timer
            lock (_muxLock)
            {
                _lastSerialDataTimestamp = DateTime.UtcNow;
            }

            DecodeMux();
        }


        /// <summary>
        /// Decode the inbound serial data into a Mux Packet
        /// </summary>
        private void DecodeMux()
        {
            MuxPacket muxPacket = new MuxPacket();

            lock (_MuxBufferIn)
            {
                if (_muxBytesInCounter < 8) return; //min packet length

                int i;

                for (i = 0; i <= _muxBytesInCounter; i++)
                {
                    if (0x02 == _MuxBufferIn[i]) //discard trash
                    {
                        break;
                    }
                }

                if (i == _muxBytesInCounter) return; //no SOH 
                if (i+3 >= _MuxBufferIn.Length) return; //Will be out of bounds.  Likely a reset

                muxPacket.Soh = _MuxBufferIn[i];
                muxPacket.Address = (EMuxAddress)Enum.ToObject(typeof(EMuxAddress), _MuxBufferIn[i + 1]);
                muxPacket.Cmd = (ECmd)Enum.ToObject(typeof(ECmd),_MuxBufferIn[i + 2]);
                muxPacket.Sequence = _MuxBufferIn[i + 3];
                muxPacket.Length = EndianBitConverter.Big.ToInt16(_MuxBufferIn, (i + 4));
                muxPacket.MuxPacketDirection = MuxPacket.MuxDirection.FromTerminal;
                if (_muxBytesInCounter < (i + 8 + muxPacket.Length)) return; //we don't have the entire packet yet

                if (muxPacket.Length < 0)
                {
                    _muxBytesInCounter -= (i + 8 + muxPacket.Length); //Pull the packet bytes off the buffer
                    return; // garbage likely on restart? 
                }

                Buffer.BlockCopy(_MuxBufferIn, (i + 6), muxPacket.Data, 0, muxPacket.Length);

                //add CRC
                muxPacket.Crc = EndianBitConverter.Big.ToUInt16(_MuxBufferIn, (i + 6 + muxPacket.Length));

                //Pull the packet bytes off the buffer
                _muxBytesInCounter -= (i + 8 + muxPacket.Length);
                Buffer.BlockCopy(_MuxBufferIn, (i + 8 + muxPacket.Length), _MuxBufferIn,0, _muxBytesInCounter);

                //release the lock so additional data may be received.
            }

            _muxDirector.AddNewMuxPacket(muxPacket);
            _debugLogWorker.AddItem(muxPacket);

        }

        
        /// <summary>
        /// Add the data to the Retail Protocol buffer
        /// </summary>
        /// <param name="muxPacket">muxpacket</param>
        public void SendPacketToRetailProtocol(MuxPacket muxPacket)
        {
            lock (RetailProtocolBytesIn)
            {
                Buffer.BlockCopy(muxPacket.Data, 0, RetailProtocolBytesIn, RetailProtocolBytesInCounter,
                    muxPacket.Length);
                RetailProtocolBytesInCounter += muxPacket.Length;
            }
        }

        

        /// <summary>
        /// This method receives retail protocol commands from the Kiosk
        /// </summary>
        /// <param name="retailProtocolBytes">Byte array of the retail protocol command</param>
        public void ReceiveRetailProtocolPacket(byte[] retailProtocolBytes)
        {
            int bytes2Send = retailProtocolBytes.Length;
            

            while (bytes2Send > 0)
            {
                MuxPacket retailProtocolPacket = new MuxPacket();
                retailProtocolPacket.Address = EMuxAddress.RetailProtocol;
                retailProtocolPacket.Cmd = ECmd.Application;
                retailProtocolPacket.Sequence = ++SequenceCounter;
                
                if (bytes2Send < 1000)
                {
                    retailProtocolPacket.Length = bytes2Send;
                    Buffer.BlockCopy(retailProtocolBytes,retailProtocolBytes.Length-bytes2Send,retailProtocolPacket.Data,0,bytes2Send);
                    bytes2Send = 0;
                }
                else
                {
                    retailProtocolPacket.Length = 1000;
                    Buffer.BlockCopy(retailProtocolBytes, retailProtocolBytes.Length - bytes2Send, retailProtocolPacket.Data, 0, 1000);
                    bytes2Send -= 1000;
                }

                retailProtocolPacket.MuxPacketDirection = MuxPacket.MuxDirection.ToTerminal;
                retailProtocolPacket.SetCrc();

                MuxPacketSend(retailProtocolPacket);
                lock (_PendingMuxPackets)
                {
                    _PendingMuxPackets.Add(retailProtocolPacket);
                }


            }
        }

        /// <summary>
        /// Puts the retail banking message on the byte array buffer for consumption
        /// </summary>
        /// <param name="muxPacket">Mux packet</param>
        public void SendPacketToBanking(MuxPacket muxPacket)
        {
            lock (BankingBytesIn)
            {
                Buffer.BlockCopy(muxPacket.Data, 0, BankingBytesIn, BankingBytesInCounter, muxPacket.Length);
                BankingBytesInCounter += muxPacket.Length;
            }


        }

        /// <summary>
        /// Puts the Syslog message on the byte array buffer for consumption
        /// </summary>
        /// <param name="muxPacket">Mux Packet</param>
        public void SendPacketToSyslog(MuxPacket muxPacket)
        {
            string msg = Encoding.UTF8.GetString(muxPacket.Data);
            _debugLogWorker.AddSysLogMessage(msg);       
        }

        /// <summary>
        /// Add the control packet to the queue for the worker thread to manage
        /// </summary>
        /// <param name="muxPacket">Mux Control Packet</param>
        public void MuxControl(MuxPacket muxPacket)
        {
            _muxControlWorker.AddControlPacket(muxPacket);
        }





        /// <summary>
        /// Remove a pending packet from the Pending Packet List
        /// </summary>
        /// <param name="sequence">Sequence</param>
        public void ClearPending(byte sequence)
        {
            foreach (MuxPacket m in _PendingMuxPackets)
            {
                if (m.Sequence == sequence) //found a packet
                {
                    lock (_PendingMuxPackets)
                    {
                        //_PendingMuxPackets.RemoveAt(_PendingMuxPackets.IndexOf(m));
                        m.stale = true;
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Place a mux packet on the send queue for the sendworker to transmit
        /// </summary>
        /// <param name="muxPacket">Mux Packet </param>
        public void MuxPacketSend(MuxPacket muxPacket)
        {
            lock (_MuxSendPackets)
            {
                _MuxSendPackets.Add(muxPacket);
            }
        }
        /// <summary>
        /// Resend packet after NAK
        /// </summary>
        /// <param name="sequence">Sequence number of packet to resend</param>
        public void ResendMuxPacket(int sequence)
        {
            lock (_PendingMuxPackets)
            {
                foreach (MuxPacket m in _PendingMuxPackets)
                {
                    if (m.Sequence == sequence)
                    {
                        MuxPacketSend(m);
                    }
                }
            }
        }

        /// <summary>
        /// Create the NAK packet for the given sequence and send it.
        /// </summary>
        /// <param name="seq">Sequence Number to NAK</param>
        public void SendNak(byte seq)
        {
            MuxPacket nakPacket = new MuxPacket();

            
            nakPacket.Address = EMuxAddress.AckNak;
            nakPacket.Cmd = ECmd.Nak;
            nakPacket.Sequence = seq;
            nakPacket.Length = 0;
            nakPacket.MuxPacketDirection = MuxPacket.MuxDirection.ToTerminal;
            
            nakPacket.SetCrc();

            MuxPacketSend(nakPacket);
        }

        /// <summary>
        /// Create the ACK packet for the given sequence and send it
        /// </summary>
        /// <param name="seq">Sequence Number to ACK</param>
        public void SendAck(byte seq)
        {
            MuxPacket ackPacket = new MuxPacket();


            ackPacket.Address = EMuxAddress.AckNak;
            ackPacket.Cmd = ECmd.Ack;
            ackPacket.Sequence = seq;
            ackPacket.Length = 0;
            ackPacket.MuxPacketDirection = MuxPacket.MuxDirection.ToTerminal;
            ackPacket.SetCrc();

            MuxPacketSend(ackPacket);
        }
    }
}
