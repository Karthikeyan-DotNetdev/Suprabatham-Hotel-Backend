using AppSettings;
using System.Security.Cryptography;
using System.Text;

namespace CommonServices
{
    public class CipherService
    {
        public static string Encrypt(string textToEncrypt)
        {
            string ret_string = "";
            try
            {
                byte[] inputArray = UTF8Encoding.UTF8.GetBytes(textToEncrypt);
                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = UTF8Encoding.UTF8.GetBytes(AppSetting.CipherKey);
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tripleDES.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                ret_string = Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch { }
            return ret_string;
        }

        public static string Decrypt(string textToDecrypt)
        {
            string ret_string = "";
            try
            {
                byte[] inputArray = Convert.FromBase64String(textToDecrypt);
                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = UTF8Encoding.UTF8.GetBytes(AppSetting.CipherKey);
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tripleDES.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                ret_string = UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch { }
            return ret_string;
        }

        public static string Encrypt(string textToEncrypt, string cipherKey)
        {
            string ret_string = "";
            try
            {
                byte[] inputArray = UTF8Encoding.UTF8.GetBytes(textToEncrypt);
                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = UTF8Encoding.UTF8.GetBytes(cipherKey);
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tripleDES.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                ret_string = Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch { }
            return ret_string;
        }

        public static string Decrypt(string textToDecrypt, string cipherKey)
        {
            string ret_string = "";
            try
            {
                byte[] inputArray = Convert.FromBase64String(textToDecrypt);
                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = UTF8Encoding.UTF8.GetBytes(cipherKey);
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tripleDES.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                ret_string = UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch { }
            return ret_string;
        }
    }
}
