namespace Mozi.IoT
{
    //MAX_TRANSMIT_SPAN = ACK_TIMEOUT * ((2 ** MAX_RETRANSMIT) - 1) * ACK_RANDOM_FACTOR
    //MAX_TRANSMIT_WAIT = ACK_TIMEOUT * ((2 ** (MAX_RETRANSMIT + 1)) - 1) *ACK_RANDOM_FACTOR
    /// <summary>
    /// CoAP协议
    /// </summary>
    public class CoAPProtocol
    {
        public const ushort Port = 5683;

        public const ushort SecurePort = 5684;
        
        public const ushort TcpPort = 5685;

        public const byte HeaderEnd = 0xFF;
        /// <summary>
        /// 确认超时时间 seconds 取值推荐>1
        /// </summary>
        public double ACK_TIMEOUT = 2;
        /// <summary>
        /// 确认超时因子 取值推荐>1
        /// </summary>
        public double ACK_RANDOM_FACTOR = 1.5;
        /// <summary>
        /// 最大重传次数
        /// </summary>
        public int MAX_RETRANSMIT = 4;

        public int NSTART = 1;
        /// <summary>
        /// seconds
        /// </summary>
        public int DEFAULT_LEISURE = 5;
        /// <summary>
        /// 查看频率byte/second
        /// </summary>
        public int PROBING_RATE = 1;
        /// <summary>
        /// 最大缓存时间 seconds
        /// </summary>
        public int MAX_AGE = 60;

        public int MAX_TRANSMIT_SPAN = 45;
        
        public int MAX_TRANSMIT_WAIT = 93;
        
        public int MAX_LATENCY = 100;
        
        public int PROCESSING_DELAY = 2;
        
        public int MAX_RTT = 202;
        
        public int EXCHANGE_LIFETIME = 247;
        
        public int NON_LIFETIME = 145;
    
    }
}
