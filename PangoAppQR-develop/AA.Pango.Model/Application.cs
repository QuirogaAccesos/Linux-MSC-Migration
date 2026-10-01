using System;

namespace AA.Pango.Model
{
    public class Application
    {
        public int _id { get; set; }
        public string Name { get; set; }
        public string LicenseKey { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? LastClientsSynchronizationDate { get; set; }
    }
}