using Mozi.Encode.Generic;
using System;
using System.Collections.Generic;

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
    /// 数据序列化抽象类
    /// </summary>
    public abstract class CBORDataSerializer
    {
        /// <summary>
        /// 数据类型，这个类型是预定义的，与实际数据值没有关系
        /// </summary>
        public CBORDataType DataType { get; set; }
        /// <summary>
        /// 转字节流
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract byte[] Pack(CBORDataInfo data);
        /// <summary>
        /// 解析字节流
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract CBORDataInfo Parse(byte[] data);
        /// <summary>
        /// 转为带特征描述符号的字符串
        /// </summary>
        /// <returns></returns>
        public abstract string ToString(CBORDataInfo di);
    }
    /// <summary>
    /// 正整数解码编码器
    /// </summary>
    public class UnsignedIntegerSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORDataInfo info)
        {
            byte[] data;
            object value = info.Value;
            ulong valueReal = ulong.Parse(value.ToString());

            if (valueReal <= 23)
            {
                data = new byte[1];
                data[0] = (byte)(info.DataType.Header | (byte)valueReal);
            }
            else if(valueReal<byte.MaxValue)
            {
                data = new byte[2];
                data[0] = (byte)(info.DataType.Header | 24);
                data[1] = (byte)valueReal;
            }
            else if (valueReal<ushort.MaxValue)
            {
                data = new byte[3];
                data[0] = (byte)(info.DataType.Header | 25);
                Array.Copy(BitConverter.GetBytes((ushort)(valueReal)).Revert(), 0, data, 1, 2);
            }
            else if (valueReal< uint.MaxValue)
            {
                data = new byte[5];
                data[0] = (byte)(info.DataType.Header | 26);
                Array.Copy(BitConverter.GetBytes((uint)(valueReal)).Revert(), 0, data, 1, 4);
            }
            else
            {
                data = new byte[9];
                data[0] = (byte)(info.DataType.Header | 27);
                Array.Copy(BitConverter.GetBytes((ulong)(valueReal)).Revert(), 0, data, 1, 8);
            }
            return data;
        }

        public override CBORDataInfo Parse(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;
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

        public override string ToString(CBORDataInfo di)
        {
            return di.Value.ToString();
        }
    }
    /// <summary>
    /// 负整数解码编码器 
    /// </summary>
    public class NegativeIntegerSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORDataInfo info)
        {
            byte[] data;
            object value = info.Value;
            Int64 valueReal = Int64.Parse(value.ToString());
            UInt64 valueRevert = (ulong)Math.Abs(valueReal + 1);
            int len = 0;
            if (valueRevert<=23)
            {
                data = new byte[1];
                data[0] = (byte)(DataType.Header | (byte)valueRevert);
            }else if (valueRevert <byte.MaxValue){
                data = new byte[2];
                data[0] = (byte)(DataType.Header | (byte)24);
                data[1] = (byte)valueRevert;
            }
            else if (valueRevert < ushort.MaxValue){
                data = new byte[3];
                data[0] = (byte)(DataType.Header | (byte)25);
                len = 2;
                Array.Copy(BitConverter.GetBytes(valueRevert).Revert(), 6, data, 1, 2);
            }
            else if (valueRevert < UInt32.MaxValue){
                data = new byte[5];
                data[0] = (byte)(DataType.Header | (byte)26);
                len = 4;
                Array.Copy(BitConverter.GetBytes(valueRevert).Revert(), 4, data, 1, 4);
            }
            else
            {
                data = new byte[9];
                data[0] = (byte)(DataType.Header | (byte)27);
                len = 8;
                Array.Copy(BitConverter.GetBytes(valueRevert).Revert(), 0, data, 1, 8);
            }
            return data;

        }

        public override CBORDataInfo Parse(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;
            //低5位
            byte indicator = (byte)(head & 0b00011111);
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

        public override string ToString(CBORDataInfo di)
        {
            return di.Value.ToString();
        }
    }
    /// <summary>
    /// Hex解码编码器 Uint64，Uint32长度在c# 实现中意义不大
    /// </summary>
    public class StringArraySerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORDataInfo info)
        {
            byte[] data;
            if (info.IsIndefinite)
            {
                //data = new byte[lenPayload + 2];
                //data[0] = (byte)(info.DataType.Header | 31);
                //data[data.Length - 1] = 0xff;
                //Array.Copy(payload, 0, data, 1, lenPayload);
                List<byte> payload = new List<byte>();
                payload.Add((byte)(info.DataType.Header | 31));
                foreach(CBORDataInfo di in (CBORDataInfo[])info.Value)
                {
                    byte[] item = Pack(di);
                    payload.AddRange(item);
                }
                payload.Add(0xff);
                data = payload.ToArray();
            }
            else
            {
                byte[] payload = Hex.From((string)info.Value);
                int lenPayload = payload.Length;
                if (lenPayload <= 23)
                {
                    data = new byte[1 + lenPayload];
                    data[0] = (byte)(info.DataType.Header | lenPayload);
                    Array.Copy(payload, 0, data, 1, payload.Length);
                }
                else if (lenPayload < byte.MaxValue) 
                {
                    data = new byte[1 +1+ lenPayload];
                    data[0] = (byte)(info.DataType.Header | 24);
                    data[1] = (byte)payload.Length;
                    Array.Copy(payload, 0, data, 2, payload.Length);
                }
                else if (lenPayload < ushort.MaxValue) {
                    data = new byte[1 + 2 + lenPayload];
                    data[0] = (byte)(info.DataType.Header | 25);
                    Array.Copy(BitConverter.GetBytes((ushort)lenPayload).Revert(), 0, data,1,2);
                    Array.Copy(payload, 0, data, 3, payload.Length);
                }
                //Uint32 uint64转换无效 仅仅是为了配合文档规范
                else if (lenPayload < uint.MaxValue) {
                    data = new byte[1 + 4 + lenPayload];
                    data[0] = (byte)(info.DataType.Header | 26);
                    Array.Copy(BitConverter.GetBytes((UInt32)lenPayload).Revert(), 0, data, 1, 4);
                    Array.Copy(payload, 0, data, 5, payload.Length);
                }
                else
                {
                    data = new byte[1 + 8 + lenPayload];
                    data[0] = (byte)(info.DataType.Header | 27);
                    Array.Copy(BitConverter.GetBytes((UInt64)lenPayload).Revert(), 0, data, 1, 8);
                    Array.Copy(payload, 0, data, 8, payload.Length);
                }
            }

            return data;
        }

        public override CBORDataInfo Parse(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;
            //低5位
            byte indicator = (byte)(head & 0b00011111);
            long lenArr = 0;
            int offset = 0;
            if (indicator != 31)
            {
                if (indicator <= 23)
                {
                    lenArr = indicator;
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
                di.Length = lenArr;
                //hex字符串 h''
                di.Value = Hex.To(data, offset + 1, (int)lenArr);
            }
            else
            {
                di.IsIndefinite = true;
                List<CBORDataInfo> infValue = new List<CBORDataInfo>();
                
                offset++;
                while (data[offset] != 0xff)
                {
                    byte digit = data[offset];
                    if ((digit & DataType.Header) == DataType.Header)
                    {
                        List<byte>  itemData = new List<byte> { digit };
                        offset++;
                        while (((data[offset] & DataType.Header) != DataType.Header)& data[offset] != 0xff)
                        {
                            itemData.Add(data[offset]);
                            offset++;
                        }

                        CBORDataInfo info = Parse(itemData.ToArray());
                        infValue.Add(info);
                    }
                    else
                    {
                        offset++;
                    }
                }

                di.Length = infValue.Count;
                di.Value = infValue.ToArray();
            }
            return di;
        }

        public override string ToString(CBORDataInfo di)
        {
            if (di.Value is string)
            {
                return "h'" + di.Value.ToString() + "'";
            }
            // array of hex string
            else
            {
                List<string> items = new List<string>();
                foreach(var r in (CBORDataInfo[])di.Value)
                {
                    items.Add(r.ToString());
                }
                return "(_ " + string.Join(",",items)+")";
            }
        }
    }
    /// <summary>
    /// UTF-8字符串解码编码器  Uint64，Uint32长度在c# 实现中意义不大
    /// </summary>
    public class StringTextSerialzier : CBORDataSerializer
    {
        public override byte[] Pack(CBORDataInfo info)
        {
            byte[] data;
            if (info.IsIndefinite)
            {
                List<byte> payload = new List<byte>();
                payload.Add((byte)(info.DataType.Header | 31));
                foreach (CBORDataInfo di in (CBORDataInfo[])info.Value)
                {
                    byte[] item = Pack(di);
                    payload.AddRange(item);
                }
                payload.Add(0xff);
                data = payload.ToArray();
            }
            else
            {
                byte[] payload = StringEncoder.Encode((string)info.Value);
                int lenPayload = payload.Length;
                if (lenPayload <= 23)
                {
                    data = new byte[lenPayload + 1];
                    data[0] = (byte)(info.DataType.Header | (byte)lenPayload);
                    Array.Copy(payload, 0, data, 1, lenPayload);
                }
                else if (lenPayload < byte.MaxValue)
                {
                    data = new byte[lenPayload + 1+1];
                    data[0] = (byte)(info.DataType.Header | (byte)24);
                    data[1] =  (byte)lenPayload;
                    Array.Copy(payload, 0, data, 2, lenPayload);
                }
                else if (lenPayload < ushort.MaxValue){
                    data = new byte[lenPayload + 1 + 2];
                    data[0] = (byte)(info.DataType.Header | (byte)25);
                    Array.Copy(BitConverter.GetBytes((ushort)lenPayload).Revert(), 0, data, 1, 2);
                    Array.Copy(payload, 0, data, 3, lenPayload);
                }
                else if (lenPayload < UInt32.MaxValue)
                {
                    data = new byte[lenPayload + 1 + 4];
                    data[0] = (byte)(info.DataType.Header | (byte)26);
                    Array.Copy(BitConverter.GetBytes((UInt32)lenPayload).Revert(), 0, data, 1, 4);
                    Array.Copy(payload, 0, data, 5, lenPayload);
                }
                else
                {
                    data = new byte[lenPayload + 1 + 8];
                    data[0] = (byte)(info.DataType.Header | (byte)27);
                    Array.Copy(BitConverter.GetBytes((UInt64)lenPayload).Revert(), 0, data, 1, 8);
                    Array.Copy(payload, 0, data, 9, lenPayload);
                }
            }
            return data;
        }

        public override CBORDataInfo Parse(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;

            //低5位
            byte indicator = (byte)(head & 0b00011111);
            long lenArr = 0;
            int offset = 0;

            if (indicator != 31)
            {
                if (indicator <= 23)
                {
                    lenArr = indicator;
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
            }
            else
            {
                di.IsIndefinite = true;
                List<CBORDataInfo> infValue = new List<CBORDataInfo>();

                offset++;
                while (data[offset] != 0xff)
                {
                    byte digit = data[offset];
                    if ((digit & DataType.Header) == DataType.Header)
                    {
                        List<byte> itemData = new List<byte> { digit };
                        offset++;
                        while (((data[offset] & DataType.Header) != DataType.Header) & data[offset] != 0xff)
                        {
                            itemData.Add(data[offset]);
                            offset++;
                        }

                        CBORDataInfo info = Parse(itemData.ToArray());
                        infValue.Add(info);
                    }
                    else
                    {
                        offset++;
                    }
                }

                di.Length = infValue.Count;
                di.Value = infValue.ToArray();
            }
            return di;
        }

        public override string ToString(CBORDataInfo di)
        {
            if (di.Value is string)
            {
                return "\"" + di.Value.ToString() + "\"";
            }
            // array of hex string
            else
            {
                List<string> items = new List<string>();
                foreach (var r in (CBORDataInfo[])di.Value)
                {
                    items.Add(r.ToString());
                }
                return "(_ " + string.Join(",", items) + ")";
            }
        }
    }
    /// <summary>
    /// 数组解码编码器
    /// </summary>
    public class ArraySerializer : CBORDataSerializer
    {
        public override CBORDataInfo Parse(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            di.DataType = DataType;
            di.Data = data;

            List<int[]> value = new List<int[]>();
            int offset = 0;
            while (offset < data.Length)
            {
                byte head = data[offset];
                //低5位
                byte indicator = (byte)(head & 0b00011111);
                long lenArr = 0;
                if (indicator <= 23)
                {
                    lenArr = indicator;
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

        public override byte[] Pack(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }

        public override string ToString(CBORDataInfo di)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 键值对解码编码器
    /// </summary>
    public class KeyPairSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }

        public override CBORDataInfo Parse(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            di.DataType = DataType;
            di.Data = data;

            Dictionary<object, object> list = new Dictionary<object, object>();
            int offset = 0;
            byte head = data[0];
            //低5位
            byte indicator = (byte)(head & 0b00011111);
            long lenArr = 0;
            if (indicator <= 23)
            {
                lenArr = indicator;
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

        public override string ToString(CBORDataInfo di)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 标签项解码编码器
    /// </summary>
    public class TagItemSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }

        public override CBORDataInfo Parse(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override string ToString(CBORDataInfo di)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 简单类型，浮点型解码编码器
    /// </summary>
    public class SimpleFloatSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORDataInfo data)
        {
            throw new NotImplementedException();
        }

        public override CBORDataInfo Parse(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;

            //di.DataType = Parse(data[0]);

            ////低5位
            //byte indicator = (byte)(head & 0b00011111);
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

        public override string ToString(CBORDataInfo di)
        {
            throw new NotImplementedException();
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

        public static CBORDataType UnsignedInteger = new CBORDataType(0b00000000, "unsigned integer", new UnsignedIntegerSerializer() );
        public static CBORDataType NegativeInteger = new CBORDataType(0b00100000, "netative integer", new NegativeIntegerSerializer());
        public static CBORDataType StringArray = new CBORDataType(0b01000000, "string array", new StringArraySerializer());
        public static CBORDataType StringText = new CBORDataType(0b01100000, "string text", new StringTextSerialzier() );
        public static CBORDataType ByteArray = new CBORDataType(0b10000000, "byte array", new ArraySerializer() );
        public static CBORDataType KeyPair = new CBORDataType(0b10100000, "map key/pair", new KeyPairSerializer());
        public static CBORDataType TagItem = new CBORDataType(0b11000000, "tag item", new TagItemSerializer() );
        public static CBORDataType SimpleFloat = new CBORDataType(0b11100000, "simple float", new SimpleFloatSerializer() );

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
        public CBORDataType DataType { get; set; }
        public long Length { get; set; }
        public object Value { get; set; }
        public byte[] Data { get; set; }
        /// <summary>
        /// 是否无限
        /// </summary>
        public bool IsIndefinite { get; set; }

        public CBORDataInfo(CBORDataType dataType,object value)
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
            throw new NotImplementedException();
        }
        /// <summary>
        /// 单精度
        /// </summary>
        /// <param name="data">4 bytes</param>
        /// <returns></returns>
        public static float DecodeSingle(byte[] data)
        {
            throw new NotImplementedException();
        }

        public static byte[] EncodeSingle(float data)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 双精度
        /// </summary>
        /// <param name="data">8 bytes</param>
        /// <returns></returns>
        public static double DecodeDouble(byte[] data)
        {
            throw new NotImplementedException();
        }

        public static byte[] EncodeDouble(double data)
        {
            throw new NotImplementedException();
        }
    }

}
