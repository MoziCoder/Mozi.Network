namespace Mozi.IoT.Encode
{
    //�����ͳһ��ȡUTF-8����
    /// <summary>
    /// �ַ�������
    /// </summary>
    public static class StringEncoder
    {
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Encode(string data)
        {
            return System.Text.Encoding.UTF8.GetBytes(data);
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Decode(byte[] data)
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }
    }
}