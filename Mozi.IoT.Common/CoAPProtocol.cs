namespace Mozi.IoT
{
    /// <summary>
    /// CoAP协议常量
    /// </summary>
    public class CoAPProtocol
    {
        /// <summary>
        /// 默认服务端口UDP
        /// </summary>
        public const int Port = 5683;
        /// <summary>
        /// 默认DTLS服务端口
        /// </summary>
        public const int SecurePort = 5684;
        /// <summary>
        /// 默认服务端口TCP
        /// </summary>
        public const int TcpPort = 5685;
        /// <summary>
        /// 头部和内容分割数据
        /// </summary>
        public const byte HeaderEnd = 0xFF;
    }
    /// <summary>
    /// CoAP传输时间参数
    /// </summary>
    public class CoAPTransmissionConfig
    {
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
        /// 空闲时间 seconds
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
        //MAX_TRANSMIT_SPAN = ACK_TIMEOUT * ((2 ** MAX_RETRANSMIT) - 1) * ACK_RANDOM_FACTOR
        /// <summary>
        /// 最大重传送间隔
        /// </summary>
        public int MAX_TRANSMIT_SPAN = 45;
        //MAX_TRANSMIT_WAIT = ACK_TIMEOUT * ((2 ** (MAX_RETRANSMIT + 1)) - 1) *ACK_RANDOM_FACTOR
        /// <summary>
        /// 最大消息确认等待时间，请求方等待ACK|RESET消息的超时时间 seconds
        /// </summary>
        public int MAX_TRANSMIT_WAIT = 93;
        /// <summary>
        /// 最大包接收延迟时间
        /// </summary>
        public int MAX_LATENCY = 100;
        /// <summary>
        /// 包确认时间 默认等于ACK_TIMEOUT seconds
        /// </summary>
        public int PROCESSING_DELAY = 2;
        //2 * MAX_LATENCY) + PROCESSING_DELAY
        /// <summary>
        /// 最大传输回环时间
        /// </summary>
        public int MAX_RTT = 202;

        //MAX_TRANSMIT_SPAN + (2 * MAX_LATENCY) + PROCESSING_DELAY
        /// <summary>
        /// 数据交换周期
        /// </summary>
        public int EXCHANGE_LIFETIME = 247;
        //MAX_TRANSMIT_SPAN + MAX_LATENCY
        /// <summary>
        /// Non-confirmable包MessageId重用周期
        /// </summary>
        public int NON_LIFETIME = 145;

    }
}
