using System.Linq;
using System.Text;

namespace Mozi.Encode.CBOR
{
    /// <summary>
    /// B16转码
    /// </summary>
    public class Base16
    {
        /// <summary>
        /// 自定义Base16编码
        /// </summary>
        /// <param name="source">需要编码的字符串</param>
        /// <param name="autoCode">自定义Base16编码数组,16个元素,可以为数字、字符、特殊符号,若不填,使用默认的Base16编码数组,解码与编码的Base16编码数组一样</param>
        /// <returns></returns>
        public static string To(string source, string[] autoCode)
        {
            StringBuilder sb = new StringBuilder();
            if (autoCode == null || autoCode.Length < 16)
                autoCode = new string[] { "a", "2", "B", "g", "E", "5", "f", "6", "C", "8", "o", "9", "Z", "p", "k", "M" };
            var arr = Encoding.Default.GetBytes(source);
            for (int i = 0; i < arr.Length; i++)
            {
                byte data = (byte)arr[i];
                int v1 = data >> 4;
                sb.Append(autoCode[v1]);
                int v2 = ((data & 0x0f) << 4) >> 4;
                sb.Append(autoCode[v2]);
            }
            return sb.ToString();
        }
        public static string To(string source)
        {
            return To(source, null);
        }
        /// <summary>
        /// 自定义Base16解码
        /// </summary>
        /// <param name="source">需要解码的字符串</param>
        /// <param name="autoCode">自定义Base16编码数组,16个元素,可以为数字、字符、特殊符号,若不填,使用默认的Base16编码数组,解码与编码的Base16编码数组一样</param>
        /// <returns></returns>
        public static string From(string source, string[] autoCode)
        {
            int k = 0;
            int lenData = source.Length;
            if (autoCode == null || autoCode.Length < 16)
                autoCode = new string[] { "a", "2", "B", "g", "E", "5", "f", "6", "C", "8", "o", "9", "Z", "p", "k", "M" };
            byte[] data = new byte[lenData / 2];
            for (int i = 0, j = 0; i < data.Length; i++, j++)
            {
                byte s = 0;
                int index1 = autoCode.ToList().IndexOf(source[j].ToString());
                j += 1;
                int index2 = autoCode.ToList().IndexOf(source[j].ToString());
                s = (byte)(s ^ index1);
                s = (byte)(s << 4);
                s = (byte)(s ^ index2);
                data[k] = s;
                k++;
            }
            string result = Encoding.Default.GetString(data);
            return result;
        }

        public static string From(string source)
        {
            return From(source, null);
        }
    }
}
