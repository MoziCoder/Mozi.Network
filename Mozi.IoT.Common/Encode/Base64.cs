using System;

namespace Mozi.IoT.Encode
{
    /// <summary>
    /// B64转码
    /// </summary>
    public class Base64
    {
        public static string To(string data)
        {
            byte[] infos = StringEncoder.Encode(data);
            return Convert.ToBase64String(infos, Base64FormattingOptions.None);
        }

        public static string From(string data)
        {
            return StringEncoder.Decode(Convert.FromBase64String(data));
        }
    }
}
