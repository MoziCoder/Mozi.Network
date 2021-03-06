using System;
using System.Collections;
using System.Collections.Generic;

namespace Mozi.SSDP
{
    /// <summary>
    /// 缓存管理器
    /// </summary>
    public class SSDPCacheCollection : IEnumerable
    {

        private static SSDPCacheCollection _cm;
        /// <summary>
        /// 单实例
        /// </summary>
        public static SSDPCacheCollection Instance
        {
            get { return _cm ?? (_cm = new SSDPCacheCollection()); }
        }

        private List<SSDPCache> _caches = new List<SSDPCache>();

        private SSDPCacheCollection()
        {

        }
        /// <summary>
        /// 增加缓存对象
        /// </summary>
        /// <param name="cache"></param>
        public void Add(SSDPCache cache)
        {
            var c = _caches.Find(x => x.Host == cache.Host);
            if (c == null)
            {
                _caches.Add(cache);
            }
            else
            {
                c = cache;
            }
        }
        /// <summary>
        /// 移除缓存对象
        /// </summary>
        /// <param name="cache"></param>
        public void Remove(SSDPCache cache)
        {
            _caches.Remove(cache);
        }
        /// <summary>
        /// 移除缓存对象
        /// </summary>
        /// <param name="usn"></param>
        public void Remove(USNDesc usn)
        {
            _caches.RemoveAll(x => x.USN.ToString() == usn.ToString());
        }
        /// <summary>
        /// 迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return new SSDPCacheCollectionEnumerator(_caches);
        }
    }

    /// <summary>
    /// 缓存
    /// </summary>
    public class SSDPCache
    {
        /// <summary>
        /// 对端HOST信息，以UDP通讯地址为准，格式为{ip}:{port}
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 终端唯一服务名
        /// </summary>
        public USNDesc USN { get; set; }
        /// <summary>
        /// 对端服务器信息
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// UTC时间
        /// </summary>
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 缓存超时时间，单位：s
        /// </summary>
        public int Expiration { get; set; }
        /// <summary>
        /// 设备|服务，描述文档地址
        /// </summary>
        public string Location { get; set; }
    }

    /// <summary>
    /// 文件集合迭代器
    /// </summary>
    public class SSDPCacheCollectionEnumerator : IEnumerator
    {
        private int _index;

        private List<SSDPCache> _collection;

        private SSDPCache value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="colletion"></param>
        public SSDPCacheCollectionEnumerator(List<SSDPCache> colletion)
        {
            _collection = colletion;
            _index = -1;
        }
        object IEnumerator.Current
        {
            get { return value; }
        }
        /// <summary>
        /// 移动到下一项
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            _index++;
            if (_index >= _collection.Count)
            {
                return false;
            }
            else
            {
                value = _collection[_index];
            }
            return true;
        }
        /// <summary>
        /// 重置索引项
        /// </summary>
        public void Reset()
        {
            _index = -1;
        }
    }
}
