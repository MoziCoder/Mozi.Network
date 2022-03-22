namespace Mozi.Encode.CBOR
{
    //TODO 进一步实现CBOR编码解码

    /// <summary>
    /// CBOR编码解码器
    /// </summary>
    /// <remarks>
    /// <list type="table">
    ///     <item>1,CBOR压缩空间效率比较高，但从编码复杂度看并不适宜作为嵌入式使用，更像是人为设计的高难度编码格式。</item>
    ///     <item>2,本解码器反对复杂数据类型嵌套</item>
    /// </list>
    /// </remarks>
    public class CBOREncoder
    {
        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CBORDataInfo Decode(byte[] data)
        {
            int offset = 0;
            CBORDataType cb = CBORDataType.Parse(data[0]);
            CBORDataInfo di = cb.Serializer.Parse(data);
            return di;
        }
        /// <summary>
        /// 编码数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Encode(CBORDataInfo data)
        {
            return data.DataType.Serializer.Pack(data);
        }
    }
    /// <summary>
    /// CBOR数据类型
    /// </summary>
    public class CBORDataType : AbsClassEnum
    {
        private byte _header;
        private string _typeName;

        public string TypeName { get => _typeName; set => _typeName = value; }
        public int TypeIndex { get => _header >> 5; }
        public byte Header { get => _header; set => _header = value; }
        protected override string Tag => _header.ToString();

        private CBORDataSerializer _serializer;
        /// <summary>
        /// 无符号整数
        /// </summary>
        public static CBORDataType UnsignedInteger = new CBORDataType(0b00000000, "unsigned integer", new UnsignedIntegerSerializer());
        /// <summary>
        /// 负整数
        /// </summary>
        public static CBORDataType NegativeInteger = new CBORDataType(0b00100000, "negative integer", new NegativeIntegerSerializer());
        /// <summary>
        /// Hex字符串
        /// </summary>
        public static CBORDataType StringArray = new CBORDataType(0b01000000, "hex string array", new StringArraySerializer());
        /// <summary>
        /// 字符串
        /// </summary>
        public static CBORDataType StringText = new CBORDataType(0b01100000, "string text", new StringTextSerialzier() );
        /// <summary>
        /// 数组
        /// </summary>
        public static CBORDataType ByteArray = new CBORDataType(0b10000000, "byte array", new ArraySerializer() );
        /// <summary>
        /// 键值对集合
        /// </summary>
        public static CBORDataType KeyPair = new CBORDataType(0b10100000, "map key/pair", new KeyPairSerializer());
        /// <summary>
        /// 标签项
        /// </summary>
        public static CBORDataType TagItem = new CBORDataType(0b11000000, "tag item", new TagItemSerializer() );
        /// <summary>
        /// 简单类型、浮点型（包含半精度）
        /// </summary>
        public static CBORDataType SimpleFloat = new CBORDataType(0b11100000, "simple float", new SimpleFloatSerializer() );
        /// <summary>
        /// 解码编码器
        /// </summary>
        public CBORDataSerializer Serializer { get { return _serializer; } }

        public CBORDataType(byte header, string dt, CBORDataSerializer serializer)
        {
            _header = header;
            _typeName = dt;
            _serializer = serializer;
            _serializer.DataType = this;
        }
        /// <summary>
        /// 解析类型
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        public static CBORDataType Parse(byte head)
        {
            head = (byte)(head & 0b11100000);
            CBORDataType cb = Get<CBORDataType>(head.ToString());
            return cb;
        }

    }

    //0x00..0x17 	unsigned integer 0x00..0x17 (0..23)
    //0x18 	        unsigned integer(one-byte uint8_t follows)
    //0x19 	        unsigned integer(two-byte uint16_t follows)
    //0x1a 	        unsigned integer(four-byte uint32_t follows)
    //0x1b 	        unsigned integer(eight-byte uint64_t follows)

    //0x20..0x37 	negative integer -1-0x00..-1-0x17 (-1..-24)
    //0x38 	        negative integer -1-n(one-byte uint8_t for n follows)
    //0x39 	        negative integer -1-n(two-byte uint16_t for n follows)
    //0x3a 	        negative integer -1-n(four-byte uint32_t for n follows)
    //0x3b 	        negative integer -1-n(eight-byte uint64_t for n follows)

    //0x40..0x57 	byte string (0x00..0x17 bytes follow)
    //0x58 	        byte string (one-byte uint8_t for n, and then n bytes follow)
    //0x59 	        byte string (two-byte uint16_t for n, and then n bytes follow)
    //0x5a 	        byte string (four-byte uint32_t for n, and then n bytes follow)
    //0x5b 	        byte string (eight-byte uint64_t for n, and then n bytes follow)
    //0x5f 	        byte string, byte strings follow, terminated by "break"

    //0x60..0x77 	UTF-8 string (0x00..0x17 bytes follow)
    //0x78 	        UTF-8 string (one-byte uint8_t for n, and then n bytes follow)
    //0x79 	        UTF-8 string (two-byte uint16_t for n, and then n bytes follow)
    //0x7a 	        UTF-8 string (four-byte uint32_t for n, and then n bytes follow)
    //0x7b 	        UTF-8 string (eight-byte uint64_t for n, and then n bytes follow)
    //0x7f 	        UTF-8 string, UTF-8 strings follow, terminated by "break"

    //0x80..0x97 	array(0x00..0x17 data items follow)
    //0x98 	        array(one-byte uint8_t for n, and then n data items follow)
    //0x99 	        array(two-byte uint16_t for n, and then n data items follow)
    //0x9a 	        array(four-byte uint32_t for n, and then n data items follow)
    //0x9b 	        array(eight-byte uint64_t for n, and then n data items follow)
    //0x9f 	        array, data items follow, terminated by "break"

    //0xa0..        0xb7 	map(0x00..0x17 pairs of data items follow)
    //0xb8 	        map(one-byte uint8_t for n, and then n pairs of data items follow)
    //0xb9 	        map(two-byte uint16_t for n, and then n pairs of data items follow)
    //0xba 	        map(four-byte uint32_t for n, and then n pairs of data items follow)
    //0xbb 	        map(eight-byte uint64_t for n, and then n pairs of data items follow)
    //0xbf 	        map, pairs of data items follow, terminated by "break"

    //0xc0 	        text-based date/time(data item follows; see Section 3.4.1)
    //0xc1 	        epoch-based date/time(data item follows; see Section 3.4.2)
    //0xc2 	        unsigned bignum(data item "byte string" follows)
    //0xc3 	        negative bignum(data item "byte string" follows)
    //0xc4 	        decimal Fraction(data item "array" follows; see Section 3.4.4)
    //0xc5 	        bigfloat(data item "array" follows; see Section 3.4.4)
    //0xc6..0xd4 	(tag)
    //0xd5..0xd7 	expected conversion(data item follows; see Section 3.4.5.2)
    //0xd8..0xdb 	(more tags; 1/2/4/8 bytes of tag number and then a data item follow)

    //0xe0..0xf3 	(simple value)
    //0xf4 	        false
    //0xf5 	        true
    //0xf6 	        null
    //0xf7 	        undefined
    //0xf8 	        (simple value, one byte follows)
    //0xf9 	        half-precision float (two-byte IEEE 754)
    //0xfa 	        single-precision float (four-byte IEEE 754)
    //0xfb 	        double-precision float (eight-byte IEEE 754)

    //0xff 	        "break" stop code

    /// <summary>
    /// CBOR数据信息
    /// </summary>
    public class CBORDataInfo
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        public CBORDataType DataType { get; set; }
        /// <summary>
        /// 指示数,解析时有意义，赋值时没有意义
        /// </summary>
        public byte Indicator { get; set; }
        /// <summary>
        /// 数据大小，简单类型为有效数据编码程度，复合类型为集合大小
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// 值，可以是多种类型，取值时先做类型判断
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// 截取的数据包
        /// </summary>
        public byte[] Data { get; set; }
        /// <summary>
        /// 是否无限，是否是集合类型，集合类型使用0xff作为数据包的结束位
        /// </summary>
        public bool IsIndefinite { get; set; }

        public CBORDataInfo(CBORDataType dataType, object value)
        {
            DataType = dataType;
            Value = value;
        }
        public CBORDataInfo()
        {

        }

        public override string ToString()
        {
            return DataType.Serializer.ToString(this);
        }
    }

}
