namespace Mozi.IoT
{

    /// <summary>
    /// CoAP基于UDP,可工作的C/S模式，多播，单播，任播（IPV6）
    /// 
    /// C/S模式
    /// URI格式:
    /// coap://{host}:{port}/{path}[?{query}]
    /// 默认端口号为5683
    /// coaps://{host}:{port}/{path}[?{query}]
    /// 默认端口号为5684
    /// 
    /// 多播模式:
    /// IPV4:224.0.1.187
    /// IPV6:FF0X::FD
    /// 
    /// 消息重传
    /// When SendTimeOut between {ACK_TIMEOUT} and (ACK_TIMEOUT * ACK_RANDOM_FACTOR)  then 
    ///     TryCount=0
    /// When TryCount <{MAX_RETRANSMIT} then 
    ///     TryCount++ 
    ///     SendTime*=2
    /// When TryCount >{MAX_RETRANSMIT} then 
    ///     Send(Rest)
    /// </summary>
    public class CoAPServer
    {

    }

    public class CoAPClient
    {

    }

    //Main Reference:RFC7252
    //Patial Reference:RFC7959, RFC8613, RFC8974 
    //内容采用UTF-8编码
    //头部截断使用0xFF填充
    public class CoAPPackage
    {
        /// <summary>
        /// 版本 2bits 
        /// </summary>
        public byte Version { get; set; }
        /// <summary>
        /// 消息类型 2bits  
        /// 0-Confirmable
        /// 1-Non-confirmable 
        /// 2-Acknowledgement
        //  3-Reset
        /// </summary>
        public byte MessageType { get; set; }
        /// <summary>
        /// Token长度 4bits
        /// 0-8bytes取值范围
        /// 9-15为保留使用，收到此消息时直接消息报错
        /// </summary>
        public byte TokenLength { get; set; }
        /// <summary>
        /// 代码 8bits=3bits+5bits
        /// 高3位为分类 
        /// 低5位为明细
        /// 
        ///  0.00      Indicates an Empty message (see Section 4.1).
        ///  0.01-0.31 Indicates a request.Values in this range are assigned by the "CoAP Method Codes" sub-registry(see Section 12.1.1).
        ///     0.01  GET    | [RFC7252] 
        ///     0.02  POST   | [RFC7252] 
        ///     0.03  PUT    | [RFC7252] 
        ///     0.04  DELETE | [RFC7252]
        ///  1.00-1.31 Reserved
        ///  2.00-5.31 Indicates a response.Values in this range are assigned bythe "CoAP Response Codes" sub-registry(see Section 12.1.2).
        ///     2.01 | Created                      | [RFC7252] |
        ///     2.02 | Deleted                      | [RFC7252] |
        ///     2.03 | Valid                        | [RFC7252] |
        ///     2.04 | Changed                      | [RFC7252] |
        ///     2.05 | Content                      | [RFC7252] |
        ///     4.00 | Bad Request                  | [RFC7252] |
        ///     4.01 | Unauthorized                 | [RFC7252] |
        ///     4.02 | Bad Option                   | [RFC7252] |
        ///     4.03 | Forbidden                    | [RFC7252] |
        ///     4.04 | Not Found                    | [RFC7252] |
        ///     4.05 | Method Not Allowed           | [RFC7252] |
        ///     4.06 | Not Acceptable               | [RFC7252] |
        ///     4.12 | Precondition Failed          | [RFC7252] |
        ///     4.13 | Request Entity Too Large     | [RFC7252] |
        ///     4.15 | Unsupported Content-Format   | [RFC7252] |
        ///     5.00 | Internal Server Error        | [RFC7252] |
        ///     5.01 | Not Implemented              | [RFC7252] |
        ///     5.02 | Bad Gateway                  | [RFC7252] |
        ///     5.03 | Service Unavailable          | [RFC7252] |
        ///     5.04 | Gateway Timeout              | [RFC7252] |
        ///     5.05 | Proxying Not Supported       | [RFC7252] |
        ///  6.00-7.31 Reserved
        /// </summary>
        public byte Code { get; set; }
        /// <summary>
        /// 用于消息确认防重，消息确认-重置
        /// </summary>
        public ushort MesssageId { get; set; }
        /// <summary>
        /// 凭据 0-8bytes
        /// 典型应用场景需>=4bytes
        /// </summary>
        public byte[] Token { get; set; }

    }
    ///CoAP Option Numbers Registry
    //|       0-255 | IETF Review or IESG Approval         
    //|    256-2047 | Specification Required               
    //|  2048-64999 | Expert Review                        
    //| 65000-65535 | Experimental use(no operational use) 

    //      0 | (Reserved)       | [RFC7252] |
    //      1 | If-Match         | [RFC7252] |
    //      3 | Uri-Host         | [RFC7252] |
    //      4 | ETag             | [RFC7252] |
    //      5 | If-None-Match    | [RFC7252] |
    //      7 | Uri-Port         | [RFC7252] |
    //      8 | Location-Path    | [RFC7252] |
    //     11 | Uri-Path         | [RFC7252] |
    //     12 | Content-Format   | [RFC7252] |
    //     14 | Max-Age          | [RFC7252] |
    //     15 | Uri-Query        | [RFC7252] |
    //     17 | Accept           | [RFC7252] |
    //     20 | Location-Query   | [RFC7252] |
    //     35 | Proxy-Uri        | [RFC7252] |
    //     39 | Proxy-Scheme     | [RFC7252] |
    //     60 | Size1            | [RFC7252] |
    //    128 | (Reserved)       | [RFC7252] |
    //    132 | (Reserved)       | [RFC7252] |
    //    136 | (Reserved)       | [RFC7252] |
    //    140 | (Reserved)       | [RFC7252] |

    ///CoAP Content-Formats Registry
    //       0-255 | Expert Review                        
    //    256-9999 | IETF Review or IESG Approval         
    // 10000-64999 | First Come First Served              
    // 65000-65535 | Experimental use(no operational use) 

    // text/plain;              | -        |  0 | [RFC2046] [RFC3676]    |
    // charset=utf-8            |          |    | [RFC5147]              |
    // application/link-format  | -        | 40 | [RFC6690]              |
    // application/xml          | -        | 41 | [RFC3023]              |
    // application/octet-stream | -        | 42 | [RFC2045] [RFC2046]    |
    // application/exi          | -        | 47 | [REC-exi-20140211]     |
    // application/json         | -        | 50 | [RFC7159]              |

    public class CoAPOption
    {
        /// <summary>
        /// 偏移值 4bits 
        /// 取值范围：0-15
        /// 保留特殊：
        ///     13:  An 8-bit unsigned integer follows the initial byte and indicates the Option Delta minus 13.
        ///     14:  A 16-bit unsigned integer in network byte order follows the initial byte and indicates the Option Delta minus 269.
        ///     15:  Reserved for the Payload Marker.If the field is set to thisvalue but the entire byte is not the payload marker, this MUST be processed as a message format error.
        /// </summary>
        public byte Delta { get; set; }
        /// <summary>
        /// 值长度 4bits
        /// 取值范围：0-15
        ///     13:  An 8-bit unsigned integer precedes the Option Value and indicates the Option Length minus 13.
        ///     14:  A 16-bit unsigned integer in network byte order precedes the Option Value and indicates the Option Length minus 269.
        ///     15:  Reserved for future use.If the field is set to this value, it MUST be processed as a message format error.
        /// </summary>
        public byte Length { get; set; }
        /// <summary>
        /// 选项值>=0 bytes
        /// 空 字节 数字 ASCII/UTF-8字符串
        /// </summary>
        public byte[] Value { get; set; }
    }

    //MAX_TRANSMIT_SPAN = ACK_TIMEOUT * ((2 ** MAX_RETRANSMIT) - 1) * ACK_RANDOM_FACTOR
    //MAX_TRANSMIT_WAIT = ACK_TIMEOUT * ((2 ** (MAX_RETRANSMIT + 1)) - 1) *ACK_RANDOM_FACTOR
    /// <summary>
    /// CoAP协议
    /// </summary>
    public class CoAPProtocol
    {
        public int Port = 5683;
        public int SecurePort = 5684;
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
        public int DEFAULT_LEISURE = 5 ;
        /// <summary>
        /// 查看频率byte/second
        /// </summary>
        public int PROBING_RATE = 1;

        public int MAX_TRANSMIT_SPAN =  45 ;
        public int MAX_TRANSMIT_WAIT =  93 ;
        public int MAX_LATENCY       = 100 ;
        public int PROCESSING_DELAY  =   2 ;
        public int MAX_RTT           = 202 ;
        public int EXCHANGE_LIFETIME = 247;
        public int NON_LIFETIME = 145  ;
    }

    public class CoAPRequest : HttpEmbedded.HttpRequest
    {

    }

    public class CoAPResponse : HttpEmbedded.HttpResponse
    {

    }
}
