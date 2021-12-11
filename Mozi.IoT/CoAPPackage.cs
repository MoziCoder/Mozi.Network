using System;
using System.Collections.Generic;

namespace Mozi.IoT
{
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
        public byte TokenLength
        {
            get
            {
                return (byte)(Token == null ? 0 : Token.Length);
            }
            set
            {
                if (value == 0)
                {
                    Token = null;
                }
                else
                {
                    Token = new byte[value];
                }
            }
        }
        /// <summary>
        /// 8bits Lengths 9-15 reserved
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
            head = (byte)(head | (Version << 6));
            head = (byte)(head | (MessageType.Value << 4));
            head = (byte)(head | TokenLength);
            data.Add(head);
            data.Add((byte)(((byte)Code.Category << 5) | ((byte)(Code.Detail << 3) >> 3)));
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
            if (Payload != null)
            {
                data.Add(CoAPProtocol.HeaderEnd);
                data.AddRange(Payload);
            }
            return data.ToArray();

        }

        public static CoAPPackage Parse(byte[] data, bool isRequest)
        {
            CoAPPackage pack = new CoAPPackage();
            byte head = data[0];
            pack.Version = (byte)(head >> 6);
            pack.MessageType = AbsClassEnum.Get<CoAPMessageType>(((byte)(head << 2) >> 4).ToString());
            pack.TokenLength = (byte)((byte)(head << 4) >> 4);

            pack.Code = isRequest ? AbsClassEnum.Get<CoAPRequestCode>(data[1].ToString()) : (CoAPCode)AbsClassEnum.Get<CoAPResponseCode>(data[1].ToString());

            byte[] arrMsgId = new byte[2], arrToken = new byte[pack.TokenLength];
            Array.Copy(data, 2, arrMsgId, 0, 2);
            Array.Copy(data, 2 + 2, arrToken, 0, arrToken.Length);
            pack.Token = arrToken;
            pack.MesssageId = BitConverter.ToUInt16(arrMsgId.Revert(), 0);
            //3+2+arrToken.Length+1开始是Option部分
            int bodySplitterPos = 2 + 2 + arrToken.Length;
            uint deltaSum = 0;
            while (bodySplitterPos < data.Length && data[bodySplitterPos] != CoAPProtocol.HeaderEnd)
            {
                CoAPOption option = new CoAPOption();
                option.OptionHead = data[bodySplitterPos];
                byte delta = (byte)(option.OptionHead >> 4);
                int lenDelta = 0, lenLength = 0;
                if (option.Delta <= 12)
                {

                }
                else if (option.Delta == 13)
                {
                    lenDelta = 1;
                }
                else if (option.Delta == 14)
                {
                    lenDelta += 2;
                }
                if (lenDelta > 0)
                {
                    byte[] arrDeltaExt = new byte[2];
                    Array.Copy(data, bodySplitterPos + 1, arrDeltaExt, 0, lenDelta);
                    option.DeltaExtend = BitConverter.ToUInt16(arrDeltaExt.Revert(), 0);
                }
                option.DeltaValue += deltaSum;
                //赋默认值
                option.Option = AbsClassEnum.Get<CoAPOptionDefine>(option.DeltaValue.ToString());
                if (object.ReferenceEquals(null, option.Option))
                {
                    option.Option = CoAPOptionDefine.Unknown;
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
                    Array.Copy(data, bodySplitterPos + 1 + lenDelta + 1, arrDeltaExt, 0, lenDelta);
                    option.DeltaExtend = BitConverter.ToUInt16(arrDeltaExt.Revert(), 0);
                }

                option.Value = new byte[option.LengthValue];
                Array.Copy(data, bodySplitterPos + 1 + lenDelta + lenLength, option.Value, 0, option.Value.Length);
                pack.Options.Add(option);
                deltaSum += delta;
                bodySplitterPos += 1 + lenDelta + lenLength + option.Value.Length;

            }
            //有效荷载
            if (data.Length > bodySplitterPos && data[bodySplitterPos] == CoAPProtocol.HeaderEnd)
            {
                pack.Payload = new byte[data.Length - bodySplitterPos - 1];

                Array.Copy(data, bodySplitterPos + 1, pack.Payload, 0, pack.Payload.Length);
            }
            return pack;

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
            get { return ((byte)(_category << 5) + _detail).ToString(); }
        }

        public byte Pack
        {
            get
            {
                return (byte)((_category << 5) | _detail);
            }
            set
            {
                _category = (byte)(value >> 5);
                _detail = (byte)((value << 3) >> 3);
            }
        }

        internal CoAPCode(string name, string description, byte category, byte detail)
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

        internal CoAPRequestCode(string name, string description, byte category, byte detail) : base(name, description, category, detail)
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

        internal CoAPResponseCode(string name, string description, byte category, byte detail) : base(name, description, category, detail)
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

        internal CoAPMessageType(string name, byte typeValue)
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
