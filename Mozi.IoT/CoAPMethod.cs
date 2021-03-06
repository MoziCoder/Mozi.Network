using Mozi.IoT.Generic;

namespace Mozi.IoT
{
    // 代码 8bits=3bits+5bits
    // 高3位为分类 
    // 低5位为明细
    // 
    //  0.00      Indicates an Empty message (see Section 4.1).
    //  0.01-0.31 Indicates a request.Values in this range are assigned by the "CoAP Method Codes" sub-registry(see Section 12.1.1).
    //  
    //     0.01  GET    | [RFC7252] 
    //     0.02  POST   | [RFC7252] 
    //     0.03  PUT    | [RFC7252] 
    //     0.04  DELETE | [RFC7252]
    //     
    //  1.00-1.31 Reserved
    //  2.00-5.31 Indicates a response.Values in this range are assigned bythe "CoAP Response Codes" sub-registry(see Section 12.1.2).
    //  
    //     2.01 | Created                      | [RFC7252] |
    //     2.02 | Deleted                      | [RFC7252] |
    //     2.03 | Valid                        | [RFC7252] |
    //     2.04 | Changed                      | [RFC7252] |
    //     2.05 | Content                      | [RFC7252] |
    //     
    //     2.31 | Continue                     | [RFC7959] |
    //     
    //     4.00 | Bad Request                  | [RFC7252] |
    //     4.01 | Unauthorized                 | [RFC7252] |
    //     4.02 | Bad Option                   | [RFC7252] |
    //     4.03 | Forbidden                    | [RFC7252] |
    //     4.04 | Not Found                    | [RFC7252] |
    //     4.05 | Method Not Allowed           | [RFC7252] |
    //     4.06 | Not Acceptable               | [RFC7252] |
    //     
    //     4.08 | Request Entity Incomplete    | [RFC7959] |
    //     
    //     4.12 | Precondition Failed          | [RFC7252] |
    //     4.13 | Request Entity Too Large     | [RFC7252] |
    //     4.15 | Unsupported Content-Format   | [RFC7252] |
    //     5.00 | Internal Server Error        | [RFC7252] |
    //     5.01 | Not Implemented              | [RFC7252] |
    //     5.02 | Bad Gateway                  | [RFC7252] |
    //     5.03 | Service Unavailable          | [RFC7252] |
    //     5.04 | Gateway Timeout              | [RFC7252] |
    //     5.05 | Proxying Not Supported       | [RFC7252] |
    //     
    //  6.00-7.31 Reserved

    /// <summary>
    /// CoAP操作代码
    /// </summary>
    public class CoAPCode : AbsClassEnum
    {

        private string _name = "", _description;

        private byte _category = 0, _detail = 0;
        /// <summary>
        /// 消息代码 Empty Message
        /// </summary>
        public static CoAPCode Empty = new CoAPCode("Empty", "Empty Message", 0, 0);

        /// <summary>
        /// 分类
        /// </summary>
        public int Category
        {
            get
            {
                return _category;
            }
        }
        /// <summary>
        /// 明细
        /// </summary>
        public byte Detail
        {
            get
            {
                return _detail;
            }
        }
        /// <summary>
        /// 代码名称
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
        /// <summary>
        /// 代码描述字符串
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }
        /// <summary>
        /// 标识符
        /// </summary>
        protected override string Tag
        {
            get { return ((byte)(_category << 5) + _detail).ToString(); }
        }
        /// <summary>
        /// 数据包
        /// </summary>
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
    /// <summary>
    /// 请求码
    /// </summary>
    public class CoAPRequestMethod : CoAPCode
    {
        /// <summary>
        /// GET方法
        /// </summary>
        public static CoAPRequestMethod Get = new CoAPRequestMethod("GET", "", 0, 1);
        /// <summary>
        /// POST方法
        /// </summary>
        public static CoAPRequestMethod Post = new CoAPRequestMethod("POST", "", 0, 2);
        /// <summary>
        /// PUT方法
        /// </summary>
        public static CoAPRequestMethod Put = new CoAPRequestMethod("PUT", "", 0, 3);
        /// <summary>
        /// DELETE方法
        /// </summary>
        public static CoAPRequestMethod Delete = new CoAPRequestMethod("DELETE", "", 0, 4);

        internal CoAPRequestMethod(string name, string description, byte category, byte detail) : base(name, description, category, detail)
        {

        }
        /// <summary>
        /// 转为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

    }
    /// <summary>
    /// 响应码
    /// </summary>
    public class CoAPResponseCode : CoAPCode
    {
        /// <summary>
        /// 响应代码Created
        /// </summary>
        public static CoAPResponseCode Created = new CoAPResponseCode("Created", "Created", 2, 1);
        /// <summary>
        /// 响应代码Deleted
        /// </summary>
        public static CoAPResponseCode Deleted = new CoAPResponseCode("Deleted", "Deleted", 2, 2);
        /// <summary>
        /// 响应代码Valid
        /// </summary>
        public static CoAPResponseCode Valid = new CoAPResponseCode("Valid", "Valid", 2, 3);
        /// <summary>
        /// 响应代码Changed
        /// </summary>
        public static CoAPResponseCode Changed = new CoAPResponseCode("Changed", "Changed", 2, 4);
        /// <summary>
        /// 响应代码Content 类似HTTP 200 
        /// </summary>
        public static CoAPResponseCode Content = new CoAPResponseCode("Content", "Content", 2, 5);
        /// <summary>
        /// 响应代码Continue
        /// </summary>
        public static CoAPResponseCode Continue = new CoAPResponseCode("Continue", "Continue", 2, 31);
        /// <summary>
        /// 响应代码Bad Request
        /// </summary>
        public static CoAPResponseCode BadRequest = new CoAPResponseCode("BadRequest", "Bad Request", 4, 0);
        /// <summary>
        /// 响应代码Unauthorized
        /// </summary>
        public static CoAPResponseCode Unauthorized = new CoAPResponseCode("Unauthorized", "Unauthorized", 4, 1);
        /// <summary>
        /// 响应代码Bad Option
        /// </summary>
        public static CoAPResponseCode BadOption = new CoAPResponseCode("BadOption", "Bad Option", 4, 2);
        /// <summary>
        /// 响应代码Forbidden
        /// </summary>
        public static CoAPResponseCode Forbidden = new CoAPResponseCode("Forbidden", "Forbidden", 4, 3);
        /// <summary>
        /// 响应代码Not Found
        /// </summary>
        public static CoAPResponseCode NotFound = new CoAPResponseCode("NotFound", "Not Found", 4, 4);
        /// <summary>
        /// 响应代码Method Not Allowed
        /// </summary>
        public static CoAPResponseCode MethodNotAllowed = new CoAPResponseCode("MethodNotAllowed", "Method Not Allowed", 4, 5);
        /// <summary>
        /// 响应代码Not Acceptable
        /// </summary>
        public static CoAPResponseCode NotAcceptable = new CoAPResponseCode("NotAcceptable", "Not Acceptable", 4, 6);
        /// <summary>
        /// 响应代码Request Entity Incomplete
        /// </summary>
        public static CoAPResponseCode RequestEntityIncomplete = new CoAPResponseCode("RequestEntityIncomplete", "Request Entity Incomplete", 4, 8);
        /// <summary>
        /// 响应代码Precondition Failed
        /// </summary>
        public static CoAPResponseCode PreconditionFailed = new CoAPResponseCode("PreconditionFailed", "Precondition Failed", 4, 12);
        /// <summary>
        /// 响应代码Request Entity Too Large
        /// </summary>
        public static CoAPResponseCode RequestEntityTooLarge = new CoAPResponseCode("RequestEntityTooLarge", "Request Entity Too Large", 4, 13);
        /// <summary>
        /// 响应代码Unsupported Content-Format
        /// </summary>
        public static CoAPResponseCode UnsupportedContentFormat = new CoAPResponseCode("UnsupportedContentFormat", "Unsupported Content-Format", 4, 15);
        /// <summary>
        /// 响应代码Internal Server Error
        /// </summary>
        public static CoAPResponseCode InternalServerError = new CoAPResponseCode("InternalServerError", "Internal Server Error", 5, 0);
        /// <summary>
        /// 响应代码Not Implemented
        /// </summary>
        public static CoAPResponseCode NotImplemented = new CoAPResponseCode("NotImplemented", "Not Implemented", 5, 1);
        /// <summary>
        /// 响应代码Bad Gateway
        /// </summary>
        public static CoAPResponseCode BadGateway = new CoAPResponseCode("BadGateway", "Bad Gateway", 5, 2);
        /// <summary>
        /// 响应代码Service Unavailable
        /// </summary>
        public static CoAPResponseCode ServiceUnavailable = new CoAPResponseCode("ServiceUnavailable", "Service Unavailable", 5, 3);
        /// <summary>
        /// 响应代码Gateway Timeout
        /// </summary>
        public static CoAPResponseCode GatewayTimeout = new CoAPResponseCode("GatewayTimeout", "Gateway Timeout", 5, 4);
        /// <summary>
        /// 响应代码Proxying Not Supported
        /// </summary>
        public static CoAPResponseCode ProxyingNotSupported = new CoAPResponseCode("ProxyingNotSupported", "Proxying Not Supported", 5, 5);

        internal CoAPResponseCode(string name, string description, byte category, byte detail) : base(name, description, category, detail)
        {

        }
        /// <summary>
        /// 转为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Category + "." + Detail + " " + Description;
        }
    }
}
