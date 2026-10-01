using AA.PangoApp.Payments.Interface.Notifications;

namespace AA.Pango.ServiceLayer.Helpers
{
    public class Utils
    {
        /// <summary>
        /// Envío de correo de alerta. La implementación real (enriquecido de asunto/cuerpo +
        /// Microsoft Graph con fallback SMTP) vive en el punto único
        /// AA.PangoApp.Payments.Interface.Notifications.MailSender.
        /// Se mantiene esta firma para no tocar los llamantes existentes.
        /// </summary>
        public static void sendMail(string subject, string body)
        {
            MailSender.Send(subject, body);
        }
    }
}
