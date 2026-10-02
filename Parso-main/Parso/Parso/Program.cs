using Microsoft.Extensions.Configuration;
using Parso.Classes;
using Parso.Classes.Helpers;
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
            _projectConstants.PICOB_PERSISTENT_CONNECTION = ReadBool(config, "AppSettings:Picob:PersistentConnection", false);
            _projectConstants.PICOB_DTR_ENABLE = ReadBool(config, "AppSettings:Picob:DtrEnable", true);
            _projectConstants.PICOB_RTS_ENABLE = ReadBool(config, "AppSettings:Picob:RtsEnable", true);
            _projectConstants.PICOB_MIN_COMMAND_INTERVAL_MS = ReadInt(config, "AppSettings:Picob:MinCommandIntervalMs", 800);
            _projectConstants.PICOB_POLL_INTERVAL_MS = ReadInt(config, "AppSettings:Picob:PollIntervalMs", 1000);
            _projectConstants.PICOB_DISCONNECT_AFTER_FAILURES = ReadInt(config, "AppSettings:Picob:DisconnectAfterFailures", 3);
            _projectConstants.PICOB_FIRE_AND_FORGET_COMMANDS = (config["AppSettings:Picob:FireAndForgetCommands"] ?? "H")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(c => c.ToUpperInvariant())
                .ToArray();

            // The firmware needs at least 800 ms between commands
            if (_projectConstants.PICOB_PERSISTENT_CONNECTION && _projectConstants.PICOB_MIN_COMMAND_INTERVAL_MS < 800)
            {
                Log.Warning($"Picob:MinCommandIntervalMs {_projectConstants.PICOB_MIN_COMMAND_INTERVAL_MS} is below the 800 ms the firmware needs. Using 800.");
                _projectConstants.PICOB_MIN_COMMAND_INTERVAL_MS = 800;
            }

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
            // A relative folder is resolved against the app folder
            string templateFolder = config[$"AppSettings:PrintingTemplatesAbsoluteLocation"] ?? "";
            _projectConstants.PRINTING_TEMPLATE_FOLDER_LOCATION = templateFolder == "" || Path.IsPathRooted(templateFolder) ? templateFolder : Path.Combine(basePath, templateFolder);
            _projectConstants.PRINTING_CONFIG_FILE_NAME = config[$"AppSettings:PrintingConfigFileName"];
            _projectConstants.PRINTING_PAPER_SIZE_MM = ReadInt(config, "AppSettings:Printing:PaperSizeMm", 58);
            _projectConstants.PRINTING_PRINTER_NAME = config["AppSettings:Printing:PrinterName"]?.Trim() ?? "";
            string? listenAddress = config["AppSettings:ListenAddress"];
            _projectConstants.LISTEN_ADDRESS = string.IsNullOrWhiteSpace(listenAddress) ? "0.0.0.0" : listenAddress.Trim();
        }
        catch (Exception ex)
        {
            Log.Error($"CRITICAL ERROR: {ex.Message}");
            throw; // Critical error should halt execution
        }

        Log.Information($"DetectedOS: {os}");
        Log.Information($"Picob COM Port: {_projectConstants.PICOB_COM_PORT}");
        Log.Information($"Printing templates folder: {_projectConstants.PRINTING_TEMPLATE_FOLDER_LOCATION}. Paper: {_projectConstants.PRINTING_PAPER_SIZE_MM} mm. Printer: '{_projectConstants.PRINTING_PRINTER_NAME}'");
        Log.Information($"Picob persistent connection: {_projectConstants.PICOB_PERSISTENT_CONNECTION}. Listen address: {_projectConstants.LISTEN_ADDRESS}");

        if (_projectConstants.PICOB_PERSISTENT_CONNECTION && !_projectConstants.ENABLE_PICOB_TEST)
        {
            Log.Information($"Starting persistent Picob session");
            PicobSession.Instance.Start();
        }

        if (_projectConstants.PICOB_AUTO_SET_TIME_ENABLED && _projectConstants.PICOB_PERSISTENT_CONNECTION)
        {
            Log.Information($"Automatic Picob time set skipped: the persistent connection does not use it");
        }
        else if (_projectConstants.PICOB_AUTO_SET_TIME_ENABLED)
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

    // Optional settings: a missing or invalid value gives the default instead of aborting startup
    private static bool ReadBool(IConfiguration config, string key, bool defaultValue)
    {
        string? value = config[key];
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase);
    }

    private static int ReadInt(IConfiguration config, string key, int defaultValue)
    {
        string? value = config[key];
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        if (int.TryParse(value, out int result)) return result;

        Log.Warning($"Invalid value '{value}' for {key}. Using {defaultValue}.");
        return defaultValue;
    }
}