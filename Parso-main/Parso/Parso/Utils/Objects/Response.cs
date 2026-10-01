namespace Parso.Utils.Objects
{
    public class Response
    {
        public bool Success { get; set; }
        public int HTTPStatus { get; set; }
        public bool OperationResult { get; set; }
        public string Message { get; set; }
        public string? DeviceResponse { get; set; }

        public Response(bool success, int hTTPStatus, bool operationResult, string message)
        {
            Success = success;
            HTTPStatus = hTTPStatus;
            OperationResult = operationResult;
            Message = message;
        }

        public Response(bool success, int hTTPStatus, bool operationResult, string message, string deviceResponse)
        {
            Success = success;
            HTTPStatus = hTTPStatus;
            OperationResult = operationResult;
            Message = message;
            DeviceResponse = deviceResponse;
        }
    }
}
