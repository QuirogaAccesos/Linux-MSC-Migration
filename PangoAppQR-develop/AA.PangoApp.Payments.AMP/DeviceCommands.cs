using AMPComm;
using AA.PangoApp.Payments.Interface;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AA.PangoApp.Payments.Interface.Dtos;
using System.Configuration;

namespace AA.PangoApp.Payments.AMP
{
    /*
         * Usage:
         *      1. Add "AMPSerialAdapter.dll" as a refrence
         *
         *      2. Create an object as "AMPSerialAdapter"
         *
         *      3. Create an object as "AMPSerialAdapter.IDataListener"
         *
         *      4. To start communication, it needs to make a connection.
         *        [Example of this step is described in the "btnConnect_Click" function in this class.]
         *
         *      5. After creating connection[step 4], to send data, just needs to call the method "SendData" of the "AMPSerialAdapter" instantiated object.
         *        [Example of this step is described in the "btnSendData_Click" function in this class.]
         *
         *      6. The Dll has two callback functions, which should be implemented. [Current class in implemented the interface]:
         *
         *        6.1. "_onDataSentResult"
         *             Provides the length of data sent after step 5.
         *             [Example of this step is described in the "_onDataSentResult" function in this class.]
         *
         *        6.2. "_onDataReceived"
         *             Provides the received data through the "ExchangeData" object.
         *             The "getResponseCode()" method of the input object, represents the length of the received data. If this value is less than zero, indicating an error.
         *             The "getData()" method of the input object, provides the received data.
         *             [Example of this step is described in the "_onDataReceived" function in this class.]
         */

    internal class DeviceCommands: AMPSerialAdapter.IDataListener
    {
        public AMPResponsePayment Response = null;

        private ILog log = LogManager.GetLogger("AMP.DeviceCommand");
        public volatile AMPSerialAdapter serialAdapter;
        public volatile AMPSerialAdapter.IDataListener _IDataListener;

        public bool IsPaymetSend = false;
        public bool IsCloseAPP = true;
        public bool result=false;

        public DeviceCommands()
        {
            log.Info("DeviceCommands Connect AMP6500");
            var comPortParam = ConfigurationManager.AppSettings["PaymentProcessor-AMP-ComPort"];
            var baudsParam = ConfigurationManager.AppSettings["PaymentProcessor-AMP-Bauds"];
            Connect("COM"+comPortParam, int.Parse(baudsParam));
        }


        public AMPResponsePayment ProcessSale(GeneratePaymentRequest request)
        {
            try
            {
                log.Info("DeviceCommands ProcessSale");
                result = false;
                string comport = "COM" + request.ComPort;
                if (serialAdapter == null)
                    Connect(comport, request.Bauds);

                AMPRequestPayment AMPrequest = new AMPRequestPayment();
                AMPrequest.EndPoint = "TRANSACTION";
                AMPrequest.cmdType = "Purchase";
                ReqPayload req = new ReqPayload();
                req.AutoPrint = "true";
                req.UserDefinedEchoData = DateTime.Now.ToString("yyMMddHHmmss");
                req.CardEntryMethod = "AUTO";
                req.BaseAmount = request.Amount.ToString().Replace(",", ".");
                AMPrequest.ReqPayload = req;

                string json = JsonConvert.SerializeObject(AMPrequest);
                serialAdapter.SendData(json);

                var firstSleep = ConfigurationManager.AppSettings["PaymentProcessor-AMP-FirstSleep"] ?? "3000";
                Thread.Sleep(int.Parse(firstSleep));

                var timeout = ConfigurationManager.AppSettings["PaymentProcessor-AMP-PurchaseTimeout"] ?? "40";
                var end = DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(int.Parse(timeout)));
                while (!result && DateTimeOffset.UtcNow < end)
                {
                    AMPRequestPayment GetResponse = new AMPRequestPayment();
                    GetResponse.EndPoint = "TRANSACTION";
                    GetResponse.cmdType = "GetTransactionResult";
                    json = JsonConvert.SerializeObject(GetResponse);
                    log.Info("DeviceCommands Send GetTransactionResult");
                    if (serialAdapter == null)
                    {
                        log.Info("serialAdapter NULL -- RECONNECT");
                        Connect(comport, request.Bauds);
                    }
                    serialAdapter.SendData(json);

                    var bucleSleep = ConfigurationManager.AppSettings["PaymentProcessor-AMP-BucleSleep"] ?? "1500";
                    Thread.Sleep(int.Parse(bucleSleep));
                }

                if (!result)
                {

                    if (IsCloseAPP)
                    {
                        log.Debug("RESPONSE CLOSED APP");
                        Response = new AMPResponsePayment
                        {
                            ecrConnectResponseCode = "-3",
                            ErrorMessage = "CLOSED APP"
                        };
                        return Response;
                    }

                    if (IsPaymetSend)
                    {
                        log.Debug("RESPONSE TIMEOUT APP");
                        Response = new AMPResponsePayment
                        {
                            ecrConnectResponseCode = "-2",
                            ErrorMessage = "TRANSACTION TIMEOUT"
                        };
                    }
                    else
                    {
                        log.Debug("AMP BLOCK APP");
                        Response = new AMPResponsePayment
                        {
                            ecrConnectResponseCode = "-1",
                            ErrorMessage = "AMP BLOCK"
                        };
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                log.Error("AMP EX ProcessSale:" + ex);
                Response = new AMPResponsePayment
                {
                    ecrConnectResponseCode = "-4",
                    ErrorMessage = "PAYMENT ERROR OR PIN ERROR"
                };
                return Response;
            }
        }



        public void Disconnect()
        {
            log.Debug(@"-- Disconnect --");
            if (serialAdapter != null)
            {
                serialAdapter.ReleasePort();
            }
        }

        public void Connect(string SerialPortName, int Baund)
        {
            try
            {
                log.Info(@"-- Connect --");

                serialAdapter = new AMPSerialAdapter();
                serialAdapter.RegisterDataReceivedListener(this);

                //El tamano del buffer de recepcion sale de la config (PaymentProcessor-AMP-BufferLength).
                //Si la clave falta o no es valida, se mantiene el 4096 de siempre.
                int bufferLength;
                if (!int.TryParse(ConfigurationManager.AppSettings["PaymentProcessor-AMP-BufferLength"], out bufferLength) || bufferLength <= 0)
                {
                    bufferLength = 4096;
                }

                log.Info("AMP receive buffer length: " + bufferLength);

                if (serialAdapter.CommConnect(SerialPortName, Baund, Parity.None, 8, StopBits.One, 600000, bufferLength))
                {
                    log.Info("Connection is stablished!");
                }
                else
                {
                    log.Error("Connection refused!");
                }
            }
            catch (Exception ex)
            {
                log.Error("AMP EX Connect:" + ex);
            }
        }

        public void _onDataReceived(ExchangeData exchangeData)
        {
            try
            {
                if (exchangeData.getResponseCode() > 0)
                {
                    //log.Debug("RESPONSE DATA1:" + exchangeData.getData());

                    if (exchangeData.getResponseCode() < 30)
                    {
                        log.Debug("RESPONSE DATA:" + exchangeData.getData());
                        AMPResponsePayment AMPresponse = JsonConvert.DeserializeObject<AMPResponsePayment>(exchangeData.getData());
                        if (AMPresponse.ecrConnectResponseCode != null && AMPresponse.ecrConnectResponseCode.Equals("0"))
                        {
                            log.Debug("RESULT DATA 0 : Success!");
                            IsPaymetSend = true;
                        }
                        else if (AMPresponse.ecrConnectResponseCode != null && AMPresponse.ecrConnectResponseCode.Equals("5"))
                        {
                            log.Debug("RESULT DATA 5 : The requested resource is busy handling another request");
                            IsCloseAPP = false;
                        }
                        else
                        {
                            log.Debug("RESULT DATA ecrConnectResponseCode: " + AMPresponse.ecrConnectResponseCode);
                            Response = new AMPResponsePayment
                            {
                                ecrConnectResponseCode = "-2",
                                ErrorMessage = "ERROR ecrConnectResponseCode:"+ AMPresponse.ecrConnectResponseCode
                            };
                            result = true;
                        }

                    }
                    else
                    {
                        string rawData = exchangeData.getData();
                        log.Debug("RESULT DATA:" + rawData);
                        int lastBrace = rawData.LastIndexOf('}');
                        if (lastBrace >= 0 && lastBrace < rawData.Length - 1)
                        {
                            log.Debug("Trimming extra characters after JSON: " + rawData.Substring(lastBrace + 1));
                            rawData = rawData.Substring(0, lastBrace + 1);
                        }
                        Response = JsonConvert.DeserializeObject<AMPResponsePayment>(rawData);   
                        result = true;
                    }
                }
                else
                {
                    Response = new AMPResponsePayment
                    {
                        ecrConnectResponseCode = "-3",                       
                        ErrorMessage = "ERROR SENT KO "
                    };
                    result = true;
                }
            }catch (Exception ex)
            {
                log.Error("RESULT DATA EX:"+ ex);
                Response = new AMPResponsePayment
                {
                    ecrConnectResponseCode = "-4",
                    ErrorMessage = ex.Message
                };
            }
            

        }

        public void _onDataSentResult(int result)
        {
            int temp = result;
            if (temp > 0)
            {
                log.Debug("[" + temp + "] data receibed!");
            }
        }
    }
}
