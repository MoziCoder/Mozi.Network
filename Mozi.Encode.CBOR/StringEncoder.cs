namespace Mozi.Encode.CBOR
{
    //服务端统一采取UTF-8编码
    /// <summary>
    /// 字符串编码
    /// </summary>
    public static class StringEncoder
    {
        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Encode(string data)
        {
            return System.Text.Encoding.UTF8.GetBytes(data);
        }
        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Decode(byte[] data)
        {
            return Decode(data, 0, data.Length);
        }
        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Decode(byte[] data, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetString(data, index, count);
        }
    }
}