using System.Collections.Generic;

namespace Mozi.IoT.Cache
{
    /// <summary>
    /// 全局数据
    /// 此处定义的数据会被全局使用 仅只读 功能类似于宏
    /// </summary>
    public class Global
    {

        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        /// <summary>
        /// 设置键值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Global Set(string key, object value)
        {
            if (_data.ContainsKey(key))
            {
                _data[key] = value;
            }
            else
            {
                _data.Add(key, value);
            }
            return this;
        }
        /// <summary>
        /// 获取键值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            return _data[key];
        }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value);
            }
        }
    }
}
