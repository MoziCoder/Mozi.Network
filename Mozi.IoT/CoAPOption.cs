using System;
using System.Collections.Generic;

namespace Mozi.IoT
{
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
    ///     
    ///     23 | Block2           | [RFC7959] |
    ///     27 | Block1           | [RFC7959] |    
    ///     28 | Size2            | [RFC7959] |  
    ///     
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



        public static CoAPOptionDefine Block2 = new CoAPOptionDefine("Block2", 23);    //RFC 7959
        public static CoAPOptionDefine Block1 = new CoAPOptionDefine("Block1", 27);    //RFC 7959

        public static CoAPOptionDefine Size2 = new CoAPOptionDefine("Location-Query", 28); //RFC 7959

        public static CoAPOptionDefine ProxyUri = new CoAPOptionDefine("Proxy-Uri", 35);
        public static CoAPOptionDefine ProxyScheme = new CoAPOptionDefine("Proxy-Scheme", 39);

        public static CoAPOptionDefine Size1 = new CoAPOptionDefine("Size1", 60);



        public static CoAPOptionDefine Unknown = new CoAPOptionDefine("Unknown", 0);

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

        internal CoAPOptionDefine(string name, ushort optionNumber)
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
            return string.Format("Option Name:{0},OptionNumber:{1},Figure:{2}", Name, OptionNumber, string.Join(",", Critical ? "Critical" : "", UnSafe ? "UnSafe" : "", NoCacheKey ? "NoCacheKey" : ""));
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
                Delta = (byte)(value >> 4);
                Length = (byte)((byte)(value << 4) >> 4);
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
        /// 空 字节数组 数字 ASCII/UTF-8字符串
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
    public class ContentFormatType : AbsClassEnum
    {
        private ushort _num = 0;
        private string _contentType = "";

        public String ContentType
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
    /// <summary>
    /// 选项值>=0 bytes
    /// 空 字节数组 数字 ASCII/UTF-8字符串
    /// </summary>
    public abstract class OptionValue
    {
        public abstract object Value { get; }
        public abstract byte[] Pack { get; set; }
    }

    public class NullOptionValue : OptionValue
    {
        public override object Value => null;

        public override byte[] Pack
        {
            get => null;
            set
            {

            }
        }
    }

    public class ArrayByteOptionValue : OptionValue
    {
        private byte[] _pack;

        public override object Value => _pack;

        public override byte[] Pack { get => _pack; set => _pack = value; }
    }

//    public class IntegerOptionValue : OptionValue
//    {
//        private byte[] _pack;

//        public override object Value =>
//            {
               
//            };

//    public override byte[] Pack { get => _pack; set => _pack = value; }
//}

public class StringOptionValue : OptionValue
    {
        private byte[] _pack;

        public override object Value => System.Text.Encoding.UTF8.GetString(_pack);

        public override byte[] Pack { get => _pack; set => _pack = value; }
    }

    /// <summary>
    /// 分块选项 数据结构 适用Block1 Block2 总长度uint24
    /// </summary>
    public class BlockOptionValue : OptionValue
    {
        /// <summary>
        /// 块内位置 占位4-20bits
        /// </summary>
        public uint Num { get; set; }
        /// <summary>
        /// 是否最后一个包 占位1bit
        /// </summary>
        public bool MoreFlag { get; set; }
        /// <summary>
        /// 数据包总大小 占位3bits 值大小为1-6，表值范围16bytes-1024bytes
        /// </summary>
        public ushort Size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public override byte[] Pack
        {
            get
            {
                byte[] data;
                uint num = (Num << 4) | (byte)((byte)Math.Log(Size, 2) - 4);
                if (MoreFlag)
                {
                    num |= 8;
                }

                if (Num < 16)
                {
                    data = new byte[1];
                    data[0] = (byte)Num;
                }
                else if (Num < 4096)
                {
                    data = BitConverter.GetBytes((ushort)num).Revert();
                }
                else
                {
                    data = new byte[3];
                    Array.Copy(BitConverter.GetBytes(num).Revert(), 1, data, 0, data.Length);
                }
                return data;
            }
            set
            {

                Size = (ushort)Math.Pow(2, (((byte)(value[0] << 5)) >> 5) + 4);
                MoreFlag = (value[0] & 8) == 8;
                byte[] data = new byte[4];
                Array.Copy(value.Revert(), 0, data, data.Length - value.Length, value.Length);
                Num = BitConverter.ToUInt32(data, 0);
            }
        }

        public override object Value { get => Size; }

        public override string ToString()
        {
            return Pack == null ? "null" : String.Format("{0},Num:{1},M:{2},SZX:{3}(bytes)", "Block", Num, MoreFlag ? 1 : 0, Size);
        }
    }
}
