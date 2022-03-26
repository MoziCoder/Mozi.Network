using System;

namespace Mozi.HttpEmbedded.Encode
{
    /// <summary>
    /// Hex转译
    /// </summary>
    public class Hex
    {
        /// <summary>
        /// Hex字符串转字节流
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 字节流转Hex字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string To(byte[] data)
        {
            return To(data, 0, data.Length);
        }
        /// <summary>
        /// 字节流转Hex字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="indStart">起始位置</param>
        /// <param name="length">截取长度</param>
        /// <returns></returns>
        public static string To(byte[] data, int indStart, int length)
        {
            string s = BitConverter.ToString(data, indStart, length).Replace("-", string.Empty).ToLower();
            return s;
        }
    }
}
