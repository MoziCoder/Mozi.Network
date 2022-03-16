using System;

namespace Mozi.HttpEmbedded.Encode
{
    public class Hex
    {
        public static byte[] From(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("参数长度不正确");
            }

            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }
        public static string To(byte[] data)
        {
            return To(data, 0, data.Length);
        }

        public static string To(byte[] data, int indStart, int length)
        {
            string s = BitConverter.ToString(data, indStart, length).Replace("-", string.Empty).ToLower();
            return s;
        }
    }
}
