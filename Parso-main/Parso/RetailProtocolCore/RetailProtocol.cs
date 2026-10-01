
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//using GlobalcomRetailProtocol;
//using static GlobalcomRetailProtocol.BankingParameter;
//using System.Runtime.Remoting.Contexts;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    /// Test Harness Override
    /// </summary>
    public delegate byte[] TestHarnessCallbackDelegate(byte[] requestBytes, string requestName);


    /// <summary>
    ///  The RetailProtocol class is the object wrapper for the Globalcom Retail Protocol
    /// </summary>
    public class RetailProtocol
    {
        public bool bInitialized;

        /// <summary>
        /// USE Ethernet Flag
        /// </summary>
        public bool UseEthernet;

        public bool UseTLSEthernet;

        /// <summary>
        /// USE Mux Flag
        /// </summary>
        public bool UseMux;

        // ECR is generally only used in Italy on older systems. It is supported for backwards compatibility.
        // Don't use it for new implementations

        /// <summary>
        /// USE ECR Mode - 0x00 means ECR is off.
        /// </summary>
        public byte EcrStopMessage;

        /// <summary>
        /// List of Message IDs for ECR Response Messages
        /// </summary>
        public readonly byte[] EcrResponseMessages = {0xE1, 0xE3, 0xE5, 0xE7, 0xE9, 0xEA, 0xEB, 0xEC};

        /// <summary>
        /// List of Message IDs for ECR Request messages
        /// </summary>
        public readonly byte[] EcrRequestMessages = {0xE0, 0xE2, 0xE4, 0xE6, 0xE8, 0xEB};
        /// <summary>
        /// Ethernet Listener
        /// </summary>
        public TcpServer EthernetListener;

        /// <summary>
        /// TLS Eth Listener
        /// </summary>
        public TlsServer TLSEthernetListener;

        /// <summary>
        /// RS232 Communication port
        /// </summary>
        public SerialPort Comport;

        /// <summary>
        /// RS232 Baud Rate
        /// </summary>
        private int _baudRate;

        /// <summary>
        /// use hardware flow control on the RS232 port?
        /// </summary>
        private bool _hwFlowControl;

        /// <summary>
        /// Buffer for inbounc RS232 data
        /// </summary>
        public byte[] BytesIn;

        /// <summary>
        /// Inbound RS232 buffer position
        /// </summary>
        public int BytesInCounter;

        /// <summary>
        /// Class for MUX protocol communication
        /// </summary>
        public MuxProtocol Mux;

        /// <summary>
        /// Busy flag
        /// </summary>
        public bool Busy;

        /// <summary>
        /// Byte to end RS232 read on
        /// </summary>
        public byte? StopByte = null;


        //public bool? GetChecksumAfterStop = true;

        /// <summary>
        /// Default expected wait time for a command.
        /// </summary>
        public int ExpectedWaitTime;

        /// <summary>
        /// String representation of the COM port to be used
        /// </summary>
        public string OutputDevice;

        /// <summary>
        /// Default Language
        /// </summary>
        public ELanguage Language;

        /// <summary>
        /// ECR Messages received buffer
        /// </summary>
        public ArrayList EcrMessagesReceived;

        /// <summary>
        /// Retail Protocol messages received buffer
        /// </summary>
        public ArrayList RpMessagesReceived;


        /// <summary>
        /// ASCII STX
        /// </summary>
        public const byte Stx = 0x02;

        /// <summary>
        /// ASCII ETX
        /// </summary>
        public const byte Etx = 0x03;

        /// <summary>
        /// Delegates for the worker threads to update the UI for input messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dArgs"></param>
        public delegate void InputDisplayHandler(object sender, DisplayArgs dArgs);

        /// <summary>
        /// Event for updating the UI for input messages
        /// </summary>
        public event InputDisplayHandler IoIn;

        /// <summary>
        /// Delegates for the worker threads ot update the UI for output messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dArgs"></param>
        public delegate void OutputDisplayHandler(object sender, DisplayArgs dArgs);

        /// <summary>
        /// Event for updating the UI for output messages
        /// </summary>
        public event OutputDisplayHandler IoOut;

        //Device Family of Attached terminal
        public EDeviceFamily? DeviceFamily;

        //Device Family of Attached terminal
        public string PCIVer;

        //Maximum data packet size
        public int WindowSize; 

        /// <summary>
        /// DisplayArg class that adds the data payload
        /// </summary>
        public class DisplayArgs : EventArgs
        {
            /// <summary>
            /// Byte array for payload data
            /// </summary>
            public byte[] Data;

            /// <summary>
            /// Creator
            /// </summary>
            /// <param name="input">payload data</param>
            public DisplayArgs(byte[] input)
            {
                Data = input;
            }
        }

        /// <summary>
        /// Refresh UI for long running communication waits
        /// </summary>
        /// <param name="sender"></param>
        public delegate void RefreshUI(object sender);

        /// <summary>
        /// Event for UI Refresh
        /// </summary>
        public event RefreshUI RefreshUIEvent;

        /// <summary>
        /// Ethernet Socket for a terminal server (ETH->SER)
        /// </summary>
        public Socket TerminalServerSocket;


        /// <summary>
        /// USE Ethernet Flag
        /// </summary>
        private TestHarnessCallbackDelegate _testHarnessCallbackDelegate;


        public SslStream SslStream;

        private bool _FullSpeedETH;

        /// <summary>
        /// The Retail Protocol Object for testing.  Does not go out to a physical device.
        /// </summary>
        /// <param name="testharnessCallbackDelagate"> Enable Unit Testing</param>
        /// <param name="eDeviceFamily"></param>
        /// <param name="language"></param>
        public RetailProtocol(TestHarnessCallbackDelegate testharnessCallbackDelagate, EDeviceFamily eDeviceFamily, ELanguage language = ELanguage.English, int windowSize=1024)
        {
            _testHarnessCallbackDelegate = testharnessCallbackDelagate;

            // Set some defaults
            EcrMessagesReceived = new ArrayList();
            RpMessagesReceived = new ArrayList();

            DeviceFamily = eDeviceFamily;
            WindowSize = windowSize;
            OutputDevice = "none";
            _baudRate = 0;
            UseMux = false;
            EcrStopMessage = 0x00;
            ExpectedWaitTime = 10; //default is 10 seconds to wait for a response.

            // Set prompts to English
            Language = language;

            bInitialized = false;
        }

        /// <summary>
        /// The Retail Protocol Object
        /// </summary>
        /// <param name="outputDevice">COM port to use</param>
        /// <param name="baudRate">Baud Rate to use</param>
        /// <param name="portNumber">Ethernet Port # to listen on</param>
        /// <param name="useMux">Flag to use the MUX protocol</param>
        /// <param name="language">Default Language</param>
        /// <param name="windowSize">Retail Protocol Packet size</param>
        /// <param name="bUseTls">use TLS?</param>
        public RetailProtocol(string outputDevice, int baudRate, int portNumber, bool useMux, bool hwFlowControl = false, ELanguage language = ELanguage.English, int windowSize = 1024,bool bUseTls = false)
        {
            //Initialize Class
            DeviceFamily = EDeviceFamily.PaymentDevice; //generic to start
            WindowSize = windowSize;
            OutputDevice = outputDevice;
            _baudRate = baudRate;
            _hwFlowControl = hwFlowControl;
            UseMux = useMux;
            EcrStopMessage = 0x00;
            ExpectedWaitTime = 10; //default is 10 seconds to wait for a response.

            // Set prompts to English
            Language = language;

            EcrMessagesReceived = new ArrayList();
            RpMessagesReceived = new ArrayList();

            if (0 == portNumber)
            {

                if (useMux)
                {
                    Mux = new MuxProtocol(OutputDevice, _baudRate, hwFlowControl);
                    Busy = false;
                }
                else
                {
                    //buffer for inbound serial port data
                    BytesIn = new byte[10*1024];
                    C_SerialResetPort();
                    Busy = false;
                }
            }
            else
            {
                if (bUseTls)
                {
                    C_TLSEthStartListener(portNumber);
                }
                else
                {
                    C_EthStartListener(portNumber);
                }
            }

        }

        /// <summary>
        /// Retail Protocol Object
        /// Overloaded for ETH communication by Terminal server (ETH->RS232 device)
        /// </summary>
        /// 
        /// <param name="terminalServerAddress">IP Address of Terminal Server</param>
        /// <param name="terminalServerPort">TCP Port of Terminal Server</param>
        /// <param name="language">Default Language</param>
        /// <param name="windowSize">Retail Protocol Info size</param>
        /// <param name="hostName">TLS Server DNS Hostname</param>
        /// <param name="fullSpeed">Full Speed ETH connection - don't spoon feed the data</param>
        /// <exception cref="Exception"></exception>
        public RetailProtocol(IPAddress terminalServerAddress, int terminalServerPort, ELanguage language = ELanguage.English, int windowSize = 1024, string hostName="",bool fullSpeed = false)
        {
            DeviceFamily = EDeviceFamily.PaymentDevice; //generic to start
            UseMux = false;
            EcrStopMessage = 0x00;
            ExpectedWaitTime = 10;
            Language = language;
            _FullSpeedETH = fullSpeed; 


            WindowSize = windowSize;
            EcrMessagesReceived = new ArrayList();
            RpMessagesReceived = new ArrayList();

            if ((terminalServerPort > 65534) || (terminalServerPort < 1))
            {
                throw new Exception("Illegal Port Number:" + terminalServerPort.ToString());
            }

            IPEndPoint tsEndPoint = new IPEndPoint(terminalServerAddress, terminalServerPort);
            TerminalServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TerminalServerSocket.ReceiveBufferSize = 10*1024;
            TerminalServerSocket.SendBufferSize = 10*1024;
            TerminalServerSocket.ReceiveTimeout = 2000;
            TerminalServerSocket.SendTimeout = 1000;
            TerminalServerSocket.ExclusiveAddressUse = true;
            TerminalServerSocket.NoDelay = false;

            // Not compatible for MAC
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                TerminalServerSocket.DontFragment = true;

            TerminalServerSocket.Blocking = true;
            TerminalServerSocket.Connect(tsEndPoint);
             
            // Setup TLS if requested
            if (hostName.Length>0)
            {
                try
                {
                    NetworkStream networkStream = new NetworkStream(TerminalServerSocket);
                    //ignore TLS Server auth
                    //TODO: Fix ignoring the TLS Server of the BV  - when we know the cert we are hitting.
                    SslStream = new SslStream(networkStream, true, new RemoteCertificateValidationCallback((sender, certificate, chain, SslPolicyErrors)=> true));
                    X509CertificateCollection clientCertificates = new X509CertificateCollection();

                    //Ignore TLS Client Auth
                    SslStream.AuthenticateAsClient(hostName,clientCertificates,SslProtocols.Tls12,checkCertificateRevocation:false);
                    Thread.Sleep(20);
                }
                catch (AuthenticationException e)
                {
                    TerminalServerSocket.Close();
                    throw new Exception("Server TLS Auth Failure:"+e.Message);
                }
            }
            
        }

        public void Initialize()
        {
            // Determine the type of device.
            if (EDeviceFamily.PaymentDevice == DeviceFamily)
            {
                RpResponseDeviceFamily rpFamily = RP_Ext_GetDeviceFamily();
                DeviceFamily = rpFamily.DeviceFamily;

                RpResponseFirmwareVersion rpf = RP_FirmwareVersionRequest();
                PCIVer = "PCI V" + rpf.getPCIVersion();
            }

            bInitialized = true;
        }

        public void DisconnectEthernet()
        {
            try
            {
                if (TerminalServerSocket != null)
                    TerminalServerSocket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                if (TerminalServerSocket != null)
                    TerminalServerSocket.Close();
            }
        }

        /// <summary>
        /// Check if the RetailProtocol class is in override test mode
        /// </summary>
        public bool InTestMode()
        {
            return _testHarnessCallbackDelegate != null ? true : false;
        }

        /// <summary>
        /// Convert a string to a fixed length byte array padded to the right with 0x00
        /// </summary>
        /// <param name="input">string to convert</param>
        /// <param name="fixedLength">byte array length</param>
        /// <returns></returns>
        public byte[] StringToByteArray(string input, int fixedLength)
        {
            if (input.Length > fixedLength)
            {
                throw new Exception("Input length exceeds byte count");
            }
            byte[] output = Enumerable.Repeat((byte) 0x00, fixedLength).ToArray();


            Buffer.BlockCopy(Encoding.ASCII.GetBytes(input), 0, output, 0, input.Length);

            return output;
        }

        ///// <summary>
        ///// Convert an IP String to an array of bytes padded to the right with 0x00
        ///// </summary>
        ///// <param name="iPv4Address">IPV4 Address</param>
        ///// <param name="fixedLength">Length of byte array</param>
        ///// <returns></returns>
        //public byte[] IPStringToBytes(string iPv4Address, int fixedLength)
        //{
        //    byte[] output = Enumerable.Repeat((byte)0x00, fixedLength).ToArray();
        //    try
        //    {

        //        IPAddress IPtoConvert;

        //        if (IPAddress.TryParse(iPv4Address, out IPtoConvert))
        //        {
        //            Buffer.BlockCopy(IPtoConvert.GetAddressBytes(),0,output,0,4);
        //        }
        //        else
        //        {
        //            throw new Exception("Illegal IP address. Unable to Convert IP address to Byte Array");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Unable to parse IP address from input. Exception:" + e.Message);
        //    }

        //    return output;
        //}

        /// <summary>
        /// Convert an Int32 to a byte array padded to the right with 0x00
        /// </summary>
        /// <param name="input">Int32 to Convert</param>
        /// <param name="fixedLength">Length of Byte Array</param>
        /// <returns>byte[] </returns>
        public byte[] BytesFromInt(int input, int fixedLength)
        {
            byte[] output = Enumerable.Repeat((byte) 0x00, fixedLength).ToArray();

            Buffer.BlockCopy(BitConverter.GetBytes(input), 0, output, 0, 4);

            return output;
        }

        /// <summary>
        /// 0x4B NOTIFY OUTCOME
        /// The PU uses this command to request the outcome of the notify and the error code.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseNotifyOutcome RP_NotifyOutcome()
        {
            RpCommand rpNotifyOutcomeRequest = new RpCommand(this);

            rpNotifyOutcomeRequest.UPTAddress = 0x00;
            rpNotifyOutcomeRequest.CommandId = 0x4B;
            rpNotifyOutcomeRequest.InfoLength = 0;
            rpNotifyOutcomeRequest.FriendlyName = "0x4B Notify Outcome Request";


            rpNotifyOutcomeRequest.CompileCommand();
            RpResponseNotifyOutcome rpNotifyOutcomeResponse =
                new RpResponseNotifyOutcome(rpNotifyOutcomeRequest.Execute());

            return rpNotifyOutcomeResponse;
        }


        /// <summary>
        /// 0x4A NOTIFY COMMAND
        /// The PU uses this command to request a notify of a preauthorization, necessary to the gasoline payment.
        /// </summary>
        /// <param name="preauthId">ID to Notify</param>
        /// <param name="amountInPennies">Amount </param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_NotifyCommand(byte preauthId, int amountInPennies)
        {
            RpCommand rpNotifyCommandRequest = new RpCommand(this);
            
            rpNotifyCommandRequest.UPTAddress = 0x00;
            rpNotifyCommandRequest.CommandId = 0x4A;
            rpNotifyCommandRequest.ewt = 45;
            rpNotifyCommandRequest.InfoLength = 5;
            rpNotifyCommandRequest.Info[0] = preauthId;
            rpNotifyCommandRequest.Info[1] = (byte) (amountInPennies >> 24);
            rpNotifyCommandRequest.Info[2] = (byte) (amountInPennies >> 16);
            rpNotifyCommandRequest.Info[3] = (byte) (amountInPennies >> 8);
            rpNotifyCommandRequest.Info[4] = (byte) amountInPennies;
            rpNotifyCommandRequest.FriendlyName = "0x4A Notify Command Request";
            
            rpNotifyCommandRequest.CompileCommand();

            RpResponseBase rpNotifyCommandResponse = new RpResponseBase((rpNotifyCommandRequest.Execute()));
            return rpNotifyCommandResponse;
        }



        /// <summary>
        /// Petroleum Application version
        /// 0x4A NOTIFY COMMAND
        /// The PU uses this command to request a notify of a preauthorization, necessary to the gasoline payment.
        /// sometimes called "Capture"
        /// This command for Petroleum includes the product details
        /// </summary>
        /// <param name="preauthId">ID to Notify</param>
        /// <param name="amountInPennies">Amount </param>
        /// <param name="petroProductDetails">list of product details to send</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_PetroNotifyCommand(byte preauthId, int amountInPennies, List<PetroProductDetails> petroProductDetails)
        {
            RpCommand rpNotifyCommandRequest = new RpCommand(this);

            rpNotifyCommandRequest.UPTAddress = 0x00;
            rpNotifyCommandRequest.CommandId = 0x4A;
            rpNotifyCommandRequest.ewt = 45;
            //rpNotifyCommandRequest.InfoLength = 5;
            rpNotifyCommandRequest.Info[0] = preauthId;
            rpNotifyCommandRequest.Info[1] = (byte)(amountInPennies >> 24);
            rpNotifyCommandRequest.Info[2] = (byte)(amountInPennies >> 16);
            rpNotifyCommandRequest.Info[3] = (byte)(amountInPennies >> 8);
            rpNotifyCommandRequest.Info[4] = (byte)amountInPennies;
            rpNotifyCommandRequest.FriendlyName = "0x4A Notify Command Request";

            PopulatePetroProducts(petroProductDetails, ref rpNotifyCommandRequest);

            rpNotifyCommandRequest.CompileCommand();

            RpResponseBase rpNotifyCommandResponse = new RpResponseBase((rpNotifyCommandRequest.Execute()));
            return rpNotifyCommandResponse;
        }





        /// <summary>
        /// 0x49 REQUEST OF PREAUTHORIZATION OUTCOME
        /// The PU uses this command to request at the terminal the outcome of the preauthorization executed.
        /// </summary>
        /// <returns>RpResponsePreAuthOutcome</returns>
        public RpResponsePreAuthOutcome RP_PreAuthorizationOutcome()
        {
            RpCommand rpPreAuthorizationOutcomeRequest = new RpCommand(this);

            rpPreAuthorizationOutcomeRequest.UPTAddress = 0x00;
            rpPreAuthorizationOutcomeRequest.CommandId = 0x49;
            rpPreAuthorizationOutcomeRequest.InfoLength = 0;

            rpPreAuthorizationOutcomeRequest.CompileCommand();
            rpPreAuthorizationOutcomeRequest.FriendlyName = "0x49 Pre Authorization Outcome Request";

            RpResponsePreAuthOutcome rpPreAuthorizationOutcomeResponse =
                new RpResponsePreAuthOutcome(rpPreAuthorizationOutcomeRequest.Execute());

            return rpPreAuthorizationOutcomeResponse;
        }

        /// <summary>
        /// 0x40 File SYstem Type Request (BV1000 only)
        /// This command is to query the type of filesystem in use on the board (for BV1000 boards only)
        /// </summary>
        /// <returns>RpResponseFileSystemType</returns>
        public RpResponseFileSystemType RP_FileSystemTypeRequest()
        {
            RpCommand rpFileSystemTypeRequest = new RpCommand(this);

            rpFileSystemTypeRequest.UPTAddress = 0x00;
            rpFileSystemTypeRequest.CommandId = 0x40;
            rpFileSystemTypeRequest.InfoLength = 0;
            rpFileSystemTypeRequest.FriendlyName = "0x40 FileSystem Type Request";

            rpFileSystemTypeRequest.CompileCommand();

            RpResponseFileSystemType rpFileSystemTypeResonse = new RpResponseFileSystemType(rpFileSystemTypeRequest.Execute());

            return rpFileSystemTypeResonse;
        }

        /// <summary>
        /// 0x48 PREAUTHORIZATION COMMAND
        /// This command is used to start a preauthorization operation. 
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_PreAuthorization(int amountInPennies, ECurrencies currencyCode)
        {
            RpCommand rpPreAuthorizationRequest = new RpCommand(this);

            rpPreAuthorizationRequest.UPTAddress = 0x00;
            rpPreAuthorizationRequest.CommandId = 0x48;
            rpPreAuthorizationRequest.InfoLength = 10;
            rpPreAuthorizationRequest.Info[0] = (byte) (amountInPennies >> 24);
            rpPreAuthorizationRequest.Info[1] = (byte) (amountInPennies >> 16);
            rpPreAuthorizationRequest.Info[2] = (byte) (amountInPennies >> 8);
            rpPreAuthorizationRequest.Info[3] = (byte) amountInPennies;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(currencyCode.ToString()), 0, rpPreAuthorizationRequest.Info, 4, 3);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(((Int32) currencyCode).ToString()), 0,
                rpPreAuthorizationRequest.Info, 7, 3);
            rpPreAuthorizationRequest.FriendlyName = "0x48 Pre Authorization Request";

            rpPreAuthorizationRequest.CompileCommand();

            RpResponseBase rpPreAuthorizationResponse = new RpResponseBase(rpPreAuthorizationRequest.Execute());

            return rpPreAuthorizationResponse;
        }

        /// <summary>
        /// 0x48 PREAUTHORIZATION COMMAND
        /// This command is used to start a preauthorization operation. 
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_IncPreAuthorizationHack(int amountInPennies, ECurrencies currencyCode, string slot)
        {
            RpCommand rpPreAuthorizationRequest = new RpCommand(this);

            rpPreAuthorizationRequest.UPTAddress = 0x00;
            rpPreAuthorizationRequest.CommandId = 0x48;
            rpPreAuthorizationRequest.InfoLength = 10;
            rpPreAuthorizationRequest.Info[0] = (byte)(amountInPennies >> 24);
            rpPreAuthorizationRequest.Info[1] = (byte)(amountInPennies >> 16);
            rpPreAuthorizationRequest.Info[2] = (byte)(amountInPennies >> 8);
            rpPreAuthorizationRequest.Info[3] = (byte)amountInPennies;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(currencyCode.ToString()), 0, rpPreAuthorizationRequest.Info, 4, 3);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(slot.ToString()), 0,         rpPreAuthorizationRequest.Info, 7, 3);
            rpPreAuthorizationRequest.FriendlyName = "0x48 Pre Authorization Request";

            rpPreAuthorizationRequest.CompileCommand();

            RpResponseBase rpPreAuthorizationResponse = new RpResponseBase(rpPreAuthorizationRequest.Execute());

            return rpPreAuthorizationResponse;
        }

        /// <summary>
        /// 0x48 PREAUTHORIZATION COMMAND
        /// This command is used to start a preauthorization operation.
        ///
        /// This method is specific for NBS payment application for Petro.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_PetroPreAuthorization(int amountInPennies, ECurrencies currencyCode, BindingList<Prompting> promptingList)
        {
            RpCommand rpPreAuthorizationRequest = new RpCommand(this);

            rpPreAuthorizationRequest.UPTAddress = 0x00;
            rpPreAuthorizationRequest.CommandId = 0x48;
            
            rpPreAuthorizationRequest.Info[0] = (byte)(amountInPennies >> 24);
            rpPreAuthorizationRequest.Info[1] = (byte)(amountInPennies >> 16);
            rpPreAuthorizationRequest.Info[2] = (byte)(amountInPennies >> 8);
            rpPreAuthorizationRequest.Info[3] = (byte)amountInPennies;

            Buffer.BlockCopy(Encoding.ASCII.GetBytes(currencyCode.ToString()), 0, rpPreAuthorizationRequest.Info, 4, 3);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(((Int32)currencyCode).ToString()), 0, rpPreAuthorizationRequest.Info, 7, 3);
            
            //Create TLV for Petro Prompt list
            ushort pos = 10;
            ushort respLength = 0;
            foreach (Prompting p in promptingList)
            {
                rpPreAuthorizationRequest.Info[pos++] = (byte) p.Tag;
                
                //determine the length of the prompt response in ASCII bytes
                respLength = (ushort)Encoding.ASCII.GetBytes(p.Response).Length;
                if (respLength > 255) respLength = 255; //nerf it to 255 chars.
                rpPreAuthorizationRequest.Info[pos++] = (byte)respLength;
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(p.Response), 0, rpPreAuthorizationRequest.Info, pos, respLength);
                pos += respLength;
            }

            rpPreAuthorizationRequest.InfoLength = pos;

            rpPreAuthorizationRequest.FriendlyName = "0x48 Petro-Pre Authorization Request";
            
            rpPreAuthorizationRequest.CompileCommand();

            RpResponseBase rpPreAuthorizationResponse = new RpResponseBase(rpPreAuthorizationRequest.Execute());

            return rpPreAuthorizationResponse;
        }

        /// <summary>
        /// 0x13 REQUEST INSERTED DATA FROM KEYBOARD
        /// This command is used to request the key pressed from the user after received the status request with “keyboard data available”
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_RequestInsertedDataFromKeyboardRequest()
        {
            RpCommand rpRequestInsertedDataFromKeyboardRequest = new RpCommand(this);

            rpRequestInsertedDataFromKeyboardRequest.UPTAddress = 0x00;
            rpRequestInsertedDataFromKeyboardRequest.CommandId = 0x13;
            rpRequestInsertedDataFromKeyboardRequest.InfoLength = 0;
            rpRequestInsertedDataFromKeyboardRequest.FriendlyName = "0x13 Request Keyboard Data";

            rpRequestInsertedDataFromKeyboardRequest.CompileCommand();
            RpResponseBase rpRequestInsertedDataFromKeyboardRequestResponse =
                new RpResponseBase(rpRequestInsertedDataFromKeyboardRequest.Execute());

            return rpRequestInsertedDataFromKeyboardRequestResponse;
        }

        ///<summary>
        /// 0x12 DISPLAY AND KEYBOARD MESSAGE
        /// This command is used to request the POS terminal to print a message and display it on the screen,and to manage the keyboard input.
        /// BigEndianBitConverter.Big.ToInt16(response, 3)
        /// </summary>
        /// <param name="language">ISO 2 letter language abbriviation</param>
        /// <param name="line0Message">Enumerated Message Number to display on Line 0</param>
        /// <param name="line1Message">Enumerated Message Number to display on Line 1</param>
        /// <param name="line2Message">Enumerated Message Number to display on Line 2</param>
        /// <param name="line3Message">Enumerated Message Number to display on Line 3</param>
        /// <param name="inputLine">Line for the input message to be echoed</param>
        /// <param name="inputLength">Maximum size for the requested input.</param>
        /// <param name="minInputLength">Minimum size for the requested input.</param>
        /// <param name="persistence">Length of time in seconds to display the prompt and wait for the response.</param>
        /// <param name="offset">Number of characters from the left of the screen to begin echoing the input.</param>
        /// <param name="insertType">0=Numerical,1=Alphanumerical, >20 is the mask in ASCII </param>
        public RpResponseBase RP_DisplayAndKeyboardMessage(byte[] language, ushort line0Message, ushort line1Message,
            ushort line2Message, ushort line3Message, byte inputLine, byte persistence, byte inputLength, byte offset,
            byte insertType, byte minInputLength)
        {
            byte[] ignoredByBv1000 = {0x00, 0x00};

            RpCommand rpDisplayAndKeyboardMessageRequest = new RpCommand(this);

            rpDisplayAndKeyboardMessageRequest.UPTAddress = 0x00;
            rpDisplayAndKeyboardMessageRequest.CommandId = 0x12;
            rpDisplayAndKeyboardMessageRequest.InfoLength = 24;

            rpDisplayAndKeyboardMessageRequest.Info[0] = language[0];
            rpDisplayAndKeyboardMessageRequest.Info[1] = language[1];
            rpDisplayAndKeyboardMessageRequest.Info[2] = (byte) (line0Message >> 8); //MSB
            rpDisplayAndKeyboardMessageRequest.Info[3] = (byte) (line0Message & 0xFF); //LSB
            rpDisplayAndKeyboardMessageRequest.Info[4] = (byte) (line1Message >> 8); //MSB
            rpDisplayAndKeyboardMessageRequest.Info[5] = (byte) (line1Message & 0xFF); //LSB
            rpDisplayAndKeyboardMessageRequest.Info[6] = (byte) (line2Message >> 8); //MSB
            rpDisplayAndKeyboardMessageRequest.Info[7] = (byte) (line2Message & 0xFF); //LSB
            rpDisplayAndKeyboardMessageRequest.Info[8] = (byte) (line3Message >> 8); //MSB
            rpDisplayAndKeyboardMessageRequest.Info[9] = (byte) (line3Message & 0xFF); //LSB

            rpDisplayAndKeyboardMessageRequest.Info[10] = ignoredByBv1000[0]; //Line4
            rpDisplayAndKeyboardMessageRequest.Info[11] = ignoredByBv1000[1];
            rpDisplayAndKeyboardMessageRequest.Info[12] = ignoredByBv1000[0]; //Line5
            rpDisplayAndKeyboardMessageRequest.Info[13] = ignoredByBv1000[1];
            rpDisplayAndKeyboardMessageRequest.Info[14] = ignoredByBv1000[0]; //Line6
            rpDisplayAndKeyboardMessageRequest.Info[15] = ignoredByBv1000[1];
            rpDisplayAndKeyboardMessageRequest.Info[16] = ignoredByBv1000[0]; //Line7
            rpDisplayAndKeyboardMessageRequest.Info[17] = ignoredByBv1000[1];

            rpDisplayAndKeyboardMessageRequest.Info[18] = inputLine;

            rpDisplayAndKeyboardMessageRequest.Info[19] = persistence;

            rpDisplayAndKeyboardMessageRequest.Info[20] = inputLength;

            rpDisplayAndKeyboardMessageRequest.Info[21] = offset;

            rpDisplayAndKeyboardMessageRequest.Info[22] = insertType;

            rpDisplayAndKeyboardMessageRequest.Info[23] = minInputLength;

            rpDisplayAndKeyboardMessageRequest.CompileCommand();
            rpDisplayAndKeyboardMessageRequest.FriendlyName = "0x12 Subject Display and Keyboard";

            RpResponseBase rpSubjectionDisplayAndKeyboardRequestResponse =
                new RpResponseBase(rpDisplayAndKeyboardMessageRequest.Execute());

            return rpSubjectionDisplayAndKeyboardRequestResponse;
        }

        /// <summary>
        /// 0x39 READER POWER OFF
        /// This command is used to power off the reader, if the reader is not powered off the reader answer after 500 milliseconds.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_ReaderPowerOff()
        {
            RpCommand rpReaderPowerOffRequest = new RpCommand(this);

            rpReaderPowerOffRequest.UPTAddress = 0x00;
            rpReaderPowerOffRequest.CommandId = 0x39;
            rpReaderPowerOffRequest.InfoLength = 0;
            rpReaderPowerOffRequest.FriendlyName = "0x39 Reader Power Off";


            rpReaderPowerOffRequest.CompileCommand();
            RpResponseBase rpReaderPowerOffResponse = new RpResponseBase(rpReaderPowerOffRequest.Execute());

            return rpReaderPowerOffResponse;
        }


        /// <summary>
        /// 0x65 GET TIME DEVICE
        /// This command is used to view the date and the hour of the device.
        /// </summary>
        /// <returns>RpResponseDateTime</returns>
        public RpResponseDateTime RP_GetTimeDevice()
        {
            RpCommand rpGetTimeDeviceRequest = new RpCommand(this);

            rpGetTimeDeviceRequest.UPTAddress = 0x00;
            rpGetTimeDeviceRequest.CommandId = 0x65;
            rpGetTimeDeviceRequest.InfoLength = 0;
            rpGetTimeDeviceRequest.FriendlyName = "0x65 Get Time Device";

            rpGetTimeDeviceRequest.CompileCommand();

            RpResponseDateTime rpGetTimeDeviceResponse = new RpResponseDateTime(rpGetTimeDeviceRequest.Execute());

            return rpGetTimeDeviceResponse;
        }

        /// <summary>
        /// 0x6C GET TIME DEVICE 2
        /// This command is used to view the date and the hour of the contactless.
        /// </summary>
        /// <returns>RpResponseDateTime</returns>
        public RpResponseDateTime RP_GetTimeNFC()
        {
            RpCommand rpGetTimeNfcRequest = new RpCommand(this);

            rpGetTimeNfcRequest.UPTAddress = 0x00;
            rpGetTimeNfcRequest.CommandId = 0x6C;
            rpGetTimeNfcRequest.InfoLength = 0;
            rpGetTimeNfcRequest.FriendlyName = "0x6C Get Time NFC";

            rpGetTimeNfcRequest.CompileCommand();

            RpResponseDateTime rpGetTimeNfcResponse = new RpResponseDateTime(rpGetTimeNfcRequest.Execute());

            return rpGetTimeNfcResponse;
        }

        /// <summary>
        /// 0x64 ANTIREMOVAL GET TIMESTAMP DEVICE
        /// This command is used to view the date and the hour of the last violation happened on the slave.
        /// </summary>
        /// <returns>RpResponseDateTime</returns>
        public RpResponseDateTime RP_AntiRemovalGetTimestampDevice()
        {
            RpCommand rpAntiRemovalGetTimestampDeviceRequest = new RpCommand(this);

            rpAntiRemovalGetTimestampDeviceRequest.UPTAddress = 0x00;
            rpAntiRemovalGetTimestampDeviceRequest.CommandId = 0x64;
            rpAntiRemovalGetTimestampDeviceRequest.InfoLength = 0;
            rpAntiRemovalGetTimestampDeviceRequest.FriendlyName = "0x64 Get AntiRemoval Timestamp Device";
            rpAntiRemovalGetTimestampDeviceRequest.CompileCommand();

            RpResponseDateTime rpAntiRemovalGetTimestampDeviceResponse =
                new RpResponseDateTime(rpAntiRemovalGetTimestampDeviceRequest.Execute());

            return rpAntiRemovalGetTimestampDeviceResponse;
        }

        /// <summary>
        /// 0x63 GET TIME MASTER
        /// This command is used to view the date and the hour of the master.
        /// </summary>
        /// <returns>RpResponseDateTime</returns>
        public RpResponseDateTime RP_GetTimeMasterDevice()
        {
            RpCommand rpGetTimeMasterDeviceRequest = new RpCommand(this);

            rpGetTimeMasterDeviceRequest.UPTAddress = 0x00;
            rpGetTimeMasterDeviceRequest.CommandId = 0x63;
            rpGetTimeMasterDeviceRequest.InfoLength = 0;
            rpGetTimeMasterDeviceRequest.FriendlyName = "0x63 Get Time Master";
            rpGetTimeMasterDeviceRequest.CompileCommand();
            RpResponseDateTime rpGetTimeMasterDeviceResponse =
                new RpResponseDateTime(rpGetTimeMasterDeviceRequest.Execute());

            return rpGetTimeMasterDeviceResponse;
        }

        /// <summary>
        /// 0x62 SET TIME MASTER DEVICE
        /// This command is used to set date and hour on master and slave.
        /// </summary>
        /// <param name="masterTime">DateTime to set</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_SetTimeMasterDevice(DateTime masterTime)
        {
            RpCommand rpSetTimeMasterDeviceRequest = new RpCommand(this);

            StringBuilder timeSetOut = new StringBuilder();

            rpSetTimeMasterDeviceRequest.UPTAddress = 0x00;
            rpSetTimeMasterDeviceRequest.CommandId = 0x62;
            rpSetTimeMasterDeviceRequest.InfoLength = 16;

            timeSetOut.Append(masterTime.Day.ToString("D2"));
            timeSetOut.Append("/");
            timeSetOut.Append(masterTime.Month.ToString("D2"));
            timeSetOut.Append("/");
            timeSetOut.Append(masterTime.Year.ToString().Substring(2, 2));
            timeSetOut.Append(masterTime.Hour.ToString("D2"));
            timeSetOut.Append(":");
            timeSetOut.Append(masterTime.Minute.ToString("D2"));
            timeSetOut.Append(":");
            timeSetOut.Append(masterTime.Second.ToString("D2"));

            Buffer.BlockCopy(Encoding.ASCII.GetBytes(timeSetOut.ToString()), 0, rpSetTimeMasterDeviceRequest.Info, 0,
                16);
            rpSetTimeMasterDeviceRequest.FriendlyName = "0x62 Set Time Master Device";

            rpSetTimeMasterDeviceRequest.CompileCommand();


            RpResponseBase rpSetTimeMasterDeviceResponse = new RpResponseBase(rpSetTimeMasterDeviceRequest.Execute());

            return rpSetTimeMasterDeviceResponse;
        }

        /// <summary>
        /// 0x61 DEVICE GET INFO
        /// This command is used to obtain information about the status of the slave.
        /// </summary>
        /// <returns>RpResponseDeviceInfo</returns>
        public RpResponseDeviceInfo RP_DeviceGetInfo()
        {
            RpCommand rpDeviceGetInfoRequest = new RpCommand(this);

            rpDeviceGetInfoRequest.UPTAddress = 0x00;
            rpDeviceGetInfoRequest.CommandId = 0x61;
            rpDeviceGetInfoRequest.InfoLength = 0;
            rpDeviceGetInfoRequest.FriendlyName = "0x61 Device Get Info";

            rpDeviceGetInfoRequest.CompileCommand();
            RpResponseDeviceInfo rpDeviceGetInfoResponse = new RpResponseDeviceInfo(rpDeviceGetInfoRequest.Execute());

            return rpDeviceGetInfoResponse;
        }

        /// <summary>
        /// 0x6B DEVICE 2 GET INFO
        /// This command is used to obtain information about the status of the contactless.
        /// </summary>
        /// <returns>RpResponseDeviceInfo</returns>
        public RpResponseDeviceInfo RP_NFCGetInfo()
        {
            RpCommand rpNfcGetInfoRequest = new RpCommand(this);

            rpNfcGetInfoRequest.UPTAddress = 0x00;
            rpNfcGetInfoRequest.CommandId = 0x6B;
            rpNfcGetInfoRequest.InfoLength = 0;
            rpNfcGetInfoRequest.FriendlyName = "0x6B NFC-Contactless Get Info";

            rpNfcGetInfoRequest.CompileCommand();
            RpResponseDeviceInfo rpNfcGetInfoResponse = new RpResponseDeviceInfo(rpNfcGetInfoRequest.Execute());

            return rpNfcGetInfoResponse;
        }

        /// <summary>
        /// 0x60 ANTIREMOVAL ENABLE
        /// This command is enable the anti-removal sensors on master and slave.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_AntiRemovalEnable()
        {
            RpCommand rpAntiRemovalEnableRequest = new RpCommand(this);

            rpAntiRemovalEnableRequest.UPTAddress = 0x00;
            rpAntiRemovalEnableRequest.CommandId = 0x60;
            rpAntiRemovalEnableRequest.InfoLength = 0;
            rpAntiRemovalEnableRequest.FriendlyName = "0x60 Anti Removal Enable";

            rpAntiRemovalEnableRequest.CompileCommand();

            RpResponseBase rpAntiRemovalEnableResponse = new RpResponseBase(rpAntiRemovalEnableRequest.Execute());

            return rpAntiRemovalEnableResponse;
        }

        /// <summary>
        /// 0x59 ANTIREMOVAL GET TIMESTAMP MASTER
        /// This command is used to view the date and the hour of the last violation happened on the master.
        /// </summary>
        /// <returns>RpResponseDateTime</returns>
        public RpResponseDateTime RP_AntiRemovalGetTimestampMaster()
        {
            RpCommand rpAntiRemovalGetTimestampMasterRequest = new RpCommand(this);


            rpAntiRemovalGetTimestampMasterRequest.UPTAddress = 0x00;
            rpAntiRemovalGetTimestampMasterRequest.CommandId = 0x59;
            rpAntiRemovalGetTimestampMasterRequest.InfoLength = 0;
            rpAntiRemovalGetTimestampMasterRequest.FriendlyName = "0x59 AntiRemoval Get Timestamp Master";

            rpAntiRemovalGetTimestampMasterRequest.CompileCommand();

            RpResponseDateTime rpAntiRemovalGetTimestampMasterResponse =
                new RpResponseDateTime(rpAntiRemovalGetTimestampMasterRequest.Execute());

            return rpAntiRemovalGetTimestampMasterResponse;
        }

        /// <summary>
        /// 0x58 NEW DEVICE PAIRING
        /// This command is used to pair two devices. 
        /// The PU send to the UPT the unlock key, the UPT verify the key, 
        /// then if the key is correct and the sensors are close, the UPT starts the pairing operations.
        /// </summary>
        /// <param name="unlockCode">Unlock key</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_NewDeviceParing(byte[] unlockCode)
        {
            RpCommand rpNewDeviceParingRequest = new RpCommand(this);

            if (unlockCode.Length != 10)
            {
                throw new Exception("Illegal Unlock Code Size!");
            }

            rpNewDeviceParingRequest.UPTAddress = 0x00;
            rpNewDeviceParingRequest.CommandId = 0x58;
            rpNewDeviceParingRequest.InfoLength = 10;
            rpNewDeviceParingRequest.FriendlyName = "0x58 New Device Paring";

            Buffer.BlockCopy(unlockCode, 0, rpNewDeviceParingRequest.Info, 0, 10);

            rpNewDeviceParingRequest.CompileCommand();

            RpResponseBase rpNewDeviceParingResponse = new RpResponseBase(rpNewDeviceParingRequest.Execute());

            return rpNewDeviceParingResponse;
        }

        /// <summary>
        /// 0x57 NEW DEVICE GET CODE
        /// This command is used to obtain a code that will be used into a program (Key Calc) 
        /// to generate a 10 bytes key that will be used to pair a new device connected.
        /// </summary>
        /// <returns>RpResponseCommissioning</returns>
        public RpResponseCommissioning RP_NewDeviceGetCode()
        {
            RpCommand rpNewDeviceGetCodeRequest = new RpCommand(this);

            rpNewDeviceGetCodeRequest.UPTAddress = 0x00;
            rpNewDeviceGetCodeRequest.CommandId = 0x57;
            rpNewDeviceGetCodeRequest.InfoLength = 0;
            rpNewDeviceGetCodeRequest.FriendlyName = "0x57 New Device Get Code Pairing Request";

            rpNewDeviceGetCodeRequest.CompileCommand();

            RpResponseCommissioning rpNewDeviceGetCodeResponse =
                new RpResponseCommissioning(rpNewDeviceGetCodeRequest.Execute());

            return rpNewDeviceGetCodeResponse;
        }

        /// <summary>
        /// 0x5A COMMISSIONING LOCK
        /// This command is used to lock the board, changing its status from Active to Violated.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_CommissioningLock()
        {
            RpCommand RpCommissioningLockRequest = new RpCommand(this);

            RpCommissioningLockRequest.UPTAddress = 0x00;
            RpCommissioningLockRequest.CommandId = 0x5A;
            RpCommissioningLockRequest.InfoLength = 0;
            RpCommissioningLockRequest.FriendlyName = "0x5A Commissioning Lock Request";

            RpCommissioningLockRequest.CompileCommand();

            RpResponseBase rpCommissioningLockResponse = new RpResponseBase(RpCommissioningLockRequest.Execute());

            return rpCommissioningLockResponse;
        }

        /// <summary>
        /// 0x66 ONE STEP GET CODE
        /// This command is used to generate a code necessary for the start of the anti-removal unlock and pairing operation.
        /// The generated code will be insert into another program (Key Calc) to generate a 
        /// 10 byte ascii key that will be used for the anti-removal unlock and pairing unlock command.
        /// </summary>
        /// <returns>RpResponseCommissioning</returns>
        public RpResponseCommissioning RP_NewDeviceGetCodePairingAndAntiRemoval()
        {
            RpCommand rpNewDeviceGetCodeRequest = new RpCommand(this);

            rpNewDeviceGetCodeRequest.UPTAddress = 0x00;
            rpNewDeviceGetCodeRequest.CommandId = 0x66;
            rpNewDeviceGetCodeRequest.InfoLength = 0;
            rpNewDeviceGetCodeRequest.FriendlyName = "0x66 New Device Get Code Pairing and AntiRemoval Request";

            rpNewDeviceGetCodeRequest.CompileCommand();

            RpResponseCommissioning rpNewDeviceGetCodeResponse =
                new RpResponseCommissioning(rpNewDeviceGetCodeRequest.Execute());

            return rpNewDeviceGetCodeResponse;
        }

        /// <summary>
        ///0x67 ONE STEP UNLOCK CODE
        /// This command is used to unlock the anti‐removals and if necessary, execute the pairing operation.
        /// The PU send to the UPT the unlock key, the UPT verify the key, then if the key is correct and the anti‐removals are closed, the UPT starts the unlock operations.
        /// </summary>
        /// <param name="unlockCode">Unlock key</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_NewDeviceCodePairingAndAntiRemoval(byte[] unlockCode)
        {
            RpCommand rpNewDeviceParingAndAntiRemovalRequest = new RpCommand(this);

            if (unlockCode.Length != 10)
            {
                throw new Exception("Illegal Unlock Code Size!");
            }

            rpNewDeviceParingAndAntiRemovalRequest.UPTAddress = 0x00;
            rpNewDeviceParingAndAntiRemovalRequest.CommandId = 0x67;
            rpNewDeviceParingAndAntiRemovalRequest.InfoLength = 10;
            rpNewDeviceParingAndAntiRemovalRequest.FriendlyName = "0x67 New Device Paring and AntiRemoval";

            Buffer.BlockCopy(unlockCode, 0, rpNewDeviceParingAndAntiRemovalRequest.Info, 0, 10);

            rpNewDeviceParingAndAntiRemovalRequest.CompileCommand();

            RpResponseBase rpNewDeviceParingAndAntiRemovalResponse =
                new RpResponseBase(rpNewDeviceParingAndAntiRemovalRequest.Execute());

            return rpNewDeviceParingAndAntiRemovalResponse;
        }

        /// <summary>
        /// 0x56 MASTER GET INFO
        /// This command is used to obtain information about the status of the master.
        /// </summary>
        /// <returns>RpResponseDeviceInfo</returns>
        public RpResponseDeviceInfo RP_MasterGetInfo()
        {
            RpCommand rpMasterGetInfoRequest = new RpCommand(this);

            rpMasterGetInfoRequest.UPTAddress = 0x00;
            rpMasterGetInfoRequest.CommandId = 0x56;
            rpMasterGetInfoRequest.InfoLength = 0;
            rpMasterGetInfoRequest.FriendlyName = "0x56 Master Get Info Request";

            rpMasterGetInfoRequest.CompileCommand();

            RpResponseDeviceInfo rpMasterGetInfoResponse = new RpResponseDeviceInfo(rpMasterGetInfoRequest.Execute());

            return rpMasterGetInfoResponse;
        }

        /// <summary>
        /// 0x00 - Extended command - 0x56 HRT1000 GET INFO
        /// This command is used to obtain information about the status of the master.
        /// </summary>
        /// <returns>RpResponseHRT1000DeviceInfo</returns>
        public RpResponseHRT1000DeviceInfo RP_HRT1000DeviceGetInfo()
        {
            RpCommand rpMasterGetInfoRequest = new RpCommand(this);

            rpMasterGetInfoRequest.UPTAddress = 0x00;
            rpMasterGetInfoRequest.CommandId = 0x00;
            rpMasterGetInfoRequest.InfoLength = 1;
            rpMasterGetInfoRequest.Info[0] = 0x56;
            rpMasterGetInfoRequest.FriendlyName = "Ext-0x56 HRT1000 Get Info Request";

            rpMasterGetInfoRequest.CompileCommand();

            RpResponseHRT1000DeviceInfo rpHRT1000GetInfoResponse = new RpResponseHRT1000DeviceInfo(rpMasterGetInfoRequest.Execute());

            return rpHRT1000GetInfoResponse;
        }



        /// <summary>
        /// 0x55 COMMISSIONING UNLOCK
        /// This command is used to unlock the board, changing its status from Violated to active.
        /// The PU send to the UPT the unlock key, the UPT verify the key, then if the key is correct and the sensors are close,
        ///  the UPT starts the unlock operations.
        /// </summary>
        /// <param name="unlockCode">Unlock key</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_CommissioningUnlock(byte[] unlockCode)
        {
            RpCommand rpCommissioningUnlockRequest = new RpCommand(this);

            if (unlockCode.Length != 10)
            {
                throw new Exception("Illegal Unlock Code Size!");
            }

            rpCommissioningUnlockRequest.UPTAddress = 0x00;
            rpCommissioningUnlockRequest.CommandId = 0x55;
            rpCommissioningUnlockRequest.InfoLength = 10;
            rpCommissioningUnlockRequest.FriendlyName = "0x55 Commissioning Unlock";

            Buffer.BlockCopy(unlockCode, 0, rpCommissioningUnlockRequest.Info, 0, 10);

            rpCommissioningUnlockRequest.CompileCommand();

            RpResponseBase rpCommissioningUnlockResponse = new RpResponseBase(rpCommissioningUnlockRequest.Execute());

            return rpCommissioningUnlockResponse;
        }

        /// <summary>
        /// Extended 0x55 COMMISSIONING UNLOCK
        /// This command is used to unlock the board, changing its status from Violated to active.
        /// The PU send to the UPT the unlock key, the UPT verify the key, then if the key is correct and the sensors are close,
        ///  the UPT starts the unlock operations.
        /// </summary>
        /// <param name="unlockCode">Unlock key</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_EXT_CommissioningUnlock(byte[] unlockCode)
        {
            RpCommand rpCommissioningUnlockRequest = new RpCommand(this);

            if (unlockCode.Length != 24)
            {
                throw new Exception("Illegal Unlock Code Size!");
            }

            rpCommissioningUnlockRequest.UPTAddress = 0x00;
            rpCommissioningUnlockRequest.CommandId = 0x00;
            rpCommissioningUnlockRequest.InfoLength = 24 + 1;
            rpCommissioningUnlockRequest.Info[0] = 0x55;
            rpCommissioningUnlockRequest.FriendlyName = "Ext-0x55 Commissioning Unlock";

            Buffer.BlockCopy(unlockCode, 0, rpCommissioningUnlockRequest.Info, 1, 24);

            rpCommissioningUnlockRequest.CompileCommand();

            RpResponseBase rpCommissioningUnlockResponse = new RpResponseBase(rpCommissioningUnlockRequest.Execute());

            return rpCommissioningUnlockResponse;
        }

        /// <summary>
        /// 0x54 COMMISSIONING GET CODE
        /// This command is used to generate a code necessary for the start of the commissioning operation.
        /// The generated code will be insert into another program (Key Calc) to generate a 10 byte ascii key that will be used for the commissioning unlock command.
        /// </summary>
        /// <returns>RpResponseCommissioning</returns>
        public RpResponseCommissioning RP_CommissioningGetCode()
        {
            RpCommand rpCommissioningGetCodeRequest = new RpCommand(this);

            rpCommissioningGetCodeRequest.UPTAddress = 0x00;
            rpCommissioningGetCodeRequest.CommandId = 0x54;
            rpCommissioningGetCodeRequest.InfoLength = 19;
            rpCommissioningGetCodeRequest.FriendlyName = "0x54 Commissioning Get Code";
            rpCommissioningGetCodeRequest.CompileCommand();

            RpResponseCommissioning rpCommissioningGetCodeResponse =
                new RpResponseCommissioning(rpCommissioningGetCodeRequest.Execute());


            return rpCommissioningGetCodeResponse;
        }

        /// <summary>
        /// Extended 0x54 COMMISSIONING GET CODE
        /// This command is used to generate a code necessary for the start of the commissioning operation.
        /// The generated code will be insert into another program (Key Calc) to generate a 10 byte ascii key that will be used for the commissioning unlock command.
        /// </summary>
        /// <returns>RpResponseCommissioning</returns>
        public RpResponseCommissioningExt RP_EXT_CommissioningGetCode()
        {
            RpCommand rpCommissioningGetCodeRequest = new RpCommand(this);

            rpCommissioningGetCodeRequest.UPTAddress = 0x00;
            rpCommissioningGetCodeRequest.CommandId = 0x00;
            rpCommissioningGetCodeRequest.InfoLength = 1;
            rpCommissioningGetCodeRequest.Info[0] = 0x54;
            rpCommissioningGetCodeRequest.FriendlyName = "Ext-0x54 Commissioning Get Code";
            rpCommissioningGetCodeRequest.CompileCommand();

            RpResponseCommissioningExt rpCommissioningGetCodeResponse =
                new RpResponseCommissioningExt(rpCommissioningGetCodeRequest.Execute());


            return rpCommissioningGetCodeResponse;
        }

        /// <summary>
        /// 0xB2 READ THE CONFIGURATION OF THE MDB PARAMETERS
        /// This command is used to read the parameters of the MDB.
        /// </summary>
        /// <returns>rpResponseMDBParams </returns>
        [Obsolete("RP_ReadMdbConfiguration is deprecated, and not used.", false)]
        public RpResponseMdbParams RP_ReadMdbConfiguration()
        {
            RpCommand rpReadMdbConfigurationRequest = new RpCommand(this);

            rpReadMdbConfigurationRequest.UPTAddress = 0x00;
            rpReadMdbConfigurationRequest.CommandId = 0xB2;
            rpReadMdbConfigurationRequest.InfoLength = 0;
            rpReadMdbConfigurationRequest.FriendlyName = "0xB2 Read MDB Configuration Parameters";

            rpReadMdbConfigurationRequest.CompileCommand();
            RpResponseMdbParams rpReadMdbConfigurationResponse =
                new RpResponseMdbParams(rpReadMdbConfigurationRequest.Execute());

            return rpReadMdbConfigurationResponse;
        }

        /// <summary>
        /// 0xB1 READ IP CONFIGURATION
        /// This command is used to read IP and port used to execute the retailer protocol with one socket in LAN connection (the UPT is client).
        /// </summary>
        /// <returns>RpResponseIPAddress</returns>
        public RpResponseIPAddress RP_ReadIPConfiguration()
        {
            RpCommand rpReadIpConfigurationRequest = new RpCommand(this);

            rpReadIpConfigurationRequest.UPTAddress = 0x00;
            rpReadIpConfigurationRequest.CommandId = 0xB1;
            rpReadIpConfigurationRequest.InfoLength = 0;
            rpReadIpConfigurationRequest.FriendlyName = "0xB1 Read IP Configuration";

            rpReadIpConfigurationRequest.CompileCommand();

            RpResponseIPAddress rpReadIpConfigurationResponse =
                new RpResponseIPAddress(rpReadIpConfigurationRequest.Execute());

            return rpReadIpConfigurationResponse;
        }

        /// <summary>
        /// 0xB0 READ BANKING PARAMETERS
        /// This command is used to read the banking parameters configured for the connection of the UPT to the banking host.
        /// </summary>
        /// <param name="tag">Enumeration of the config param to return</param>
        /// <returns>RpResponseBankingParameters</returns>
        public RpResponseBase RP_ReadBankingParameters(BankingParameter param)
        {
            RpCommand rpReadBankingParametersRequest = new RpCommand(this);

            rpReadBankingParametersRequest.UPTAddress = 0x00;
            rpReadBankingParametersRequest.CommandId = 0xB0;
            rpReadBankingParametersRequest.InfoLength = 1;
            rpReadBankingParametersRequest.Info[0] = param.tag;
            rpReadBankingParametersRequest.FriendlyName = "0xB0 Read Banking Parameters";

            rpReadBankingParametersRequest.CompileCommand();
            RpResponseBase rpReadBankingParametersResponse =
                new RpResponseBase(rpReadBankingParametersRequest.Execute());

            return rpReadBankingParametersResponse;
        }

        /// <summary>
        /// 0xB0 READ BANKING PARAMETERS
        /// This command is used to read the banking parameters configured for the connection of the UPT to the banking host.
        /// </summary>
        /// <param name="tag">Enumeration of the config param to return</param>
        /// <returns>RpResponseBankingParam class</returns>
        public RpResponseBankingParam RP_ReadBankingParametersV2(BankingParameter param)
        {
            RpCommand rpReadBankingParametersRequest = new RpCommand(this);

            rpReadBankingParametersRequest.UPTAddress = 0x00;
            rpReadBankingParametersRequest.CommandId = 0xB0;
            rpReadBankingParametersRequest.InfoLength = 1;
            rpReadBankingParametersRequest.Info[0] = param.tag;
            rpReadBankingParametersRequest.FriendlyName = "0xB0 Read Banking Parameters";

            rpReadBankingParametersRequest.CompileCommand();
            RpResponseBankingParam rpReadBankingParametersResponse =
                new RpResponseBankingParam(param.tag, rpReadBankingParametersRequest.Execute(), DeviceFamily == null ? EDeviceFamily.PaymentDevice : (EDeviceFamily)DeviceFamily);

            return rpReadBankingParametersResponse;
        }

        /// <summary>
        /// Filter through all the Bank Parameters and create a printable string.
        /// </summary>
        [Obsolete("RP_PrintBankingParameters is deprecated, please use RP_GetBankingParameters instead.", false)]
        public string RP_PrintBankingParameters()
        {
            StringBuilder sb = new StringBuilder(1000);

            if (DeviceFamily == EDeviceFamily.HRT1000)
            {
                sb.AppendLine("HRT Terminal and Banking Configuration:");
                foreach (EBankingParamsHRT parm in Enum.GetValues(typeof(EBankingParamsHRT)))
                {
                    BankingParameter bankParm = new BankingParameter((byte)parm, EDeviceFamily.HRT1000);

                    RpResponseBase rpBankParamResponse = RP_ReadBankingParameters(bankParm);
                    if (rpBankParamResponse.VerifyOutcome() != EOutcome.OK)
                    {
                        sb.AppendLine("-->{" + bankParm.GetTagAsHexString() + "}" + bankParm.GetTagDescription() + ":" + "Error. Outcome:" + rpBankParamResponse.VerifyOutcome().ToString());
                        continue;
                    }

                    sb.AppendLine("-->{" + bankParm.GetTagAsHexString() + "}" + bankParm.GetTagDescription() + ":" + bankParm.ConvertValueToString(rpBankParamResponse.Info));
                }
            }
            else
            {
                sb.AppendLine("BV Terminal and Banking Configuration:");
                foreach (EBankingParamsBV parm in Enum.GetValues(typeof(EBankingParamsBV)))
                {
                    BankingParameter bankParm = new BankingParameter((byte)parm, EDeviceFamily.BV1000);

                    RpResponseBase rpBankParamResponse = RP_ReadBankingParameters(bankParm);
                    if (rpBankParamResponse.VerifyOutcome() != EOutcome.OK)
                    {
                        sb.AppendLine("-->{" + bankParm.GetTagAsHexString() + "}" + bankParm.GetTagDescription() + ":" + "Error. Outcome:" + rpBankParamResponse.VerifyOutcome().ToString());
                        continue;
                    }

                    sb.AppendLine("-->{" + bankParm.GetTagAsHexString() + "}" + bankParm.GetTagDescription() + ":" + bankParm.ConvertValueToString(rpBankParamResponse.Info));
                }
            }

            sb.AppendLine(Environment.NewLine);
            return sb.ToString();
        }

        /// <summary>
        /// Filter through all the Bank Parameters and create a printable xml string.
        /// Supresses and RFU fields
        /// </summary>
        public RpResponseBankingParameters RP_GetBankingParameters(bool bSupportTwoApps = true)
        {
            RpResponseBankingParameters rpResponse = new RpResponseBankingParameters(DeviceFamily == null? EDeviceFamily.PaymentDevice : (EDeviceFamily)DeviceFamily);

            if (DeviceFamily == EDeviceFamily.HRT1000)
            {
                foreach (EBankingParamsHRT parm in Enum.GetValues(typeof(EBankingParamsHRT)))
                {
                    if (parm.ToString().Contains("RFU"))
                        continue;

                    RpResponseBase rpBankParamResponse = RP_ReadBankingParameters(new BankingParameter((byte)parm, EDeviceFamily.HRT1000));
                    rpResponse.Add((byte)parm, rpBankParamResponse);
                }
            }
            else
            {
                foreach (EBankingParamsBV parm in Enum.GetValues(typeof(EBankingParamsBV)))
                {
                    if (parm.ToString().Contains("RFU"))
                        continue;
                    if (!bSupportTwoApps && parm == EBankingParamsBV.TD2_TerminalID)
                        break;

                    RpResponseBase rpBankParamResponse = RP_ReadBankingParameters(new BankingParameter((byte)parm, EDeviceFamily.BV1000));
                    rpResponse.Add((byte)parm, rpBankParamResponse);
                }
            }
            return rpResponse;
        }

        /// <summary>
        /// 0xC0 DELETE KEYS COMMAND REQUEST STEP
        /// This command is used to delete the application keys, the random key and the RSA key.At the end
        /// of the erase process, BV1000 goes in removal state.
        /// The execution of the command is composed by two steps: the first generates a code that will be
        /// sent back to the terminal to complete the erase process (second step).
        /// </summary>
        /// <returns>RpResponseEraseKeys</returns>
        public RpResponseEraseKeys RP_EraseKeysReq()
        {
            RpCommand rpEraseKeysRequest = new RpCommand(this);

            rpEraseKeysRequest.UPTAddress = 0x00;
            rpEraseKeysRequest.CommandId = 0xC0;
            rpEraseKeysRequest.InfoLength = 0;
            rpEraseKeysRequest.FriendlyName = "0xC0 Delete Keys Command Request";

            rpEraseKeysRequest.CompileCommand();
            RpResponseEraseKeys rpEraseKeysReqResponse = new RpResponseEraseKeys(rpEraseKeysRequest.Execute());

            return rpEraseKeysReqResponse;
        }

        /// <summary>
        /// 0xC0 DELETE KEYS COMMAND CONFIRM STEP
        /// This command is used to delete the application keys, the random key and the RSA key.At the end
        /// of the erase process, BV1000 goes in removal state.
        /// The execution of the command is composed by two steps: the first generates a code that will be
        /// sent back to the terminal to complete the erase process (second step).
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public RpResponseBase Rp_EraseKeysConfirm(byte[] random)
        {

            if (random.Length != 4)
            {
                throw new Exception("0xC0 Delete key confirmation requires a 4 byte confirmation.");
            }

            RpCommand rpEraseKeysConfirmRequest = new RpCommand(this);

            rpEraseKeysConfirmRequest.UPTAddress = 0x00;
            rpEraseKeysConfirmRequest.CommandId = 0xC0;
            rpEraseKeysConfirmRequest.InfoLength = 4;
            rpEraseKeysConfirmRequest.Info = new byte[4];
            Buffer.BlockCopy(random, 0, rpEraseKeysConfirmRequest.Info, 0, 4);
            rpEraseKeysConfirmRequest.FriendlyName = "0xC0 Delete Keys Command Confirmation Request";

            rpEraseKeysConfirmRequest.CompileCommand();
            RpResponseEraseKeys rpEraseKeysReqResponse = new RpResponseEraseKeys(rpEraseKeysConfirmRequest.Execute());

            return rpEraseKeysReqResponse;
        }


        /// <summary>
        /// 0xC1 GET DATE TIME LAST DELETE KEYS COMMAND
        /// This command is used to view the date and the hour of the last execution of delete keys command.
        /// </summary>
        /// <returns>RpResponseDateTime</returns>
        public RpResponseDateTime2 RP_GetTimeDateofEraseKeys()
        {
            RpCommand rpGetTimeDateofEraseKeysRequest = new RpCommand(this);

            rpGetTimeDateofEraseKeysRequest.UPTAddress = 0x00;
            rpGetTimeDateofEraseKeysRequest.CommandId = 0xC1;
            rpGetTimeDateofEraseKeysRequest.InfoLength = 0;
            rpGetTimeDateofEraseKeysRequest.FriendlyName = "0xC1 Get Time and Date of last Delete Keys Command";

            rpGetTimeDateofEraseKeysRequest.CompileCommand();
            RpResponseDateTime2 rpEraseGetTimeDateKeysResponse =
                new RpResponseDateTime2(rpGetTimeDateofEraseKeysRequest.Execute());

            return rpEraseGetTimeDateKeysResponse;
        }


        /// <summary>
        /// 0xAF CONFIGURATION OF THE MDB PARAMETERS
        /// This command is used to configure the parameters of the MDB.
        /// </summary>
        /// <param name="cashless">Cashless number: 0x01 -> cashless 1 0x02 -> cashless 2</param>
        /// <param name="foundInEurocent">Found in eurocent</param>
        /// <param name="mDbMaxWaitTime">Max time of answer for the MDB commands</param>
        /// <param name="vendMaxTime">Max time of answer between vend request and vend approved</param>
        /// <param name="saleType">Sale type: 0x01 -> single sale 0x02 -> multiple sale</param>
        /// <returns>RpResponseBase</returns>
        [Obsolete("RP_ConfigurationOfTheMdbParameters is deprecated, and not used.", false)]
        public RpResponseBase RP_ConfigurationOfTheMdbParameters(byte cashless, int foundInEurocent, int mDbMaxWaitTime,
            int vendMaxTime, byte saleType)
        {
            RpCommand rpConfigurationOfTheMdbParametersRequest = new RpCommand(this);

            if (foundInEurocent < 0)
            {
                foundInEurocent = 0;
            }

            if (mDbMaxWaitTime > 255)
            {
                mDbMaxWaitTime = 255;
            }
            if (mDbMaxWaitTime < 0)
            {
                mDbMaxWaitTime = 0;
            }

            if (vendMaxTime > 255)
            {
                vendMaxTime = 255;
            }
            if (vendMaxTime < 0)
            {
                vendMaxTime = 0;
            }

            rpConfigurationOfTheMdbParametersRequest.UPTAddress = 0x00;
            rpConfigurationOfTheMdbParametersRequest.CommandId = 0xAF;

            rpConfigurationOfTheMdbParametersRequest.InfoLength = 8;
            rpConfigurationOfTheMdbParametersRequest.Info[0] = cashless;
            Buffer.BlockCopy(BytesFromInt(foundInEurocent, 4), 0, rpConfigurationOfTheMdbParametersRequest.Info, 1, 4);
            Buffer.BlockCopy(BytesFromInt(mDbMaxWaitTime, 1), 0, rpConfigurationOfTheMdbParametersRequest.Info, 5, 1);
            Buffer.BlockCopy(BytesFromInt(vendMaxTime, 1), 0, rpConfigurationOfTheMdbParametersRequest.Info, 6, 1);
            rpConfigurationOfTheMdbParametersRequest.Info[7] = saleType;
            rpConfigurationOfTheMdbParametersRequest.FriendlyName = "0xAF Configuration of MDB Parameters";

            rpConfigurationOfTheMdbParametersRequest.CompileCommand();

            RpResponseBase rpConfigurationOfTheMdbParametersResponse =
                new RpResponseBase(rpConfigurationOfTheMdbParametersRequest.Execute());


            return rpConfigurationOfTheMdbParametersResponse;

        }


        /// <summary>
        /// 0xC2 ACTIVATE/DEACTIVATE RETAIL PROTOCOL ON ETHERNET
        /// This command is used to tell the reader to connect via ethernet.  dependent on 0xAE commdna.
        /// </summary>
        /// <param name="bActivate">true of false</param>
        /// <param name="bType">0x00 UPT as Client, 0x01 UPT as TLS Client, 0x02 UPT as Server, 0x03 UPT as Server</param>
        /// <param name="portNumber">Port to listen on when directed to be a Server</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_ActivateOrDeactivateProtocolOnEthernet(bool bActivate, byte bType = 0x00, int portNumber = 0)
        {
            RpCommand rpActOrDeactRPonEthernet = new RpCommand(this);

            rpActOrDeactRPonEthernet.UPTAddress = 0x00;
            rpActOrDeactRPonEthernet.CommandId = 0xC2;

            // If deactivate only that needs to be sent
            if (!bActivate)
            {
                rpActOrDeactRPonEthernet.InfoLength = 1;
                rpActOrDeactRPonEthernet.Info[0] = 0x00;
            }
            // If Activate send type and opt port
            else
            {
                rpActOrDeactRPonEthernet.InfoLength = 2;
                rpActOrDeactRPonEthernet.Info[0] = 0x01;
                rpActOrDeactRPonEthernet.Info[1] = bType;

                // Assuming if port came in it should be sent bType 
                if (portNumber > 0)
                {
                    rpActOrDeactRPonEthernet.FriendlyName =
                        "0xC2 " + (bActivate ? "Activate" : "Deactivate") + " Retail Protocol On Ethernet as Server";
                    rpActOrDeactRPonEthernet.InfoLength = 7;

                    Buffer.BlockCopy(StringToByteArray(portNumber.ToString(), 5), 0,
                        rpActOrDeactRPonEthernet.Info, 2, 5);
                }
                else
                {
                    rpActOrDeactRPonEthernet.FriendlyName =
                        "0xC2 " + (bActivate ? "Activate" : "Deactivate") + " Retail Protocol On Ethernet to Clinet";
                }
            }

            rpActOrDeactRPonEthernet.CompileCommand();

            RpResponseBase rpActOrDeactRPonEthernetResponse =
                new RpResponseBase(rpActOrDeactRPonEthernet.Execute());

            return rpActOrDeactRPonEthernetResponse;
        }

        /// <summary>
        /// 0xAE IP CONFIGURATION FOR RETAIL PROTOCOL ON ETHERNET
        /// This command is used to configure IP and port to execute the retailer protocol with one socket in LAN connection (the UPT is client).
        /// </summary>
        /// <param name="kioskIpAddress">IP address PC (filled to right with 0x00)</param>
        /// <param name="portNumber">PC Port (filled to right with 0x00)</param>
        /// <param name="pPpEnabled">Enable the serial to be used as PPP channel</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_ConfigurationForRetailProtocolOnEthernet(string kioskIpAddress, int portNumber,
            bool pPpEnabled)
        {
            RpCommand rpConfigurationForRetailProtocolOnEthernetRequest = new RpCommand(this);

            rpConfigurationForRetailProtocolOnEthernetRequest.UPTAddress = 0x00;
            rpConfigurationForRetailProtocolOnEthernetRequest.CommandId = 0xAE;

            rpConfigurationForRetailProtocolOnEthernetRequest.InfoLength = 21;
            Buffer.BlockCopy(StringToByteArray(kioskIpAddress, 15), 0,
                rpConfigurationForRetailProtocolOnEthernetRequest.Info, 0, 15);
            Buffer.BlockCopy(StringToByteArray(portNumber.ToString(), 5), 0,
                rpConfigurationForRetailProtocolOnEthernetRequest.Info, 15, 5);

            if (pPpEnabled)
            {
                rpConfigurationForRetailProtocolOnEthernetRequest.Info[20] = 0x01;
            }
            else
            {
                rpConfigurationForRetailProtocolOnEthernetRequest.Info[20] = 0x00;
            }
            rpConfigurationForRetailProtocolOnEthernetRequest.FriendlyName =
                "0xAE Configuration for the Retail Protocol on Ethernet";
            rpConfigurationForRetailProtocolOnEthernetRequest.CompileCommand();

            RpResponseBase rpConfigurationForRetailProtocolOnEthernetResponse =
                new RpResponseBase(rpConfigurationForRetailProtocolOnEthernetRequest.Execute());

            return rpConfigurationForRetailProtocolOnEthernetResponse;
        }

        /// <summary>
        /// 0xAD BANKING PARAMETERS CONFIGURATION
        /// This command is used to configure the banking parameters for the connection of the UPT to the banking host.
        /// </summary>
        /// <param name="sync">Sync (1=last conf. packet, write conf; 0=there are still packages)</param>
        /// <param name="tagId">The Tag of the Parameter to Modify</param>
        /// <param name="data">The value you wish to assign to the tag</param>
        /// <returns>RpResponseBase</returns>        
        public RpResponseBase RP_BankingParametersConfiguration(bool sync, byte tagId, byte[] data)
        {
            RpCommand rpBankingParametersConfigurationRequest = new RpCommand(this);

            rpBankingParametersConfigurationRequest.UPTAddress = 0x00;
            rpBankingParametersConfigurationRequest.CommandId = 0xAD;

            rpBankingParametersConfigurationRequest.Info[0] = (sync) ? (byte) 0x01 : (byte) 0x00;
            rpBankingParametersConfigurationRequest.Info[1] = tagId;


            if (data == null)
            {
                rpBankingParametersConfigurationRequest.InfoLength = 3;
                rpBankingParametersConfigurationRequest.Info[2] = 0x00;
            }
            else
            {
                rpBankingParametersConfigurationRequest.InfoLength = (ushort) (2 + data.Length);
                Buffer.BlockCopy(data, 0, rpBankingParametersConfigurationRequest.Info, 2, data.Length);
            }


            rpBankingParametersConfigurationRequest.CompileCommand();
            rpBankingParametersConfigurationRequest.FriendlyName = "0xAD Banking configuration";

            RpResponseBase rpBankingParametersConfigurationResponse =
                new RpResponseBase(rpBankingParametersConfigurationRequest.Execute());

            return rpBankingParametersConfigurationResponse;
        }




        /// <summary>
        /// 0x53 READ SERIAL NUMBER
        /// This command is used to read the serial number of the UPT master and of all the UPT slaves.
        /// </summary>
        /// <returns>RpResponseSerialNumbers</returns>
        public RpResponseSerialNumbers RP_ReadSerialNumber()
        {
            RpCommand rpReadSerialNumberRequest = new RpCommand(this);

            rpReadSerialNumberRequest.UPTAddress = 0x00;
            rpReadSerialNumberRequest.CommandId = 0x53;
            rpReadSerialNumberRequest.InfoLength = 0;
            rpReadSerialNumberRequest.FriendlyName = "0x53 Read Serial Number";

            rpReadSerialNumberRequest.CompileCommand();

            RpResponseSerialNumbers rpReadSerialNumberResponse =
                new RpResponseSerialNumbers(rpReadSerialNumberRequest.Execute());

            return rpReadSerialNumberResponse;
        }

        /// <summary>
        /// Null Test ETH returns:
        /// 
        /// LINK UP: NO
        //ADDRESS TYPE: DHCP
        //IP ADDRESS: 0.0.0.0
        //NETMASK: 0.0.0.0
        //GATEWAY: 0.0.0.0
        //DNS1: 8.8.8.8
        //DNS2: 0.0.0.0
        //MAC ADDRESS: 9C:69:B4:40:AD:9F
        //PING error
        /// </summary>
        /// <returns></returns>
        public string RP_GetMACAddress(bool withColons)
        {
            string ret = "{Failed}";
            RpCommand rpTestEth = new RpCommand(this);

            rpTestEth.UPTAddress = 0x00;
            rpTestEth.CommandId = 0x03;
            rpTestEth.InfoLength = 0;
            rpTestEth.FriendlyName = "0x03 Test Ethernet - Get MAC";

            rpTestEth.CompileCommand();
            try
            {
                RpResponseBase rpTestEthResponse = new RpResponseBase(rpTestEth.Execute());
                if (rpTestEthResponse.VerifyOutcome() == EOutcome.OK)
                {
                    string resp = Encoding.ASCII.GetString(rpTestEthResponse.Response);

                    int nPos = resp.LastIndexOf("MAC ADDRESS: ");
                    if (nPos >= 0)
                    {
                        ret = resp.Substring(nPos + 13, 17);
                        if (false == withColons)
                        {
                            ret = ret.Replace(":", string.Empty);
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                //swallow me
            }
            return ret;
        }

        /// <summary>
        /// 0x03 TEST ETH
        /// This command is used to check the ETH configuration and perform a ping to a selected ip address
        /// </summary>
        /// <param name="ipAddress">Address to ping</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_TestEth(string ipAddress)
        {
            byte[] iptest = Encoding.ASCII.GetBytes(ipAddress);
            RpCommand rpTestEth = new RpCommand(this);

            rpTestEth.UPTAddress = 0x00;
            rpTestEth.CommandId = 0x03;
            rpTestEth.InfoLength = (byte) iptest.Length;
            Buffer.BlockCopy(iptest, 0, rpTestEth.Info, 0, iptest.Length);
            rpTestEth.FriendlyName = "0x03 Test Ethernet";

            rpTestEth.CompileCommand();

            RpResponseBase rpTestEthResponse = new RpResponseBase(rpTestEth.Execute());

            return rpTestEthResponse;
        }

        /// <summary>
        /// 0x4D Notification
        /// This command is used  to delete a preauthorization slot. 
        /// </summary>
        /// <param name="slotIndex">SlotIndex</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_Notification(int slotIndex)
        {
            RpCommand rpNotification = new RpCommand(this);

            rpNotification.UPTAddress = 0x00;
            rpNotification.CommandId = 0x4D;
            rpNotification.InfoLength = 1;
            rpNotification.Info[0] = (byte) slotIndex;
            rpNotification.FriendlyName = "0x4D Delete Preauthorization Slot";


            rpNotification.CompileCommand();

            RpResponseBase rpNotificationResponse = new RpResponseBase(rpNotification.Execute());

            return rpNotificationResponse;
        }

        /// <summary>
        /// 0x52 Get Notify
        /// This command is used to retrieve the details needed to Notify via controller
        /// </summary>
        /// <param name="slotId">SlotIndex</param>
        /// <param name="amountInPennies">amount to complete</param>
        /// <param name="petroProductDetails">Petro product details</param>
        /// <returns>RpSlotDetails</returns>
        public RpResponseGetNotify RP_GetNotify(byte slotId, int amountInPennies, List<PetroProductDetails> petroProductDetails)
        {
            RpCommand rpNotifyCommandRequest = new RpCommand(this);

            rpNotifyCommandRequest.UPTAddress = 0x00;
            rpNotifyCommandRequest.CommandId = 0x52;

            rpNotifyCommandRequest.Info[0] = slotId;
            rpNotifyCommandRequest.Info[1] = (byte)(amountInPennies >> 24);
            rpNotifyCommandRequest.Info[2] = (byte)(amountInPennies >> 16);
            rpNotifyCommandRequest.Info[3] = (byte)(amountInPennies >> 8);
            rpNotifyCommandRequest.Info[4] = (byte)amountInPennies;
            rpNotifyCommandRequest.FriendlyName = "0x52 Notify Command Request";

            PopulatePetroProducts(petroProductDetails, ref rpNotifyCommandRequest);
            rpNotifyCommandRequest.CompileCommand();

            RpResponseGetNotify rpSlotDetails = new RpResponseGetNotify(rpNotifyCommandRequest.Execute());
            return rpSlotDetails;
        }

        /// <summary>
        /// Encode the Petro Product Details into "Product TLV Data"
        /// </summary>
        /// <param name="petroProductDetails">Petro product details</param>
        /// <param name="rpCmdReq">command object being built up</param>
        public void PopulatePetroProducts(List<PetroProductDetails> petroProductDetails, ref RpCommand rpCmdReq)
        {
            //Create TLV for Petro product details
            ushort pos = 5;
            ushort pdLength = 0;
            foreach (PetroProductDetails p in petroProductDetails)
            {
                //UnitPrice = 51, 4.3 Decimal
                //Quantity = 52, 4.5 Decimal
                //ProductCode = 53, 2-6 String
                //Amount = 54, 4.3 Decimal

                if (null != p.UnitPrice)
                {
                    rpCmdReq.Info[pos++] = (byte)EPetroProductDetailTags.UnitPrice;
                    //pdLength = (ushort)Encoding.ASCII.GetBytes(p.UnitPrice.ToString("0000.000",CultureInfo.CreateSpecificCulture("en-US"))).Length;
                    pdLength = (ushort)Encoding.ASCII
                        .GetBytes(((decimal)p.UnitPrice).ToString("N3", CultureInfo.CreateSpecificCulture("en-US"))).Length;
                    if (pdLength > 255) pdLength = 255;
                    rpCmdReq.Info[pos++] = (byte)pdLength;
                    Buffer.BlockCopy(
                        Encoding.ASCII.GetBytes(((decimal)p.UnitPrice).ToString("N3", CultureInfo.CreateSpecificCulture("en-US"))),
                        0, rpCmdReq.Info, pos, pdLength);
                    pos += pdLength;
                }

                if (null != p.Quantity)
                {
                    rpCmdReq.Info[pos++] = (byte)EPetroProductDetailTags.QuantityNBS;
                    pdLength = (ushort)Encoding.ASCII
                        .GetBytes(((decimal)p.Quantity).ToString("F5", CultureInfo.CreateSpecificCulture("en-US"))).Length;
                    if (pdLength > 255) pdLength = 255;
                    rpCmdReq.Info[pos++] = (byte)pdLength;
                    Buffer.BlockCopy(
                        Encoding.ASCII.GetBytes(((decimal)p.Quantity).ToString("F5", CultureInfo.CreateSpecificCulture("en-US"))),
                        0, rpCmdReq.Info, pos, pdLength);
                    pos += pdLength;


                    rpCmdReq.Info[pos++] = (byte)EPetroProductDetailTags.QuantityCFN;
                    pdLength = (ushort)Encoding.ASCII
                        .GetBytes(((decimal)p.Quantity).ToString("F3", CultureInfo.CreateSpecificCulture("en-US"))).Length;
                    if (pdLength > 255) pdLength = 255;
                    rpCmdReq.Info[pos++] = (byte)pdLength;
                    Buffer.BlockCopy(
                        Encoding.ASCII.GetBytes(((decimal)p.Quantity).ToString("F3", CultureInfo.CreateSpecificCulture("en-US"))),
                        0, rpCmdReq.Info, pos, pdLength);
                    pos += pdLength;
                }

                if (null != p.ProductCode)
                {
                    rpCmdReq.Info[pos++] = (byte)EPetroProductDetailTags.ProductCode;
                    pdLength = (ushort)Encoding.ASCII.GetBytes(p.ProductCode.Trim()).Length;
                    rpCmdReq.Info[pos++] = (byte)pdLength;
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(p.ProductCode.Trim()), 0, rpCmdReq.Info, pos,
                        pdLength);
                    pos += pdLength;
                }

                rpCmdReq.Info[pos++] = (byte)EPetroProductDetailTags.Amount;
                pdLength = (ushort)Encoding.ASCII.GetBytes(p.Amount.ToString("F3", CultureInfo.CreateSpecificCulture("en-US"))).Length;
                if (pdLength > 255) pdLength = 255;
                rpCmdReq.Info[pos++] = (byte)pdLength;
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(p.Amount.ToString("F3", CultureInfo.CreateSpecificCulture("en-US"))), 0, rpCmdReq.Info, pos, pdLength);
                pos += pdLength;

                rpCmdReq.Info[pos++] = (byte)EPetroProductDetailTags.AmountChase;
                pdLength = (ushort)Encoding.ASCII.GetBytes(p.Amount.ToString("F2", CultureInfo.CreateSpecificCulture("en-US"))).Length;
                if (pdLength > 255) pdLength = 255;
                rpCmdReq.Info[pos++] = (byte)pdLength;
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(p.Amount.ToString("F2", CultureInfo.CreateSpecificCulture("en-US"))), 0, rpCmdReq.Info, pos, pdLength);
                pos += pdLength;

                rpCmdReq.Info[pos++] = (byte)EPetroProductDetailTags.PumpNumber;
                pdLength = (ushort)Encoding.ASCII.GetBytes(p.Pump.ToString("00")).Length;
                rpCmdReq.Info[pos++] = (byte)pdLength;
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(p.Pump.ToString("00", CultureInfo.CreateSpecificCulture("en-US"))), 0, rpCmdReq.Info, pos, pdLength);
                pos += pdLength;

                if (null != p.GSTAmount)
                {
                    rpCmdReq.Info[pos++] = (byte)EPetroProductDetailTags.GSTAmount;
                    pdLength = (ushort)Encoding.ASCII
                        .GetBytes(((decimal)p.GSTAmount).ToString("F3", CultureInfo.CreateSpecificCulture("en-US"))).Length;
                    if (pdLength > 255) pdLength = 255;
                    rpCmdReq.Info[pos++] = (byte)pdLength;
                    Buffer.BlockCopy(
                        Encoding.ASCII.GetBytes(((decimal)p.GSTAmount).ToString("F3", CultureInfo.CreateSpecificCulture("en-US"))),
                        0, rpCmdReq.Info, pos, pdLength);
                    pos += pdLength;
                }

                if (null != p.PSTAmount)
                {
                    rpCmdReq.Info[pos++] = (byte)EPetroProductDetailTags.PSTAmount;
                    pdLength = (ushort)Encoding.ASCII
                        .GetBytes(((decimal)p.PSTAmount).ToString("F3", CultureInfo.CreateSpecificCulture("en-US"))).Length;
                    if (pdLength > 255) pdLength = 255;
                    rpCmdReq.Info[pos++] = (byte)pdLength;
                    Buffer.BlockCopy(
                        Encoding.ASCII.GetBytes(((decimal)p.PSTAmount).ToString("F3", CultureInfo.CreateSpecificCulture("en-US"))),
                        0, rpCmdReq.Info, pos, pdLength);
                    pos += pdLength;
                }

                if (!string.IsNullOrEmpty(p.FleetUniqueId))
                {
                    rpCmdReq.Info[pos++] = (byte)EPetroProductDetailTags.FleetUniqueId;
                    pdLength = (ushort)Encoding.ASCII.GetBytes(p.FleetUniqueId.Trim()).Length;
                    rpCmdReq.Info[pos++] = (byte)pdLength;
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(p.FleetUniqueId.Trim()), 0, rpCmdReq.Info, pos,
                        pdLength);
                    pos += pdLength;
                }

                if (null != p.ProductName)
                {
                    rpCmdReq.Info[pos++] = (byte)EPetroProductDetailTags.ProductName;
                    pdLength = (ushort)Encoding.ASCII.GetBytes(p.ProductName.Trim()).Length;
                    rpCmdReq.Info[pos++] = (byte)pdLength;
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(p.ProductName.Trim()), 0, rpCmdReq.Info, pos,
                        pdLength);
                    pos += pdLength;
                }
            }

            rpCmdReq.InfoLength = pos;
        }

        /// <summary>
        /// 0x51 Get Slot Details
        /// This command is used to retrieve details on a slot
        /// </summary>
        /// <param name="slotIndex">SlotIndex</param>
        /// <returns>RpSlotDetails</returns>
        public RpResponseGetSlotDetails RP_GetSlotDetails(int slotIndex)
        {
            RpCommand rpNotification = new RpCommand(this);

            rpNotification.UPTAddress = 0x00;
            rpNotification.CommandId = (byte)ERetailProtocolCommands.GetSlotDetails;
            rpNotification.InfoLength = 1;
            rpNotification.Info[0] = (byte)slotIndex;
            rpNotification.FriendlyName = "0x51 Get Slot Details";

            rpNotification.CompileCommand();

            RpResponseGetSlotDetails rpSlotDetails = new RpResponseGetSlotDetails(rpNotification.Execute());

            return rpSlotDetails;
        }

        /// <summary>
        /// 0x0F RESULT OF LAST REFUND OPERATION
        /// This command return the outcome of the refund.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_ResultOfLastRefundOperation()
        {
            RpCommand rpResultOfLastRefundOperationRequest = new RpCommand(this);

            rpResultOfLastRefundOperationRequest.UPTAddress = 0x00;
            rpResultOfLastRefundOperationRequest.CommandId = 0x0F;
            rpResultOfLastRefundOperationRequest.InfoLength = 0;

            rpResultOfLastRefundOperationRequest.CompileCommand();
            rpResultOfLastRefundOperationRequest.FriendlyName = "0x0F Result Of Last Refund.";

            RpResponseBase rpResultOfLastRefundOperationResponse =
                new RpResponseBase(rpResultOfLastRefundOperationRequest.Execute());

            return rpResultOfLastRefundOperationResponse;
        }

        /// <summary>
        ///0x0C VOID TRANSACTION
        /// This command execute the void operation of the last operation executed.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_VoidLastTransaction()
        {
            RpCommand rpVoidLastTransaction = new RpCommand(this);

            rpVoidLastTransaction.UPTAddress = 0x00;
            rpVoidLastTransaction.CommandId = 0x0C;
            rpVoidLastTransaction.InfoLength = 0;
            rpVoidLastTransaction.FriendlyName = "0x0C Void Last Transaction.";

            rpVoidLastTransaction.CompileCommand();

            RpResponseBase rpVoidLastTransactionResponse = new RpResponseBase(rpVoidLastTransaction.Execute());

            return rpVoidLastTransactionResponse;
        }

        /// <summary>
        /// 0x16 OUTCOME VOID TRANSACTION
        /// This command returns the outcome of the void operation.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_OutcomeOfVoid()
        {
            RpCommand rpOutcomeOfVoid = new RpCommand(this);

            rpOutcomeOfVoid.UPTAddress = 0x00;
            rpOutcomeOfVoid.CommandId = 0x16;
            rpOutcomeOfVoid.InfoLength = 0;
            rpOutcomeOfVoid.FriendlyName = "0x16 Outcome of Voided Transaction.";

            rpOutcomeOfVoid.CompileCommand();

            RpResponseBase rpOutcomeOfVoidResponse = new RpResponseBase(rpOutcomeOfVoid.Execute());

            return rpOutcomeOfVoidResponse;
        }


        /// <summary>
        /// 0x0E REFUND LAST OPERATION
        /// This command execute the refund of the last payment executed.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_RefundLastOperation()
        {
            RpCommand rpRefundLastOperationRequest = new RpCommand(this);

            rpRefundLastOperationRequest.UPTAddress = 0x00;
            rpRefundLastOperationRequest.CommandId = 0x0E;
            rpRefundLastOperationRequest.InfoLength = 0;
            rpRefundLastOperationRequest.FriendlyName = "0x0E Refund Last Operation";

            rpRefundLastOperationRequest.CompileCommand();

            RpResponseBase rpRefundLastOperationResponse = new RpResponseBase(rpRefundLastOperationRequest.Execute());

            return rpRefundLastOperationResponse;
        }

        /// <summary>
        /// 0x4C Preauthorization status
        /// This command is used  to request the status of the 10 preauthorization slots. 
        /// </summary>
        /// <returns>RpResponseAuthStatus</returns>
        public RpResponseAuthStatus RP_PreauthorizationStatusRequest()
        {
            RpCommand rpPreauthorizationStatusRequest = new RpCommand(this);

            rpPreauthorizationStatusRequest.UPTAddress = 0x00;
            rpPreauthorizationStatusRequest.CommandId = 0x4C;
            rpPreauthorizationStatusRequest.InfoLength = 0;
            rpPreauthorizationStatusRequest.FriendlyName = "0x4C Preauthorization status";

            rpPreauthorizationStatusRequest.CompileCommand();
            RpResponseAuthStatus rpPreauthorizationStatusResponse =
                new RpResponseAuthStatus(rpPreauthorizationStatusRequest.Execute());

            return rpPreauthorizationStatusResponse;
        }

        /// <summary>
        /// 0x27 TERMINAL RESET
        /// The PU uses this command to request reset of the UPT.
        /// </summary>

        public void RP_TerminalResetRequest()
        {
            RpCommand rpTerminalResetRequest = new RpCommand(this);

            rpTerminalResetRequest.UPTAddress = 0x00;
            rpTerminalResetRequest.CommandId = 0x27;
            rpTerminalResetRequest.InfoLength = 0;
            rpTerminalResetRequest.FriendlyName = "0x27 Terminal Reset Request";
            rpTerminalResetRequest.ewt = 2;

            rpTerminalResetRequest.CompileCommand();

            rpTerminalResetRequest.Execute();

            //return;
        }

        /// <summary>
        /// 0x21 GSM STATUS REQUEST
        /// The PU uses this command to request the status of the GSM:
        ///  SIM presence;
        ///  level of the signal (from 1-31, or 99 if not present);
        ///  errors of the last transmission;
        ///  carrier present.
        /// </summary>
        /// <returns>RpResponseGSMStatus</returns>
        public RpResponseGSMStatus RP_GsmStatusRequest()
        {
            RpCommand rpGsmStatusRequest = new RpCommand(this);

            rpGsmStatusRequest.UPTAddress = 0x00;
            rpGsmStatusRequest.CommandId = 0x21;
            rpGsmStatusRequest.InfoLength = 0;
            rpGsmStatusRequest.FriendlyName = "0x21 GSM Status Request";

            rpGsmStatusRequest.CompileCommand();

            RpResponseGSMStatus rpGsmStatusRequestResponse = new RpResponseGSMStatus(rpGsmStatusRequest.Execute());

            return rpGsmStatusRequestResponse;
        }

        /// <summary>
        /// 0x22 GET CARDEASEREFERENCE/STAN 
        /// This command is used to read the cardeasereference/stan of the last saved transaction; the transaction can be an approved transaction or a transaction that has to be downloaded with a void.
        /// </summary>
        /// <returns>RpResponseGatewayReference</returns>
        public RpResponseGatewayReference RP_GetLastReference()
        {
            RpCommand rpGetLastReference = new RpCommand(this);

            rpGetLastReference.UPTAddress = 0x00;
            rpGetLastReference.CommandId = 0x22;
            rpGetLastReference.InfoLength = 0;
            rpGetLastReference.FriendlyName = "0x22 Get Gateway Reference of Last Transaction";

            rpGetLastReference.CompileCommand();

            RpResponseGatewayReference rpGetLastReferenceResponse =
                new RpResponseGatewayReference(rpGetLastReference.Execute());

            return rpGetLastReferenceResponse;
        }

        /// <summary>
        /// 0x23 DELETE CARDEASEREFERENCE/STAN 
        /// This command is used to delete the cardeasereference/stan of a transaction that has to be downloaded with a void. The
        /// cardeasereference is not deleted if the transaction to which it refers has been settled.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_DeleteLastGatewayReference()
        {
            RpCommand rpDeleteLastGatewayReference = new RpCommand(this);

            rpDeleteLastGatewayReference.UPTAddress = 0x00;
            rpDeleteLastGatewayReference.CommandId = 0x23;
            rpDeleteLastGatewayReference.InfoLength = 0;
            rpDeleteLastGatewayReference.FriendlyName = "0x23 Delete Gateway Reference of Last Transaction";

            rpDeleteLastGatewayReference.CompileCommand();

            RpResponseBase rpDeleteLastReferenceResponse = new RpResponseBase(rpDeleteLastGatewayReference.Execute());

            return rpDeleteLastReferenceResponse;
        }


        /// <summary>
        /// 0x20 ACTIVATION MAINTENANCE MENU
        /// The PU uses this command to request at the UPT terminal to active the programming of the banking parameters or execute the diagnostic functions.
        /// The parameters necessary are inserted through PINPAD in an autonomous way from the UPT terminal. 
        /// The UPT goes into the busy state and return automatically at its normal state if it isn’t made any choice or received any serial command for 3 minutes.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_ActivationMaintenanceMenuRequest()
        {
            RpCommand rpActivationMaintenanceMenuRequest = new RpCommand(this);

            rpActivationMaintenanceMenuRequest.UPTAddress = 0x00;
            rpActivationMaintenanceMenuRequest.CommandId = 0x20;
            rpActivationMaintenanceMenuRequest.InfoLength = 0;
            rpActivationMaintenanceMenuRequest.FriendlyName = "0x20 Activate Maintenance Menu";

            rpActivationMaintenanceMenuRequest.CompileCommand();

            RpResponseBase rpActivationMaintenanceMenuRequestResponse =
                new RpResponseBase(rpActivationMaintenanceMenuRequest.Execute());

            return rpActivationMaintenanceMenuRequestResponse;
        }

        /// <summary>
        /// 0x18 LOCAL TOTALS REQUEST
        /// This command is used to read the local totals.
        /// The UPT return the transacted total for every enabled circuit (bancomat included), 16 byte of identifier and 4 byte of totals.
        /// </summary>
        /// <returns>RpResponseTotals</returns>
        [Obsolete("RP_LocalTotalsRequest is deprecated, and note used.", false)]
        public RpResponseTotals RP_LocalTotalsRequest()
        {
            RpCommand rpLocalTotalsRequest = new RpCommand(this);


            rpLocalTotalsRequest.UPTAddress = 0x00;
            rpLocalTotalsRequest.CommandId = 0x18;
            rpLocalTotalsRequest.InfoLength = 0;
            rpLocalTotalsRequest.FriendlyName = "0x18 Read Local Totals";

            rpLocalTotalsRequest.CompileCommand();

            RpResponseTotals rpLocalTotalsRequestResponse = new RpResponseTotals(rpLocalTotalsRequest.Execute());

            return rpLocalTotalsRequestResponse;
        }

        /// <summary>
        /// 0xC3 Get Pairing CA Certificate
        /// This command is used to return the Pairing CA Certification Info
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_GetCAPairingCert()
        {
            RpCommand rpGetClientCert = new RpCommand(this);

            rpGetClientCert.UPTAddress = 0x00;
            rpGetClientCert.CommandId = 0xC3;
            rpGetClientCert.InfoLength = 1;
            rpGetClientCert.Info[0] = 0x01;
            rpGetClientCert.FriendlyName = "0xC3 Get CA Pairing Certificate";

            rpGetClientCert.CompileCommand();

            RpResponseBase rpGetClientCertResponse = new RpResponseBase(rpGetClientCert.Execute());

            return rpGetClientCertResponse;
        }

        /// <summary>
        /// 0xC4 Get Pairing Certificate
        /// This command is used to return the Pairing CA Certification Info
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_GetPairingCert()
        {
            RpCommand rpGetClientCert = new RpCommand(this);

            rpGetClientCert.UPTAddress = 0x00;
            rpGetClientCert.CommandId = 0xC4;
            rpGetClientCert.InfoLength = 1;
            rpGetClientCert.Info[0] = 0x01;
            rpGetClientCert.FriendlyName = "0xC4 Get Pairing Certificate";

            rpGetClientCert.CompileCommand();

            RpResponseBase rpGetClientCertResponse = new RpResponseBase(rpGetClientCert.Execute());

            return rpGetClientCertResponse;
        }


        /// <summary>
        /// 0x11 Get Client TLS Certificate
        /// This command is used to return the TLS Client Certificate loaded in the terminal
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_GetClientCert()
        {
            RpCommand rpGetClientCert = new RpCommand(this);

            rpGetClientCert.UPTAddress = 0x00;
            rpGetClientCert.CommandId = 0x11;
            rpGetClientCert.InfoLength = 1;
            rpGetClientCert.Info[0] = 0x01;
            rpGetClientCert.FriendlyName = "0x11 Get Client TLS Certificate";

            rpGetClientCert.CompileCommand();

            RpResponseBase rpGetClientCertResponse = new RpResponseBase(rpGetClientCert.Execute());

            return rpGetClientCertResponse;
        }

        /// <summary>
        /// 0x10 CA Cert Request
        /// Returns the CA Cert loaded on the terminal
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_GetCaCert()
        {
            RpCommand rpGetCaCert = new RpCommand(this);

            rpGetCaCert.UPTAddress = 0x00;
            rpGetCaCert.CommandId = 0x10;
            rpGetCaCert.InfoLength = 1;
            rpGetCaCert.Info[0] = 0x01;
            rpGetCaCert.FriendlyName = "0x10 Get TLS CA Cert";

            rpGetCaCert.CompileCommand();
            RpResponseBase rpGetCaCertResponse = new RpResponseBase(rpGetCaCert.Execute());

            return rpGetCaCertResponse;
        }

        /// <summary>
        /// 0x0B LAST OPERATION DATA REQUEST
        /// The PU uses this command to request the data of the last transaction.
        /// 
        /// We use the RpResponsePaymentOutcome object for the response
        /// RpResponsePaymentOutcome.RFU is the Preauthorization Index for this method
        /// </summary>
        /// <returns>RpResponsePaymentOutcome</returns>
        public RpResponsePaymentOutcome RP_LastOperationDataRequest()
        {
            RpCommand rpLastOperationDataRequest = new RpCommand(this);

            rpLastOperationDataRequest.UPTAddress = 0x00;
            rpLastOperationDataRequest.CommandId = 0x0B;
            rpLastOperationDataRequest.InfoLength = 0;
            rpLastOperationDataRequest.FriendlyName = "0x0B Last Operation Data Request";


            rpLastOperationDataRequest.CompileCommand();
            RpResponsePaymentOutcome rpPaymentOutcomeRequestResponse =
                new RpResponsePaymentOutcome(rpLastOperationDataRequest.Execute());

            return rpPaymentOutcomeRequestResponse;
        }


        /// <summary>
        /// 0x0B LAST OPERATION DATA REQUEST MSF TOS detail
        /// The PU uses this command to request the data of the last transaction.
        /// 
        /// We use the RpResponsePaymentOutcome object for the response
        /// RpResponsePaymentOutcome.RFU is the Preauthorization Index for this method
        /// </summary>
        /// <param name="ReadMSFTOSTransactionDetails">Read details from the top of the stack for store and forward transactions.</param>
        /// <returns>RpResponsePaymentOutcome</returns>
        public RpResponsePaymentOutcome RP_LastOperationDataRequest(bool ReadMSFTOSTransactionDetails)
        {
            RpCommand rpLastOperationDataRequest = new RpCommand(this);

            rpLastOperationDataRequest.UPTAddress = 0x00;
            rpLastOperationDataRequest.CommandId = 0x0B;
            rpLastOperationDataRequest.InfoLength = 0;
            rpLastOperationDataRequest.FriendlyName = "0x0B Last Operation Data Request";

            if (ReadMSFTOSTransactionDetails)
            {
                rpLastOperationDataRequest.InfoLength = 1;
                rpLastOperationDataRequest.Info[0] = 0x01;
            }

            rpLastOperationDataRequest.CompileCommand();
            RpResponsePaymentOutcome rpPaymentOutcomeRequestResponse =
                new RpResponsePaymentOutcome(rpLastOperationDataRequest.Execute());

            return rpPaymentOutcomeRequestResponse;
        }



        /// <summary>
        /// 0x0D PAYMENT OUTCOME REQUEST
        /// The PU uses this command to request at the terminal the outcome of the payment executed.
        /// </summary>
        /// <returns>RpResponsePaymentOutcome</returns>
        public RpResponsePaymentOutcome RP_PaymentOutcomeRequest()
        {
            RpCommand rpPaymentOutcomeRequest = new RpCommand(this);

            rpPaymentOutcomeRequest.UPTAddress = 0x00;
            rpPaymentOutcomeRequest.CommandId = 0x0D;
            rpPaymentOutcomeRequest.InfoLength = 0;
            rpPaymentOutcomeRequest.FriendlyName = "0x0D Payment Outcome Request";
            rpPaymentOutcomeRequest.CompileCommand();
            RpResponsePaymentOutcome rpPaymentOutcomeRequestResponse =
                new RpResponsePaymentOutcome(rpPaymentOutcomeRequest.Execute());

            return rpPaymentOutcomeRequestResponse;
        }

        public RpResponseHRTLevel2 RP_GetTerminalLevel2(EDeviceFamily eFamily)
        {
            RpCommand rpTermConfigRequest = new RpCommand(this);

            // It can handle the larger files
            if (eFamily == EDeviceFamily.HRT1000)
            {
                rpTermConfigRequest.UPTAddress = 0x00;
                rpTermConfigRequest.CommandId = 0x1E;
                rpTermConfigRequest.InfoLength = 1;
                rpTermConfigRequest.Info[0] = (byte) EConfigFiles.HRT_L2Params;
                rpTermConfigRequest.FriendlyName = "0x1E Terminal Config request - level2.prm";
                rpTermConfigRequest.CompileCommand();

                RpResponseHRTLevel2 rpTermConfigResponse = new RpResponseHRTLevel2(rpTermConfigRequest.Execute());
                return rpTermConfigResponse;
            }
            // Max 1K
            else
            {
                rpTermConfigRequest.UPTAddress = 0x00;
                rpTermConfigRequest.CommandId = 0x1E;
                rpTermConfigRequest.InfoLength = 5;
                rpTermConfigRequest.Info[0] = (byte) EConfigFiles.HRT_L2Params; // ID of the file
                rpTermConfigRequest.Info[1] = 0x03; // 1k (max)
                rpTermConfigRequest.Info[2] = 0xE8;
                rpTermConfigRequest.Info[3] = 0x00; //the buffer beginning
                rpTermConfigRequest.Info[4] = 0x00;
                rpTermConfigRequest.FriendlyName = "0x1E Terminal Config request (initial) - level2.prm";

                rpTermConfigRequest.CompileCommand();

                RpResponseHRTLevel2 rpTermConfigResponse = new RpResponseHRTLevel2(rpTermConfigRequest.Execute(), true);

                if (rpTermConfigResponse.VerifyOutcome() != EOutcome.OK)
                    return rpTermConfigResponse;

                byte[] l2File = new byte[rpTermConfigResponse.fullPrmFileLen];
                int nPos = 0;

                nPos += rpTermConfigResponse.GetPartialFile(ref l2File, nPos);
                
                int fileSize = rpTermConfigResponse.fullPrmFileLen;
                while (nPos < fileSize)
                {
                    //If there is more than 1000 bytes remaining, set the size to 1000, otherwise set the size to what is remaining.
                    int remaining = ((fileSize - nPos) > 1000) ? 1000 : (fileSize - nPos);

                    rpTermConfigRequest.Info[0] = (byte)EConfigFiles.HRT_L2Params; // ID of the file
                    rpTermConfigRequest.Info[1] = (byte)(remaining >> 8);
                    rpTermConfigRequest.Info[2] = (byte)(remaining);
                    rpTermConfigRequest.Info[3] = (byte)(nPos >> 8);
                    rpTermConfigRequest.Info[4] = (byte)(nPos);
                    rpTermConfigRequest.FriendlyName = "0x1E Terminal Config request (additional) - level2.prm";

                    rpTermConfigRequest.CompileCommand();

                    rpTermConfigResponse = new RpResponseHRTLevel2(rpTermConfigRequest.Execute(), true);
                    if (rpTermConfigResponse.VerifyOutcome() != EOutcome.OK) throw new Exception("Error Reading Config File level2.prm." + "Outcome:" + rpTermConfigResponse.VerifyOutcome());
                    nPos += rpTermConfigResponse.GetPartialFile(ref l2File, nPos);                
                }

                rpTermConfigResponse.LoadPrm(l2File, nPos);
                return rpTermConfigResponse;
            }
        }

        /// <summary> Send the PRM file for Contactless to the HRT device </summary>
        /// <returns>RpResponseBase with the result</returns>
        public RpResponseBase RP_SetTerminalHRTLevel2(byte[] prmFileBytes)
        {
            RpCommand rpTermConfigRequest = new RpCommand(this);

            rpTermConfigRequest.UPTAddress = 0x00;
            rpTermConfigRequest.CommandId = 0x00; // Extended
            rpTermConfigRequest.InfoLength = (ushort)(prmFileBytes.Length + 1);
            rpTermConfigRequest.Info[0] = 0xAA;
            rpTermConfigRequest.FriendlyName = "0xAA Set Terminal Config - level2.prm";

            Buffer.BlockCopy(prmFileBytes, 0, rpTermConfigRequest.Info, 1, prmFileBytes.Length);

            rpTermConfigRequest.CompileCommand();
            RpResponseBase rpTermConfigResponse = new RpResponseBase(rpTermConfigRequest.Execute());

            return rpTermConfigResponse;

        }

        /// <summary>
        /// 0x1E GET CONFIGURATION FILES 
        /// This command uses 0x1E to retrieve and parse the dynamic prompts file
        /// </summary>
        /// <returns>RpResponseDynmicPrompts</returns>
        public RpResponseDynamicPromptList RP_GetDynamicPromptsFile()
        {
            RpCommand rpRequest = new RpCommand(this);

            rpRequest.UPTAddress = 0x00;
            rpRequest.CommandId = 0x1E;
            rpRequest.InfoLength = 1;
            rpRequest.Info[0] = (byte)EConfigFiles.Dynamic_Prompts;
            rpRequest.FriendlyName = "0x1E Terminal Config request - dynamic prompts";
            rpRequest.CompileCommand();
            RpResponseDynamicPromptList rpResponse = new RpResponseDynamicPromptList(rpRequest.Execute());

            return rpResponse;
        }

        /// <summary>
        /// 0x1E GET CONFIG FILE FOR VISA FLEET 2.0
        /// </summary>
        /// <returns></returns>
        public RpResponseVisaFleet2List RP_GetVisaFleet2File()
        {
            RpCommand rpRequest = new RpCommand(this);

            rpRequest.UPTAddress = 0x00;
            rpRequest.CommandId = 0x1E;
            rpRequest.InfoLength = 1;
            rpRequest.Info[0] = (byte)EConfigFiles.VisaFleet2;
            rpRequest.FriendlyName = "0x1E Terminal Config request - Visa Fleet 2";
            rpRequest.CompileCommand();
            RpResponseVisaFleet2List rpResponse = new RpResponseVisaFleet2List(rpRequest.Execute());

            return rpResponse;
        }


        /// <summary>
        /// 0x1E GET CONFIGURATION FILES 
        /// This command uses 0x1E to retrieve and parse the termconfig.dat file on the terminal's filesystem
        /// </summary>
        /// <returns>RpResponseTermConfig</returns>
        public RpResponseTermConfig RP_GetTerminalConfig()
        {
            RpCommand rpTermConfigRequest = new RpCommand(this);

            rpTermConfigRequest.UPTAddress = 0x00;
            rpTermConfigRequest.CommandId = 0x1E;
            rpTermConfigRequest.InfoLength = 1;
            rpTermConfigRequest.Info[0] = (byte) EConfigFiles.TermConfig;
            rpTermConfigRequest.FriendlyName = "0x1E Terminal Config request - termconfig.dat";
            rpTermConfigRequest.CompileCommand();
            RpResponseTermConfig rpTermConfigResponse = new RpResponseTermConfig(rpTermConfigRequest.Execute());

            return rpTermConfigResponse;
        }

        /// <summary>
        /// 0x1E GET CONFIGURATION FILES 
        /// This command uses 0x1E to retrieve the AID List applist.pp file on the terminal's filesystem
        /// </summary>
        /// <returns>RpResponseAIDList</returns>
        public RpResponseAIDList RP_GetTerminalAIDList()
        {
            RpCommand rpTermAidListRequest = new RpCommand(this);

            rpTermAidListRequest.UPTAddress = 0x00;
            rpTermAidListRequest.CommandId = 0x1E;
            rpTermAidListRequest.InfoLength = 1;
            rpTermAidListRequest.Info[0] = (byte) EConfigFiles.AID;
            rpTermAidListRequest.FriendlyName = "0x1E Get AID List request - applist.pp";
            rpTermAidListRequest.CompileCommand();
            RpResponseAIDList rpTermAidListResponse = new RpResponseAIDList(rpTermAidListRequest.Execute());

            return rpTermAidListResponse;
        }

        /// <summary>
        /// 0x1E GET CONFIGURATION FILES 
        /// This command uses 0x1E to retrieve the CommonAID List applist_common.pp file on the terminal's filesystem
        /// </summary>
        /// <returns>RpResponseAIDList</returns>
        public RpResponseAIDList RP_GetTerminalCommonAIDList()
        {
            RpCommand rpTermCommonAidListRequest = new RpCommand(this);

            rpTermCommonAidListRequest.UPTAddress = 0x00;
            rpTermCommonAidListRequest.CommandId = 0x1E;
            rpTermCommonAidListRequest.InfoLength = 1;
            rpTermCommonAidListRequest.Info[0] = (byte) EConfigFiles.CommonAID;
            rpTermCommonAidListRequest.FriendlyName = "0x1E Get AID List request - applist_common.pp";
            rpTermCommonAidListRequest.CompileCommand();
            RpResponseAIDList rpTermCommonAidListResponse = new RpResponseAIDList(rpTermCommonAidListRequest.Execute());

            return rpTermCommonAidListResponse;
        }

        /// <summary>
        /// 0x1E GET CONFIGURATION FILES 
        /// This command uses 0x1E to retrieve the CommonAID List applist_debit.pp file on the terminal's filesystem
        /// </summary>
        /// <returns>RpResponseAIDList</returns>
        public RpResponseAIDList RP_GetTerminalDebitAIDList()
        {
            RpCommand rpTermDebitAidListRequest = new RpCommand(this);

            rpTermDebitAidListRequest.UPTAddress = 0x00;
            rpTermDebitAidListRequest.CommandId = 0x1E;
            rpTermDebitAidListRequest.InfoLength = 1;
            rpTermDebitAidListRequest.Info[0] = (byte) EConfigFiles.DebitAID;
            rpTermDebitAidListRequest.FriendlyName = "0x1E Get AID List request - applist_debit.pp";
            rpTermDebitAidListRequest.CompileCommand();
            RpResponseAIDList rpTermDebitAidListResponse = new RpResponseAIDList(rpTermDebitAidListRequest.Execute());

            return rpTermDebitAidListResponse;
        }

        /// <summary>
        /// 0x1E GET CONFIGURATION FILES 
        /// This command uses 0x1E to retrieve the mag_config file on the terminal's filesystem
        /// </summary>
        /// <returns>RpResponseMagConfig</returns>
        public RpResponseMagConfig RP_GetTerminalMagConfig()
        {
            RpCommand rpTermGetMagConfigRequest = new RpCommand(this);

            rpTermGetMagConfigRequest.UPTAddress = 0x00;
            rpTermGetMagConfigRequest.CommandId = 0x1E;
            rpTermGetMagConfigRequest.InfoLength = 1;
            rpTermGetMagConfigRequest.Info[0] = (byte) EConfigFiles.MAG_Config;
            rpTermGetMagConfigRequest.FriendlyName = "0x1E Get Mag config File request - mag_config.txt";
            rpTermGetMagConfigRequest.CompileCommand();
            RpResponseMagConfig rpTermGetMagConfigResponse =
                new RpResponseMagConfig(rpTermGetMagConfigRequest.Execute());

            return rpTermGetMagConfigResponse;
        }

        /// <summary>
        /// 0x1E GET CONFIGURATION FILES 
        /// This command uses 0x1E to retrieve the white_list.dat file on the terminal's filesystem
        /// </summary>
        /// <returns>RpResponseWhiteList</returns>
        public RpResponseWhiteList RP_GetTerminalWhiteList()
        {
            RpCommand rpTermGetWhiteListRequest = new RpCommand(this);

            rpTermGetWhiteListRequest.UPTAddress = 0x00;
            rpTermGetWhiteListRequest.CommandId = 0x1E;
            rpTermGetWhiteListRequest.InfoLength = 1;
            rpTermGetWhiteListRequest.Info[0] = (byte)EConfigFiles.Whitelist;
            rpTermGetWhiteListRequest.FriendlyName = "0x1E Get Mag config File request - mag_config.txt";
            rpTermGetWhiteListRequest.CompileCommand();
            RpResponseWhiteList rpTermGetWhiteListResponse =
                new RpResponseWhiteList(rpTermGetWhiteListRequest.Execute());

            return rpTermGetWhiteListResponse;
        }


        /// <summary>
        /// 0x1E GET CONFIGURATION FILES
        /// This command uses 0x1E to retrieve the ConfigE1_pr file from the terminal's filesystem
        /// </summary>
        /// <returns>RpResponseTLVData</returns>
        public RpResponseTLVData RP_GetTerminalEMVConfig()
        {
            RpCommand rpTermGetEmvConfigRequest = new RpCommand(this);

            rpTermGetEmvConfigRequest.UPTAddress = 0x00;
            rpTermGetEmvConfigRequest.CommandId = 0x1E;
            rpTermGetEmvConfigRequest.InfoLength = 1;
            rpTermGetEmvConfigRequest.Info[0] = (byte) EConfigFiles.EMV_Config;
            rpTermGetEmvConfigRequest.FriendlyName = "0x1E Get EMV config File request - ConfigE1.prt";
            rpTermGetEmvConfigRequest.CompileCommand();
            RpResponseTLVData rpTermGetEmvConfigResponse = new RpResponseTLVData(rpTermGetEmvConfigRequest.Execute());

            return rpTermGetEmvConfigResponse;
        }

        /// <summary>
        /// 0x1E Get Dynamic TLV Data
        /// </summary>
        /// <returns></returns>
        public RpResponseDynamicTLVData RP_GetTerminalDynamicTlvConfig()
        {
            RpCommand rpTermGetDyanamicTlvRequest = new RpCommand(this);

            rpTermGetDyanamicTlvRequest.UPTAddress = 0x00;
            rpTermGetDyanamicTlvRequest.CommandId = 0x1E;
            rpTermGetDyanamicTlvRequest.InfoLength = 1;
            rpTermGetDyanamicTlvRequest.Info[0] = (byte) EConfigFiles.EMV_Dynamic_Config;
            rpTermGetDyanamicTlvRequest.FriendlyName = "0x1E Get Dyanmic EMV config File request - DynamicF1.prt";
            rpTermGetDyanamicTlvRequest.CompileCommand();
            RpResponseDynamicTLVData rpTermGetDynamicTlvResponse =
                new RpResponseDynamicTLVData(rpTermGetDyanamicTlvRequest.Execute());

            return rpTermGetDynamicTlvResponse;
        }

        /// <summary>
        /// 0x1E GET CAKey List of files
        /// </summary>
        /// <returns>RpResponseCAKeyList</returns>
        public RpResponseCAKeyList RP_GetTerminalCAKeyList()
        {
            RpCommand rpTermGetCaKeyListRequest = new RpCommand(this);

            rpTermGetCaKeyListRequest.UPTAddress = 0x00;
            rpTermGetCaKeyListRequest.CommandId = 0x1E;
            rpTermGetCaKeyListRequest.InfoLength = 1;
            rpTermGetCaKeyListRequest.Info[0] = (byte) EConfigFiles.CAKeys_Files;
            rpTermGetCaKeyListRequest.FriendlyName = "0x1E Get CAKey Files";
            rpTermGetCaKeyListRequest.CompileCommand();
            RpResponseCAKeyList rpTermGetCaKeyListResponse =
                new RpResponseCAKeyList(rpTermGetCaKeyListRequest.Execute());

            return rpTermGetCaKeyListResponse;
        }

        /// <summary>
        /// 0x1E GET Loaded FS
        /// </summary>
        /// <returns>RpResponseLoadedFS</returns>
        public RpResponseGetLoadedFS RP_GetLoadedFS()
        {
            RpCommand rpTermGetLoadedFSRequest = new RpCommand(this);

            rpTermGetLoadedFSRequest.UPTAddress = 0x00;
            rpTermGetLoadedFSRequest.CommandId = 0x1E;
            rpTermGetLoadedFSRequest.InfoLength = 1;
            rpTermGetLoadedFSRequest.Info[0] = (byte)EConfigFiles.LoadedFilesystem;
            rpTermGetLoadedFSRequest.FriendlyName = "0x1E Get Loaded FS";
            rpTermGetLoadedFSRequest.CompileCommand();
            RpResponseGetLoadedFS rpTermGetLoadedFsResponse =
                new RpResponseGetLoadedFS(rpTermGetLoadedFSRequest.Execute());

            return rpTermGetLoadedFsResponse;
        }

        /// <summary> handle to allow for feedback on progress of pulling the file </summary>
        /// <param name="pos">size to initlize, 1 to increase, 0 to reset</param>
        public delegate void ProgressBarDelegate(int pos);

        /// <summary>
        /// 0x1E GET FS List
        /// </summary>
        /// <returns>RpResponseGetFSList</returns>
        public RpResponseGetFSList RP_GetFSList(bool withMd5, ProgressBarDelegate pb)
        {
            byte fileType = (byte)EConfigFiles.FS_List;
            if (withMd5)
                fileType = (byte)EConfigFiles.FS_List_MD5;

            RpCommand rpTermGetFSRequest = new RpCommand(this);

            rpTermGetFSRequest.UPTAddress = 0x00;
            rpTermGetFSRequest.CommandId = 0x1E;
            rpTermGetFSRequest.InfoLength = 5;
            rpTermGetFSRequest.Info[0] = fileType;
            rpTermGetFSRequest.Info[1] = 0x03; // 1k (max)
            rpTermGetFSRequest.Info[2] = 0xE8;
            rpTermGetFSRequest.Info[3] = 0x00; //the buffer beginning
            rpTermGetFSRequest.Info[4] = 0x00;

            rpTermGetFSRequest.FriendlyName = "0x1E Get FS List";
            rpTermGetFSRequest.CompileCommand();

            RpResponseGetFSList rpTermGetFsResponse =
                new RpResponseGetFSList(rpTermGetFSRequest.Execute());

            if (rpTermGetFsResponse.VerifyOutcome() != EOutcome.OK)
                throw new Exception("Failed to get File List.  Result: " + rpTermGetFsResponse.VerifyOutcome());

            byte[] l2File = new byte[rpTermGetFsResponse.fullFileLen];
            int nPos = 0;

            nPos += rpTermGetFsResponse.GetPartialFile(ref l2File, nPos);

            int fileSize = rpTermGetFsResponse.fullFileLen;
            pb(fileSize / 1000);

            while (nPos < fileSize)
            {
                //If there is more than 1000 bytes remaining, set the size to 1000, otherwise set the size to what is remaining.
                int remaining = ((fileSize - nPos) > 1000) ? 1000 : (fileSize - nPos);

                rpTermGetFSRequest.Info[1] = (byte)(remaining >> 8);
                rpTermGetFSRequest.Info[2] = (byte)(remaining);
                rpTermGetFSRequest.Info[3] = (byte)(nPos >> 8);
                rpTermGetFSRequest.Info[4] = (byte)(nPos);
                rpTermGetFSRequest.FriendlyName = "0x1E Get FS List (additional)";

                rpTermGetFSRequest.CompileCommand();

                rpTermGetFsResponse = new RpResponseGetFSList(rpTermGetFSRequest.Execute());
                if (rpTermGetFsResponse.VerifyOutcome() != EOutcome.OK)
                {
                    pb(0);
                    throw new Exception("Error Reading Config File level2.prm." + "Outcome:" + rpTermGetFsResponse.VerifyOutcome());
                }
                
                nPos += rpTermGetFsResponse.GetPartialFile(ref l2File, nPos);
                pb(1);
            }

            pb(0);

            if (withMd5)
                rpTermGetFsResponse.LoadCLMD5File(l2File, nPos);
            else
                rpTermGetFsResponse.LoadFile(l2File, nPos);
            
            return rpTermGetFsResponse;
        }

        /// <summary>
        /// 0x1E GET CONFIG FILE by Name
        /// </summary>
        /// <returns></returns>
        public RpResponseGetFileSystemFile RP_GetConfigFileByName(bool bFromChild, string filename)
        {
            RpCommand rpRequest = new RpCommand(this);

            rpRequest.UPTAddress = 0x00;
            rpRequest.CommandId = 0x1E;
            rpRequest.InfoLength = (ushort)(6 + filename.Length);
            rpRequest.Info[0] = (byte)EConfigFiles.Specific_File;

            rpRequest.Info[1] = 0x03; // 1k (max)
            rpRequest.Info[2] = 0xE8;
            rpRequest.Info[3] = 0x00; //the buffer beginning
            rpRequest.Info[4] = 0x00;

            if (bFromChild)
                rpRequest.Info[5] = 0x01;  // Device Type
            else
                rpRequest.Info[5] = 0x00;  // Device Type

            byte[] filenameBytes = StringToByteArray(filename, filename.Length);
            Buffer.BlockCopy(filenameBytes, 0, rpRequest.Info, 6, filename.Length);

            rpRequest.FriendlyName = "0x1E Terminal Config request - Specific File";
            rpRequest.CompileCommand();

            RpResponseGetFileSystemFile rpGetFilesystemFile = new RpResponseGetFileSystemFile(rpRequest.Execute());
            int fullFileSize = rpGetFilesystemFile.FileLength;
            int receivedSize = rpGetFilesystemFile.FullFile.Length;

            while (receivedSize < fullFileSize)
            {
                //If there is more than 1000 bytes remaining, set the size to 1000, otherwise set the size to what is remaining.
                int remaining = ((fullFileSize - receivedSize) > 1000) ? 1000 : (fullFileSize - receivedSize);

                rpRequest.Info[1] = (byte)(remaining >> 8);
                rpRequest.Info[2] = (byte)(remaining);
                rpRequest.Info[3] = (byte)(receivedSize >> 8);
                rpRequest.Info[4] = (byte)(receivedSize);

                rpRequest.CompileCommand();

                RpResponseGetFileSystemFile rpTemp = new RpResponseGetFileSystemFile(rpRequest.Execute());
                if (rpTemp.VerifyOutcome() != EOutcome.OK) 
                    throw new Exception("Error reading File." + "Outcome:" + rpTemp.VerifyOutcome());

                receivedSize = rpGetFilesystemFile.append(rpTemp.FullFile);
            }

            return rpGetFilesystemFile;
        }



        /// <summary>
        /// 0x05 PAYMENT START
        /// This command is used to start a payment operation. 
        /// </summary>
        /// <param name="amountInPennies">Import to pay in Euro Cent</param>
        /// <param name="currencyCode">Currency Code for the sale</param>
        /// <param name="mode">0x30 = online, 0x31 = offline, 0x32 = online w/backup</param>
        /// <param name="clientTransactionID">Invoice # passed from the POS to the gateway</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_PaymentCommand(int amountInPennies, ECurrencies? currencyCode, EMode? mode,
            string clientTransactionID)
        {
            if (amountInPennies < 0)
            {
                throw new Exception("Illegal amount requested for payment");
            }
            if (null == currencyCode)
            {
                currencyCode = ECurrencies.USD;
            }
            if (null == clientTransactionID)
            {
                clientTransactionID = "";
            }
            if (clientTransactionID.Length > 50) clientTransactionID = clientTransactionID.Substring(0, 50);
            byte[] clientTransactionIdBytes = StringToByteArray(clientTransactionID, 51);

            RpCommand rpPaymentCommand = new RpCommand(this);

            rpPaymentCommand.UPTAddress = 0x00;
            rpPaymentCommand.CommandId = 0x05;
            rpPaymentCommand.InfoLength = 62;
            //rpPaymentCommand.InfoLength = (clientTransactionID.Length == 0) ? (ushort)11: (ushort)62;
            rpPaymentCommand.Info[0] = (byte) (amountInPennies >> 24);
            rpPaymentCommand.Info[1] = (byte) (amountInPennies >> 16);
            rpPaymentCommand.Info[2] = (byte) (amountInPennies >> 8);
            rpPaymentCommand.Info[3] = (byte) amountInPennies;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(currencyCode.ToString()), 0, rpPaymentCommand.Info, 4, 3);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(((Int32) currencyCode).ToString()), 0, rpPaymentCommand.Info, 7,
                3);
            if (null != mode) rpPaymentCommand.Info[10] = (byte) mode;
            Buffer.BlockCopy(clientTransactionIdBytes, 0, rpPaymentCommand.Info, 11, 51);
            //if (clientTransactionID.Length > 0) Buffer.BlockCopy(clientTransactionIdBytes, 0, rpPaymentCommand.Info, 11, 51);
            rpPaymentCommand.FriendlyName = "0x05 Payment Command";

            rpPaymentCommand.CompileCommand();
            RpResponseBase rpPaymentCommandResponse = new RpResponseBase(rpPaymentCommand.Execute());

            return rpPaymentCommandResponse;
        }

        /// <summary>
        /// 0x0A STATUS REQUEST
        /// This command is used from the PU to know the status of the UPT terminal module used for the banking transactions.
        /// </summary>
        /// <param name="language">Language ID
        ///     0x01: Italian  //Reversed English to default in G041
        ///     0x00: English  
        ///     0x02: German
        ///     0x03: France
        ///     0x04: Spanish
        /// </param>
        /// <returns>RpResponseStatus</returns>
        public RpResponseStatus RP_StatusRequest(ELanguage? language)
        {
            if (null == language)
            {
                language = ELanguage.English;
            }

            RpCommand rpStatusRequest = new RpCommand(this);

            rpStatusRequest.UPTAddress = 0x00;
            rpStatusRequest.CommandId = 0x0A;
            rpStatusRequest.InfoLength = 2;
            rpStatusRequest.Info[0] = (byte) language;
            rpStatusRequest.Info[1] = 0x01; //Give me all your data
            rpStatusRequest.FriendlyName = "0x0A Status Request";
            rpStatusRequest.ewt = 15;

            rpStatusRequest.CompileCommand();

            RpResponseStatus rpStatusRequestResponse = new RpResponseStatus(rpStatusRequest.Execute());
            //BytesInCounter = 0;

            return rpStatusRequestResponse;
        }

        /// <summary>
        /// 0x08 CARD DATA REQUEST
        /// This command is used by the PU to request the transmission of the data of the magnetic card about the tracks.
        /// The tracks available are T1, T2, T3 for the not banking card and only T1 for banking card.
        /// 
        /// The answer of the terminal has a variable size depending from the effective length of the available track. 
        /// It goes from a minimum of 1 byte for the only outcome , when the requested track is not available, 
        /// to a maximum of byte calculated as follow: 22 + N byte where N is equivalent at the length of the sent track.
        /// </summary>
        /// <param name="trackNumber">TrackNumber</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_CardDataRequest(int trackNumber)
        {
            if ((trackNumber < 1) || (trackNumber > 3))
            {
                throw new Exception("Illegal Track Number in Card Data Request.");
            }

            RpCommand rpCardDataRequest = new RpCommand(this);

            rpCardDataRequest.UPTAddress = 0x00;
            rpCardDataRequest.CommandId = 0x08;
            rpCardDataRequest.InfoLength = 1;
            rpCardDataRequest.Info[0] = (byte) trackNumber;
            rpCardDataRequest.FriendlyName = "0x08 Card Data Request";

            rpCardDataRequest.CompileCommand();

            RpResponseBase rpCardDataRequestResponse = new RpResponseBase(rpCardDataRequest.Execute());

            return rpCardDataRequestResponse;
        }

        /// <summary>
        /// 0x17 READ TRACKS TYPE REQUEST
        /// This command is used to know the tracks type of the tracks read by the UPT.
        /// </summary>
        /// <returns>RpResponseTrackType</returns>
        public RpResponseTrackType RP_ReadTracksTypeRequest(bool requestExtraByte)
        {
            RpCommand rpReadTracksTyepRequest = new RpCommand(this);

            rpReadTracksTyepRequest.UPTAddress = 0x00;
            rpReadTracksTyepRequest.CommandId = 0x17;
            rpReadTracksTyepRequest.ewt = 60;  //60 seconds for application selection
            if (requestExtraByte)
            {
                rpReadTracksTyepRequest.InfoLength = (ushort)1;
                rpReadTracksTyepRequest.Info[0] = 0x01;
            }
            else
            {
                rpReadTracksTyepRequest.InfoLength = 0;
            }

            rpReadTracksTyepRequest.FriendlyName = "0x17 Read Tracks Type";


            rpReadTracksTyepRequest.CompileCommand();

            RpResponseTrackType rpReadTracksTyepRequestResponse =
                new RpResponseTrackType(rpReadTracksTyepRequest.Execute());

            return rpReadTracksTyepRequestResponse;
        }



        /// <summary>
        /// 0x07 CARD READ DISABLE
        /// This command is used to disable the read of the card. So if a card is inserted into the card reader the UPT ignore its presence and doesn’t read it.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_ReadCardDisable()
        {
            RpCommand rpReadCardDisable = new RpCommand(this);

            rpReadCardDisable.UPTAddress = 0x00;
            rpReadCardDisable.CommandId = 0x07;
            rpReadCardDisable.InfoLength = 0;

            rpReadCardDisable.CompileCommand();
            rpReadCardDisable.FriendlyName = "0x07 Read Card Disable";

            RpResponseBase rpReadCardDisableResponse = new RpResponseBase(rpReadCardDisable.Execute());

            return rpReadCardDisableResponse;
        }

        /// <summary>
        /// 0x06 READ CARD ENABLE
        /// This command is used to enable the read of the card.
        /// </summary>
        /// <param name="activeTime">
        /// Time in seconds indicating the time for which the reader is active:
        ///     60 = default value 
        ///     255 = reader always active
        /// </param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_ReadCardEnable(int activeTime)
        {
            RpCommand rpReadCardEnable = new RpCommand(this);

            if (0 >= activeTime) //default
            {
                activeTime = 60;
            }

            if (activeTime > 255)
            {
                activeTime = 255;
            }

            rpReadCardEnable.UPTAddress = 0x00;
            rpReadCardEnable.CommandId = 0x06;
            rpReadCardEnable.InfoLength = 1;
            rpReadCardEnable.Info[0] = (byte) activeTime;

            rpReadCardEnable.CompileCommand();
            rpReadCardEnable.FriendlyName = "0x06 Read Card Enable";

            RpResponseBase rpReadCardEnableResponse = new RpResponseBase(rpReadCardEnable.Execute());

            return rpReadCardEnableResponse;
        }


        /// <summary>
        /// 0x06 ENABLE With NFC 
        /// This command is used to enable the reader when the CL is connected.
        /// </summary>
        /// <param name="activeTime">Time in seconds the reader is active</param>
        /// <param name="amountInPennies">Import to pay in Euro Cent</param>
        /// <param name="currencyCode">Currency Code for the sale</param>
        /// <param name="mode">0x30 = online, 0x31 = offline, 0x32 = online w/backup</param>
        /// <param name="clientTransactionID">Invoice# passed from the POS system to the gateway</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_ReadCardEnableWithNFC(int activeTime, int amountInPennies, ECurrencies? currencyCode,
            EMode? mode, string clientTransactionID)
        {
            if (amountInPennies < 0)
            {
                throw new Exception("Illegal amount requested for payment");
            }
            if (null == currencyCode)
            {
                currencyCode = ECurrencies.USD;
            }
            if (null == clientTransactionID)
            {
                clientTransactionID = "";
            }

            RpCommand rpEnableWithPaymentCommand = new RpCommand(this);
            if (clientTransactionID.Length > 50) clientTransactionID = clientTransactionID.Substring(0, 50);
            byte[] clientTransactionIdBytes = StringToByteArray(clientTransactionID, 51);

            rpEnableWithPaymentCommand.UPTAddress = 0x00;
            rpEnableWithPaymentCommand.CommandId = 0x06;
            rpEnableWithPaymentCommand.InfoLength = 63;
            rpEnableWithPaymentCommand.Info[0] = (byte) (activeTime);
            rpEnableWithPaymentCommand.Info[1] = (byte) (amountInPennies >> 24);
            rpEnableWithPaymentCommand.Info[2] = (byte) (amountInPennies >> 16);
            rpEnableWithPaymentCommand.Info[3] = (byte) (amountInPennies >> 8);
            rpEnableWithPaymentCommand.Info[4] = (byte) amountInPennies;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(currencyCode.ToString()), 0, rpEnableWithPaymentCommand.Info, 5,
                3);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(((Int32) currencyCode).ToString()), 0,
                rpEnableWithPaymentCommand.Info, 8, 3);
            if (null != mode) rpEnableWithPaymentCommand.Info[11] = (byte) mode;
            Buffer.BlockCopy(clientTransactionIdBytes, 0, rpEnableWithPaymentCommand.Info, 12, 51);
            rpEnableWithPaymentCommand.FriendlyName = "0x06 Enable with Payment Command";

            rpEnableWithPaymentCommand.CompileCommand();
            RpResponseBase rpPaymentCommandResponse = new RpResponseBase(rpEnableWithPaymentCommand.Execute());

            return rpPaymentCommandResponse;
        }

        /// <summary>
        /// CMD 0x19
        /// </summary>
        /// <param name="authType">0x00 val for payment, 0x01 val for preauth)</param>
        /// <param name="activeTime">Time in seconds the reader is active</param>
        /// <param name="amountInPennies">Import to pay in Euro Cent</param>
        /// <param name="currencyCode">Currency Code for the sale</param>
        /// <param name="mode">0x30 = online, 0x31 = offline, 0x32 = online w/backup</param>
        /// <param name="clientTransactionID">Invoice# passed from the POS system to the gateway</param>
        /// <returns></returns>
        public RpResponseBase RP_EnableWithPayment(byte authType, int activeTime, int amountInPennies, ECurrencies? currencyCode, EMode? mode, string clientTransactionID)
        { 
            if (amountInPennies< 0)
            {
                throw new Exception("Illegal amount requested for payment");
            }

            if (null == currencyCode)
            {
                currencyCode = ECurrencies.USD;
            }
            if (null == clientTransactionID)
            {
            clientTransactionID = "";
            }

            RpCommand rpEnableWithPaymentCommand = new RpCommand(this);
            if (clientTransactionID.Length > 50) clientTransactionID = clientTransactionID.Substring(0, 50);
            byte[] clientTransactionIdBytes = StringToByteArray(clientTransactionID, 51);

            rpEnableWithPaymentCommand.UPTAddress = 0x00;
            rpEnableWithPaymentCommand.CommandId = 0x19;
            rpEnableWithPaymentCommand.InfoLength = 64;

            rpEnableWithPaymentCommand.Info[0] = authType;
            rpEnableWithPaymentCommand.Info[1] = (byte)(activeTime);
            rpEnableWithPaymentCommand.Info[2] = (byte)(amountInPennies >> 24);
            rpEnableWithPaymentCommand.Info[3] = (byte)(amountInPennies >> 16);
            rpEnableWithPaymentCommand.Info[4] = (byte)(amountInPennies >> 8);
            rpEnableWithPaymentCommand.Info[5] = (byte)amountInPennies;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(currencyCode.ToString()), 0, rpEnableWithPaymentCommand.Info, 6, 3);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(((Int32)currencyCode).ToString()), 0, rpEnableWithPaymentCommand.Info, 9, 3
            );
            if (null != mode) rpEnableWithPaymentCommand.Info[12] = (byte)mode;
            Buffer.BlockCopy(clientTransactionIdBytes, 0, rpEnableWithPaymentCommand.Info, 13, 51);
            rpEnableWithPaymentCommand.FriendlyName = "0x19 Enable with Payment Command";

            rpEnableWithPaymentCommand.CompileCommand();
            RpResponseBase rpPaymentCommandResponse = new RpResponseBase(rpEnableWithPaymentCommand.Execute());

            return rpPaymentCommandResponse;
        }

        /// <summary>
        /// 0x6A GET UPGRADE RESULT
        /// This command is used to get the status of the last upgrade.
        /// </summary>
        /// <returns>RpResponseUpgradeResult</returns>
        public RpResponseUpgradeResult RP_GetUpgradeResult()
        {
            RpCommand rpGetUpgrade = new RpCommand(this);
            rpGetUpgrade.UPTAddress = 0x00;
            rpGetUpgrade.CommandId = 0x6A;
            rpGetUpgrade.InfoLength = 0;

            
            rpGetUpgrade.CompileCommand();
            rpGetUpgrade.FriendlyName = "0x6A Get Upgrade Result";

            RpResponseUpgradeResult rpResult = new RpResponseUpgradeResult(rpGetUpgrade.Execute());

            return rpResult;
        }

        /// <summary>
        /// 0x69 INSTALL PACKET
        /// This command is used to install a packet loaded with 0x68 command (some updates may not return the UPT to PU message).
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_InstallPacket(int ewt = 300)
        {
            RpCommand rpInstallPacket = new RpCommand(this);
            rpInstallPacket.UPTAddress = 0x00;
            rpInstallPacket.CommandId = 0x69;
            rpInstallPacket.InfoLength = 0;
            rpInstallPacket.FriendlyName = "0x69 Install Packet";
            rpInstallPacket.ewt = ewt; // Expect to wait 5 minutes for CL.

            rpInstallPacket.CompileCommand();

            RpResponseBase rpResult = new RpResponseBase(rpInstallPacket.Execute());

            return rpResult;
        }

        /// <summary>
        /// 0x30 OFFLINE TRANSACTION STATUS
        /// This command is used to get the number of offline transactions stored on the UPT.
        /// </summary>
        /// <returns>RpResponseMSFStatus</returns>
        public RpResponseMsfStatus RP_MSFStatus()
        {
            RpCommand rpStatusRequest = new RpCommand(this);
            rpStatusRequest.UPTAddress = 0x00;
            rpStatusRequest.CommandId = 0x30;
            rpStatusRequest.InfoLength = 0;
            rpStatusRequest.CompileCommand();
            rpStatusRequest.FriendlyName = "0x30 Mag Store and Forward Status";

            RpResponseMsfStatus rpMsfStatusResult = new RpResponseMsfStatus(rpStatusRequest.Execute());
            return rpMsfStatusResult;
        }

        /// <summary>
        /// 0x31 SETTLEMENT OFFLINE TRANSACTION
        /// This command is used to settle the first offline transaction on the UPT.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_MSFSettle()
        {
            RpCommand rpMsfSettle = new RpCommand(this);
            rpMsfSettle.UPTAddress = 0x00;
            rpMsfSettle.CommandId = 0x31;
            rpMsfSettle.InfoLength = 0;
            rpMsfSettle.CompileCommand();
            rpMsfSettle.FriendlyName = "0x31 Mag Store and Forward Settle";

            RpResponseBase rpMsfSettleResponse = new RpResponseBase(rpMsfSettle.Execute());
            return rpMsfSettleResponse;
        }

        /// <summary>
        /// 0x32 SETTLEMENT OUTCOME REQUEST
        /// The PU uses this command to request at the terminal the outcome of the settlement executed.
        /// </summary>
        /// <returns>RpResponseMSFOutcome</returns>
        public RpResponseMsfOutcome RP_MSFSettlementOutcome()
        {
            RpCommand rpMsfOutcome = new RpCommand(this);
            rpMsfOutcome.UPTAddress = 0x00;
            rpMsfOutcome.CommandId = 0x32;
            rpMsfOutcome.InfoLength = 0;
            rpMsfOutcome.CompileCommand();
            rpMsfOutcome.FriendlyName = "0x32 Mag Store and Forward Settlement Outcome";

            RpResponseMsfOutcome rpMsfSettleOutcomeResponse = new RpResponseMsfOutcome(rpMsfOutcome.Execute());
            return rpMsfSettleOutcomeResponse;
        }

        /// <summary>
        /// 0x33 DELETE FIRST/ALL OFFLINE TRANSACTION
        /// The PU uses this command to delete the first (or all) offline transaction.
        /// </summary>
        /// <param name="mode">Mode (0 = first transaction; 1 = all transactions)</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_MSFDelete(byte mode)
        {
            if (!((0x00 == mode) || (0x01 == mode)))
            {
                throw new Exception("Illegal mode for Delete MSF Operation.");
            }

            RpCommand rpMsfDelete = new RpCommand(this);
            rpMsfDelete.UPTAddress = 0x00;
            rpMsfDelete.CommandId = 0x33;
            rpMsfDelete.InfoLength = 1;
            rpMsfDelete.Info[0] = mode;
            rpMsfDelete.FriendlyName = "0x33 Mag Store and Forward Settlement Delete";

            rpMsfDelete.CompileCommand();

            RpResponseBase rpMsfDeleteResponse = new RpResponseBase(rpMsfDelete.Execute());
            return rpMsfDeleteResponse;
        }

        /// <summary>
        /// 0x04 CARD HASH
        /// This command is used to get the unique hash of the magnetic card.
        /// </summary>
        /// <returns>RpResponseHash</returns>
        public RpResponseHash RP_Hash(bool getMaskedPan, bool getExpiryDate, bool getCardHolderName, bool getHash)
        {
            BitArray infoMsg = new BitArray(8);

            if (getMaskedPan) infoMsg.Set(0,true);
            if (getExpiryDate) infoMsg.Set(1,true);
            if (getCardHolderName) infoMsg.Set(2,true);
            if (!getHash) infoMsg.Set(4, true);  // if its off

            RpCommand rpHash = new RpCommand(this);
            rpHash.UPTAddress = 0x00;
            rpHash.CommandId = 0x04;
            rpHash.InfoLength = 0;
            rpHash.InfoLength = 1;
            infoMsg.CopyTo(rpHash.Info,0);
            
            rpHash.FriendlyName = "0x04 Card Hash";

            rpHash.CompileCommand();

            RpResponseHash rpHashResponse = new RpResponseHash(rpHash.Execute());
            return rpHashResponse;
        }

        /// <summary>
        /// 0x68 UPGRADE PACKET
        /// This command is used to download an sd5 application or a sd5 file into the terminal.
        /// </summary>
        /// <param name="packetIndex">Progressive packet Number</param>
        /// <param name="dataPacket">Data to upload</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_UploadPacket(int packetIndex, byte[] dataPacket, bool isHRT = false)
        {
            if (!isHRT && dataPacket.Length > WindowSize)
            {
                throw new Exception("0x68 Upload Packet - Data Packet too Long.");
            }

            RpCommand rpInstallPacket = new RpCommand(this);
            rpInstallPacket.UPTAddress = 0x00;
            rpInstallPacket.CommandId = 0x68;
            rpInstallPacket.InfoLength = (ushort)(dataPacket.Length + 4);
            rpInstallPacket.Info[0] = (byte)(packetIndex >> 24);
            rpInstallPacket.Info[1] = (byte)(packetIndex >> 16);
            rpInstallPacket.Info[2] = (byte)(packetIndex >> 8);
            rpInstallPacket.Info[3] = (byte)packetIndex;
            Buffer.BlockCopy(dataPacket,0,rpInstallPacket.Info,4,dataPacket.Length);
            rpInstallPacket.FriendlyName = "0x68 Upload Packet" + Environment.NewLine + "--Index:" +
                                           packetIndex.ToString();
            rpInstallPacket.ewt = 20;


            rpInstallPacket.CompileCommand();

            RpResponseBase rpResult = new RpResponseBase(rpInstallPacket.Execute());

            return rpResult;
        }

        /// <summary>
        /// 0x70 LOG TYPE
        /// This command is used to selectively enable LOG filtered by LOG Type.
        /// With this command is possible to select the LOG Type to enable.
        /// </summary>
        /// <param name="logType">
        /// LOG Type: 
        ///     Every bit identifies a LOG Type.If the bit is 1 the corresponding type is enabled otherwise the type is disabled.
        ///     he Bytes takes the value 0 the LOG system is disabled.
        /// </param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_SetLogType(int logType)
        {
            RpCommand rpSetLogType = new RpCommand(this);
            rpSetLogType.UPTAddress = 0x00;
            rpSetLogType.CommandId = 0x70;
            rpSetLogType.InfoLength = 2;
            rpSetLogType.Info[0] = (byte)(logType >> 8);
            rpSetLogType.Info[1] = (byte)logType;
            rpSetLogType.FriendlyName = "0x70 Set Log Type";

            rpSetLogType.CompileCommand();
            RpResponseBase rpResult = new RpResponseBase(rpSetLogType.Execute());
            return rpResult;
        }

        /// <summary>
        /// 0x71 LOG DELETE
        /// This command is used to delete all saved LOG.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_DeleteLog()
        {
            RpCommand rpDeleteLog = new RpCommand(this);
            rpDeleteLog.UPTAddress = 0x00;
            rpDeleteLog.CommandId = 0x71;
            rpDeleteLog.ewt = 20; //this can take a while - set expected wait time to 20 seconds.
            rpDeleteLog.InfoLength = 0;
            rpDeleteLog.FriendlyName = "0x71 Log Delete";

            rpDeleteLog.CompileCommand();
            RpResponseBase rpResult = new RpResponseBase(rpDeleteLog.Execute());
            return rpResult;
        }

        /// <summary>
        /// 0x72 GET LOG
        /// This command is used to get saved LOG.
        /// </summary>
        /// <returns>byte[] of Log</returns>
        public byte[] RP_GetLog(bool readall)
        {
            

            RpCommand rpGetLog = new RpCommand(this);

            rpGetLog.UPTAddress = 0x00;
            rpGetLog.CommandId = 0x72;
            rpGetLog.InfoLength = 1;
            rpGetLog.Info[0] = (readall) ? (byte)0x01 : (byte)0x00; //Start from the beginning?
            rpGetLog.FriendlyName = "0x72 Get Log";

            rpGetLog.CompileCommand();
            
            RpResponseBase rpGetLogResponse = new RpResponseBase(rpGetLog.Execute());
            byte[] ret = new byte[rpGetLogResponse.Info.Length ];
            Buffer.BlockCopy(rpGetLogResponse.Info,0,ret,0,rpGetLogResponse.Info.Length);
            
            

            while (rpGetLogResponse.InfoLength == WindowSize)
            {
                rpGetLog.Info[0] = 0x00; // read from last POS
                rpGetLog.FriendlyName = "0x72 Get Log";
                rpGetLog.CompileCommand();

                rpGetLogResponse = new RpResponseBase(rpGetLog.Execute());
                if (rpGetLogResponse.VerifyOutcome() != EOutcome.OK) throw new Exception("Error Reading Log data." + Environment.NewLine + "Outcome:" + rpGetLogResponse.VerifyOutcome());

                
                ret = ByteArrayCombine(ret, rpGetLogResponse.Info, rpGetLogResponse.Info.Length);
            }
            return ret;
        }


        /// <summary>
        /// 0x73 STATISTICS ON/OFF
        /// This command is used to enable statistical counters.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_EnableStatistics(bool enable)
        {
            RpCommand rpEnableStatistics = new RpCommand(this);
            rpEnableStatistics.UPTAddress = 0x00;
            rpEnableStatistics.CommandId = 0x73;
            rpEnableStatistics.InfoLength = 1;
            rpEnableStatistics.Info[0] = (enable) ? (byte)0x01 : (byte)0x00;
            rpEnableStatistics.FriendlyName = "0x73 Enable Statistics";
            rpEnableStatistics.CompileCommand();
            RpResponseBase rpResult = new RpResponseBase(rpEnableStatistics.Execute());
            return rpResult;
        }


        /// <summary>
        /// 0x74 STATISTICS ERASE
        /// This command is used to erase statistical counters file.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_EraseStatistics()
        {
            RpCommand rpEraseStatistics = new RpCommand(this);
            rpEraseStatistics.UPTAddress = 0x00;
            rpEraseStatistics.CommandId = 0x74;
            rpEraseStatistics.InfoLength = 0;
            rpEraseStatistics.FriendlyName = "0x74 Erase Statistics";
            rpEraseStatistics.CompileCommand();
            RpResponseBase rpResult = new RpResponseBase(rpEraseStatistics.Execute());
            return rpResult;
        }

        /// <summary>
        /// 0x75 GET STATISTICS 
        /// This command is used to get statistical counters by reading them from a file contained in device file system.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseStatistics RP_GetStatistics()
        {
            RpCommand rpGetStatistics = new RpCommand(this);
            rpGetStatistics.UPTAddress = 0x00;
            rpGetStatistics.CommandId = 0x75;
            rpGetStatistics.InfoLength = 0;
            rpGetStatistics.FriendlyName = "0x75 Get Statistics";
            rpGetStatistics.CompileCommand();
            RpResponseStatistics rpResult = new RpResponseStatistics(rpGetStatistics.Execute());
            return rpResult;
        }

        /// <summary>
        /// 0x76 TMS ADDRESS 
        /// This command is used to provide to TMS the IP address or URL where upgrade packets are available.
        /// </summary>
        /// <param name="ipAddressOrUrl">IP Address or URL of the TMS system used by this terminal</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_SetTmsAddress(string ipAddressOrUrl)
        {
            if (ipAddressOrUrl.Length < 3) throw new Exception("Illegal IP/URL."); //size of 3 is arbitrary
            byte[] url = Encoding.UTF8.GetBytes(ipAddressOrUrl);

            RpCommand rpSetTmsAddress = new RpCommand(this);
            rpSetTmsAddress.UPTAddress = 0x00;
            rpSetTmsAddress.CommandId = 0x76;
            rpSetTmsAddress.InfoLength = (ushort)url.Length;
            rpSetTmsAddress.FriendlyName = "0x76 TMS Address";
            Buffer.BlockCopy(url,0,rpSetTmsAddress.Info,0,url.Length);
            rpSetTmsAddress.CompileCommand();
            RpResponseBase rpResult = new RpResponseBase((rpSetTmsAddress.Execute()));
            return rpResult;
        }

        /// <summary>
        /// 0x77 TMS GET STATUS
        /// This command is used to read TMS status.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_GetTmsStatus()
        {
            RpCommand rpGetTmsStatus = new RpCommand(this);
            rpGetTmsStatus.UPTAddress = 0x00;
            rpGetTmsStatus.CommandId = 0x77;
            rpGetTmsStatus.InfoLength = 0;
            rpGetTmsStatus.FriendlyName = "0x77 TMS Get Status";
            rpGetTmsStatus.CompileCommand();
            RpResponseBase rpResult = new RpResponseBase(rpGetTmsStatus.Execute());
            return rpResult;
        }

        /// <summary>
        /// 0x78 TMS INSTALL AND REBOOT
        /// This command is used to install upgrade and allows to select if terminal must be restarted.
        /// </summary>
        /// <param name="rebootTerminal">Flag to reboot the terminal if desired</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_TmsInstallAndReboot(bool rebootTerminal)
        {
            RpCommand rpGetTmsStatus = new RpCommand(this);
            rpGetTmsStatus.UPTAddress = 0x00;
            rpGetTmsStatus.CommandId = 0x78;
            rpGetTmsStatus.InfoLength = 1;
            rpGetTmsStatus.Info[0] = (rebootTerminal) ? (byte)0x01 : (byte)0x00;
            rpGetTmsStatus.FriendlyName = "0x78 TMS Install and Reboot";
            rpGetTmsStatus.CompileCommand();
            RpResponseBase rpResult = new RpResponseBase(rpGetTmsStatus.Execute());
            return rpResult;
        }

        /// <summary>
        /// 0x6D GET Key Fingerprints
        /// This command is used to get the fingerprints of all the keys loaded on the terminal.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_GetKeyInformation()
        {
            RpCommand rpGetKeyInfo = new RpCommand(this);

            rpGetKeyInfo.UPTAddress = 0x00;
            rpGetKeyInfo.CommandId = 0x6D;
            rpGetKeyInfo.InfoLength = 0;
            rpGetKeyInfo.FriendlyName = "0x6D Get Key Fingerprints";

            rpGetKeyInfo.CompileCommand();

            RpResponseBase rpGetFingerprintResponse = new RpResponseBase(rpGetKeyInfo.Execute());


            return rpGetFingerprintResponse;
        }

        /// <summary>
        /// 0x6D GET Key Fingerprints
        /// This command is used to get the fingerprints of all the keys loaded on the terminal.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseKeyInfo RP_GetKeyInformationV2()
        {
            RpCommand rpGetKeyInfo = new RpCommand(this);

            rpGetKeyInfo.UPTAddress = 0x00;
            rpGetKeyInfo.CommandId = 0x6D;
            rpGetKeyInfo.InfoLength = 0;
            rpGetKeyInfo.FriendlyName = "0x6D Get Key Fingerprints";

            rpGetKeyInfo.CompileCommand();

            RpResponseKeyInfo rpGetKeyInfoResponse = new RpResponseKeyInfo(rpGetKeyInfo.Execute());
            return rpGetKeyInfoResponse;
        }



        /// <summary>
        /// 0x02 FIRMWARE VERSION REQUEST
        /// This command is used to know the firmware version of the UPT master and the firmware versions of all the UPT slaves.
        /// Whenever is not possible to know the firmware version of a device, the field will be filled with spaces.
        /// The answer message contains a list of the firmware versions of all the UPTs.
        /// </summary>
        /// <returns>RpResponseFirmwareVersion</returns>
        public RpResponseFirmwareVersion RP_FirmwareVersionRequest()
        {
            RpCommand rpFirmwareVersion = new RpCommand(this);

            rpFirmwareVersion.UPTAddress = 0x00;
            rpFirmwareVersion.CommandId = 0x02;
            rpFirmwareVersion.InfoLength = 0;

            rpFirmwareVersion.CompileCommand();
            rpFirmwareVersion.FriendlyName = "0x02 Firmware Version";

            RpResponseFirmwareVersion rpFirmwareVersionResponse = new RpResponseFirmwareVersion(rpFirmwareVersion.Execute());
            //BytesInCounter = 0;

            return rpFirmwareVersionResponse;
        }

        /// <summary>
        /// 0x01 INFO ERASE
        /// This command is used to erase the information of the status message defined as “outcome”.
        /// </summary>
        /// <param name="close">This byte take the value 1 to indicate to the UPT that no more request will arrive from the last card, 
        /// this means that the operations on the card are ended, so the UPT must send the closing message to the user.
        /// This byte take the value 0 in all the other cases.</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_InfoErase(Boolean close)
        {
            RpCommand rpInfoErase = new RpCommand(this);
            
            rpInfoErase.UPTAddress = 0x00;
            rpInfoErase.CommandId = 0x01;
            rpInfoErase.InfoLength = 1;

            rpInfoErase.Info[0] = (close) ?  (byte) 0x01 :  (byte) 0x00;

            rpInfoErase.CompileCommand();
            rpInfoErase.FriendlyName = "0x01 Erase Info";
            rpInfoErase.ewt = 20;

            RpResponseBase rpInfoEraseResponse = new RpResponseBase(rpInfoErase.Execute());
            
            return rpInfoEraseResponse;
        }

        /// <summary>
        /// 0x80 Journal File System Writes of KeyFiles
        /// When enabled this function causes all writes of the key files to be journaled to prevent file system corruption that can happen from sudden loss of power during writes.
        /// Benefit = key files are backed up and compared to hashes of prior versions
        /// Side effect = Approx +1 second to transaction times.
        /// </summary>
        /// <param name="enableJournal">True to enable </param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_JournalKeyWrites(Boolean enableJournal)
        {
            RpCommand rpJournalKeyWrites = new RpCommand(this);

            rpJournalKeyWrites.UPTAddress = 0x00;
            rpJournalKeyWrites.CommandId = 0x80;
            rpJournalKeyWrites.InfoLength = 1;

            rpJournalKeyWrites.Info[0] = (enableJournal) ? (byte) 0x01 : (byte) 0x00;

            rpJournalKeyWrites.CompileCommand();
            rpJournalKeyWrites.FriendlyName = "0x80 Journal Key Writes";
            rpJournalKeyWrites.ewt = 20;

            RpResponseBase rpJournalKeyWritesResponseBase = new RpResponseBase(rpJournalKeyWrites.Execute());
            return rpJournalKeyWritesResponseBase;
        }


        /// <summary>
        /// 0x82 Batch Closure
        /// This command instructs the UPT to execute the Batch CLosuer towards the host. Upon successful execution,
        /// the UPT totals are zeroed and the Block 1 returned in the Status command (0x0A) is set to 0x17="Batch Closure outcome available"
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_BatchClosure()
        {
            RpCommand rpBatchClosure = new RpCommand(this);
            rpBatchClosure.UPTAddress = 0x00;
            rpBatchClosure.CommandId = 0x82;
            rpBatchClosure.InfoLength = 0;
            rpBatchClosure.FriendlyName = "0x82 Batch Closure";
            rpBatchClosure.CompileCommand();
            RpResponseBase rpResult = new RpResponseBase(rpBatchClosure.Execute());
            return rpResult;
        }

        /// <summary>
        /// 0x83 Retrieve Batch Closure Outcome
        /// Upon completion of the Batch closure, when Block 1 returned in the Status command (0x0A) is 0x17="Batch Closure outcome available",
        /// this command is intended to retrieve from teh UPS the result and the Totals of the Batch Closure.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_BatchClosureOutcome(byte index)
        {
            if (index != 0x00) index = 0x01; //only 0x00 and 0x01 are legal. If illegal - make it 0x01.

            RpCommand rpBatchClosureOutcome = new RpCommand(this);
            rpBatchClosureOutcome.UPTAddress = 0x00;
            rpBatchClosureOutcome.CommandId = 0x83;
            rpBatchClosureOutcome.InfoLength = 1;
            rpBatchClosureOutcome.Info[0] = index;
            rpBatchClosureOutcome.FriendlyName = "0x83 Batch Closure Outcome";
            rpBatchClosureOutcome.CompileCommand();
            RpResponseBase rpResult = new RpResponseBase(rpBatchClosureOutcome.Execute());
            return rpResult;
        }

        /// <summary>
        /// 0x84 Retrieve Totals
        /// This command is intended to retrieve from the UPT the Totals of the Batches.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_BatchRetrieveTotals(byte index)
        {
            if (index > 0x09) index = 0x00; // 0-9 is legal

            RpCommand rpBatchRetrieveTotals = new RpCommand(this);
            rpBatchRetrieveTotals.UPTAddress = 0x00;
            rpBatchRetrieveTotals.CommandId = 0x84;
            rpBatchRetrieveTotals.InfoLength = 1;
            rpBatchRetrieveTotals.Info[0] = index;
            rpBatchRetrieveTotals.FriendlyName = "0x84 Retrieve Totals";
            rpBatchRetrieveTotals.CompileCommand();
            RpResponseBase rpResult = new RpResponseBase(rpBatchRetrieveTotals.Execute());
            return rpResult;
        }

        /// <summary>
        /// 0x1D Get Receipt Text
        /// The PU uses this command to return the TXT of the receipt to be printed by the kiosk
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public string RP_GetReceiptText()
        {
            StringBuilder ret = new StringBuilder();

            RpCommand rpGetReceiptText = new RpCommand(this);

            rpGetReceiptText.UPTAddress = 0x00;
            rpGetReceiptText.CommandId = 0x1D;
            rpGetReceiptText.InfoLength = 4;
            rpGetReceiptText.Info[0] = 0x03;
            rpGetReceiptText.Info[1] = 0xE8;
            rpGetReceiptText.Info[2] = 0x00;
            rpGetReceiptText.Info[3] = 0x00;
            rpGetReceiptText.FriendlyName = "Get Receipt Text";

            rpGetReceiptText.CompileCommand();

            RpResponseReceipt rpGetReceiptTextResponse = new RpResponseReceipt(rpGetReceiptText.Execute());
            ret.Append(rpGetReceiptTextResponse.FullReceipt);
            int fileSize = rpGetReceiptTextResponse.ReceiptLength;

            while (ret.Length < fileSize)
            {
                //If there is more than 1000 bytes remaining, set the size to 1000, otherwise set the size to what is remaining.
                int remaining = ((fileSize - ret.Length) > 1000) ? 1000 : (fileSize - ret.Length);

                rpGetReceiptText.Info[0] = (byte)(remaining >> 8);
                rpGetReceiptText.Info[1] = (byte)(remaining);
                rpGetReceiptText.Info[2] = (byte)(ret.Length >> 8);
                rpGetReceiptText.Info[3] = (byte)(ret.Length);
                rpGetReceiptText.FriendlyName = "0x1D Get Receipt Text";

                rpGetReceiptText.CompileCommand();
                rpGetReceiptTextResponse = new RpResponseReceipt(rpGetReceiptText.Execute());
                if (rpGetReceiptTextResponse.VerifyOutcome() != EOutcome.OK) throw new Exception("Error reading receipt text data."+"Outcome:"+rpGetReceiptTextResponse.VerifyOutcome());
                ret.Append(rpGetReceiptTextResponse.FullReceipt);
            }
            return ret.ToString();
        }

        /// <summary>
        /// Get Exteneded Firmware Information
        /// </summary>
        /// <returns>RpResponseExtFirmwareVersion</returns>
        public RpResponseExtFirmwareVersion RP_Ext_GetFirmwareVersion(ExtFirmwareVersionType versionType)
        {
            RpCommand rpGetExtFirmwareVersion = new RpCommand(this);

            rpGetExtFirmwareVersion.UPTAddress = 0x00;
            rpGetExtFirmwareVersion.CommandId = 0x00;
            rpGetExtFirmwareVersion.InfoLength = 2;
            rpGetExtFirmwareVersion.Info[0] = 0x02;
            rpGetExtFirmwareVersion.Info[1] = (byte)versionType;
            rpGetExtFirmwareVersion.ewt = 2;
            rpGetExtFirmwareVersion.FriendlyName = "Ext-0x02 Get Firmware Version";

            rpGetExtFirmwareVersion.CompileCommand();

            RpResponseExtFirmwareVersion rpGetExtFirmwareVersionResp = new RpResponseExtFirmwareVersion(rpGetExtFirmwareVersion.Execute(), versionType);
            
            return rpGetExtFirmwareVersionResp;
        }

        /// <summary>
        /// Get Device Family
        /// </summary>
        /// <returns>RpResponseDeviceFamily</returns>
        public RpResponseDeviceFamily RP_Ext_GetDeviceFamily()
        {
            RpCommand rpGetDeviceFamily = new RpCommand(this);

            rpGetDeviceFamily.UPTAddress = 0x00;
            rpGetDeviceFamily.CommandId = 0x00;
            rpGetDeviceFamily.InfoLength = 1;
            rpGetDeviceFamily.Info[0] = 0xD0;
            rpGetDeviceFamily.ewt = 2;
            rpGetDeviceFamily.FriendlyName = "Ext-0xD0 Get Device Family";

            rpGetDeviceFamily.CompileCommand();
            //if (!rpGetReceiptXml.VerifyChecksum()) throw new Exception("Invalid Checksum.");

            RpResponseDeviceFamily rpGetDeviceFamilyResponse = new RpResponseDeviceFamily(rpGetDeviceFamily.Execute());
            
            return rpGetDeviceFamilyResponse;
        }


        /// <summary>
        /// 0x1C Get Receipt XML
        /// The PU uses this command to return the XML of the receipt to be printed by the kiosk
        /// </summary>
        /// <returns>String of XML or empty string if no receipt available</returns>
        public string RP_GetReceiptXML()
        {
            StringBuilder ret = new StringBuilder();

            RpCommand rpGetReceiptXml = new RpCommand(this);

            rpGetReceiptXml.UPTAddress = 0x00;
            rpGetReceiptXml.CommandId = 0x1C;
            rpGetReceiptXml.InfoLength = 4;
            rpGetReceiptXml.Info[0] = 0x03; // 1k (max)
            rpGetReceiptXml.Info[1] = 0xE8;
            rpGetReceiptXml.Info[2] = 0x00; //the buffer beginning
            rpGetReceiptXml.Info[3] = 0x00;
            rpGetReceiptXml.FriendlyName = "0x1C Get Receipt Text";

            rpGetReceiptXml.CompileCommand();
            //if (!rpGetReceiptXml.VerifyChecksum()) throw new Exception("Invalid Checksum.");

            RpResponseReceipt rpGetReceiptTextResponse = new RpResponseReceipt(rpGetReceiptXml.Execute());
            if (rpGetReceiptTextResponse.VerifyOutcome() != EOutcome.OK)
                return (""); // Dont think a throw is the right response

            ret.Append(rpGetReceiptTextResponse.FullReceipt);
            int fileSize = rpGetReceiptTextResponse.ReceiptLength;

            while (ret.Length < fileSize)
            {
                //If there is more than 1000 bytes remaining, set the size to 1000, otherwise set the size to what is remaining.
                int remaining = ((fileSize - ret.Length) > 1000) ? 1000 : (fileSize - ret.Length);

                rpGetReceiptXml.Info[0] = (byte) (remaining >> 8);
                rpGetReceiptXml.Info[1] = (byte) (remaining);
                rpGetReceiptXml.Info[2] = (byte) (ret.Length >> 8);
                rpGetReceiptXml.Info[3] = (byte) (ret.Length);
                rpGetReceiptXml.FriendlyName = "0x1C Get Receipt Text";


                rpGetReceiptXml.CompileCommand();
                //if (!rpGetReceiptXml.VerifyChecksum()) throw new Exception("Invalid Checksum.");

                rpGetReceiptTextResponse = new RpResponseReceipt(rpGetReceiptXml.Execute());
                if (rpGetReceiptTextResponse.VerifyOutcome() != EOutcome.OK)
                    return ("");  // Dont think a throw is the right response

                ret.Append(rpGetReceiptTextResponse.FullReceipt);
            }

            return ret.ToString();
        }


        /// <summary>
        /// 0x79 LED MANAGE
        /// This command is used to set the status and the color of the led.
        /// </summary>
        /// <param name="ledColor">LED Color</param>
        /// <param name="ledStatus">LED state</param>
        public RpResponseBase RP_SetLED(ELEDColor ledColor, ELEDStatus ledStatus)
        {
            RpCommand rpSetLed = new RpCommand(this);

            rpSetLed.UPTAddress = 0x00;
            rpSetLed.CommandId = 0x79;
            rpSetLed.InfoLength = 2;
            rpSetLed.Info[0] = (byte)ledColor;
            rpSetLed.Info[1] = (byte)ledStatus;
            rpSetLed.FriendlyName = "0x79 LED Manage";

            rpSetLed.CompileCommand();
            
            RpResponseReceipt rpSetLedResponse = new RpResponseReceipt(rpSetLed.Execute());
            return rpSetLedResponse;
        }


        // ECR Mode

        /// <summary>
        /// 0xE0 ECR PAYMENT START
        /// This command is used to start a payment operation.
        /// </summary>
        /// <param name="amountInPennies">Import to pay in Euro Cent</param>
        /// <param name="currencyCode">Currency Code for the sale</param>
        /// <param name="mode">0x30 = online, 0x31 = offline, 0x32 = online w/backup</param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_EcrPaymentCommand(int amountInPennies, ECurrencies? currencyCode, EMode? mode)
        {
            if (amountInPennies < 0)
            {
                throw new Exception("Illegal amount requested for payment");
            }
            if (null == currencyCode)
            {
                currencyCode = ECurrencies.USD;
            }

            RpCommand rpEcrPaymentCommand = new RpCommand(this);

            rpEcrPaymentCommand.UPTAddress = 0x00;
            rpEcrPaymentCommand.CommandId = 0xE0;
            rpEcrPaymentCommand.InfoLength = 11;
            rpEcrPaymentCommand.EcrStopCommand = 0xE1;
            rpEcrPaymentCommand.Info[0] = (byte)(amountInPennies >> 24);
            rpEcrPaymentCommand.Info[1] = (byte)(amountInPennies >> 16);
            rpEcrPaymentCommand.Info[2] = (byte)(amountInPennies >> 8);
            rpEcrPaymentCommand.Info[3] = (byte)amountInPennies;
            rpEcrPaymentCommand.FriendlyName = "0xE0 Payment Start";
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(currencyCode.ToString()), 0, rpEcrPaymentCommand.Info, 4, 3);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(((Int32)currencyCode).ToString()), 0, rpEcrPaymentCommand.Info, 7, 3);
            if (null != mode) rpEcrPaymentCommand.Info[10] = (byte)mode;

            rpEcrPaymentCommand.CompileCommand();
            RpResponseBase rpEcrPaymentCommandResponse = new RpResponseBase(rpEcrPaymentCommand.Execute());
//            EcrStopMessage = (rpEcrPaymentCommandResponse.VerifyOutcome() == EOutcome.OK) ? (byte)0xE1 : (byte)0x00; //If the ECR command is ERROR, Disable ECR
            return rpEcrPaymentCommandResponse;
        }

        /// <summary>
        /// 0xE2 ECR PREAUTHORIZATION COMMAND
        /// This command is used to start a preauthorization operation. 
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_EcrPreAuthorization(int amountInPennies, ECurrencies currencyCode)
        {
            RpCommand rpEcrPreAuthorizationRequest = new RpCommand(this);

            rpEcrPreAuthorizationRequest.UPTAddress = 0x00;
            rpEcrPreAuthorizationRequest.CommandId = 0xE2;
            rpEcrPreAuthorizationRequest.InfoLength = 7;
            rpEcrPreAuthorizationRequest.EcrStopCommand = 0xE3;
            rpEcrPreAuthorizationRequest.Info[0] = (byte)(amountInPennies >> 24);
            rpEcrPreAuthorizationRequest.Info[1] = (byte)(amountInPennies >> 16);
            rpEcrPreAuthorizationRequest.Info[2] = (byte)(amountInPennies >> 8);
            rpEcrPreAuthorizationRequest.Info[3] = (byte)amountInPennies;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(currencyCode.ToString()), 0, rpEcrPreAuthorizationRequest.Info, 4, 3);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(((Int32)currencyCode).ToString()), 0, rpEcrPreAuthorizationRequest.Info, 7, 3);



            rpEcrPreAuthorizationRequest.CompileCommand();
            rpEcrPreAuthorizationRequest.FriendlyName = "0xE2 ECR Pre Authorization Request";

            RpResponseBase rpEcrPreAuthorizationResponse = new RpResponseBase(rpEcrPreAuthorizationRequest.Execute());
            //EcrStopMessage = (rpEcrPreAuthorizationResponse.VerifyOutcome() == EOutcome.OK) ? (byte)0xE3 : (byte)0x00; //If the ECR command is ERROR, Disable ECR

            return rpEcrPreAuthorizationResponse;
        }


        /// <summary>
        /// 0xE4 NOTIFY COMMAND
        /// This command is used to request a notification of a previous preauthorization. 
        /// </summary>
        /// <param name="preauthId">ID to Notify</param>
        /// <param name="amountInPennies">Amount </param>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_EcrNotifyCommand(byte preauthId, int amountInPennies)
        {
            RpCommand rpEcrNotifyCommandRequest = new RpCommand(this);


            rpEcrNotifyCommandRequest.UPTAddress = 0x00;
            rpEcrNotifyCommandRequest.CommandId = 0xE4;
            rpEcrNotifyCommandRequest.InfoLength = 5;
            rpEcrNotifyCommandRequest.EcrStopCommand = 0xE5;
            rpEcrNotifyCommandRequest.Info[0] = preauthId;
            rpEcrNotifyCommandRequest.Info[1] = (byte)(amountInPennies >> 24);
            rpEcrNotifyCommandRequest.Info[2] = (byte)(amountInPennies >> 16);
            rpEcrNotifyCommandRequest.Info[3] = (byte)(amountInPennies >> 8);
            rpEcrNotifyCommandRequest.Info[4] = (byte)amountInPennies;
            rpEcrNotifyCommandRequest.FriendlyName = "0xE4 ECR Notify Command Request";

            rpEcrNotifyCommandRequest.CompileCommand();

            RpResponseBase rpEcrNotifyCommandResponse = new RpResponseBase((rpEcrNotifyCommandRequest.Execute()));
            //EcrStopMessage = (rpEcrNotifyCommandResponse.VerifyOutcome() == EOutcome.OK) ? (byte)0xE5 : (byte)0x00; //If the ECR command is ERROR, Disable ECR

            return rpEcrNotifyCommandResponse;
        }



        /// <summary>
        /// 0xE6 ECR REFUND LAST OPERATION
        /// This command execute the refund of the last payment executed.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_EcrRefundLastOperation()
        {
            RpCommand rpEcrRefundLastOperationRequest = new RpCommand(this);

            rpEcrRefundLastOperationRequest.UPTAddress = 0x00;
            rpEcrRefundLastOperationRequest.CommandId = 0xE6;
            rpEcrRefundLastOperationRequest.EcrStopCommand = 0xE7;
            rpEcrRefundLastOperationRequest.InfoLength = 0;

            rpEcrRefundLastOperationRequest.CompileCommand();
            rpEcrRefundLastOperationRequest.FriendlyName = "0xE6 ECR Refund Last Operation";

            RpResponseBase rpEcrRefundLastOperationResponse = new RpResponseBase(rpEcrRefundLastOperationRequest.Execute());
            //EcrStopMessage = (rpEcrRefundLastOperationResponse.VerifyOutcome() == EOutcome.OK) ? (byte)0xE7 : (byte)0x00; //If the ECR command is ERROR, Disable ECR

            return rpEcrRefundLastOperationResponse;
        }

        /// <summary>
        /// 0xE8 ECR SETTLEMENT OFFLINE TRANSACTION
        /// This command is used to settle the first offline transaction on the UPT.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_EcrMSFSettle()
        {
            RpCommand rpEcrMsfSettle = new RpCommand(this);
            rpEcrMsfSettle.UPTAddress = 0x00;
            rpEcrMsfSettle.CommandId = 0xE8;
            rpEcrMsfSettle.EcrStopCommand = 0xE9;
            rpEcrMsfSettle.InfoLength = 0;
            rpEcrMsfSettle.CompileCommand();
            rpEcrMsfSettle.FriendlyName = "0xE8 ECR Mag Store and Forward Settle";

            RpResponseBase rpEcrMsfSettleResponse = new RpResponseBase(rpEcrMsfSettle.Execute());
            //EcrStopMessage = (rpEcrMsfSettleResponse.VerifyOutcome() == EOutcome.OK) ? (byte)0xE9 : (byte)0x00; //If the ECR command is ERROR, Disable ECR
            return rpEcrMsfSettleResponse;
        }

        /// <summary>
        /// 0xEB ECR VOID TRANSACTION
        /// This command execute the void operation of the last operation executed.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_EcrVoidLastTransaction()
        {
            RpCommand rpEcrVoidLastTransaction = new RpCommand(this);

            rpEcrVoidLastTransaction.UPTAddress = 0x00;
            rpEcrVoidLastTransaction.CommandId = 0xEB;
            rpEcrVoidLastTransaction.EcrStopCommand = 0xEC;
            rpEcrVoidLastTransaction.InfoLength = 0;

            rpEcrVoidLastTransaction.CompileCommand();
            rpEcrVoidLastTransaction.FriendlyName = "0xEB ECR Void Last Transaction.";

            RpResponseBase rpEcrVoidLastTransactionResponse = new RpResponseBase(rpEcrVoidLastTransaction.Execute());
            //EcrStopMessage = (rpEcrVoidLastTransactionResponse.VerifyOutcome() == EOutcome.OK) ? (byte)0xEC : (byte)0x00; //If the ECR command is ERROR, Disable ECR
            return rpEcrVoidLastTransactionResponse;
        }


        /// <summary>
        /// 0x87 REGISTER TERMINAL
        /// This command is required by Apriva Chase Canada to Requires Registration of Serial Number of the terminal
        /// before to any transaction and after commissioning command.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_RegisterTerminalRequest()
        {
            RpCommand rpRegister = new RpCommand(this);

            rpRegister.UPTAddress = 0x00;
            rpRegister.CommandId = 0x87;
            rpRegister.InfoLength = 0;

            rpRegister.CompileCommand();
            rpRegister.FriendlyName = "0x87 Register Terminal";

            RpResponseBase rpRegisterResponse = new RpResponseBase(rpRegister.Execute());
            return rpRegisterResponse;
        }


        /// <summary>
        /// 0x88 GET REGISTER TERMINAL STATUS
        /// This command is required by Apriva Chase Canada to read the status of Registration of Serial Number of the
        /// terminal before to any transaction and after commissioning command.
        /// </summary>
        /// <returns>RpResponseRegisterTerminalStatus</returns>
        public RpResponseRegisterTerminalStatus RP_RegisterTerminalStatus()
        {
            RpCommand rpRegister = new RpCommand(this);

            rpRegister.UPTAddress = 0x00;
            rpRegister.CommandId = 0x88;
            rpRegister.InfoLength = 0;

            rpRegister.CompileCommand();
            rpRegister.FriendlyName = "0x02 Firmware Version";

            RpResponseRegisterTerminalStatus rpRegisterResponse = new RpResponseRegisterTerminalStatus(rpRegister.Execute());
            return rpRegisterResponse;
        }

        /// <summary>
        /// 0x88 SET HOST TIMEOUT
        /// This command is used to change the timeout value present in termconfig file DE00 TAG on Master.
        /// </summary>
        /// <returns>RpResponseBase</returns>
        public RpResponseBase RP_SetTimeout(int nTimeout)
        {
            RpCommand rpRegister = new RpCommand(this);

            rpRegister.UPTAddress = 0x00;
            rpRegister.CommandId = 0x89;
            rpRegister.InfoLength = 1;
            rpRegister.Info[0] = (byte)nTimeout;

            rpRegister.CompileCommand();
            rpRegister.FriendlyName = "0x88 Set Timeout";

            RpResponseBase response = new RpResponseBase(rpRegister.Execute());
            return response;
        }







        /// <summary>
        /// Utility Method to concat two byte arrays as efficiently as possible
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="sizeOfSecondArray">Size of the 2nd array to concat</param>
        /// <returns></returns>
        public byte[] ByteArrayCombine(byte[] first, byte[] second, int sizeOfSecondArray)
        {
            byte[] ret = new byte[first.Length + sizeOfSecondArray];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, sizeOfSecondArray);
            return ret;
        }



        /// <summary>
        /// Generic BytesOut method
        /// 
        /// This method determines which communication path is in use, and directs the data
        ///
        /// </summary>
        /// <param name="output">Data to Transmit</param>
        /// <param name="requestName">Human Readable Request Name</param>
        /// <returns></returns>
        public byte[] BytesOut(byte[] output, string requestName)
        {
            // Test Harness
            if (_testHarnessCallbackDelegate != null)
            {
                return _testHarnessCallbackDelegate(output, requestName);
            }

            //someday - build a factory
            if (TerminalServerSocket != null)
            {
                if (SslStream == null)
                    return C_TsSocketBytesOut(output, requestName);
                return C_TsSocketViaTLSBytesOut(output, requestName);
            }

            if (UseEthernet)
            {
                return C_EthBytesOut(output, requestName);
            }

            if (UseTLSEthernet)
            {
                return C_TLSEthBytesOut(output, requestName);
            }

            if (UseMux)
            {
                return C_MuxBytesOut(output, requestName);
            }


            return C_SerialBytesOut(output, requestName);
        }


        // The remaining methods handle communications

        /// <summary>
        /// Start the Ethernet Socket Server to communicate with the BV1000
        /// </summary>
        /// <param name="portNumber">Ethernet Port # for the BV1000 to connect to</param>
        public void C_TLSEthStartListener(int portNumber)
        {

            IPAddress listeningIp = null;

            //find first IPV4 address on this machine.
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in ipHostInfo.AddressList)
            {
                if ((ip.AddressFamily == AddressFamily.InterNetwork) && (!(IPAddress.IsLoopback(ip)))) //get the first IPV4 address
                {
                    listeningIp = ip;
                    break;
                }
            }

            if (null != listeningIp)
            {
                //TODO: Fix this when it works
                //add 443 so we can test with stunnel.
                //Stunnel config:
                    //engine = capi
                    //[madic]
                    //engineId = capi
                    //client = yes
                    //accept = 10.254.253.100:4000
                    //connect = 10.254.253.100:4443

                //TLSEthernetListener = new TlsServer(IPAddress.Any, portNumber + 443, WindowSize); //bind to all the addresses on the Pc
                TLSEthernetListener = new TlsServer(IPAddress.Any, portNumber, WindowSize); //bind to all the addresses on the Pc
                UseTLSEthernet = true;
            }
            else
            {
                throw new Exception("No IPV4 Addresses available on Host.");
            }
        }



        /// <summary>
        /// Start the Ethernet Socket Server to communicate with the BV1000
        /// </summary>
        /// <param name="portNumber">Ethernet Port # for the BV1000 to connect to</param>
        public void C_EthStartListener(int portNumber)
        {
            //EthernetListener = new RetailProtocolSocketListener(portNumber);
            
            IPAddress listeningIp = null;
            
            //find first IPV4 address on this machine.
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            foreach(IPAddress ip in ipHostInfo.AddressList)
            {
                if ((ip.AddressFamily == AddressFamily.InterNetwork) && (!(IPAddress.IsLoopback(ip)))) //get the first IPV4 address
                {
                    listeningIp = ip;
                    break;
                }
            }

            if (null != listeningIp)
            {
                EthernetListener = new TcpServer(IPAddress.Any, portNumber, WindowSize); //bind to all the addresses on the Pc
                UseEthernet = true;
            }
            else
            {
                throw new Exception("No IPV4 Addresses available on Host.");
            }
        }
        
        /// <summary>
        /// Send Bytes out the Ethernet Socket of the terminal server via TLS
        /// </summary>
        /// <param name="command">Bytes to Transmit</param>
        /// <param name="requestName">Human Readable Request Name</param>
        /// <returns>bytes in response</returns>
        public byte[] C_TsSocketViaTLSBytesOut(byte[] command, string requestName)
        {
            if (null != IoIn)
            {
                DisplayArgs de = new DisplayArgs(command);
                IoIn(this, de);
            }

            if (TerminalServerSocket?.Connected == true)
            {
                try
                {
                    int sendBlockSize = 1024*10;
                    int rcvBlockSize = 1024*10;

                    Busy = true;
                    SocketError se;

                    SslStream.Write(command,0,command.Length);
                    SslStream.Flush();

                    int timeoutCounter = 0;
                    bool finished = false;
                    byte[] buffer = new byte[1024*10];
                    int retPosition = 0;
                    byte[] ret = new byte[0];

                    while (!finished)
                    {

                        if (timeoutCounter++ > (10 * ExpectedWaitTime)) //Total wait time is 100ms * 10 * Expected Wait time = Expected wait time in seconds
                        {

                            if (retPosition > 0)
                            {
                                StringBuilder displayText = new StringBuilder();
                                for (int i = 0; i < retPosition; i++)
                                {
                                    displayText.Append(buffer[i].ToString("X2") + " ");
                                }

                                throw new Exception("Incomplete Response:" + displayText);
                            }
                            throw new Exception("No Response!");
                        }

                        int bytesread = SslStream.Read(buffer, 0, 1024 * 10);
                        retPosition += bytesread; 

                        if (bytesread > 0)
                        {
                            int infoLength = EndianBitConverter.Big.ToInt16(buffer, 3); //make a 16 bit int from byte 3 and 4

                            if (retPosition > (infoLength + 6))
                            {
                                finished = true;
                                ret = new byte[retPosition];
                                Buffer.BlockCopy(buffer, 0, ret, 0, retPosition);
                            }
                        }
                        Thread.Sleep(100);
                        RefreshUIEvent?.Invoke(this);
                    }

                    if ((null != IoOut))
                    {
                        byte[] displayed = new byte[ret.Length];
                        Buffer.BlockCopy(ret, 0, displayed, 0, ret.Length);
                        DisplayArgs de = new DisplayArgs(displayed);
                        IoOut(this, de);
                    }

                    Busy = false;

                    //if (ret[2] != targetCommandID)
                    //{
                    //    //erase everything
                    //    lock (RpMessagesReceived) RpMessagesReceived.RemoveRange(0,RpMessagesReceived.Count);
                    //    throw new Exception("No matching response for Command ID:" + targetCommandID.ToString("X2") + " We received:" + ret[2].ToString("X2"));
                    //}

                    return ret;
                }
                catch (Exception ex)
                {
                    Busy = false;
                    throw new Exception("Exception sending Ethernet data:" + ex.Message);
                }
            }

            throw new Exception("No BV1000 is connected.");
        }


        /// <summary>
        /// Send Bytes out the Ethernet Socket of the terminal server
        /// </summary>
        /// <param name="command">Bytes to Transmit</param>
        /// <param name="requestName">Human Readable Request Name</param>
        /// <returns>bytes in response</returns>
        public byte[] C_TsSocketBytesOut(byte[] command, string requestName)
        {
            if (null != IoIn)
            {
                DisplayArgs de = new DisplayArgs(command);
                IoIn(this, de);
            }

            if (TerminalServerSocket?.Connected == true)
            {
                try
                {
                    int sendBlockSize = 100;
                    int rcvBlockSize = 256;

                    if (_FullSpeedETH)
                    {
                        sendBlockSize = 1024*10;
                        rcvBlockSize = 1024*10;
                    }

                    Busy = true;
                    SocketError se;
                    int sendPos = TerminalServerSocket.Send(command,0,(command.Length > sendBlockSize) ? sendBlockSize : command.Length, SocketFlags.None,out se);
                    if (se != SocketError.Success)
                    {
                        Busy = false;
                        throw new Exception("Unable to send data for:" + requestName + Environment.NewLine + "Socket error:"+se.ToString());
                    }

                    if (sendPos < 0) sendPos = 0;

                    int iteration = 0;
                    while (sendPos < command.Length)
                    {
                        int bytesOut =
                         TerminalServerSocket.Send(command, sendPos, ((command.Length - sendPos) > sendBlockSize) ? sendBlockSize : (command.Length - sendPos),SocketFlags.None, out se);

                        if (bytesOut > 0) sendPos += bytesOut;

                        if (iteration++ > 100) // 100 * 100ms = 10,000ms seconds = 10 seconds
                        {
                            Busy = false;
                            throw new Exception("Unable to send command. Only sent " + sendPos.ToString() + " bytes." + Environment.NewLine + "Socket error:"+se.ToString());
                        }
                        Thread.Sleep(50);
                    }

                    
                    int timeoutCounter = 0;
                    bool finished = false;
                    byte[] buffer = new byte[4096];
                    int retPosition = 0;
                    byte[] ret = new byte[0];

                    while(!finished)
                    {
                       
                        if (timeoutCounter++ > (10 * ExpectedWaitTime)) //Total wait time is 100ms * 10 * Expected Wait time = Expected wait time in seconds
                        {
                            
                            if (retPosition > 0)
                            {
                                StringBuilder displayText = new StringBuilder();
                                for (int i = 0; i < retPosition; i++)
                                {
                                    displayText.Append(buffer[i].ToString("X2") + " ");
                                }

                                throw new Exception("Incomplete Response:" + displayText);
                            }
                            throw new Exception("No Response!");
                        }

                        int numberOfBytesToFetch = TerminalServerSocket.Available;
                        if (numberOfBytesToFetch > 0)
                        {
                            byte[] tempBytes = new byte[numberOfBytesToFetch];
                            int bytesread = TerminalServerSocket.Receive(tempBytes,0,(numberOfBytesToFetch > rcvBlockSize) ? rcvBlockSize : numberOfBytesToFetch, SocketFlags.None);

                            if (buffer.Length < retPosition + bytesread)
                                Array.Resize(ref buffer, retPosition + bytesread);

                            Buffer.BlockCopy(tempBytes, 0, buffer, retPosition, bytesread);
                            retPosition += bytesread;
                        }

                        int infoLength = EndianBitConverter.Big.ToInt16(buffer, 3); //make a 16 bit int from byte 3 and 4

                        if (retPosition > (infoLength + 6) )
                        {
                            finished = true;
                            ret = new byte[retPosition];
                            Buffer.BlockCopy(buffer,0,ret,0,retPosition);
                        }

                        Thread.Sleep(100);
                        RefreshUIEvent?.Invoke(this);
                    }

                    if ((null != IoOut))
                    {
                        byte[] displayed = new byte[ret.Length];
                        Buffer.BlockCopy(ret, 0, displayed, 0, ret.Length);
                        DisplayArgs de = new DisplayArgs(displayed);
                        IoOut(this, de);
                    }

                    Busy = false;

                    //if (ret[2] != targetCommandID)
                    //{
                    //    //erase everything
                    //    lock (RpMessagesReceived) RpMessagesReceived.RemoveRange(0,RpMessagesReceived.Count);
                    //    throw new Exception("No matching response for Command ID:" + targetCommandID.ToString("X2") + " We received:" + ret[2].ToString("X2"));
                    //}

                    return ret;
                }
                catch (Exception ex)
                {
                    Busy = false;
                    throw new Exception("Exception sending Ethernet data:" + ex.Message);
                }
            }

            throw new Exception("No BV1000 is connected.");
        }

        /// <summary>
        /// Send Bytes out the Ethernet Socket
        /// </summary>
        /// <param name="command">Bytes to Transmit</param>
        /// <param name="requestName">Human Readable Request Name</param>
        /// <returns></returns>
        public byte[] C_EthBytesOut(byte[] command, string requestName)
        {
            
            if (null != IoIn)
            {
                DisplayArgs de = new DisplayArgs(command);
                IoIn(this, de);
            }

            if (UseEthernet )
            {
                if (EthernetListener.SocketList.Count > 0)
                {
                    Busy = true;

                    if (EthernetListener.SendData(command, requestName) == 0)
                    {
                        throw new Exception("Unable to send data for:"+requestName);   
                    }
                    byte[] ret = new byte[EthernetListener.BytesInCounter];
                    lock (EthernetListener.BytesIn)
                    {
                        Buffer.BlockCopy(EthernetListener.BytesIn, 0, ret, 0, EthernetListener.BytesInCounter);
                        EthernetListener.BytesInCounter = 0;
                    }
                    Busy = false;

                    if (null != IoOut)
                    {
                        DisplayArgs de = new DisplayArgs(ret);
                        IoOut(this, de);
                    }

                    return CheckForEcr(ret);

                    //return ret;
                }

                throw new Exception("No BV1000 is connected.");
                
            }

            throw new Exception("Ethernet Server is not in use.");
        }

        /// <summary>
        /// Send Bytes out the Ethernet Socket
        /// </summary>
        /// <param name="command">Bytes to Transmit</param>
        /// <param name="requestName">Human Readable Request Name</param>
        /// <returns></returns>
        public byte[] C_TLSEthBytesOut(byte[] command, string requestName)
        {

            if (null != IoIn)
            {
                DisplayArgs de = new DisplayArgs(command);
                IoIn(this, de);
            }

            if (UseTLSEthernet)
            {
                if (TLSEthernetListener.SocketList.Count > 0)
                {
                    Busy = true;

                    if (TLSEthernetListener.SendData(command, requestName) == 0)
                    {
                        throw new Exception("Unable to send data for:" + requestName);
                    }
                    byte[] ret = new byte[TLSEthernetListener.BytesInCounter];
                    lock (TLSEthernetListener.BytesIn)
                    {
                        Buffer.BlockCopy(TLSEthernetListener.BytesIn, 0, ret, 0, TLSEthernetListener.BytesInCounter);
                        TLSEthernetListener.BytesInCounter = 0;
                    }
                    Busy = false;

                    if (null != IoOut)
                    {
                        DisplayArgs de = new DisplayArgs(ret);
                        IoOut(this, de);
                    }

                    return CheckForEcr(ret);

                    //return ret;
                }

                throw new Exception("No payment device is connected via TLS.");

            }

            throw new Exception("TLS Ethernet Server is not in use.");
        }


        private byte[] CheckForEcr(byte[] rpPackets )
        {
            if (rpPackets.Length < 8) return rpPackets; //Not enough data - Go get more.

            bool finished = false;
            while(!finished)
            {

                int i;
                for (i = 0; i <= rpPackets.Length; i++)
                {
                    if (0x02 == rpPackets[i]) //Discard Trash
                    {
                        break;
                    }
                }
                Debug.Assert(i == 0, "Garbage in Buffer.");

                int infoLength = EndianBitConverter.Big.ToInt16(rpPackets, 3 + i); //make a 16 bit int from byte 3 and 4

                if ((i + 7 + infoLength) > rpPackets.Length) return rpPackets; //Not enough data for a RP packet

                Debug.Assert(rpPackets[(i + 5 + infoLength)] == 0x03, "ETX not found.");


                byte[] output = new byte[infoLength + 7];

                Buffer.BlockCopy(rpPackets, i, output, 0, (infoLength + 7));
                RpResponseBase rpResponseMessage = new RpResponseBase(output);


                if (EcrResponseMessages.Contains(rpResponseMessage.CommandId)) //Check to see if this is an ECR message
                {
                    lock (EcrMessagesReceived)
                    {
                        EcrMessagesReceived.Add(rpResponseMessage);
                    }

                    if (rpResponseMessage.CommandId == EcrStopMessage)
                        EcrStopMessage = 0x00; //If its the stop message - stop looking for more.

                    Buffer.BlockCopy(rpPackets, i, rpPackets, 0, (infoLength + 7)); //cut the ECR packet out of the buffer
                }
                else
                {
                    finished = (rpPackets.Length == (infoLength + 7)); //if I checked this packet and its not ECR and I am at the end of the buffer - then I am finished.
                    rpPackets = rpResponseMessage.Response;
                }
            }

            return rpPackets;

        }



        /// <summary>
        /// Create the com port object. This method is called when the Outputdevice (COM port) changes.
        /// </summary>
        public void C_SerialResetPort()
        {
            try
            {
                Comport = new SerialPort(OutputDevice, _baudRate, Parity.None, 8, StopBits.One);
                //comport.Handshake = Handshake.RequestToSend;
                Comport.Handshake = (_hwFlowControl) ? Handshake.RequestToSend : Handshake.None;
                //_comport.Handshake = Handshake.RequestToSendXOnXOff;
                Comport.ReadTimeout = 1500;
                Comport.WriteTimeout = 1500;
                Comport.DataReceived += C_SerialDataReader;

            }
            catch (Exception ex)
            {
                throw new Exception("Error Setting COM port:" + ex.Message);
            }
        } //End ResetPort Method

        /// <summary>
        /// Transmit Data via MUX
        /// </summary>
        /// <param name="output">Output Data of the retailer protocol Command</param>
        /// <param name="requestName">Human Readable Description of the command.</param>
        /// <returns></returns>
        public byte[] C_MuxBytesOut(byte[] output, string requestName)
        {
            if (Mux == null)
            {
                throw new Exception("No Mux Protocol Object");
            }

            byte targetCommandId = output[2];

            //Update UI with outbound byte array data for Debugging
            if (null != IoIn)
            {
                DisplayArgs de = new DisplayArgs(output);
                IoIn(this, de);
            }

            try
            {
                //Busy = true;
                Mux.ReceiveRetailProtocolPacket(output);
                int timeoutCounter = 0;

                while (true)
                {
                    if (Mux.RetailProtocolMessages.Count > 0)
                    {
                        
                        //foreach (byte[] rpPacket in Mux.RetailProtocolMessages)
                        byte[] rpPacket;
                        rpPacket = Mux.PullNextRpPacket(targetCommandId);
                        while ( rpPacket != null)
                        {
                            //Mux.RetailProtocolMessages.Remove(rpPacket);
                            if (null != IoOut)
                            {
                                DisplayArgs de = new DisplayArgs(rpPacket);
                                IoOut(this, de);
                            }

                            RpResponseBase rpResult = new RpResponseBase(rpPacket);
                            if (EcrResponseMessages.Contains(rpResult.CommandId))
                            {
                                lock (EcrResponseMessages)
                                {
                                    EcrMessagesReceived.Add(rpResult);
                                }
                            }
                            else
                            {
                                Debug.Assert(targetCommandId == rpResult.CommandId,"We received CommandID:"+rpResult.CommandId.ToString("X2")+" But where looking for CommandID:"+targetCommandId.ToString("X2"));
                                return rpPacket;
                            }
                            rpPacket = Mux.PullNextRpPacket(targetCommandId);
                        }
                    }

                    if (timeoutCounter++ > (10 * ExpectedWaitTime)) //Total wait time is 10 * 100ms * Expected wait time = Expected wait time in seconds
                    {
                        throw new Exception("No Response:" + requestName);
                    }

                    Thread.Sleep(100);
                    RefreshUIEvent?.Invoke(this);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Communication Error! Exception:" + ex.Message);
            }
        }

        /// <summary>
        /// Method to send retail protocol commands (data) to the RS232 port
        /// 
        /// Note:   This method is not used for MUX. 
        ///         Mux communication happens in the MUX class.
        /// 
        /// </summary>
        /// <param name="output">buffer for outbound data</param>
        /// <param name="requestName">Retail Protocol Request Name</param>
        public byte[] C_SerialBytesOut(byte[] output, string requestName)
        {
            if (Comport == null)
            {
                throw new Exception("No COM port set");
            }


            //Update UI with outbound byte array data for Debugging
            if (null != IoIn)
            {
                DisplayArgs de = new DisplayArgs(output);
                IoIn(this, de);
            }


            try
            {
                Busy = true;
                byte Bankingtag = 0x00;

                if (!Comport.IsOpen) Comport.Open();

                int targetCommandID = output[2];
                
                if (targetCommandID == 0x0B)
                {
                    Bankingtag = output[5];
                }
                
                int bytestowrite = output.Length;


                //add a delay if we have more than 1k to write
                int buffSize = (DeviceFamily == EDeviceFamily.HRT1000) ? (10*1024) : WindowSize;
                
                if (bytestowrite < buffSize)
                {
                    Comport.Write(output, 0, bytestowrite);
                }
                else
                {
                    int i = 0;
                    while(i < bytestowrite)
                    {
                        int remaining = ((bytestowrite - i) > buffSize) ? buffSize : (bytestowrite - i);
                        Comport.Write(output, i , remaining);
                        i += remaining;
                        Thread.Sleep(100);
                    }
                }

                bool notFinished = true;
                int timeoutCounter = 0;
                byte[] ret = new byte[1]; //baby at first

                while (notFinished)
                {
                    if (timeoutCounter++ > (10 * ExpectedWaitTime)) //Total wait time is 100ms * 10 * Expected Wait time = Expected wait time in seconds
                    {
                        BytesInCounter = 0;
                        Busy = false;
                        throw new Exception("No Response!");
                    }

                    RefreshUIEvent?.Invoke(this);

                    Thread.Sleep(100);

                    if (RpMessagesReceived.Count != 0)
                    {
                        lock (RpMessagesReceived)
                        {
                            foreach (byte[] rpResponse in RpMessagesReceived)
                            {
                                if (rpResponse[2] == targetCommandID)
                                {
                                    if (targetCommandID == 0x0B)
                                    {
                                        //check that the answer matches
                                        if (rpResponse[5] == Bankingtag)
                                        {
                                            ret = rpResponse;
                                            notFinished = false;
                                        }
                                    }
                                    else
                                    {
                                        ret = rpResponse;
                                        notFinished = false;
                                    }
                                }
                            }
                        } 
                    } 
                } //end while

                lock(RpMessagesReceived) RpMessagesReceived.Remove(ret);

                if ((null != IoOut))
                {
                    byte[] displayed = new byte[ret.Length];
                    Buffer.BlockCopy(ret, 0, displayed, 0, ret.Length);
                    DisplayArgs de = new DisplayArgs(displayed);
                    IoOut(this, de);
                }
                
                Busy = false;

                //if (ret[2] != targetCommandID)
                //{
                //    //erase everything
                //    lock (RpMessagesReceived) RpMessagesReceived.RemoveRange(0,RpMessagesReceived.Count);
                //    throw new Exception("No matching response for Command ID:" + targetCommandID.ToString("X2") + " We received:" + ret[2].ToString("X2"));
                //}

                return ret;
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException)
                {
                    throw new Exception("RS232 Port on your PC is in use by another application!  Exception:" + ex.Message + " During:" + requestName);
                }

                throw new Exception("Communication Error!  Exception:" + ex.Message + " During:" + requestName);
            }
        }//End C_SerialBytesOut Method

        /// <summary>
        /// Close the serial port
        /// </summary>
        public void C_SerialClose()
        {
            Comport.Close();
        }

        /// <summary>
        /// Return the status of the COM port
        /// </summary>
        /// <returns>Boolean indicating if the COM port is open</returns>
        public bool C_SerialIsComOpen()
        {
            if (Comport != null)
            {
                return Comport.IsOpen;
            }
            return false;
        }

        /// <summary>
        /// Serial Port Receive Data Handlers
        /// 
        /// This executes on the SerialPort class's DataReceived event thread
        /// </summary>
        /// <param name="sender">sender </param>
        /// <param name="e">Event Arguments</param>
        private void C_SerialDataReader(object sender, SerialDataReceivedEventArgs e)
        {
            BytesInCounter += Comport.Read(BytesIn, BytesInCounter, Comport.BytesToRead);
            Decode();
        }

        /// <summary>
        /// Decode the data in the receive serial buffer into retail protocol packets
        /// </summary>
        public void Decode()
        {

            if (BytesInCounter < 8) return; //Not enough data - Go get more.

            int i;
            for (i = 0; i <= BytesInCounter; i++)
            {
                if (0x02 == BytesIn[i]) //Discard Trash
                {
                    break;
                }
            }

            //if (i != 0)
            //{
            //    if (Encoding.ASCII.GetString(BytesIn, 1, 10) == "----------")
            //    {
            //        Thread.Sleep(1);
            //        throw new Exception("Found:" + Encoding.ASCII.GetString(BytesIn));
                    
            //    }
            //}

            Debug.Assert(i == 0, "Garbage in Serial Buffer.");
            

            int infoLength = EndianBitConverter.Big.ToInt16(BytesIn, 3 + i); //make a 16 bit int from byte 3 and 4

            if ((i + 7 + infoLength) > BytesInCounter) return; //Not enough data in the buffer yet

            Debug.Assert(BytesIn[(i + 5 + infoLength)] == 0x03, "ETX not found.");


            byte[]  output = new byte[infoLength + 7]; 

            Buffer.BlockCopy(BytesIn, i, output, 0, (infoLength + 7));
            RpResponseBase rpResponseMessage = new RpResponseBase(output);


            lock (BytesIn)
            {
                BytesInCounter -= (i + infoLength + 7);
                Buffer.BlockCopy(BytesIn, (infoLength + 7), BytesIn, 0, BytesIn.Length - (infoLength + 7));
            }

            if (EcrResponseMessages.Contains(rpResponseMessage.CommandId)) //Check to see if this is an ECR message
            {
                lock (EcrMessagesReceived)
                {
                    EcrMessagesReceived.Add(rpResponseMessage);
                }

                if (rpResponseMessage.CommandId == EcrStopMessage)
                    EcrStopMessage = 0x00; //If its the stop message - stop looking for more.

                // Logging ECR messages is causing problems...
                //if (null != IOout)
                //{
                //    DisplayArgs de = new DisplayArgs(rpResponseMessage.Response);
                //    IOout(this, de);
                //}
            }
            else
            {
                lock (RpMessagesReceived)
                {
                    RpMessagesReceived.Add(output);
                }
            }
        }

/*
        /// <summary> Checks to see if the banking parameter value is a single byte </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool IsByteBankingConfigurationParmValue(EBankingParams tag)
        {
            if (EBankingParams.ExtractTimeout == tag) return true;
            if (EBankingParams.TD2_ExtractTimeout == tag) return true;
            return false;
        }
*/

    }
}
