using Microsoft.Extensions.Configuration;
using Parso.Classes;
using Parso.Utils;
using Serilog;
using System.Runtime.InteropServices;

internal class Program
{
    private static void Main(string[] args)
    {
        // Enable Serilog self-logging for debugging (optional)
        // Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"SERILOG INTERNAL: {msg}"));

        // Initialize logging and configuration FIRST
        string basePath = AppContext.BaseDirectory;
        string logDirectory = Path.Combine(basePath, "logs");
        string logFilePath = Path.Combine(logDirectory, "log-.txt");

        // Ensure the log directory exists and set up logging with fallback
        try
        {
            Directory.CreateDirectory(logDirectory);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day
                )
                .CreateLogger();
        }
        catch (Exception ex)
        {
            // Fallback to console if file logging fails
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            Log.Warning($"Could not create log directory at {logDirectory}. Using console only. Error: {ex.Message}");
        }

        Log.Information($"--------STARTING SERVICE----------");
        Log.Information($"Log directory: {logDirectory}");

        // Determine OS and load configuration
        string os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "Linux";
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Initialize ProjectConstants BEFORE creating any dependent objects
        ProjectConstants _projectConstants = ProjectConstants.Instance;
        try
        {
            //Picob
            _projectConstants.PICOB_COM_PORT = config[$"AppSettings:Picob:{os}"];
            _projectConstants.PICOB_REPONSE_TIMEOUT_MS = int.Parse(config[$"AppSettings:Picob:ResponseTimeoutMs"]);
            _projectConstants.PICOB_BAUD_RATE = int.Parse(config[$"AppSettings:Picob:BaudRate"]);
            _projectConstants.PICOB_AUTO_SET_TIME_ENABLED = config[$"AppSettings:Picob:AutoSetTimeEnabled"] == "TRUE";

            //Card Payment
            _projectConstants.CARD_PAYMENT_CARD_READER_TYPE = int.Parse(config[$"AppSettings:CardReader:CardReaderType"]);
            _projectConstants.CARD_PAYMENT_COM_PORT = config[$"AppSettings:CardReader:{os}"];
            _projectConstants.CARD_PAYMENT_ETHERNET_PORT_NUMBER = int.Parse(config[$"AppSettings:CardReader:EthernetPortNumber"]);
            _projectConstants.CARD_PAYMENT_BAUD_RATE = int.Parse(config[$"AppSettings:CardReader:BaudRate"]);
            _projectConstants.CARD_PAYMENT_CURRENCY_CODE = int.Parse(config[$"AppSettings:CardReader:CurrencyCode"]);
            _projectConstants.CARD_PAYMENT_EMODE = int.Parse(config[$"AppSettings:CardReader:EMode"]);
            _projectConstants.CARD_PAYMENT_LANGUAGE = int.Parse(config[$"AppSettings:CardReader:Language"]);
            _projectConstants.CARD_PAYMENT_TIMEOUT_SECOND = int.Parse(config[$"AppSettings:CardReader:TimeoutSeconds"]);
            _projectConstants.CARD_PAYMENT_SCAN_TIMER_MS = int.Parse(config[$"AppSettings:CardReader:ScanTimerMs"]);
            _projectConstants.CARD_PAYMENT_INCLUDE_RP_LOG = config["AppSettings:CardReader:IncludeRPlog"] == "TRUE";
            _projectConstants.CARD_PAYMENT_SEND_RP_CARD_INSERTED_NOTIFICATION = config["AppSettings:CardReader:SendRPCardInsertedNotification"] == "TRUE";
            _projectConstants.CARD_PAYMENT_RP_CARD_INSERTED_MESSAGE = config["AppSettings:CardReader:RPCardInsertedMessage"];

            //Other settings
            _projectConstants.ENABLE_PAYMENT_TEST = config["AppSettings:TestingMode:EnablePaymentTest"] == "TRUE";
            _projectConstants.ENABLE_PICOB_TEST = config["AppSettings:TestingMode:EnablePicobTest"] == "TRUE";
            _projectConstants.ENABLE_PRINTER_TEST = config["AppSettings:TestingMode:EnablePrinterTest"] == "TRUE";
            _projectConstants.CUSTOM_PROCESSOR = config["AppSettings:CustomProcessor"] == "TRUE";
            _projectConstants.PRINTING_TEMPLATE_FOLDER_LOCATION = config[$"AppSettings:PrintingTemplatesAbsoluteLocation"];
            _projectConstants.PRINTING_CONFIG_FILE_NAME = config[$"AppSettings:PrintingConfigFileName"];
        }
        catch (Exception ex)
        {
            Log.Error($"CRITICAL ERROR: {ex.Message}");
            throw; // Critical error should halt execution
        }

        Log.Information($"DetectedOS: {os}");
        Log.Information($"Picob COM Port: {_projectConstants.PICOB_COM_PORT}");

        if (_projectConstants.PICOB_AUTO_SET_TIME_ENABLED)
        {
            CommandProcessor processor = new CommandProcessor();

            Log.Information($"Setting Time automatically for Picob Device");
            try
            {
                string setResponse = processor.processCommand($"{{\"2\":{{\"Z\":\"{DateTime.Now:HH:mm:ss}\"}}}}");
                Log.Information($"Time set correctly. Response: {setResponse}");
            }
            catch (Exception ex)
            {
                Log.Error($"CRITICAL ERROR - AUTOMATIC TIME SET COULD NOT BE COMPLETE: {ex.Message}");
            }
        }

        // NOW start the server (after constants are set)
        ServerListener.Instance.StartServer();
    }
}