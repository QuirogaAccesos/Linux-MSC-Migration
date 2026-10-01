using AA.PangoApp.Payments.Interface.Dtos;

namespace AA.PangoApp.Payments.Interface
{

    public enum ProcessPaymentStatus
    {
        Approved,
        Denied,
        Error,
        Undefined
    }

    public interface IPaymentManager
    {
        GeneratePaymentResponse ProcessPayment(GeneratePaymentRequest request);
    }
}
