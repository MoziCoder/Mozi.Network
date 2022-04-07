using System;
using System.Collections.Generic;

namespace Mozi.IoT.Cache
{

    public interface ICache
    {
        void Add(string name, string param, string data);
        void Add(string name, string param, string data, long expire, string owner, byte isprivate);
        void Clear();
        void ClearExpired();
        CacheInfo Find(string name, string param);
        void Remove(string name, string param);
        void ReStore();
        void Store();

    }

    public class MemoryCache : ICache
    {
        private List<CacheInfo> _caches = new List<CacheInfo>();

        private readonly object _sync = new object();
        /// <summary>
        /// 新增缓存项
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="data"></param>
        public void Add(string name, string param, string data)
        {
            Add(name, param, data, 0, "", 0);
        }
        /// <summary>
        /// 新增缓存
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="data"></param>
        /// <param name="expire">过期时间 单位ms</param>
        /// <param name="owner"></param>
        /// <param name="isprivate"></param>
        public void Add(string name, string param, string data, long expire, string owner, byte isprivate)
        {
            lock (_sync)
            {
                var cache = _caches.Find(x => x.Name == name && x.Param == param);
                var isNew = false;
                if (cache == null)
                {
                    isNew = true;
                    cache = new CacheInfo()
                    {
                        Name = name,
                        Param = param
                    };
                }
                cache.Data = data;
                cache.Expire = expire;
                cache.Owner = owner;
                cache.Size = data.Length;
                cache.IsPrivate = isprivate;
                if (isNew)
                {
                    _caches.Add(cache);
                }
            }
        }
        /// <summary>
        /// 查找缓存项
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public CacheInfo Find(string name, string param)
        {
            lock (_sync)
            {
                return _caches.Find(x => x.Name == name && x.Param == param);
            }
        }
        /// <summary>
        /// 移除缓存项
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        public void Remove(string name, string param)
        {
            lock (_sync)
            {
                _caches.RemoveAll(x => x.Name == name && x.Param == param);
            }
        }
        /// <summary>
        /// 清理过期缓存
        /// </summary>
        public void ClearExpired()
        {
            lock (_sync)
            {
                _caches.RemoveAll(x => x.Expire != 0 && (DateTime.UtcNow - x.CacheTime).TotalMilliseconds > x.Expire);
            }
        }
        public void Clear()
        {
            lock (_sync)
            {
                _caches.RemoveAll(x => x != null);
            }
        }

        public void Store()
        {

        }

        public void ReStore()
        {

        }
    }
    /// <summary>
    /// 缓存信息
    /// </summary>
    public class CacheInfo
    {
        public string Name { get; set; }
        public string Param { get; set; }
        public string Data { get; set; }
        public int Size { get; set; }
        public DateTime CacheTime { get; set; }
        public long Expire { get; set; }
        public string Owner { get; set; }
        /// <summary>
        /// 是否公域缓存
        /// </summary>
        public byte IsPrivate { get; set; }

        public CacheInfo()
        {
            CacheTime = DateTime.UtcNow;
        }
        public void SetExpired()
        {
            Expire = -1;
        }
    }
}
