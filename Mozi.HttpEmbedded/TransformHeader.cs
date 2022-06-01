using System.Collections.Generic;
using System.Text;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 通讯头报文
    /// </summary>
    public class TransformHeader
    {

        public static byte[] Carriage = { ASCIICode.CR, ASCIICode.LF };

        /// <summary>
        /// 通讯头集合
        /// </summary>
        private readonly Dictionary<string, string> HeaderData = new Dictionary<string, string>(new StringCompareIgnoreCase())
        {

        };
        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            return HeaderData.ContainsKey(key);
        }
        /// <summary>
        /// 获取头属性值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            return HeaderData.ContainsKey(key) ? HeaderData[key] : null;
        }
        /// <summary>
        /// 获取头属性值，参见<see cref="GetValue(string)"/>
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public string GetValue(HeaderProperty header)
        {
            return GetValue(header.PropertyName);
        }
        /// <summary>
        /// 增加头部信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public TransformHeader Add(string key, string value)
        {
            if (HeaderData.ContainsKey(key))
            {
                HeaderData[key] = value;
            }
            else
            {
                HeaderData.Add(key, value);
            }
            return this;
        }
        /// <summary>
        /// 增加头部信息
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public TransformHeader Add(HeaderProperty header, string value)
        {
            Add(header.PropertyName, value);
            return this;
        }
        /// <summary>
        /// 获取缓存数据 带分割符号
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
           return GetBuffer(false);
        }
        public byte[] GetBuffer(bool keyNameUpperCase)
        {
            List<byte> buffer = new List<byte>();
            foreach (var item in HeaderData)
            {
                buffer.AddRange(Encoding.UTF8.GetBytes(string.Format("{0}: {1}", keyNameUpperCase?item.Key.ToUpper():item.Key, item.Value)));
                buffer.AddRange(Carriage);
            }
            return buffer.ToArray();
        }

        public Dictionary<string, string> GetAll()
        {
            return HeaderData;
        }
        public string this[string key]
        {
            get 
            {
                return HeaderData[key];
            }
            set { HeaderData[key] = value; }
        }

        ~TransformHeader()
        {

        }
    }
}