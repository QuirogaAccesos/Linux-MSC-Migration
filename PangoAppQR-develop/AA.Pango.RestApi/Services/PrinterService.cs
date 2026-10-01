using Newtonsoft.Json;
using QR_API.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

namespace AA.Pango.RestApi.Services
{
    public class PrinterService
    {

        public static string generateTicketQR(int transientId)
        {
            string result = "";
            try
            {

                var checksum = (transientId + Int32.Parse(transientId.ToString().Substring(transientId.ToString().Length - 2))) * 131;

                QRTicketData data = new QRTicketData();
                data.TransientId = transientId;
                data.Datetime = DateTime.UtcNow;
                data.Checksum = checksum;

                result = JsonConvert.SerializeObject(data);

                result = CryptService.encryptAppString<AesManaged>(result);
            }
            catch (Exception e)
            {
                throw e;
            }

            return result;
        }

        public static string[] GetTicketHeader()
        {
            return ConfigurationManager.AppSettings["Header"].Split('\n');
        }

        public static string[] GetTicketFooter()
        {
            return ConfigurationManager.AppSettings["Footer"].Split('\n');
        }
    }

    [DataContract]
    public class QRTicketData
    {
        [DataMember]
        public int TransientId { get; set; }
        [DataMember]
        public long Checksum { get; set; }
        [DataMember]
        public DateTime Datetime { get; set; }
    }
}