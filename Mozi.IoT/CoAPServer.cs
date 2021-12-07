using System;
using System.Collections.Generic;

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
    //Patial Reference:
    //RFC7959 分块传输
    //RFC8613 对象安全
    //RFC8974 扩展凭据和无状态客户端 

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
        /// </summary>
        public CoAPMessageType MessageType { get; set; }
        /// <summary>
        /// Token长度 4bits
        /// 0-8bytes取值范围
        /// 9-15为保留使用，收到此消息时直接消息报错
        /// </summary>
        public byte TokenLength { get; set; }
        /// <summary>
        /// 8bits
        /// </summary>
        public CoAPCode Code { get; set; }
        /// <summary>
        /// 用于消息确认防重，消息确认-重置
        /// </summary>
        public ushort MesssageId { get; set; }
        /// <summary>
        /// 凭据 0-8bytes
        /// 典型应用场景需>=4bytes
        /// </summary>
        public byte[] Token { get; set; }
        /// <summary>
        /// 选项 类似HTTP头属性
        /// </summary>
        public List<CoAPOption> Options = new List<CoAPOption>();
        /// <summary>
        /// 包体
        /// </summary>
        public byte[] Payload { get; set; }

        public byte[] Pack()
        {
            List<byte> data = new List<byte>();
            byte head = 0b00000000;
            head = (byte)(head | Version << 6);
            head = (byte)(head | (MessageType.Value << 4));
            head = (byte)(head | TokenLength);
            data.Add(head);
            data.Add((byte)((byte)Code.Category << 5 | (byte)(Code.Detail << 3) >> 3));
            data.AddRange(BitConverter.GetBytes(MesssageId).Revert());
            data.AddRange(Token);
            Options.Sort((x, y) => x.Option.OptionNumber.CompareTo(y.Option.OptionNumber));
            uint delta = 0;
            foreach (var op in Options)
            {
                op.DeltaValue = op.Option.OptionNumber - delta;
                data.AddRange(op.Pack);
                delta += op.DeltaValue;
            }
            data.Add(CoAPProtocol.HeaderEnd);
            data.AddRange(Payload);
            return data.ToArray();

        }

        public static CoAPPackage Parse(byte[] data, bool isRequest)
        {
            CoAPPackage pack = new CoAPPackage();
            byte head = data[0];
            pack.Version = (byte)(head >> 6);
            pack.MessageType = AbsClassEnum.Get<CoAPMessageType>(((byte)(head << 2) >> 4).ToString());
            pack.TokenLength = (byte)((byte)(head << 4) >> 4);
            //pack.Code = !isRequest ? AbsClassEnum.Get<CoAPResponseCode>(data[2].ToString()) : AbsClassEnum.Get<CoAPRequestCode>(data[2].ToString());
            byte[] arrMsgId = new byte[2], arrToken = new byte[pack.TokenLength];
            Array.Copy(data, 3, arrMsgId, 0, 2);
            Array.Copy(data, 3 + 2, arrToken, 0, arrToken.Length);
            pack.MesssageId = BitConverter.ToUInt16(arrMsgId.Revert(), 0);
            //3+2+arrToken.Length+1开始是Option部分
            int bodySplitterPos = 3 + 2 + arrMsgId.Length + 1;
            while (data[bodySplitterPos] != CoAPProtocol.HeaderEnd)
            {
                CoAPOption option = new CoAPOption();
                option.OptionHead = data[bodySplitterPos];
                int lenDelta = 0,lenLength=0;
                if (option.Delta <= 12)
                {
                   
                }else if (option.Delta == 13){

                    lenDelta = 1;

                }else if(option.Delta==14){

                    lenDelta += 2;

                }
                if (lenDelta > 0)
                {
                    byte[] arrDeltaExt = new byte[2];
                    Array.Copy(data, bodySplitterPos + 1, arrDeltaExt, 0,lenDelta);
                    option.DeltaExtend = BitConverter.ToUInt16(arrDeltaExt.Revert(),0);
                }

                if (option.Length <= 12)
                {

                }
                else if (option.Length == 13)
                {

                    lenLength = 1;

                }
                else if (option.Length == 14)
                {
                    lenLength = 2;
                }

                if (lenLength > 0)
                {
                    byte[] arrDeltaExt = new byte[2];
                    Array.Copy(data, bodySplitterPos + 1+lenDelta+1, arrDeltaExt, 0, lenDelta);
                    option.DeltaExtend = BitConverter.ToUInt16(arrDeltaExt.Revert(), 0);
                }
                byte[] arrOptionValue = new byte[option.LengthValue];
                Array.Copy(data, bodySplitterPos + 1 + lenDelta + 1 + lenLength + 1, arrOptionValue, 0, arrOptionValue.Length);
                bodySplitterPos += 1 + lenDelta + lenLength + arrOptionValue.Length + 1;
            }

            pack.Payload = new byte[data.Length - bodySplitterPos - 1];
            //有效荷载
            Array.Copy(data, bodySplitterPos + 1, pack.Payload,0, pack.Payload.Length);
            return pack;
        }
    }



    ///CoAP Content-Formats Registry
    ///       0-255 | Expert Review                        
    ///    256-9999 | IETF Review or IESG Approval         
    /// 10000-64999 | First Come First Served              
    /// 65000-65535 | Experimental use(no operational use) 
    ///
    /// text/plain;              | -        |  0 | [RFC2046] [RFC3676]    |
    /// charset=utf-8            |          |    | [RFC5147]              |
    /// application/link-format  | -        | 40 | [RFC6690]              |
    /// application/xml          | -        | 41 | [RFC3023]              |
    /// application/octet-stream | -        | 42 | [RFC2045] [RFC2046]    |
    /// application/exi          | -        | 47 | [REC-exi-20140211]     |
    /// application/json         | -        | 50 | [RFC7159]              |
    /// applicaiton/cbor         | -        | 60 | [RFC7159]              |

    ///CoAP Option Numbers Registry
    ///|       0-255 | IETF Review or IESG Approval         
    ///|    256-2047 | Specification Required               
    ///|  2048-64999 | Expert Review                        
    ///| 65000-65535 | Experimental use(no operational use) 

    ///      0 | (Reserved)       | [RFC7252] |
    ///      1 | If-Match         | [RFC7252] |
    ///      3 | Uri-Host         | [RFC7252] |
    ///      4 | ETag             | [RFC7252] |
    ///      5 | If-None-Match    | [RFC7252] |
    ///      7 | Uri-Port         | [RFC7252] |
    ///      8 | Location-Path    | [RFC7252] |
    ///     11 | Uri-Path         | [RFC7252] |
    ///     12 | Content-Format   | [RFC7252] |
    ///     14 | Max-Age          | [RFC7252] |
    ///     15 | Uri-Query        | [RFC7252] |
    ///     17 | Accept           | [RFC7252] |
    ///     20 | Location-Query   | [RFC7252] |
    ///     28 | Size2            | [RFC7252] |   
    ///     35 | Proxy-Uri        | [RFC7252] |
    ///     39 | Proxy-Scheme     | [RFC7252] |
    ///     60 | Size1            | [RFC7252] |
    ///    128 | (Reserved)       | [RFC7252] |
    ///    132 | (Reserved)       | [RFC7252] |
    ///    136 | (Reserved)       | [RFC7252] |
    ///    140 | (Reserved)       | [RFC7252] |
    ///    
    ///Option Delta代表Option的类型，该值代表了上表中Option类型的代码值与上一个Option代码值之间的差值
    ///（如果该Option为第一个Option，则直接表达该Option的Option Delta）

    ///由于Option Delta只有4位，最大只能表达15，为了解决这个问题，coap协议有着如下规定：

    ///当Option Delta号码<=12时：Option Delta位为实际的Option Delta值
    ///当Option Delta号码<269时：Option Delta位填入13；并且在后面的Option Delta(extended) 位会占用1字节，并且填入的数为实际Option Delta值减去13
    ///当Option Delta号码<65804时：Option Delta位填入14；并且在后面的Option Delta(extended)位会占用2字节，并且填入的数为实际Option Delta值减去269

    ///特别注意，填入的Option Delta值不可能为15（0x0f）当遇到15时，该包无效
    public class CoAPOptionDefine : AbsClassEnum
    {
        private string _name = "";

        public static CoAPOptionDefine IfMatch = new CoAPOptionDefine("If-Match", 1);
        public static CoAPOptionDefine UriHost = new CoAPOptionDefine("Uri-Host", 3);
        public static CoAPOptionDefine ETag = new CoAPOptionDefine("ETag", 4);
        public static CoAPOptionDefine IfNoneMatch = new CoAPOptionDefine("If-None-Match", 5);
        public static CoAPOptionDefine UriPort = new CoAPOptionDefine("Uri-Port", 7);
        public static CoAPOptionDefine LocationPath = new CoAPOptionDefine("Location-Path", 8);
        public static CoAPOptionDefine UriPath = new CoAPOptionDefine("Uri-Path", 11);
        public static CoAPOptionDefine ContentFormat = new CoAPOptionDefine("Content-Format", 12);
        public static CoAPOptionDefine MaxAge = new CoAPOptionDefine("Max-Age", 14);
        public static CoAPOptionDefine UriQuery = new CoAPOptionDefine("Uri-Query", 15);
        public static CoAPOptionDefine Accept = new CoAPOptionDefine("Accept", 17);
        public static CoAPOptionDefine LocationQuery = new CoAPOptionDefine("Location-Query", 20);
        public static CoAPOptionDefine ProxyUri = new CoAPOptionDefine("Proxy-Uri", 35);
        public static CoAPOptionDefine ProxyScheme = new CoAPOptionDefine("Proxy-Scheme", 39);
        public static CoAPOptionDefine Size1 = new CoAPOptionDefine("Size1", 60);

        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// 选项序号
        /// </summary>
        public ushort OptionNumber { get; set; }

        protected override string Tag => OptionNumber.ToString();

        public CoAPOptionDefine(string name, ushort optionNumber)
        {
            _name = name;
            OptionNumber = optionNumber;
        }
    }

    ///<summary>
    ///CoAP头属性
    /// </summary>
    public class CoAPOption
    {
        private uint _deltaValue = 0, _lenValue = 0;

        /// <summary>
        /// 选项序号
        /// </summary>
        public CoAPOptionDefine Option { get; set; }

        public byte OptionHead 
        { 
            get 
            {
                return (byte)(((byte)(Delta << 4)) | Length);
            }
            set 
            {
                Delta = (byte)(Delta >> 4);
                Length = (byte)((byte)(Delta << 4) >> 4);
            }
         }

        public uint DeltaValue
        {
            get
            {
                return (uint)(Delta + DeltaExtend);
            }
            set
            {
                _deltaValue = value;
                if (_deltaValue < 12)
                {
                    Delta = (byte)_deltaValue;
                }
                else if (_deltaValue < 269)
                {
                    Delta = 13;
                    DeltaExtend = (ushort)(_deltaValue - 13);
                }
                else if (_deltaValue < 65804)
                {
                    Delta = 14;
                    DeltaExtend = (ushort)(_deltaValue - 269);
                }
            }
        }
        public uint LengthValue
        {
            get
            {
                return (uint)(Length + LengthExtend);
            }
            set
            {
                _lenValue = value;
                if (_lenValue < 12)
                {
                    Length = (byte)_lenValue;
                }
                else if (_lenValue < 269)
                {
                    Length = 13;
                    LengthExtend = (ushort)(_lenValue - 13);
                }
                else if (_lenValue < 65804)
                {
                    Length = 14;
                    LengthExtend = (ushort)(_lenValue - 269);
                }
            }
        }
        /// <summary>
        /// 偏移值 4bits 
        /// 取值范围：0-15
        /// 保留特殊：
        ///     13:  An 8-bit unsigned integer follows the initial byte and indicates the Option Delta minus 13.
        ///     14:  A 16-bit unsigned integer in network byte order follows the initial byte and indicates the Option Delta minus 269.
        ///     15:  Reserved for the Payload Marker.If the field is set to thisvalue but the entire byte is not the payload marker, this MUST be processed as a message format error.
        /// </summary>
        public byte Delta { get; set; }
        public ushort DeltaExtend { get; set; }

        /// <summary>
        /// 值长度 4bits
        /// 取值范围：0-15
        ///     13:  An 8-bit unsigned integer precedes the Option Value and indicates the Option Length minus 13.
        ///     14:  A 16-bit unsigned integer in network byte order precedes the Option Value and indicates the Option Length minus 269.
        ///     15:  Reserved for future use.If the field is set to this value, it MUST be processed as a message format error.
        /// </summary>
        public byte Length { get; set; }
        public ushort LengthExtend { get; set; }
        /// <summary>
        /// 选项值>=0 bytes
        /// 空 字节 数字 ASCII/UTF-8字符串
        /// </summary>
        public byte[] Value { get; set; }

        public byte[] Pack
        {
            get
            {
                byte head = (byte)((byte)(Delta << 4) | Length);

                List<byte> data = new List<byte>();
                data.Add(head);
                if (Delta == 14)
                {
                    data.AddRange(BitConverter.GetBytes(DeltaExtend));
                }
                else if (Delta == 13)
                {
                    data.AddRange(BitConverter.GetBytes((byte)(DeltaExtend)));
                }
                if (LengthExtend == 14)
                {
                    data.AddRange(BitConverter.GetBytes(LengthExtend));
                }
                else if (LengthExtend == 13)
                {
                    data.AddRange(BitConverter.GetBytes((byte)(LengthExtend)));
                }
                return data.ToArray();
            }
        }
    }

    //MAX_TRANSMIT_SPAN = ACK_TIMEOUT * ((2 ** MAX_RETRANSMIT) - 1) * ACK_RANDOM_FACTOR
    //MAX_TRANSMIT_WAIT = ACK_TIMEOUT * ((2 ** (MAX_RETRANSMIT + 1)) - 1) *ACK_RANDOM_FACTOR
    /// <summary>
    /// CoAP协议
    /// </summary>
    public class CoAPProtocol
    {
        public const ushort Port = 5683;
        public const ushort SecurePort = 5684;

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

        public int MAX_TRANSMIT_SPAN = 45;
        public int MAX_TRANSMIT_WAIT = 93;
        public int MAX_LATENCY = 100;
        public int PROCESSING_DELAY = 2;
        public int MAX_RTT = 202;
        public int EXCHANGE_LIFETIME = 247;
        public int NON_LIFETIME = 145;
    }
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
    ///     
    ///     2.31 | Continue                     | [RFC7959] |
    ///     
    ///     4.00 | Bad Request                  | [RFC7252] |
    ///     4.01 | Unauthorized                 | [RFC7252] |
    ///     4.02 | Bad Option                   | [RFC7252] |
    ///     4.03 | Forbidden                    | [RFC7252] |
    ///     4.04 | Not Found                    | [RFC7252] |
    ///     4.05 | Method Not Allowed           | [RFC7252] |
    ///     4.06 | Not Acceptable               | [RFC7252] |
    ///     
    ///     4.08 | Request Entity Incomplete    | [RFC7959] |
    ///     
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
    /// <summary>
    /// CoAP操作代码
    /// </summary>
    public class CoAPCode : AbsClassEnum
    {
        private string _name = "", _description;

        private byte _category = 0, _detail = 0;

        public int Category
        {
            get
            {
                return _category;
            }
        }

        public byte Detail
        {
            get
            {
                return _detail;
            }
        }
        public string Name
        {
            get { return _name; }
        }
        protected override string Tag
        {
            get { return _category.ToString() + _detail.ToString().PadLeft(2, '0'); }
        }

        public byte Pack
        {
            get
            {
                return (byte)(_category << 5 | _detail);
            }
            set
            {
                _category = (byte)(value >> 5);
                _detail = (byte)((value << 3) >> 3);
            }
        }

        public CoAPCode(byte data)
        {
            Pack = data;
        }
        public CoAPCode(string name, string description, byte category, byte detail)
        {
            _name = name;
            _description = description;
            _category = category;
            _detail = detail;
        }
    }

    public class CoAPRequestCode : CoAPCode
    {

        public static CoAPRequestCode Get = new CoAPRequestCode("GET", "", 0, 1);
        public static CoAPRequestCode Post = new CoAPRequestCode("POST", "", 0, 2);
        public static CoAPRequestCode Put = new CoAPRequestCode("PUT", "", 0, 3);
        public static CoAPRequestCode Delete = new CoAPRequestCode("DELETE", "", 0, 4);

        public CoAPRequestCode(string name, string description, byte category, byte detail) : base(name, description, category, detail)
        {

        }
    }

    public class CoAPResponseCode : CoAPCode
    {

        public static CoAPResponseCode Created = new CoAPResponseCode("Created", "Created", 2, 1);
        public static CoAPResponseCode Deleted = new CoAPResponseCode("Deleted", "Deleted", 2, 2);
        public static CoAPResponseCode Valid = new CoAPResponseCode("Valid", "Valid", 2, 3);
        public static CoAPResponseCode Changed = new CoAPResponseCode("Changed", "Changed", 2, 4);
        public static CoAPResponseCode Content = new CoAPResponseCode("Content", "Content", 2, 5);

        public static CoAPResponseCode Continue = new CoAPResponseCode("Content", "Content", 2, 31);

        public static CoAPResponseCode BadRequest = new CoAPResponseCode("BadRequest", "Bad Request", 4, 0);
        public static CoAPResponseCode Unauthorized = new CoAPResponseCode("Unauthorized", "Unauthorized", 4, 1);
        public static CoAPResponseCode BadOption = new CoAPResponseCode("BadOption", "Bad Option", 4, 2);
        public static CoAPResponseCode Forbidden = new CoAPResponseCode("Forbidden", "Forbidden", 4, 3);
        public static CoAPResponseCode NotFound = new CoAPResponseCode("NotFound", "Not Found", 4, 4);
        public static CoAPResponseCode MethodNotAllowed = new CoAPResponseCode("MethodNotAllowed", "Method Not Allowed", 4, 5);
        public static CoAPResponseCode NotAcceptable = new CoAPResponseCode("NotAcceptable", "Not Acceptable", 4, 6);

        public static CoAPResponseCode RequestEntityIncomplete = new CoAPResponseCode(" RequestEntityIncomplete", " Request Entity Incomplete", 4, 8);

        public static CoAPResponseCode PreconditionFailed = new CoAPResponseCode("PreconditionFailed", "Precondition Failed", 4, 12);
        public static CoAPResponseCode RequestEntityTooLarge = new CoAPResponseCode("RequestEntityTooLarge", "Request Entity Too Large", 4, 13);
        public static CoAPResponseCode UnsupportedContentFormat = new CoAPResponseCode("UnsupportedContentFormat", "Unsupported Content-Format", 4, 15);
        public static CoAPResponseCode InternalServerError = new CoAPResponseCode("InternalServerError", "Internal Server Error", 5, 0);
        public static CoAPResponseCode NotImplemented = new CoAPResponseCode("NotImplemented", "Not Implemented", 5, 1);
        public static CoAPResponseCode BadGateway = new CoAPResponseCode("BadGateway", "Bad Gateway", 5, 2);
        public static CoAPResponseCode ServiceUnavailable = new CoAPResponseCode("ServiceUnavailable", "Service Unavailable", 5, 3);
        public static CoAPResponseCode GatewayTimeout = new CoAPResponseCode("GatewayTimeout", "Gateway Timeout", 5, 4);
        public static CoAPResponseCode ProxyingNotSupported = new CoAPResponseCode("ProxyingNotSupported", "Proxying Not Supported", 5, 5);

        public CoAPResponseCode(string name, string description, byte category, byte detail) : base(name, description, category, detail)
        {

        }
    }

    /// <summary>
    /// 0-Confirmable
    /// 1-Non-confirmable 
    /// 2-Acknowledgement
    //  3-Reset
    /// </summary>
    public class CoAPMessageType : AbsClassEnum
    {
        private string _name = "";
        private byte _typeValue;

        public static CoAPMessageType Confirmable = new CoAPMessageType("Confirmable", 0);
        public static CoAPMessageType NonConfirmable = new CoAPMessageType("NonConfirmable", 1);
        public static CoAPMessageType Acknowledgement = new CoAPMessageType("Acknowledgement", 2);
        public static CoAPMessageType Reset = new CoAPMessageType("Reset", 3);

        public byte Value
        {
            get
            {
                return _typeValue;
            }
        }

        public string Name
        {
            get { return _name; }
        }
        protected override string Tag
        {
            get
            {
                return _typeValue.ToString();
            }
        }

        public CoAPMessageType(string name, byte typeValue)
        {
            _name = name;
            _typeValue = typeValue;
        }

        internal static object Get<T>(int v)
        {
            throw new NotImplementedException();
        }
    }
}
