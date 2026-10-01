using AA.Pango.ServiceLayer;
using AA.ParkingStation.Service;
using log4net;
using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AA.Pango.App
{
    /// <summary>
    /// Entry class for the whole application
    /// </summary>
    internal static class Program
    {
        private static ILog _log = LogManager.GetLogger("Program");

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // redirect console output to parent process; must be before any calls to Console.WriteLine()
            AttachConsole(ATTACH_PARENT_PROCESS);

            _log.Debug("Application started");

            SeedDatabase();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _log.Debug("application running");

            var screenForm = ConfigurationManager.AppSettings["ScreenForm"];
            if (screenForm == "V2")
                Application.Run(new FormV2());
            else if (screenForm == "V3")
                Application.Run(new FormV3());
            else if (screenForm == "V3_QRTicket")
                Application.Run(new FormV3_QRTicket());
            else if (screenForm == "V3_3opt")
                Application.Run(new FormV3_3opt());
            else if (screenForm == "V3_1opt")
                Application.Run(new FormV3_1opt());
            else if (screenForm == "V4")
                Application.Run(new FormV4());
            else if (screenForm == "V4_QRTicket")
                Application.Run(new FormV4_QRTicket());
            else if (screenForm == "V4_3opt")
                Application.Run(new FormV4_3opt());
            else if (screenForm == "V4_1opt")
                Application.Run(new FormV4_1opt());
            else if (screenForm == "V4_Ticket_PH")
                Application.Run(new FormV4_Ticket_PH());
        }

        /// <summary>
        /// Inits the litedb database with sample data in case it doesn't exist.
        /// </summary>
        private static void SeedDatabase()
        {
            _log.Debug("Seed database");

            //seed application
            ApplicationService.Seed(System.Configuration.ConfigurationManager.AppSettings["AppName"], true, DateTime.Now.AddMonths(2));

            //seed parkings
            ParkingService.Seed();

            //seed terminals
            TerminalService.Seed();

            //seed tickets
            TicketService.Seed();

            //seed templates
            TemplateService.Seed();

            //seed changeScreen
            ChangeScreenService.Seed();

            //seed payments
            PaymentService.Seed();

        }

        /// <summary>
        /// Shutdowns the application
        /// </summary>
        public static void Exit()
        {
            try
            {
                // Quit app with success code.
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                // Quit app reporting exit error code.
                Environment.Exit(1);
            }
        }
    }
}