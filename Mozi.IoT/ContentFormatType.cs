using System;

namespace Mozi.IoT
{

    ///<summary>
    /// 内容格式
    ///</summary>
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
    ///     applicaiton/cbor         | -        | 60 | [RFC7159]              |
    ///     
    public class ContentFormatType : AbsClassEnum
    {
        private ushort _num = 0;
        private string _contentType = "";

        public string ContentType
        {
            get
            {
                return _contentType;
            }
        }

        public ushort OptionValue
        {
            get
            {
                return _num;
            }
        }
        public ushort Num { get { return _num; } }
        protected override string Tag => _num.ToString();

        public ContentFormatType TextPlain = new ContentFormatType("text/plain", 0);
        public ContentFormatType LinkFormat = new ContentFormatType("application/link-format", 40);
        public ContentFormatType XML = new ContentFormatType("application/xml", 41);
        public ContentFormatType Stream = new ContentFormatType("application/octet-stream", 42);
        public ContentFormatType EXI = new ContentFormatType("application/exi", 47);
        public ContentFormatType JSON = new ContentFormatType("application/json", 50);
        public ContentFormatType CBOR = new ContentFormatType("applicaiton/cbor", 60);

        internal ContentFormatType(string contentType, ushort num)
        {
            _contentType = contentType;
            _num = num;
        }
    }
}
