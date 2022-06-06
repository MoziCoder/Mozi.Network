using System;
using System.Collections.Generic;

namespace Mozi.IoT.Cache
{
    /// <summary>
    /// 缓存对象接口
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// 增加缓存数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="data"></param>
        void Add(string name, string param, string data);
        /// <summary>
        /// 增加缓存数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="data"></param>
        /// <param name="expire"></param>
        /// <param name="owner"></param>
        /// <param name="isprivate"></param>
        void Add(string name, string param, string data, long expire, string owner, byte isprivate);
        /// <summary>
        /// 清理所有缓存数据
        /// </summary>
        void Clear();
        /// <summary>
        /// 清理过期的缓存
        /// </summary>
        void ClearExpired();
        /// <summary>
        /// 条件查找缓存对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        CacheInfo Find(string name, string param);
        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        void Remove(string name, string param);
        /// <summary>
        /// 恢复
        /// </summary>
        void ReStore();
        /// <summary>
        /// 保存
        /// </summary>
        void Save();

    }
    /// <summary>
    /// 内存缓存
    /// </summary>
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
        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void Clear()
        {
            lock (_sync)
            {
                _caches.RemoveAll(x => x != null);
            }
        }
        /// <summary>
        /// 保存缓存
        /// </summary>
        public void Save()
        {

        }
        /// <summary>
        /// 恢复数据
        /// </summary>
        public void ReStore()
        {

        }
    }
    /// <summary>
    /// 缓存信息
    /// </summary>
    public class CacheInfo
    {
        /// <summary>
        /// 缓存名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 缓存参数
        /// </summary>
        public string Param { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 大小
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// 入堆时间
        /// </summary>
        public DateTime CacheTime { get; set; }
        /// <summary>
        /// 过期时间，ms
        /// </summary>
        public long Expire { get; set; }
        /// <summary>
        /// 拥有者
        /// </summary>
        public string Owner { get; set; }
        /// <summary>
        /// 是否公域缓存
        /// </summary>
        public byte IsPrivate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public CacheInfo()
        {
            CacheTime = DateTime.UtcNow;
        }
        /// <summary>
        /// 设为已过期
        /// </summary>
        public void SetExpired()
        {
            Expire = -1;
        }
    }
}
