using Mozi.Encode.Generic;
using System;
using System.Collections.Generic;

namespace Mozi.Encode.CBOR
{
    //TODO 进一步实现CBOR编码解码
    internal delegate CBORDataInfo CBORDataParser(byte[] data);

    internal delegate byte[] CBORDataPacker(CBORDataInfo data);

    /// <summary>
    /// CBOR编码解码器
    /// </summary>
    /// <remarks>
    /// CBOR压缩空间效率比较高，但从编码复杂度看并不适宜作为嵌入式使用，更像是人为设计的高难度编码格式。
    /// </remarks>
    internal class CBOREncoder
    {
        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CBORDataInfo Decode(byte[] data)
        {
            CBORDataType cb = CBORDataType.Parse(data[0]);
            CBORDataInfo di = cb.Parser(data);
            return di;
        }
        /// <summary>
        /// 编码数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Encode(CBORDataInfo data)
        {
            return data.DataType.Packer(data);
        }
    }

    /// <summary>
    /// CBOR数据类型
    /// </summary>
    internal class CBORDataType : AbsClassEnum
    {
        private byte _header;
        private string _typeName;

        public string TypeName { get => _typeName; set => _typeName = value; }
        public int TypeIndex { get => _header >> 5; }
        public byte Header { get => _header; set => _header = value; }
        protected override string Tag => _header.ToString();

        private CBORDataPacker _packer;
        private CBORDataParser _parser;

        public static CBORDataType UnSignedInteger = new CBORDataType(0b00000000, "unsigned integer", new CBORDataParser(ParseUnSignedInteger), new CBORDataPacker(PackUnSignedInteger));
        public static CBORDataType NegativeInteger = new CBORDataType(0b00100000, "netative integer", new CBORDataParser(ParseNegativeInteger), new CBORDataPacker(PackNegativeInteger));
        public static CBORDataType StringArray = new CBORDataType(0b01000000, "string array", new CBORDataParser(ParseStringArray), new CBORDataPacker(PackStringArray));
        public static CBORDataType StringText = new CBORDataType(0b01100000, "string text", new CBORDataParser(ParseStringText), new CBORDataPacker(PackStringText));
        public static CBORDataType ByteArray = new CBORDataType(0b10000000, "byte array", new CBORDataParser(ParseByteArray), new CBORDataPacker(PackByteArray));
        public static CBORDataType KeyPair = new CBORDataType(0b10100000, "map key/pair", new CBORDataParser(ParseKeyPair), new CBORDataPacker(PackKeyPair));
        public static CBORDataType TagItem = new CBORDataType(0b11000000, "tag item", new CBORDataParser(ParseTagItem), new CBORDataPacker(ParseTagItem));
        public static CBORDataType SimpleFloat = new CBORDataType(0b11100000, "simple float", new CBORDataParser(ParseSimpleFloat), new CBORDataPacker(PackSimpleFloat));

        public CBORDataParser Parser { get { return _parser; } }
        public CBORDataPacker Packer { get { return _packer; } }

        public CBORDataType(byte header, string dt, CBORDataParser parser, CBORDataPacker packer)
        {
            _header = header;
            _typeName = dt;
            _packer = packer;
            _parser = parser;
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

        private static byte[] PackUnSignedInteger(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }

        private static CBORDataInfo ParseUnSignedInteger(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            byte head = data[0];
            di.Data = data;

            di.DataType = Parse(data[0]);

            //低5位
            byte indicator = (byte)(head & 00011111);
            //uint8
            if (indicator <= 23)
            {
                di.Value = indicator;
                di.Length = 1;
                //uint8
            }
            else if (indicator == 24)
            {
                di.Value = data[0];
                di.Length = 1;

            }//uint16
            else if (indicator == 25)
            {
                di.Value = BitConverter.ToUInt16(data.Revert(), 0);
                di.Length = 2;
                //uint32
            }
            else if (indicator == 26)
            {
                di.Value = BitConverter.ToUInt32(data.Revert(), 0);
                di.Length = 4;
                //uint64
            }
            else if (indicator == 27)
            {
                di.Value = BitConverter.ToUInt64(data.Revert(), 0);
                di.Length = 8;
            }
            return di;
        }

        private static byte[] PackNegativeInteger(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }

        private static CBORDataInfo ParseNegativeInteger(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            byte head = data[0];
            di.Data = data;

            di.DataType = Parse(data[0]);

            //低5位
            byte indicator = (byte)(head & 00011111);
            if (indicator <= 23)
            {
                di.Value = (sbyte)(-1 - indicator);
                di.Length = 1;
            }
            //uint8
            else if (indicator == 24)
            {
                di.Value = (sbyte)(-1 - data[0]);
                di.Length = 1;
            }//uint16+
            else if (indicator == 25)
            {
                di.Value = (int)(-1 - BitConverter.ToUInt16(data.Revert(), 0));
                di.Length = 2;
            }//uint32
            else if (indicator == 26)
            {
                di.Value = (long)(-1 - BitConverter.ToUInt32(data.Revert(), 0));
                di.Length = 4;
            }
            //uint64 此处会出现溢出，RFC文档并没有讲清楚这一点，属于设计漏洞
            else if (indicator == 27)
            {
                di.Value = (long)(-1 - BitConverter.ToUInt32(data.Revert(), 0));
                di.Length = 8;
            }
            return di;
        }
        private static byte[] PackStringArray(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }

        private static CBORDataInfo ParseStringArray(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            byte head = data[0];
            di.Data = data;

            di.DataType = Parse(data[0]);

            //低5位
            byte indicator = (byte)(head & 00011111);
            long lenArr = 0;
            int offset = 0;
            if (indicator <= 23)
            {
                lenArr = 0;
                offset = 0;
            }
            else if (indicator == 24)
            {
                lenArr = data[1];
                offset = 1;
            }
            else if (indicator == 25)
            {
                lenArr = BitConverter.ToUInt16(data, 1);
                offset = 2;
            }
            else if (indicator == 26)
            {
                lenArr = BitConverter.ToUInt32(data, 1);
                offset = 4;
            }
            else if (indicator == 27)
            {

                lenArr = (long)BitConverter.ToUInt64(data, 1);
                offset = 8;
            }
            else if (indicator == 31)
            {
                //查找结束符号
                lenArr = Array.LastIndexOf(data, 0xff);
                lenArr = lenArr - 1;
                di.IsIndefinite = true;
            }
            di.Length = lenArr;
            //hex字符串 h''
            di.Value = "h'" + Hex.To(data, offset + 1, (int)lenArr) + "'";
            return di;
        }
        private static byte[] PackStringText(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }

        private static CBORDataInfo ParseStringText(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            byte head = data[0];
            di.Data = data;

            di.DataType = Parse(data[0]);

            //低5位
            byte indicator = (byte)(head & 00011111);
            long lenArr = 0;
            int offset = 0;
            if (indicator <= 23)
            {
                lenArr = 0;
                offset = 0;
            }
            else if (indicator == 24)
            {
                lenArr = data[1];
                offset = 1;
            }
            else if (indicator == 25)
            {
                lenArr = BitConverter.ToUInt16(data, 1);
                offset = 2;
            }
            else if (indicator == 26)
            {
                lenArr = BitConverter.ToUInt32(data, 1);
                offset = 4;
            }
            else if (indicator == 27)
            {
                lenArr = (long)BitConverter.ToUInt64(data, 1);
                offset = 8;
            }
            else if (indicator == 31)
            {
                //查找结束符号
                lenArr = Array.LastIndexOf(data, 0xff);
                lenArr--;
                di.IsIndefinite = true;
            }
            di.Length = lenArr;
            //字符串 
            di.Value = StringEncoder.Decode(data, offset + 1, (int)lenArr);
            return di;
        }
        private static CBORDataInfo ParseByteArray(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();

            di.Data = data;
            di.DataType = Parse(data[0]);

            List<int[]> value = new List<int[]>();
            int offset = 0;
            while (offset < data.Length)
            {
                byte head = data[offset];
                //低5位
                byte indicator = (byte)(head & 00011111);
                long lenArr = 0;
                if (indicator <= 23)
                {
                    lenArr = 0;
                    offset = 0;
                }
                else if (indicator == 24)
                {
                    lenArr = data[1];
                    offset = 1;
                }
                else if (indicator == 25)
                {
                    lenArr = BitConverter.ToUInt16(data, 1);
                    offset = 2;
                }
                else if (indicator == 26)
                {
                    lenArr = BitConverter.ToUInt32(data, 1);
                    offset = 4;
                }
                else if (indicator == 27)
                {
                    lenArr = (long)BitConverter.ToUInt64(data, 1);
                    offset = 8;
                }
                else if (indicator == 31)
                {
                    //查找结束符号
                    lenArr = Array.IndexOf(data, 0xff);
                    lenArr = lenArr - 1;
                    di.IsIndefinite = true;
                }
                int[] item = new int[lenArr];
                for (int i = 0; i < lenArr; i++)
                {
                    //最多可表示 254
                    item[i] = data[offset + 1 + i];
                }
                value.Add(item);
                offset = (int)(offset + lenArr) + 1;
            }
            di.Value = value.ToArray();
            di.Length = value.Count;
            return di;
        }

        private static byte[] PackByteArray(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }
        private static byte[] PackKeyPair(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }

        private static CBORDataInfo ParseKeyPair(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();

            di.Data = data;
            di.DataType = Parse(data[0]);

            Dictionary<object, object> list = new Dictionary<object, object>();
            int offset = 0;
            byte head = data[0];
            //低5位
            byte indicator = (byte)(head & 00011111);
            long lenArr = 0;
            if (indicator <= 23)
            {
                lenArr = 0;
                offset = 0;
            }
            else if (indicator == 24)
            {
                lenArr = data[1];
                offset = 1;
            }
            else if (indicator == 25)
            {
                lenArr = BitConverter.ToUInt16(data, 1);
                offset = 2;
            }
            else if (indicator == 26)
            {
                lenArr = BitConverter.ToUInt32(data, 1);
                offset = 4;
            }
            else if (indicator == 27)
            {
                lenArr = (long)BitConverter.ToUInt64(data, 1);
                offset = 8;
            }
            else if (indicator == 31)
            {
                //查找结束符号
                lenArr = Array.IndexOf(data, 0xff);
                lenArr = lenArr - 1;
                di.IsIndefinite = true;
            }
            //符合类型
            for (int i = 0; i < lenArr; i++)
            {

            }
            di.Value = list;
            di.Length = list.Count;
            return di;
        }
        private static byte[] ParseTagItem(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }

        private static CBORDataInfo ParseTagItem(byte[] data)
        {
            throw new NotImplementedException();
        }
        private static byte[] PackSimpleFloat(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }

        private static CBORDataInfo ParseSimpleFloat(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            byte head = data[0];
            di.Data = data;

            //di.DataType = Parse(data[0]);

            ////低5位
            //byte indicator = (byte)(head & 00011111);
            ////uint8
            //if (indicator <= 19)
            //{
            //    di.Value = indicator;
            //    di.Length = 1;
            //    //uint8
            //}
            //else if (indicator == 20)
            //{
            //    di.Value = false;
            //    di.Length = 1;

            //}
            //else if (indicator == 21)
            //{
            //    di.Value = true;
            //    di.Length = 1;
            //}
            //else if (indicator == 22)
            //{
            //    di.Value = null;
            //    di.Length = 1;
            //}
            //else if (indicator == 23)
            //{
            //    //undefined
            //    di.Value =Undefined.Value;
            //    di.Length = 1;
            //}
            //else if (indicator == 24)
            //{
            //    //byte
            //    di.Value = data[1];
            //    di.Length = 1;
            //}
            //else if (indicator == 25)
            //{
            //    //half float->single float
            //    byte[] arrFloat = new byte[2];
            //    di.Value =
            //    di.Length = 1;
            //}
            //else if (indicator == 26)
            //{
            //    //single float
            //    byte[] arrFloat = new byte[4];
            //    di.Value =
            //    di.Length = 1;
            //}
            //else if (indicator == 27)
            //{
            //    //double float
            //    byte[] arrFloat = new byte[8];
            //    di.Value =
            //    di.Length = 1;
            //}
            return di;
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
    internal class CBORDataInfo
    {
        public CBORDataType DataType { get; set; }
        public long Length { get; set; }
        public object Value { get; set; }
        public byte[] Data { get; set; }
        /// <summary>
        /// 是否无限
        /// </summary>
        public bool IsIndefinite { get; set; }
    }

    //.Net4不支持半精度类型，故需要另行实现

    /// <summary>
    /// 浮点型解码器
    /// </summary>
    public class FloatEncoder
    {
        /// <summary>
        /// 半精度
        /// </summary>
        /// <param name="data">2 bytes</param>
        /// <returns></returns>
        public static double DecodeHalf(byte[] data)
        {
            uint half = (uint)((data[0] << 8) + data[1]);
            uint exp = (half >> 10) & 0x1f;
            uint mant = half & 0x3ff;
            double val;
            if (exp == 0) 
                val = mant*Math.Pow(2,-24);
            else if (exp != 31) 
                val = (mant + 1024)*Math.Pow(2, exp - 25);
            else val = mant == 0 ? double.PositiveInfinity : double.NaN;
            return (half & 0x8000) == 0x8000 ? -val : val;
        }

        public static byte[] EncodeHalf(float half)
        {

        }
        /// <summary>
        /// 单精度
        /// </summary>
        /// <param name="data">4 bytes</param>
        /// <returns></returns>
        public static float DecodeSingle(byte[] data)
        {

        }

        public static byte[] EncodeSingle(float data)
        {

        }
        /// <summary>
        /// 双精度
        /// </summary>
        /// <param name="data">8 bytes</param>
        /// <returns></returns>
        public static double DecodeDouble(byte[] data)
        {

        }

        public static byte[] EncodeDouble(double data)
        {

        }
    }

    public static class MathEx
    {
        public static double LDExp(Math t,double value,int exponent)
        {
            return value * Math.Pow(2, exponent);
        }
    }
}
