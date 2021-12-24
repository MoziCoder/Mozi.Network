using System;

namespace Mozi.IoT.Cache
{
    public class MessageCache
    {
        public ushort MessageId { get; set; }
        public CoAPPackage[] Request { get; set; }
        public CoAPPackage[] Response { get; set; }
        public ushort TryCount { get; set; }
        public DateTime TransmitTime { get; set; }
        public DateTime ReceiveTime { get; set; }

    }

    public class MessageCacheManager
    {
        
        public MessageCacheManager(CoAPPeer peer)
        {

        }
        
        /// <summary>
        /// 生成MessageId
        /// </summary>
        /// <returns></returns>
        public ushort GenerateMessageId()
        {
            return 12345;
        }
    }
}
