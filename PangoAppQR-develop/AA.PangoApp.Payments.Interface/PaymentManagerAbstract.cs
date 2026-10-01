using AA.PangoApp.Payments.Interface.Dtos;
using System;

namespace AA.PangoApp.Payments.Interface
{

    public enum CurrentReaderStatus
    {
        Idle,
        Busy,
        WaitingForCard,
        CardInserted,
        RemoveCard,
        InError
    }

    public class PaymentManagerAbstract : IPaymentManager
    {

        public volatile CurrentReaderStatus CurrentReaderStatus = CurrentReaderStatus.Idle;

        #region Main Methods

        public virtual GeneratePaymentResponse ProcessPayment(GeneratePaymentRequest request)
        {
            throw new NotImplementedException("Implement me");
        }

        #endregion

        #region Event Handlers

        public event EventHandler PaymentResponseObtained;

        protected virtual void OnPaymentResponseObtained(EventArgs e)
        {
            PaymentResponseObtained?.Invoke(this, e);
        }

        #endregion
    }
}
