using System;

namespace AA.Pango.Model
{
    public class TemplateChangeScreen
    {
        public Guid _id { get; set; }
        public DateTime? Date { get; set; }
        public bool Processed { get; set; }
        public string InstallationId { get; set; }
        public string TerminalId { get; set; }
        public string TemplateId { get; set; }
        public int SecondsToShowOnScreen { get; set; }
        public bool RequiresUserResponse { get; set; }
        // CR-378 Free Flow: matrícula leída por LPR a mostrar en la pantalla de entrada.
        public string Plate { get; set; }

    }
}
