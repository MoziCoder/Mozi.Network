namespace Mozi.Encode.CBOR
{
    /// <summary>
    /// CBOR数据类型
    /// </summary>
    public class CBORDataType : AbsClassEnum
    {
        private byte _header;
        private string _name;

        /// <summary>
        /// 类型名
        /// </summary>
        public string Name { get => _name; set => _name = value; }
        /// <summary>
        /// 主类型序号
        /// </summary>
        public int TypeIndex { get => _header >> 5; }
        /// <summary>
        /// 主类型高三位值
        /// </summary>
        public byte Header { get => _header; set => _header = value; }
        /// <summary>
        /// 标识符
        /// </summary>
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
        public static CBORDataType DataArray = new CBORDataType(0b10000000, "data array", new ArraySerializer() );
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
            _name = dt;
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
}
