using System;
using System.Collections.Generic;

namespace Mozi.SSDP
{
    /// <summary>
    /// 缓存管理器
    /// </summary>
    public class SSDPCacheManager
    {

        public static SSDPCacheManager _cm;
        /// <summary>
        /// 单实例
        /// </summary>
        public static SSDPCacheManager Instance
        {
            get { return _cm ?? (_cm = new SSDPCacheManager()); }
        }

        private List<SSDPCache> _caches = new List<SSDPCache>();

        private SSDPCacheManager()
        {

        }

    }
    /// <summary>
    /// 缓存
    /// </summary>
    public class SSDPCache
    {
        public string USN { get; set; }
        public string ServiceType { get; set; }
        public DateTime AddTime { get; set; }
        public int Expiration { get; set; }
        public string Location { get; set; }
    }
}
