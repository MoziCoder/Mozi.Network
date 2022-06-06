using System;

namespace Mozi.HttpEmbedded.Encode
{
    /// <summary>
    /// B64转码
    /// </summary>
    public class Base64
    {
        /// <summary>
        /// 字符串中B64
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string To(string data)
        {
            byte[] infos = StringEncoder.Encode(data);
            return Convert.ToBase64String(infos, Base64FormattingOptions.None);
        }
        /// <summary>
        /// B64转字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string From(string data)
        {
            return StringEncoder.Decode(Convert.FromBase64String(data));
        }
    }
}
