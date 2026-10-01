namespace AA.Pango.Model
{
    public enum TerminalType
    {
        Entry,
        Exit,
        Validator,
        Atm
    }

    public class Terminal
    {
        public int _id { get; set; }

        public string TerminalNumber { get; set; }

        public string SerialNumber { get; set; }

        public string InstallationId { get; set; }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public TerminalType Type { get; set; }

        public bool ForceOpenBarrier { get; set; }
        public bool ForceWhitelistUpload { get; set; }
        public bool ShowWhitelist { get; set; }
    }
}