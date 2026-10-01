using AA.Pango.ServiceLayer.ApiDtos;
using System;

namespace AA.Pango.TestWinApp
{

    public class PlateEnteredEventArgs : EventArgs
    {
        public string Plate { get; set; }
        public DateTime Date { get; set; }
    }

    public class PhoneEnteredEventArgs : EventArgs
    {
        public string Phone { get; set; }
        public DateTime Date { get; set; }
    }

    public class StatusEventArgs : EventArgs
    {
        public string Status { get; set; }
        public int Type { get; set; }
        public ChargePlateResponse chargeResponse { get; set; }
        public DateTime Date { get; set; }
        public int TransientId { get; set; }
        public string TicketQR { get; set; }
        public bool HasPermit { get; set; }
        public string TemplateId { get; set; }
        public string Plate { get; set;}
        public string Phone { get; set;}
    }
}
