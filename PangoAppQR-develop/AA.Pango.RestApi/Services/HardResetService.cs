using AA.Pango.Model;
using AA.Pango.ServiceLayer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using AA.Pango.RestApi.Controllers;
using log4net;

namespace AA.Pango.RestApi.Services
{
    public class HardResetService
    {
        private static readonly byte[] ResetCommand = { 0x48 };//H NEW RESET             //OLD// E 0x45 //RESET
        private static ILog Log = LogManager.GetLogger(typeof(RestController));
        public static async void HardReset()
        {
            try
            {
                Thread.Sleep(2000);
                int bauds = int.Parse(ConfigurationManager.AppSettings["PicovBauds"] ?? "9600");
                SerialPort _picobSerialPort = null;
                _picobSerialPort = new SerialPort
                {
                    BaudRate = bauds,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Parity = Parity.None,
                    PortName = ConfigurationManager.AppSettings["PicovComPort"],
                    ReadTimeout = 4000,
                    WriteTimeout = 6000,
                    Handshake = Handshake.None,
                    Encoding = Encoding.ASCII,
                    ReceivedBytesThreshold = 9,
                };

                try
                {
                    _picobSerialPort.Open();
                    Log.Debug("HardReset -- Picob connected successfully");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Fatal("CANNOT CONNECT TO PICOB DEVICE!", ex);
                }
                catch (IOException ex)
                {
                    Log.Fatal("CANNOT CONNECT TO OUTPUT DEVICE!", ex);
                }

                if (_picobSerialPort?.IsOpen == true)
                {
                    _picobSerialPort.Write(ResetCommand, 0, ResetCommand.Length);
                    var response = _picobSerialPort.ReadExisting();
                    Log.Info("HardReset -- Successfully Hard Reset - RESPONSE:"+ response);
                    /*Thread.Sleep(2000);
                    Process.Start(ConfigurationManager.AppSettings["RebootTerminal"]);
                    Log.Info("HardReset --Reboot PC");  */              }
                else
                {
                    Log.Error("NO ENTRA AL PUERTO - CLOSE");
                }                
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

    }
}