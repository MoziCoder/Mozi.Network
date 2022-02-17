using System;
using System.Security.Cryptography;
using Mozi.HttpEmbedded.Extension;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 缓存管理
    /// 服务端用ETag标识缓存资源
    /// 客户机利用If-None-Match请求缓存
    /// </summary>
    public class CacheControl
    {
        private static CacheControl _control;

        public CacheControl Instance
        {
            get { return _control ?? (_control = new CacheControl()); }
        }

        private CacheControl()
        {

        }
        /// <summary>
        /// ETAG生成器
        /// </summary>
        /// <param name="lastModifyTime"></param>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public static string GenerateETag(DateTime lastModifyTime,int fileSize)
        {
            var time = BitConverter.ToString(BitConverter.GetBytes(lastModifyTime.ToUniversalTime().ToTimestamp())).Replace("-","").ToLower();
            return string.Format("{0}:{1}",time, fileSize);
        }
        /// <summary>
        /// 生成真随机数
        /// </summary>
        /// <param name="len"></param>
        /// <param name="isPureNumber"></param>
        /// <returns></returns>
        public static string GenerateRandom(int len, int isPureNumber = 0)
        {
            var seads = isPureNumber == 1 ? "0123456789" : "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string gr = "";
            byte[] bytes = new byte[4];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            var random = new Random(BitConverter.ToInt32(bytes, 0));
            for (int i = 0; i < len; i++)
            {
                gr += seads[random.Next(0, seads.Length)];
            }
            return gr;
        }
    }
}