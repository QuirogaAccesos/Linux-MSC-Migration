using Parso.Utils.Objects;
using System.Configuration;

namespace Parso.Utils
{
    internal class ProjectConstants
    {
        private static ProjectConstants _instance;
        public static ProjectConstants Instance => _instance ??= new ProjectConstants();
        private ProjectConstants() { }

        //Picob settings
        public string PICOB_COM_PORT { get; set; } = "";
        public int PICOB_REPONSE_TIMEOUT_MS { get; set; } = 1500;
        public int PICOB_BAUD_RATE { get; set; } = 115200;
        public bool PICOB_AUTO_SET_TIME_ENABLED = false; 
        public bool PICOB_PERSISTENT_CONNECTION { get; set; } = false;
        public bool PICOB_DTR_ENABLE { get; set; } = true;
        public bool PICOB_RTS_ENABLE { get; set; } = true;
        public int PICOB_MIN_COMMAND_INTERVAL_MS { get; set; } = 800;
        public int PICOB_POLL_INTERVAL_MS { get; set; } = 1000;
        public int PICOB_DISCONNECT_AFTER_FAILURES { get; set; } = 3;
        public string[] PICOB_FIRE_AND_FORGET_COMMANDS { get; set; } = { "H" };

        //Card payment settings
        public int CARD_PAYMENT_CARD_READER_TYPE { get; set; } = 0;
        public string CARD_PAYMENT_COM_PORT { get; set; } = "";
        public int CARD_PAYMENT_ETHERNET_PORT_NUMBER { get; set; } = 0;
        public int CARD_PAYMENT_BAUD_RATE { get; set; } = 115200;
        public int CARD_PAYMENT_CURRENCY_CODE { get; set; } = 840;
        public int CARD_PAYMENT_EMODE { get; set; } = 48;
        public int CARD_PAYMENT_LANGUAGE { get; set; } = 0;
        public int CARD_PAYMENT_TIMEOUT_SECOND { get; set; } = 30;
        public int CARD_PAYMENT_SCAN_TIMER_MS { get; set; } = 2500;
        public bool CARD_PAYMENT_INCLUDE_RP_LOG { get; set; } = false;
        public bool CARD_PAYMENT_SEND_RP_CARD_INSERTED_NOTIFICATION { get; set; } = false;
        public string CARD_PAYMENT_RP_CARD_INSERTED_MESSAGE { get; set; } = "CARDINSERTED";

        //Other settings
        public string LISTEN_ADDRESS { get; set; } = "0.0.0.0";
        public bool ENABLE_PAYMENT_TEST { get; set; } = false;
        public bool ENABLE_PICOB_TEST { get; set; } = false;
        public bool ENABLE_PRINTER_TEST { get; set; } = false;
        public bool CUSTOM_PROCESSOR { get; set; } = true;
        public string PRINTING_TEMPLATE_FOLDER_LOCATION { get; set; } = "";
        public string PRINTING_CONFIG_FILE_NAME { get; set; } = "";
        public int PRINTING_PAPER_SIZE_MM { get; set; } = 58;
        public string PRINTING_PRINTER_NAME { get; set; } = "";
        public int PRINTING_QR_PIXELS_PER_MODULE { get; set; } = 6;
        public string PRINTING_QR_ECC_LEVEL { get; set; } = "L";
    }
}
