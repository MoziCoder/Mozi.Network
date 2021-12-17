namespace Mozi.IoT.Encode
{
    //服务端统一采取UTF-8编码
    /// <summary>
    /// 字符串编码
    /// </summary>
    public static class StringEncoder
    {
        public static byte[] Encode(string data)
        {
            return System.Text.Encoding.UTF8.GetBytes(data);
        }

        public static string Decode(byte[] data)
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }
    }
}