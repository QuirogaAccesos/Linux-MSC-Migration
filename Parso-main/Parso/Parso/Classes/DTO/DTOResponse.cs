using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parso.Classes.DTO
{
    internal class DTOResponse
    {
        public bool status { get; set; }
        public object? data { get; set; }
        public bool respond { get; set; } = true;
        public bool timeout { get; set; } = false;

        // Primary constructor for full initialization
        public DTOResponse(bool status, object data)
        {
            this.status = status;
            this.data = data;
        }

        public DTOResponse(bool status, object data, bool respond)
        {
            this.status = status;
            this.data = data;
            this.respond = respond;
        }

        public DTOResponse(bool status, object data, bool respond, bool timeout)
        {
            this.status = status;
            this.data = data;
            this.respond = respond;
            this.timeout = timeout;
        }

        public DTOResponse(bool respond)
        {
            status = false;
            this.respond = respond;
        }
    }
}
