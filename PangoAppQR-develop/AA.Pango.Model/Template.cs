using LiteDB;
using System;

namespace AA.Pango.Model
{
    public class Template
    {
        [BsonId]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }
        public bool Show { get; set; }
        public string TerminalId { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        public int SecondsToShowOnScreen { get; set; }
        public bool RequiresUserResponse { get; set; }
    }
}
