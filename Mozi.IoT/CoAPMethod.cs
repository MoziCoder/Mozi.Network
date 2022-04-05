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
        /// 
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

        public static CoAPRequestMethod Get = new CoAPRequestMethod("GET", "", 0, 1);
        public static CoAPRequestMethod Post = new CoAPRequestMethod("POST", "", 0, 2);
        public static CoAPRequestMethod Put = new CoAPRequestMethod("PUT", "", 0, 3);
        public static CoAPRequestMethod Delete = new CoAPRequestMethod("DELETE", "", 0, 4);

        internal CoAPRequestMethod(string name, string description, byte category, byte detail) : base(name, description, category, detail)
        {

        }
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

        public static CoAPResponseCode Created = new CoAPResponseCode("Created", "Created", 2, 1);
        public static CoAPResponseCode Deleted = new CoAPResponseCode("Deleted", "Deleted", 2, 2);
        public static CoAPResponseCode Valid = new CoAPResponseCode("Valid", "Valid", 2, 3);
        public static CoAPResponseCode Changed = new CoAPResponseCode("Changed", "Changed", 2, 4);
        /// <summary>
        /// 类似HTTP 200 
        /// </summary>
        public static CoAPResponseCode Content = new CoAPResponseCode("Content", "Content", 2, 5);
        public static CoAPResponseCode Continue = new CoAPResponseCode("Continue", "Continue", 2, 31);

        public static CoAPResponseCode BadRequest = new CoAPResponseCode("BadRequest", "Bad Request", 4, 0);
        public static CoAPResponseCode Unauthorized = new CoAPResponseCode("Unauthorized", "Unauthorized", 4, 1);
        public static CoAPResponseCode BadOption = new CoAPResponseCode("BadOption", "Bad Option", 4, 2);
        public static CoAPResponseCode Forbidden = new CoAPResponseCode("Forbidden", "Forbidden", 4, 3);
        public static CoAPResponseCode NotFound = new CoAPResponseCode("NotFound", "Not Found", 4, 4);
        public static CoAPResponseCode MethodNotAllowed = new CoAPResponseCode("MethodNotAllowed", "Method Not Allowed", 4, 5);
        public static CoAPResponseCode NotAcceptable = new CoAPResponseCode("NotAcceptable", "Not Acceptable", 4, 6);

        public static CoAPResponseCode RequestEntityIncomplete = new CoAPResponseCode("RequestEntityIncomplete", "Request Entity Incomplete", 4, 8);

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
        public override string ToString()
        {
            return Category + "." + Detail + " " + Description;
        }
    }
}
