using System;

namespace Mozi.IoT.Cache
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
        public static string GenerateETag(DateTime lastModifyTime, int fileSize)
        {
            var time = BitConverter.ToString(BitConverter.GetBytes(lastModifyTime.ToUniversalTime().ToTimestamp())).Replace("-", "").ToLower();
            return string.Format("{0}:{1}", time, fileSize);
        }
        /// <summary>
        /// 随机生成Token
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateToken(int tokenLen)
        {
            byte[] data = new byte[tokenLen];
            Random ran = new Random();
            ran.NextBytes(data);
            return data;
        }
    }
}