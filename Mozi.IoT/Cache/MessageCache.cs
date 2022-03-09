using System;
using System.Collections;
using System.Collections.Generic;

namespace Mozi.IoT.Cache
{
    /// <summary>
    /// 消息缓存
    /// </summary>
    public class MessageCache
    {
        /// <summary>
        /// 消息序号
        /// </summary>
        public ushort MessageId { get; set; }
        /// <summary>
        /// 消息凭据
        /// </summary>
        public byte[] Token { get; set; }
        /// <summary>
        /// 主机地址
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 请求包
        /// </summary>
        public List<CoAPPackage> Request { get; set; }
        /// <summary>
        /// 响应包
        /// </summary>
        public List<CoAPPackage> Response { get; set; }
        /// <summary>
        /// 重试次数
        /// </summary>
        public ushort TryCount { get; set; }
        /// <summary>
        /// 传输次数
        /// </summary>
        public ushort TransmitCount { get; set; }
        /// <summary>
        /// 接收包次数
        /// </summary>
        public ushort ReceiveCount { get; set; }
        /// <summary>
        /// 初次请求时间
        /// </summary>
        public DateTime TransmitTime { get; set; }
        /// <summary>
        /// 接收响应时间
        /// </summary>
        public DateTime ReceiveTime { get; set; }
        /// <summary>
        /// 是否已响应
        /// </summary>
        public bool Answered { get; set; }
        /// <summary>
        /// 是否通讯已完成
        /// </summary>
        public bool Completed { get; set; }

        public MessageCache()
        {
            Request = new List<CoAPPackage>();
            Response = new List<CoAPPackage>();
        }
    }



    /// <summary>
    /// 信息缓存管理
    /// </summary>
    public class MessageCacheManager
    {
        private const string _seads = "0123456789ABCDEF";

        private List<MessageCache> _cm = new List<MessageCache>();
        
        private ushort _indStart = 0;

        public MessageCacheManager(CoAPPeer peer)
        {

        }
        /// <summary>
        /// 随机生成Token
        /// </summary>
        /// <returns></returns>
        public  byte[] GenerateToken(int tokenLen)
        {
            byte[] data = new byte[tokenLen];
            Random ran = new Random();
            ran.NextBytes(data);
            return data;
        }
        /// <summary>
        /// 生成未使用的MessageId
        /// </summary>
        /// <returns></returns>
        public ushort GenerateMessageId()
        {
            Random random = new Random();
            byte high = byte.Parse(new string(new char[] { _seads[random.Next(16)], _seads[random.Next(16)] }), System.Globalization.NumberStyles.HexNumber);
            byte low = byte.Parse(new string(new char[] { _seads[random.Next(16)], _seads[random.Next(16)] }), System.Globalization.NumberStyles.HexNumber);
            ushort msgId = BitConverter.ToUInt16(new byte[] { high, low }, 0);

            if (_cm.Exists(x => x.Host == "" && x.MessageId == msgId))
            {
               
            }
            return msgId;

        }
        //TODO 消息缓存的标识是MessageId?Token?
        public ushort Request(string host,CoAPPackage req)
        {
            MessageCache cache = _cm.Find(x => x.Host.Equals(host) && x.MessageId == req.MesssageId);
            if (cache == null)
            {
                cache = new MessageCache()
                {
                    Host = host,
                    TransmitTime = DateTime.Now,
                    MessageId = req.MesssageId,
                };
                cache.Request.Add(req);
                lock((_cm as ICollection).SyncRoot)
                {
                    _cm.Add(cache);
                }
            }
            else
            {

            }
            cache.TransmitCount++;
            
            return req.MesssageId;
        }
        //TODO 消息缓存的标识是MessageId?Token?
        internal ushort Response(string host,CoAPPackage resp)
        {
            MessageCache cache = _cm.Find(x => x.Host.Equals(host) && x.MessageId == resp.MesssageId);
            if (cache != null)
            {
                cache = new MessageCache()
                {
                    Host = host,
                    TransmitTime = DateTime.Now,
                    MessageId = resp.MesssageId,
                };
                cache.Response.Add(resp);
                cache.ReceiveCount++;
                cache.ReceiveTime = DateTime.Now;
                cache.Answered = true;
                //判断包是否已完成，完成则触发回调
                return resp.MesssageId;
            }
            else
            {
                //如果消息缓存不存在，说明消息请求已过生命周期
                return 0;
            }
        }
        //TODO 消息缓存的标识是MessageId?Token?
        internal ushort Remove(string host,ushort msgId)
        {
            lock ((_cm as ICollection).SyncRoot)
            {
                _cm.RemoveAll(x => x.Host.Equals(host) && x.MessageId == msgId);
            }
            return msgId;
        }
    }
}
