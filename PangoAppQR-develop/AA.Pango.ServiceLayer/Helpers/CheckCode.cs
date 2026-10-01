using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AA.Pango.ServiceLayer.Helpers
{
    public static class CheckCode
    {
        private static int LENGTH_QRCODE = 108;
        public static string GetValidCode(this string code)
        {
            string value = code.Trim();
            try
            {
                byte[] data = Convert.FromBase64String(value);
                return value;
            }
            catch
            {
                int length = value.Length;
                int lastIndexEquals = value.LastIndexOf('=') + 1;
                if(length > LENGTH_QRCODE && lastIndexEquals == LENGTH_QRCODE)
                {
                    return value.Substring(0, lastIndexEquals);
                }
            }
            return value;
        }
    }
}
