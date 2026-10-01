using CCI.Globalcom.GlobalcomRetailProtocol;
using System;
using System.Text;

namespace CCI.Globalcom.GlobalcomRetailProtocol
{
    /// <summary>
    ///   Class to wrap some common functions when accessing a Banking Parameter
    /// </summary>
    public class RpResponseBankingParam : RpResponseBase
    {
        /// <summary>
        ///   The parameters being retrieved
        /// </summary>
        BankingParameter _param;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="param"></param>
        /// <param name="response"></param>
        /// <param name="deviceFamily"></param>
        public RpResponseBankingParam(byte param, byte[] response, EDeviceFamily deviceFamily)
            : base(response)
        {
            _param = new BankingParameter(param, deviceFamily);
        }

        /// <summary>
        ///  The Value as a string.
        /// </summary>
        /// <param name="sValue">Value of the banking parameter or the error in the event of an error</param>
        /// <returns>If retrieveing the value was successful</returns>
        public bool ValueAsString(ref string sValue)
        {
            // If the terminal for some reason is not configured or erroring out let them know.
            if (VerifyOutcome() != EOutcome.OK)
            {
                sValue = VerifyOutcome().ToString();
                return false;
            }

            sValue = _param.ConvertValueToString(Info);
            return true;
        }
    }
}
