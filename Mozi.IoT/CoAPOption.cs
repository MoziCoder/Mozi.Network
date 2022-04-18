using System;
using System.Collections.Generic;
using Mozi.IoT.Generic;

namespace Mozi.IoT
{
    // CoAP Option Numbers Registry
    // |       0-255 | IETF Review or IESG Approval         
    // |    256-2047 | Specification Required               
    // |  2048-64999 | Expert Review                        
    // | 65000-65535 | Experimental use(no operational use) 

    //       0 | (Reserved)             | [RFC7252] |
    //       1 | If-Match               | [RFC7252] |
    //       3 | Uri-Host               | [RFC7252] |
    //       4 | ETag                   | [RFC7252] |
    //       5 | If-None-Match          | [RFC7252] |
    //       6 | Extended-Token-Length  | [RFC8974] |
    //       7 | Uri-Port               | [RFC7252] |
    //       8 | Location-Path          | [RFC7252] |
    //      11 | Uri-Path               | [RFC7252] |
    //      12 | Content-Format         | [RFC7252] |
    //      14 | Max-Age                | [RFC7252] |
    //      15 | Uri-Query              | [RFC7252] |
    //      17 | Accept                 | [RFC7252] |
    //      20 | Location-Query         | [RFC7252] |
    //      
    //      23 | Block2                 | [RFC7959] |
    //      27 | Block1                 | [RFC7959] |    
    //      28 | Size2                  | [RFC7959] |  
    //      
    //      35 | Proxy-Uri              | [RFC7252] |
    //      39 | Proxy-Scheme           | [RFC7252] |
    //      60 | Size1                  | [RFC7252] |
    //     128 | (Reserved)             | [RFC7252] |
    //     132 | (Reserved)             | [RFC7252] |
    //     136 | (Reserved)             | [RFC7252] |
    //     140 | (Reserved)             | [RFC7252] |
    //     
    // Option Delta代表Option的类型，该值代表了上表中Option类型的代码值与上一个Option代码值之间的差值
    // （如果该Option为第一个Option，则直接表达该Option的Option Delta）

    // 由于Option Delta只有4位，最大只能表达15，为了解决这个问题，coap协议有着如下规定：

    // 当Option Delta号码<=12时：Option Delta位为实际的Option Delta值
    // 当Option Delta号码<269时：Option Delta位填入13；并且在后面的Option Delta(extended) 位会占用1字节，并且填入的数为实际Option Delta值减去13
    // 当Option Delta号码<65804时：Option Delta位填入14；并且在后面的Option Delta(extended)位会占用2字节，并且填入的数为实际Option Delta值减去269

    //特别注意，填入的Option Delta值不可能为15（0x0f）当遇到15时，该包无效

    /// <summary>
    /// 选项值定义
    /// </summary>
    public class CoAPOptionDefine : AbsClassEnum
    {
        private string _name = "";

        public static CoAPOptionDefine IfMatch              = new CoAPOptionDefine("If-Match", 1);
        public static CoAPOptionDefine UriHost              = new CoAPOptionDefine("Uri-Host", 3);
        public static CoAPOptionDefine ETag                 = new CoAPOptionDefine("ETag", 4);
        public static CoAPOptionDefine IfNoneMatch          = new CoAPOptionDefine("If-None-Match", 5);
        public static CoAPOptionDefine ExtendedTokenLength  = new CoAPOptionDefine("Extended-Token-Length", 6); //RFC8974

        public static CoAPOptionDefine UriPort              = new CoAPOptionDefine("Uri-Port", 7);
        public static CoAPOptionDefine LocationPath         = new CoAPOptionDefine("Location-Path", 8);
        public static CoAPOptionDefine UriPath              = new CoAPOptionDefine("Uri-Path", 11);
        public static CoAPOptionDefine ContentFormat        = new CoAPOptionDefine("Content-Format", 12);
        public static CoAPOptionDefine MaxAge               = new CoAPOptionDefine("Max-Age", 14);
        public static CoAPOptionDefine UriQuery             = new CoAPOptionDefine("Uri-Query", 15);
        public static CoAPOptionDefine Accept               = new CoAPOptionDefine("Accept", 17);
        public static CoAPOptionDefine LocationQuery        = new CoAPOptionDefine("Location-Query", 20);

        public static CoAPOptionDefine Block2               = new CoAPOptionDefine("Block2", 23);              //RFC 7959
        public static CoAPOptionDefine Block1               = new CoAPOptionDefine("Block1", 27);              //RFC 7959

        public static CoAPOptionDefine Size2                = new CoAPOptionDefine("Size2", 28);               //RFC 7959

        public static CoAPOptionDefine ProxyUri             = new CoAPOptionDefine("Proxy-Uri", 35);
        public static CoAPOptionDefine ProxyScheme          = new CoAPOptionDefine("Proxy-Scheme", 39);

        public static CoAPOptionDefine Size1                = new CoAPOptionDefine("Size1", 60);

        public static CoAPOptionDefine Unknown              = new CoAPOptionDefine("Unknown", 0);
        /// <summary>
        /// 选项名称
        /// </summary>
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
        public bool Critical
        {
            get
            {
                return (((byte)OptionNumber) & 0x01) == 0x01;
            }
        }
        public bool UnSafe
        {
            get
            {
                return (((byte)OptionNumber) & 0x02) == 0x02;
            }
        }
        public bool NoCacheKey
        {
            get
            {
                return (((byte)OptionNumber) & 0x1e) == 0x1e;
            }
        }
        public override string ToString()
        {
            return string.Format("OptionName:{0},OptionNumber:{1},Figure:{2}", Name, OptionNumber, string.Join(",", Critical ? "Critical" : "", UnSafe ? "UnSafe" : "", NoCacheKey ? "NoCacheKey" : ""));
        }
    }

    ///<summary>
    ///CoAP头属性
    /// </summary>
    public class CoAPOption
    {
        private uint _deltaValue = 0, _lenValue = 0;

        private OptionValue _optValue;

        /// <summary>
        /// 选项序号
        /// </summary>
        public CoAPOptionDefine Option { get; set; }
        /// <summary>
        /// 首部字节码
        /// </summary>
        public byte OptionHead
        {
            get
            {
                return (byte)(((byte)(Delta << 4)) | Length);
            }
            set
            {
                Delta = (byte)(value >> 4);
                Length = (byte)((byte)(value << 4) >> 4);
            }
        }
        /// <summary>
        /// Delta总值<see cref="Delta"/>+<see cref="DeltaExtend"/>
        /// </summary>
        public uint DeltaValue
        {
            get
            {
                return (uint)(Delta + DeltaExtend);
            }
            set
            {
                _deltaValue = value;
                if (_deltaValue <= 12)
                {
                    Delta = (byte)_deltaValue;
                    DeltaExtend = 0;
                }
                else if (_deltaValue < 269)
                {
                    Delta = 13;
                    DeltaExtend = (byte)(_deltaValue-13);
                }
                else if (_deltaValue < 65804)
                {
                    Delta = 14;
                    DeltaExtend = (ushort)(_deltaValue-269);
                }
            }
        }
        /// <summary>
        /// 长度总值<see cref="Length"/>+<see cref="LengthExtend"/>
        /// </summary>
        public uint LengthValue
        {
            get
            {
                return (uint)(Length + LengthExtend);
            }
            set
            {
                _lenValue = value;
                if (_lenValue <= 12)
                {
                    Length = (byte)_lenValue;
                    LengthExtend = 0;
                }
                else if (_lenValue < 269)
                {
                    Length = 13;
                    LengthExtend = (byte)(_lenValue-13);
                }
                else if (_lenValue < 65804)
                {
                    Length = 14;
                    LengthExtend = (ushort)(_lenValue-269);
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
        /// <summary>
        /// Delta扩展值
        /// </summary>
        public ushort DeltaExtend { get; set; }

        /// <summary>
        /// 值长度 4bits
        /// 取值范围：0-15
        ///     13:  An 8-bit unsigned integer precedes the Option Value and indicates the Option Length minus 13.
        ///     14:  A 16-bit unsigned integer in network byte order precedes the Option Value and indicates the Option Length minus 269.
        ///     15:  Reserved for future use.If the field is set to this value, it MUST be processed as a message format error.
        /// </summary>
        public byte Length { get; set; }
        /// <summary>
        /// Length扩展值
        /// </summary>
        public ushort LengthExtend { get; set; }
        /// <summary>
        /// 选项值>=0 bytes
        /// 空 字节数组 数字 ASCII/UTF-8字符串
        /// </summary>
        public OptionValue Value 
        { 
            get 
            { 
                return _optValue; 
            } 
            set 
            {
                _optValue = value; 
                LengthValue = value != null ? (uint)value.Length : 0;
            }
        }
        /// <summary>
        /// 选项数据包 定义头+数据值 { Delta,OptionsLength,DeltaExtend,LengthExtend,OptionValue} 字节数据值
        /// </summary>
        public byte[] Pack
        {
            get
            {

                List<byte> data = new List<byte>();
                data.Add(OptionHead);
                //delta
                if (Delta == 14)
                {
                    data.AddRange(BitConverter.GetBytes(DeltaExtend));
                }
                else if (Delta == 13)
                {
                    data.Add((byte)DeltaExtend);
                }
                //length
                if (Length == 14)
                {
                    data.AddRange(BitConverter.GetBytes(LengthExtend));
                }
                else if (Length == 13)
                {
                    data.Add((byte)LengthExtend);
                }
                data.AddRange(Value.Pack);
                return data.ToArray();

            }
        }
        public CoAPOption()
        {
            Value = new OptionValue();
        }
    }
}
