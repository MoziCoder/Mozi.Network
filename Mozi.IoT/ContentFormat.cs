using Mozi.IoT.Generic;

namespace Mozi.IoT
{
    ///<summary>
    /// 内容格式
    ///</summary>
    ///
    /// CoAP Content-Formats Registry
    /// 
    ///           0-255 | Expert Review                        
    ///        256-9999 | IETF Review or IESG Approval         
    ///     10000-64999 | First Come First Served              
    ///     65000-65535 | Experimental use(no operational use) 
    ///
    ///     text/plain;              | -        |  0 | [RFC2046] [RFC3676]    |
    ///     charset=utf-8            |          |    | [RFC5147]              |
    ///     application/link-format  | -        | 40 | [RFC6690]              |
    ///     application/xml          | -        | 41 | [RFC3023]              |
    ///     application/octet-stream | -        | 42 | [RFC2045] [RFC2046]    |
    ///     application/exi          | -        | 47 | [REC-exi-20140211]     |
    ///     application/json         | -        | 50 | [RFC7159]              |
    ///     application/cbor         | -        | 60 | [RFC7159]              |
    ///     
    public class ContentFormat : AbsClassEnum
    {
        private ushort _num = 0;

        private string _contentType = "";
        /// <summary>
        /// 内容MIME的字符串表示
        /// </summary>
        public string ContentType
        {
            get
            {
                return _contentType;
            }
        }
        /// <summary>
        /// MIME注册序号
        /// </summary>
        public ushort Num { get { return _num; } }

        protected override string Tag => _num.ToString();

        public static ContentFormat TextPlain   = new ContentFormat("text/plain", 0);
        public static ContentFormat LinkFormat  = new ContentFormat("application/link-format", 40);
        public static ContentFormat XML         = new ContentFormat("application/xml", 41);
        public static ContentFormat Stream      = new ContentFormat("application/octet-stream", 42);
        public static ContentFormat EXI         = new ContentFormat("application/exi", 47);
        public static ContentFormat JSON        = new ContentFormat("application/json", 50);
        public static ContentFormat CBOR        = new ContentFormat("application/cbor", 60);

        internal ContentFormat(string contentType, ushort num)
        {
            _contentType = contentType;
            _num = num;
        }
        /// <summary>
        /// 从内容类型字符串解析内容类型,如text/plain,application/cbor
        /// </summary>
        /// <param name="contentFormat"></param>
        /// <returns></returns>
        public static ContentFormat Parse(string contentFormat)
        {

            if (!string.IsNullOrEmpty(contentFormat))
            {
                string[] items = contentFormat.Split(new char[] { ';' });
                string cf = items[0].Trim();
                switch (cf)
                {
                    case "text/plain":
                    case "plain":
                        return TextPlain;
                    case "application/link-format":
                    case "link-format":
                    case "linkformat":
                        return LinkFormat;
                    case "application/xml":
                    case "xml":
                        return XML;
                    case "application/octet-stream":
                    case "octet-stream":
                    case "octetstream":
                    case "stream":
                        return Stream;
                    case "application/exi":
                    case "exi":
                        return EXI;
                    case "application/json":
                    case "json":
                        return JSON;
                    case "application/cbor":
                    case "cbor":
                        return CBOR;
                }
                return null;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 从内容类型注册号解析内容类型,取值范围请查看<see cref="ContentFormat"/>中定义的几个静态变量的{Num}值
        /// <list type="bullet">
        ///     <item><see cref="ContentFormat.TextPlain"/></item>
        ///     <item><see cref="ContentFormat.TextPlain"/> </item>
        ///     <item><see cref="ContentFormat.LinkFormat"/></item>
        ///     <item><see cref="ContentFormat.XML"/>       </item>
        ///     <item><see cref="ContentFormat.Stream"/>    </item>
        ///     <item><see cref="ContentFormat.EXI"/>       </item>
        ///     <item><see cref="ContentFormat.JSON"/>      </item>
        ///     <item><see cref="ContentFormat.CBOR"/></item>
        /// </list>         
        /// </summary>
        /// <param name="formatRegistryNumber"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public static ContentFormat Parse(int formatRegistryNumber)
        {
            return Get<ContentFormat>(formatRegistryNumber.ToString());
        }
    }
}
