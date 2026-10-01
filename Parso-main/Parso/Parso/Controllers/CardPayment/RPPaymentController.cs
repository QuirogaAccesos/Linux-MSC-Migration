using CCI.Globalcom.GlobalcomRetailProtocol;
using Parso.Classes;
using Parso.Utils;
using Parso.Utils.Objects;
using RetailProtocolIntegration;
using RetailProtocolTest.Public;
using Serilog;
using System.Text;

namespace Parso.Controllers.RPPaymentController
{
    internal class RPPaymentController
    {
        private static ProjectConstants _projectConstants = ProjectConstants.Instance;
        RetailProtocol _retailProtocol = new RetailProtocol(_projectConstants.CARD_PAYMENT_COM_PORT, _projectConstants.CARD_PAYMENT_BAUD_RATE, _projectConstants.CARD_PAYMENT_ETHERNET_PORT_NUMBER, false);

        private RetailProtocolParser _protocolParser = new RetailProtocolParser();
        private Timer _scanTimer;
        private object _syncLock = new object();

        private byte[] statusResponse;
        private RPResponseDto _response;
        private PaymentRequest _paymentRequest;

        private ScanResponseDto lastParsedScanResponse = new ScanResponseDto();
        private byte[] LastRPresponse;

        private StringBuilder RPLogger = new StringBuilder();

        //Progress Flags
        private bool _isScanning = true;
        private bool _paymentOngoing = false;
        private bool _cardDetected = false;
        private bool _transactionOngoing = false;
        private bool _transactionComplete = false;
        private bool _finalScan = false;

        //Cancelation flag
        private volatile bool _cancellationRequested = false;
        private readonly object _paymentLock = new object();
        private volatile bool _isPaymentActive = false;

        //other flags
        private bool _logResult = false;
        private bool _result = false;
        private bool _timeout = false;


        public RPResponse RetailProtocolTransaction(PaymentRequest paymentRequest, int timeoutSeconds, int scanTimerMili, bool testEth, bool includeRPLog,
            bool sendCardInsertedNotification, string CardInsertedMessage)
        {
            lock (_paymentLock)
            {
                if (_isPaymentActive)
                {
                    return new RPResponse(false, "Another payment is already in progress", "BUSY", "Payment controller busy");
                }
                _isPaymentActive = true;
            }

            try
            {
                _paymentRequest = paymentRequest;

                //Reset everything to first status
                resetFlags();

                //Logger if needed
                if (includeRPLog)
                {
                    _logResult = true;
                    RPLogger.Clear();
                    RPLogger.AppendLine($"{DateTime.Now.ToString()} - RP START | BEGINNING RETAIL PROTOCOL TRANSACTION \r\n");
                }

                //testETH before operation
                if (testEth)
                {
                    if (!TestRPEth())
                    {
                        return new RPResponse(false, _response.ParsedInfo, RPLogger.ToString());
                    }
                }

                //Get key device information
                LastRPresponse = _retailProtocol.RP_GetKeyInformationV2().Response;
                _response = _protocolParser.ParseResponse(LastRPresponse);
                if (_logResult) logRPOperation(_response.ParsedInfo, "KEY INFORMATION REQUESTED. RESPONSE: ");

                //Obtain firmware version request
                LastRPresponse = _retailProtocol.RP_FirmwareVersionRequest().Response;
                _response = _protocolParser.ParseResponse(LastRPresponse);
                if (_logResult) logRPOperation(_response.ParsedInfo, "FIRMWARE VERSION REQUESTED. RESPONSE");

                //Get device info
                LastRPresponse = _retailProtocol.RP_DeviceGetInfo().Response;
                _response = _protocolParser.ParseResponse(LastRPresponse);
                if (_logResult) logRPOperation(_response.ParsedInfo, "DEVICE INFO REQUESTED. RESPONSE");

                //Remove old response info
                LastRPresponse = _retailProtocol.RP_InfoErase(true).Response;
                _response = _protocolParser.ParseResponse(LastRPresponse);
                if (_logResult) logRPOperation(_response.ParsedInfo, "OLD INFO REMOVED");

                //Start Scanning periodically
                _scanTimer = new Timer(ScanCallback, null, Timeout.Infinite, Timeout.Infinite);
                StartPeriodicScanning(scanTimerMili);

                //Enable card reading and set status to ongoing payment
                StopPeriodicScanning();
                LastRPresponse = _retailProtocol.RP_ReadCardEnable(timeoutSeconds).Response;
                _response = _protocolParser.ParseResponse(LastRPresponse);
                if (_logResult) logRPOperation(_response.ParsedInfo, "READ CARD COMMAND SENT. RESPONSE");
                _paymentOngoing = true;
                StartPeriodicScanning(scanTimerMili);

                while (_paymentOngoing)
                {
                    if (_cancellationRequested)
                    {
                        StopPeriodicScanning();
                        _retailProtocol.RP_ReadCardDisable(); // This is the cancel method you mentioned
                        _retailProtocol.RP_InfoErase(true);
                        if (_logResult) logRPOperation("Payment cancelled by user", "CANCELLATION");
                        return new RPResponse(false);
                    }

                    if (_cardDetected && !_transactionOngoing)
                    {
                        if (sendCardInsertedNotification)
                        {
                            ServerListener.Instance.BroadcastMessage(CardInsertedMessage);
                            Log.Information($"Card inserted notifcation sent: {CardInsertedMessage}");
                        }
                        StopPeriodicScanning();
                        LastRPresponse = _retailProtocol.RP_PaymentCommand(_paymentRequest.AmountInPennies, GetCurrencyFromInt(_paymentRequest.CurrencyCode), GetModeFromInt(_paymentRequest.EMode), _paymentRequest.ClientTransactionID).Response;
                        _response = _protocolParser.ParseResponse(LastRPresponse);
                        if (_logResult) logRPOperation(_response.ParsedInfo, "BEING PAYMENT COMMAND SENT. RESPONSE");
                        _transactionOngoing = true;
                        StartPeriodicScanning(scanTimerMili);
                    }

                    if (_transactionComplete && !_finalScan)
                    {
                        StopPeriodicScanning();
                        LastRPresponse = _retailProtocol.RP_PaymentOutcomeRequest().Response;
                        _response = _protocolParser.ParseResponse(LastRPresponse);
                        if (_logResult) logRPOperation(_response.ParsedInfo, "PAYMENT RESULT REQUESTED. RESPONSE");
                        _finalScan = true;
                        StartPeriodicScanning(scanTimerMili);
                    }
                }

                StopPeriodicScanning();
                _retailProtocol.RP_InfoErase(true);

                if (_timeout) { return new RPResponse(_result, _response.ParsedInfo, lastParsedScanResponse.TransactionStatus, RPLogger.ToString(), _timeout); }
                return new RPResponse(_result, _retailProtocol.RP_GetReceiptXML(), lastParsedScanResponse.TransactionStatus, RPLogger.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return new RPResponse(_response.ParsedInfo, ex.Message);
            }
            finally
            {
                lock (_paymentLock)
                {
                    _isPaymentActive = false;
                }
            }
            
        }

        private void StartPeriodicScanning(int scanTimerMili)
        {
            lock (_syncLock)
            {
                _isScanning = true;
                // Start immediately and every 3 seconds thereafter
                _scanTimer.Change(0, scanTimerMili);
            }
        }

        private void StopPeriodicScanning()
        {
            lock (_syncLock)
            {
                _isScanning = false;
                // Disable timer
                _scanTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void ScanCallback(object state)
        {
            lock (_syncLock)
            {
                if (_isScanning)
                {
                    statusResponse = _retailProtocol.RP_StatusRequest(GetLanguageFromInt(_paymentRequest.Language)).Response;
                    if (_protocolParser.TryParseScanResponse(statusResponse, out ScanResponseDto scanResponse))
                    {
                        lastParsedScanResponse = scanResponse;
                        if (_logResult) { loglastScan(); }

                        if (scanResponse.Block1State == "Card reader timeout/error")
                        {
                            _result = false;
                            _paymentOngoing = false;
                            _timeout = true;
                        }

                        if (_finalScan) {
                            if (scanResponse.TransactionStatus == "Status outcome bad") { _result = false; } 
                            else { _result = true; }
                            _paymentOngoing = false;
                        }

                        if (_paymentOngoing)
                        {
                            if (scanResponse.Block1State == "Card data available")
                            {
                                _cardDetected = true;
                            }
                        }

                        if (_paymentOngoing && _transactionOngoing)
                        {
                            if (scanResponse.Block1State != "BUSY")
                            {
                                _transactionComplete = true;
                            }
                        }
                    }
                    else { Console.WriteLine(" - SCAN FAILED"); }
                    Console.WriteLine("");
                }
            }
        }

        // Add public method to request cancellation
        public void CancelPayment()
        {
            _cancellationRequested = true;
        }

        private bool TestRPEth() {
            var EthResponse = _retailProtocol.RP_TestEth("8.8.8.8");
            LastRPresponse = EthResponse.Response;
            _response = _protocolParser.ParseResponse(LastRPresponse);
            if (EthResponse.Outcome == 224) {
                if (_logResult) logRPOperation(_response.ParsedInfo, "INTERNET TEST CONCLUDED");
                return true;
            }
            return false;
        }

        private void logRPOperation(string info, string header) {
            RPLogger.AppendLine($"{DateTime.Now.ToString()} - {header}: {info} \r\n");
        }

        private void loglastScan()
        {
            string logMessage =
                $"{BitConverter.ToString(statusResponse)} - Scan complete. Output: " +
                $"Outcome: {lastParsedScanResponse.OutcomeMeaning}; " +
                $"CashOpen: {lastParsedScanResponse.CashOpen}; " +
                $"Block1State: {lastParsedScanResponse.Block1State}; " +
                $"KeyPressed: {lastParsedScanResponse.KeyPressed}; " +
                $"TransactionStatus: {lastParsedScanResponse.TransactionStatus}";
            RPLogger.AppendLine($"{DateTime.Now.ToString()} - SCAN COMPLETE: {logMessage} \r\n");
        }

        private void resetFlags() {
            _isScanning = true;
            _paymentOngoing = false;
            _cardDetected = false;
            _transactionOngoing = false;
            _transactionComplete = false;
            _finalScan = false;
            _logResult = false;
            _result = false;
            _cancellationRequested = false;
            _timeout = false;
        }

        public static ECurrencies GetCurrencyFromInt(int currencyCode)
        {
            if (Enum.IsDefined(typeof(ECurrencies), currencyCode))
            {
                return (ECurrencies)currencyCode;
            }
            throw new ArgumentException($"Invalid currency code: {currencyCode}");
        }

        public static ELanguage GetLanguageFromInt(int languageCode)
        {
            if (Enum.IsDefined(typeof(ELanguage), languageCode))
            {
                return (ELanguage)languageCode;
            }
            throw new ArgumentException($"Invalid language code: 0x{languageCode:X2}");
        }

        public static EMode GetModeFromInt(int modeCode)
        {
            if (Enum.IsDefined(typeof(EMode), modeCode))
            {
                return (EMode)modeCode;
            }
            throw new ArgumentException($"Invalid mode code: 0x{modeCode:X2}");
        }
    }
}
