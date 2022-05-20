using System;
using System.Security.Cryptography;
using System.Text;

namespace Mozi.HttpEmbedded.Encode
{
    public class Encrypt
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="encryptString">待加密的密文</param>
        /// <returns></returns>
        public static string MD5Encrypt(string encryptString)
        {
            string result;
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                result = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(encryptString))).Replace("-", "");
                md5.Clear();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
    }
}
