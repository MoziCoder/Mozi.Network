using System;
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
        /// 主机地址
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 请求包
        /// </summary>
        public CoAPPackage[] Request { get; set; }
        /// <summary>
        /// 响应包
        /// </summary>
        public CoAPPackage[] Response { get; set; }
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
    }
    /// <summary>
    /// 信息缓存管理
    /// </summary>
    public class MessageCacheManager
    {
        private List<MessageCache> _cache = new List<MessageCache>();
        
        private ushort _indStart = 0;

        public MessageCacheManager(CoAPPeer peer)
        {

        }
        
        /// <summary>
        /// 生成未使用的MessageId
        /// </summary>
        /// <returns></returns>
        public ushort GenerateMessageId()
        {
            return 12345;
        }
    }
}
