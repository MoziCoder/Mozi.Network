using System;

namespace Mozi.StateService.Generic
{
    internal static class Others
    {
        /// <summary>
        /// 数据翻转
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Revert(this byte[] data)
        {
            Array.Reverse(data);
            return data;
        }
        /// <summary>
        /// 时间转Unix时间戳
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime date)
        {
            var mills = (date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
            return (long)mills;
        }
        public static DateTime ToDateTime(this long timestamp)
        {
            var dtMin = new DateTime(1970, 1, 1, 0, 0, 0);
            return dtMin.AddMilliseconds(timestamp).ToLocalTime();
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

        public static string ToCharString(this byte data)
        {
            return System.Text.Encoding.ASCII.GetString(new byte[] { data });
        }

        public static byte ToCharByte(this int data)
        {
            if (data >= 0 && data <= 9)
            {
                return System.Text.Encoding.ASCII.GetBytes(data.ToString())[0];
            }
            else
            {
                return 0;
            }
        }
    }
}
