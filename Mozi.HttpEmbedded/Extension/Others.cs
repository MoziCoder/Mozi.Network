using System;

namespace Mozi.HttpEmbedded.Extension
{
    public static class Others
    {
        /// <summary>
        /// 数据翻转，不改变原始数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Revert(this byte[] data)
        {
            byte[] d2 = new byte[data.Length];
            Array.Copy(data, d2, d2.Length);
            Array.Reverse(d2);
            return d2;
        }
        /// <summary>
        /// DateTime转Unix时间戳
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime date)
        {
            var mills = (date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
            return (long)mills;
        }

        public static byte[] ToBytes(this ushort num)
        {
            return BitConverter.GetBytes(num);
        }
        public static byte[] ToBytes(this long num)
        {
            return BitConverter.GetBytes(num);
        }

        public static ushort ToUInt16(this byte[] data, int startIndex)
        {
            return BitConverter.ToUInt16(data, startIndex);
        }

        public static uint ToUInt32(this byte[] data, int startIndex)
        {
            return BitConverter.ToUInt32(data, startIndex);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static long ToInt64(this byte[] data, int startIndex)
        {
            return BitConverter.ToInt64(data, startIndex);
        }
    }
}
