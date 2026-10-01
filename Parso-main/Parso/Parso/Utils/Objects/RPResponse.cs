namespace Parso.Utils.Objects
{
    internal class RPResponse
    {
        public bool ExceptionEncountered { get; set; }
        public bool Success { get; set; }
        public string LastOperation { get; set; }
        public string? LastTransactionStatus { get; set; }
        public string? Log { get; set; }
        public bool Respond { get; set; } = true;
        public bool Timeout { get; set; } = false;

        public RPResponse(bool success, string lastOperation, string lastTransactionStatus, string log, bool timeout)
        {
            ExceptionEncountered = false;
            Success = success;
            LastOperation = lastOperation;
            LastTransactionStatus = lastTransactionStatus;
            Log = log;
            Timeout = timeout;
        }

        public RPResponse(bool success, string lastOperation, string lastTransactionStatus, string log)
        {
            ExceptionEncountered = false;        
            Success = success;
            LastOperation = lastOperation;
            LastTransactionStatus = lastTransactionStatus;
            Log = log;
        }

        public RPResponse(bool success, string lastOperation, string log)
        {
            ExceptionEncountered = false;
            Success = success;
            LastOperation = lastOperation;
            Log = log;
        }
        public RPResponse(string lastOperation, string exceptionDetails)
        {
            ExceptionEncountered = true;
            Success = false;
            LastOperation = lastOperation;
            Log = exceptionDetails;
        }

        public RPResponse(bool respond)
        {
            ExceptionEncountered = true;
            Success = false;
            LastOperation = "";
            Log = "";
            Respond = respond;
        }
    }
}
