namespace QR_API.Helper
{
    using System;
    using System.Configuration;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Service for crypting data using symmetric algorithms.
    /// </summary>
    public class CryptService
    {
        private string seed = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptService"/> class.
        /// </summary>
        /// <param name="seed">The seed.</param>
        public CryptService(string seed)
        {
            this.seed = seed;
        }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <typeparam name="TSymmetricAlgorithm">The type of the symmetric algorithm.</typeparam>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public string Encrypt<TSymmetricAlgorithm>(string input) 
            where TSymmetricAlgorithm : SymmetricAlgorithm, new()
        {
            var pwdBytes = Encoding.UTF8.GetBytes(this.seed);
            
            using (TSymmetricAlgorithm sa = new TSymmetricAlgorithm())
            {
                ICryptoTransform transform = sa.CreateEncryptor(pwdBytes, pwdBytes);
                var encBytes = Encoding.UTF8.GetBytes(input);            
                var resultBytes = transform.TransformFinalBlock(encBytes, 0, encBytes.Length);

                return Convert.ToBase64String(resultBytes);
            }
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <typeparam name="TSymmetricAlgorithm">The type of the symmetric algorithm.</typeparam>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public string Decrypt<TSymmetricAlgorithm>(string input) 
            where TSymmetricAlgorithm : SymmetricAlgorithm, new()
        {
            var pwdBytes = Encoding.UTF8.GetBytes(this.seed);
            using (TSymmetricAlgorithm sa = new TSymmetricAlgorithm())
            {
                ICryptoTransform transform = sa.CreateDecryptor(pwdBytes, pwdBytes);
                var encBytes = Convert.FromBase64String(input);
                var resultBytes = transform.TransformFinalBlock(encBytes, 0, encBytes.Length);

                return Encoding.UTF8.GetString(resultBytes, 0, resultBytes.Length);
            }
        }


        public static string encryptAppString<TSymmetricAlgorithm>(string input)
            where TSymmetricAlgorithm : SymmetricAlgorithm, new()
        {
            var pwdBytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["encryptAppSecurityKey"]);

            using (TSymmetricAlgorithm sa = new TSymmetricAlgorithm())
            {
                ICryptoTransform transform = sa.CreateEncryptor(pwdBytes, pwdBytes);
                var encBytes = Encoding.UTF8.GetBytes(input);
                var resultBytes = transform.TransformFinalBlock(encBytes, 0, encBytes.Length);

                return Convert.ToBase64String(resultBytes);
            }
        }

        public static string decryptAppString<TSymmetricAlgorithm>(string input)
            where TSymmetricAlgorithm : SymmetricAlgorithm, new()
        {
            try
            {
                var pwdBytes = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["encryptAppSecurityKey"]);
                using (TSymmetricAlgorithm sa = new TSymmetricAlgorithm())
                {
                    ICryptoTransform transform = sa.CreateDecryptor(pwdBytes, pwdBytes);
                    var encBytes = Convert.FromBase64String(input);
                    var resultBytes = transform.TransformFinalBlock(encBytes, 0, encBytes.Length);

                    return Encoding.UTF8.GetString(resultBytes, 0, resultBytes.Length);
                }
            }catch (Exception ex)
            {
                return "";
            }
        }
    }
}
