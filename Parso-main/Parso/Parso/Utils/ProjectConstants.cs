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
        public bool ENABLE_PAYMENT_TEST { get; set; } = false;
        public bool ENABLE_PICOB_TEST { get; set; } = false;
        public bool ENABLE_PRINTER_TEST { get; set; } = false;
        public bool CUSTOM_PROCESSOR { get; set; } = true;
        public string PRINTING_TEMPLATE_FOLDER_LOCATION { get; set; } = "";
        public string PRINTING_CONFIG_FILE_NAME { get; set; } = "";
    }
}
