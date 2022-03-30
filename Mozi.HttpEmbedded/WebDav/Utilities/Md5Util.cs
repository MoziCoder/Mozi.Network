using System.Security.Cryptography;
using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded.WebDav.Utilities
{
    internal static class Md5Util
    {
        public static string Md5Hash4Utf8String(string s)
        {
            byte[] bytes = StringEncoder.Encode(s);

            MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(bytes);

            return Hex.To(hashBytes);
        }
    }
}