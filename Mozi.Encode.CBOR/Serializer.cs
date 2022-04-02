using System;
using System.Collections;
using System.Collections.Generic;
using Mozi.Encode.CBOR;
using Mozi.Encode.Generic;

namespace Mozi.Encode
{
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
        public abstract byte[] Pack(CBORData info);
        /// <summary>
        /// 解析字节流
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract CBORData Parse(byte[] data);
        /// <summary>
        /// 转为带特征描述符号的字符串
        /// </summary>
        /// <returns></returns>
        public abstract string ToString(CBORData di);

    }
    /// <summary>
    /// 正整数解码编码器
    /// </summary>
    internal class UnsignedIntegerSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORData info)
        {
            byte[] data;
            object value = info.Value;
            ulong valueReal = ulong.Parse(value.ToString());

            if (valueReal <= 23)
            {
                data = new byte[1];
                data[0] = (byte)(info.DataType.Header | (byte)valueReal);
            }
            else if (valueReal < byte.MaxValue)
            {
                data = new byte[2];
                data[0] = (byte)(info.DataType.Header | 24);
                data[1] = (byte)valueReal;
            }
            else if (valueReal < ushort.MaxValue)
            {
                data = new byte[3];
                data[0] = (byte)(info.DataType.Header | 25);
                Array.Copy(BitConverter.GetBytes((ushort)(valueReal)).Revert(), 0, data, 1, 2);
            }
            else if (valueReal < uint.MaxValue)
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

        public override CBORData Parse(byte[] data)
        {
            CBORData di = new CBORData();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;
            //低5位
            byte indicator = (byte)(head & 0b00011111);
            di.Indicator = indicator;
            //uint8
            if (indicator <= 23)
            {
                di.Value = indicator;
                di.Length = 1;
                di.PackSize = 1;
                //uint8
            }
            else if (indicator == 24)
            {
                di.Value = data[0];
                di.Length = 1;
                di.PackSize = 2;

            }//uint16
            else if (indicator == 25)
            {
                di.Value = BitConverter.ToUInt16(data.Revert(), 0);
                di.Length = 2;
                di.PackSize = 3;
                //uint32
            }
            else if (indicator == 26)
            {
                di.Value = BitConverter.ToUInt32(data.Revert(), 0);
                di.Length = 4;
                di.PackSize = 5;
                //uint64
            }
            else if (indicator == 27)
            {
                di.Value = BitConverter.ToUInt64(data.Revert(), 0);
                di.Length = 8;
                di.PackSize = 9;
            }
            return di;
        }

        public override string ToString(CBORData di)
        {
            return di.Value.ToString();
        }
    }
    /// <summary>
    /// 负整数解码编码器 
    /// </summary>
    internal class NegativeIntegerSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORData info)
        {
            byte[] data;
            object value = info.Value;
            long valueReal = long.Parse(value.ToString());
            ulong valueRevert = (ulong)Math.Abs(valueReal + 1);
            int len = 0;
            if (valueRevert <= 23)
            {
                data = new byte[1];
                data[0] = (byte)(DataType.Header | (byte)valueRevert);
                
            }
            else if (valueRevert < byte.MaxValue)
            {
                data = new byte[2];
                data[0] = (byte)(DataType.Header | (byte)24);
                data[1] = (byte)valueRevert;
            }
            else if (valueRevert < ushort.MaxValue)
            {
                data = new byte[3];
                data[0] = (byte)(DataType.Header | (byte)25);
                len = 2;
                Array.Copy(BitConverter.GetBytes(valueRevert).Revert(), 6, data, 1, 2);
            }
            else if (valueRevert < uint.MaxValue)
            {
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

        public override CBORData Parse(byte[] data)
        {
            CBORData di = new CBORData();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;
            //低5位
            byte indicator = (byte)(head & 0b00011111);
            di.Indicator = indicator;
            if (indicator <= 23)
            {
                di.Value = (sbyte)(-1 - indicator);
                di.Length = 1;
                di.PackSize = 1;
            }
            //uint8
            else if (indicator == 24)
            {
                di.Value = (sbyte)(-1 - data[0]);
                di.Length = 1;
                di.PackSize = 2;
            }//uint16+
            else if (indicator == 25)
            {
                di.Value = -1 - BitConverter.ToUInt16(data.Revert(), 0);
                di.Length = 2;
                di.PackSize = 3;
            }//uint32
            else if (indicator == 26)
            {
                di.Value = -1 - BitConverter.ToUInt32(data.Revert(), 0);
                di.Length = 4;
                di.PackSize = 5;
            }
            //uint64 此处会出现溢出，RFC文档并没有讲清楚这一点，属于设计漏洞
            else if (indicator == 27)
            {
                di.Value = -1 - BitConverter.ToUInt32(data.Revert(), 0);
                di.Length = 8;
                di.PackSize = 9;
            }
            return di;
        }

        public override string ToString(CBORData di)
        {
            return di.Value.ToString();
        }
    }
    /// <summary>
    /// Hex解码编码器 Uint64，Uint32长度在c# 实现中意义不大
    /// </summary>
    internal class StringArraySerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORData info)
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
                foreach (CBORData di in (CBORData[])info.Value)
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
                    data = new byte[1 + 1 + lenPayload];
                    data[0] = (byte)(info.DataType.Header | 24);
                    data[1] = (byte)payload.Length;
                    Array.Copy(payload, 0, data, 2, payload.Length);
                }
                else if (lenPayload < ushort.MaxValue)
                {
                    data = new byte[1 + 2 + lenPayload];
                    data[0] = (byte)(info.DataType.Header | 25);
                    Array.Copy(BitConverter.GetBytes((ushort)lenPayload).Revert(), 0, data, 1, 2);
                    Array.Copy(payload, 0, data, 3, payload.Length);
                }
                //Uint32 uint64转换无效 仅仅是为了配合文档规范
                else if (lenPayload < uint.MaxValue)
                {
                    data = new byte[1 + 4 + lenPayload];
                    data[0] = (byte)(info.DataType.Header | 26);
                    Array.Copy(BitConverter.GetBytes((uint)lenPayload).Revert(), 0, data, 1, 4);
                    Array.Copy(payload, 0, data, 5, payload.Length);
                }
                else
                {
                    data = new byte[1 + 8 + lenPayload];
                    data[0] = (byte)(info.DataType.Header | 27);
                    Array.Copy(BitConverter.GetBytes((ulong)lenPayload).Revert(), 0, data, 1, 8);
                    Array.Copy(payload, 0, data, 8, payload.Length);
                }
            }

            return data;
        }

        public override CBORData Parse(byte[] data)
        {
            CBORData di = new CBORData();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;
            //低5位
            byte indicator = (byte)(head & 0b00011111);
            di.Indicator = indicator;
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
                di.PackSize = 1 + offset + lenArr;
            }
            else
            {
                di.IsIndefinite = true;
                List<CBORData> infValue = new List<CBORData>();

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
                        CBORData info = Parse(itemData.ToArray());
                        infValue.Add(info);
                        di.PackSize += info.PackSize;
                    }
                    else
                    {
                        offset++;
                    }
                }
                di.Length = infValue.Count;
                di.Value = infValue.ToArray();
                di.PackSize = di.PackSize + 1 + 1;//Header+Breaker
            }
            return di;
        }

        public override string ToString(CBORData di)
        {
            if (di.Value is string)
            {
                return "h'" + di.Value.ToString() + "'";
            }
            // array of hex string
            else
            {
                List<string> items = new List<string>();
                foreach (var r in (ICollection)di.Value)
                {
                    items.Add(r.ToString());
                }
                return "(_ " + string.Join(",", items) + ")";
            }
        }
    }
    /// <summary>
    /// UTF-8字符串解码编码器  Uint64，Uint32长度在c# 实现中意义不大
    /// </summary>
    internal class StringTextSerialzier : CBORDataSerializer
    {
        public override byte[] Pack(CBORData info)
        {
            byte[] data;
            if (info.IsIndefinite)
            {
                List<byte> payload = new List<byte>();
                payload.Add((byte)(info.DataType.Header | 31));
                foreach (CBORData di in (CBORData[])info.Value)
                {
                    byte[] item = Pack(di);
                    payload.AddRange(item);
                }
                payload.Add(0xff);
                data = payload.ToArray();
            }
            else
            {
                //TODO 此处应对UNICODE进行解码
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
                    data = new byte[lenPayload + 1 + 1];
                    data[0] = (byte)(info.DataType.Header | (byte)24);
                    data[1] = (byte)lenPayload;
                    Array.Copy(payload, 0, data, 2, lenPayload);
                }
                else if (lenPayload < ushort.MaxValue)
                {
                    data = new byte[lenPayload + 1 + 2];
                    data[0] = (byte)(info.DataType.Header | (byte)25);
                    Array.Copy(BitConverter.GetBytes((ushort)lenPayload).Revert(), 0, data, 1, 2);
                    Array.Copy(payload, 0, data, 3, lenPayload);
                }
                else if (lenPayload < uint.MaxValue)
                {
                    data = new byte[lenPayload + 1 + 4];
                    data[0] = (byte)(info.DataType.Header | (byte)26);
                    Array.Copy(BitConverter.GetBytes((uint)lenPayload).Revert(), 0, data, 1, 4);
                    Array.Copy(payload, 0, data, 5, lenPayload);
                }
                else
                {
                    data = new byte[lenPayload + 1 + 8];
                    data[0] = (byte)(info.DataType.Header | (byte)27);
                    Array.Copy(BitConverter.GetBytes((ulong)lenPayload).Revert(), 0, data, 1, 8);
                    Array.Copy(payload, 0, data, 9, lenPayload);
                }
            }
            return data;
        }

        public override CBORData Parse(byte[] data)
        {
            CBORData di = new CBORData();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;

            //低5位
            byte indicator = (byte)(head & 0b00011111);
            di.Indicator = indicator;
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
                //TODO 此处应对UNICODE进行编码
                //字符串 
                di.Value = StringEncoder.Decode(data, offset + 1, (int)lenArr);
                di.PackSize = 1 + offset + lenArr;
            }
            else
            {
                di.IsIndefinite = true;
                List<CBORData> infValue = new List<CBORData>();

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

                        CBORData info = Parse(itemData.ToArray());
                        infValue.Add(info);
                        di.PackSize += info.PackSize;
                    }
                    else
                    {
                        offset++;
                    }
                }

                di.Length = infValue.Count;
                di.Value = infValue.ToArray();
                di.PackSize += 2; //Header+Breaker
            }
            return di;
        }

        public override string ToString(CBORData di)
        {
            if (di.Value is string)
            {
                return "\"" + di.Value.ToString() + "\"";
            }
            // array of hex string
            else
            {
                List<string> items = new List<string>();
                foreach (var r in (CBORData[])di.Value)
                {
                    items.Add(r.ToString());
                }
                return "(_ " + string.Join(",", items) + ")";
            }
        }
    }
    /// <summary>
    /// 数组解码编码器 复合类型
    /// </summary>
    internal class ArraySerializer : CBORDataSerializer
    {
        public override CBORData Parse(byte[] data)
        {
            CBORData di = new CBORData();
            di.DataType = DataType;
            di.Data = data;

            List<CBORData> list = new List<CBORData>();
            long offset = 0;
            byte head = data[0];
            //低5位
            byte indicator = (byte)(head & 0b00011111);
            di.Indicator = indicator;
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
            }else if (indicator == 31){
                //TODO 会有问题吗？
                lenArr = Array.LastIndexOf(data, 0xff);
            }
            if (lenArr > 0)
            {
                for (int i = 0; i < lenArr; i++)
                {
                    var tag = data[offset + 1];
                    //结束符号
                    if (tag == 0xff)
                    {
                        di.PackSize = offset + 1 + 1;
                        di.IsIndefinite = true;
                        break;
                    }
                    else
                    {
                        byte[] fragKey = new byte[data.Length - offset - 1];
                        Array.Copy(data, offset + 1, fragKey, 0, fragKey.Length);
                        CBORData info = CBOREncoder.Decode(fragKey);

                        info.ClearRedundant();
                        offset += info.PackSize;
                        di.PackSize = offset + 1;
                        list.Add(info);
                    }
                }
            }
            di.Value = list;
            di.Length = list.Count;
            return di;
        }

        public override byte[] Pack(CBORData info)
        {
            var value = (List<CBORData>)info.Value;
            int length = value.Count;
            List<byte> payload = new List<byte>();
            if (info.IsIndefinite)
            {
                payload.Add((byte)(DataType.Header | 31));
            }
            else
            {
                if (length <= 23)
                {
                    payload.Add((byte)(DataType.Header | (byte)length));
                }
                else if (length == 24)
                {
                    payload.Add((byte)(DataType.Header | (byte)24));
                    payload.Add((byte)length);
                }
                else if (length == 25)
                {
                    payload.Add((byte)(DataType.Header | (byte)25));
                    payload.AddRange(BitConverter.GetBytes((ushort)length));
                }
                else if (length == 26)
                {
                    payload.Add((byte)(DataType.Header | (byte)26));
                    payload.AddRange(BitConverter.GetBytes((uint)length));
                }
                else if (length == 27)
                {
                    payload.Add((byte)(DataType.Header | (byte)27));
                    payload.AddRange(BitConverter.GetBytes((ulong)length));
                }
            }
            foreach (var i in value)
            {
                payload.AddRange(CBOREncoder.Encode(i));
            }
            if (info.IsIndefinite)
            {
                payload.Add(0xff);
            }
            return payload.ToArray();
        }

        public override string ToString(CBORData di)
        {
            List<string> info = new List<string>();
            foreach(var ci in (List<CBORData>)di.Value)
            {
                info.Add(ci.ToString());
            }
            return (di.IsIndefinite?"[_ ":"[") +string.Join(",",info)+ "]";
        }
    }
    /// <summary>
    /// 键值对解码编码器 复合类型
    /// </summary>
    internal class KeyPairSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORData info)
        {
            var value = (Dictionary<CBORData, CBORData>)info.Value;
            int length = value.Count;
            List<byte> payload = new List<byte>();
            if (info.IsIndefinite)
            {
                payload.Add((byte)(DataType.Header | 31));
            }
            else
            {
                if (length <= 23)
                {
                    payload.Add((byte)(DataType.Header | (byte)length));
                }
                else if (length == 24)
                {
                    payload.Add((byte)(DataType.Header | (byte)24));
                    payload.Add((byte)length);
                }
                else if (length == 25)
                {
                    payload.Add((byte)(DataType.Header | (byte)25));
                    payload.AddRange(BitConverter.GetBytes((ushort)length));
                }
                else if (length == 26)
                {
                    payload.Add((byte)(DataType.Header | (byte)26));
                    payload.AddRange(BitConverter.GetBytes((uint)length));
                }
                else if (length == 27)
                {
                    payload.Add((byte)(DataType.Header | (byte)27));
                    payload.AddRange(BitConverter.GetBytes((ulong)length));
                }
            }
            foreach(var i in value)
            {
                payload.AddRange(CBOREncoder.Encode(i.Key));
                if(!(i.Value is null))
                {
                    payload.AddRange(CBOREncoder.Encode(i.Value));
                }
            }
            if (info.IsIndefinite)
            {
                payload.Add(0xff);
            }
            return payload.ToArray();
        }

        public override CBORData Parse(byte[] data)
        {
            CBORData di = new CBORData();
            di.DataType = DataType;
            di.Data = data;

            Dictionary<CBORData, CBORData> list = new Dictionary<CBORData, CBORData>();
            long offset = 0;
            byte head = data[0];
            //低5位
            byte indicator = (byte)(head & 0b00011111);
            di.Indicator = indicator;
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
            }else if (indicator == 31)
            {
                //TODO 会有问题吗？
                lenArr = Array.LastIndexOf(data, 0xff);
            }
            if (lenArr > 0)
            {
                for (int i = 0; i < lenArr; i++)
                {
                    var tag = data[offset + 1];
                    byte[] fragKey = new byte[data.Length - offset-1];
                    Array.Copy(data, offset + 1, fragKey, 0, fragKey.Length);
                    CBORData info;
                    CBORData value;
                    bool isKeyonly = false;
                    //结束符号
                    if (tag == 0xff)
                    {
                        di.PackSize = offset + 1+1;
                        di.IsIndefinite = true;
                        break;
                    }
                    //嵌套
                    if ((tag & DataType.Header) == DataType.Header)
                    {
                        info = Parse(fragKey);
                        isKeyonly = true;
                    } else {
                        //字符串键名
                        if (tag >= 0x60)
                        {
                            var st = CBORDataType.StringText.Serializer;
                            info = st.Parse(fragKey);
                        }
                        //数字键名
                        else
                        {
                            var st = CBORDataType.UnsignedInteger.Serializer;
                            info = st.Parse(fragKey);
                        }
                    }
                    info.ClearRedundant();
                    offset += info.PackSize;
                    if (!isKeyonly)
                    {
                        //获取值
                        byte[] fragValue = new byte[data.Length - offset - 1];
                        Array.Copy(data, offset + 1, fragValue, 0, fragValue.Length);
                        value = CBOREncoder.Decode(fragValue);
                        value.ClearRedundant();
                    }
                    else
                    {
                        value = null;
                    }
                    offset += value != null ? value.PackSize : 0;
                    di.PackSize = offset + 1;
                    list.Add(info, value);
                }
            }
            di.Value = list;
            di.Length = list.Count;
            return di;
        }

        public override string ToString(CBORData di)
        {
            Dictionary<CBORData, CBORData> data = (Dictionary<CBORData, CBORData>)di.Value;
            List<string> ls = new List<string>();
            foreach(var r in data)
            {
                var key =r.Key.ToString();
                if (r.Value == null)
                {
                    ls.Add(key);
                }
                else
                {
                    string value = r.Value.ToString();
                    ls.Add(key + ":" + value);
                }
            }
            return (di.IsIndefinite ? "{_ " : "{" )+ string.Join(",", ls) + "}";
        }
    }
    /// <summary>
    /// 标记项类型解码编码器 复合类型
    /// </summary>
    internal class TagItemSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORData info)
        {
            List<byte> data = new List<byte>();
            byte[] payload;
            byte header = (byte)(DataType.Header| info.Indicator);
            byte indicator = info.Indicator;
            //TODO self described 55799
            payload = CBOREncoder.Encode((CBORData)info.Value);
            data.Add(header);
            if (payload != null)
            {
                data.AddRange(payload);
            }
            return data.ToArray();
        }

        public override CBORData Parse(byte[] data)
        {
            CBORData di = new CBORData();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;
            //低5位
            byte indicator = (byte)(head & 0b00011111);
            di.Indicator = indicator;
            //utc time
            if (indicator == 0)
            {
                var serialzier =CBORDataType.StringText.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }
            //unix timestamp
            else if (indicator == 1)
            {
                CBORDataSerializer serialzier;
                //正数
                if ((data[1] & CBORDataType.UnsignedInteger.Header) != CBORDataType.UnsignedInteger.Header)
                {
                    serialzier = CBORDataType.UnsignedInteger.Serializer;
                }
                //负数
                else if((data[1] & CBORDataType.NegativeInteger.Header) != CBORDataType.NegativeInteger.Header)
                {
                    serialzier = CBORDataType.NegativeInteger.Serializer;
                }
                else
                {
                    serialzier = CBORDataType.SimpleFloat.Serializer;
                }
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }
            //unsigned bignum
            else if (indicator == 2)
            {
                var serialzier =  CBORDataType.UnsignedInteger.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }
            //negative bignum
            else if (indicator == 3)
            {
                var serialzier = CBORDataType.UnsignedInteger.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }
            //decimal Fraction
            else if (indicator == 4)
            {
                var serialzier = CBORDataType.SimpleFloat.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }
            //big float
            else if(indicator==5)
            {
                var serialzier = CBORDataType.SimpleFloat.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            } //base64 url zero padding
            else if(indicator==21){

                var serialzier = CBORDataType.StringText.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }//base64 zero padding
            else if (indicator == 22){
                var serialzier = CBORDataType.StringText.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;

            }//base16 hex
            else if (indicator == 23){

                var serialzier = CBORDataType.DataArray.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }//byte string
            else if (indicator == 24)
            {
                var serialzier = CBORDataType.DataArray.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }
            //URIs
            else if (indicator == 32){

                var serialzier = CBORDataType.StringText.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }//base64 url RFC4648
            else if (indicator == 33)
            {
                var serialzier = CBORDataType.StringText.Serializer;
                //base64 encoded text RFC4648
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }
            else if (indicator == 34)
            {
                var serialzier = CBORDataType.StringText.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;

            }//regular expressions
            else if (indicator == 35)
            {
                var serialzier = CBORDataType.StringText.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;

            }//MIME
            else if (indicator == 36)
            {
                var serialzier = CBORDataType.StringText.Serializer;
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }//TODO self described 55799 0xd9d9f7
            else if(indicator==25&&data[1]==0xd9)
            {
                throw new NotImplementedException("Self-Described type is not supported until now");   
            }
            //0xc6..0xd4(tag)
            //0xd5..0xd7  expected conversion(data item follows; see Section 3.4.5.2)
            //0xd8..0xdb(more tags; 1 / 2 / 4 / 8 bytes of tag number and then a data item follow)
            return di;
        }

        public override string ToString(CBORData di)
        {
            return $"{di.Indicator}({di.Value})";
        }
    }
    /// <summary>
    /// 简单类型，浮点型解码编码器
    /// </summary>
    internal class SimpleFloatSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORData info)
        {
            byte[] data;
            if(info.Value is double)
            {
                data = new byte[9];
                data[0] = (byte)(info.DataType.Header | 27);
                byte[] value = BitConverter.GetBytes((double)info.Value).Revert();
                Array.Copy(value, 0, data, 1, value.Length);
            }else if(info.Value is float){
                data = new byte[5];
                data[0] = (byte)(info.DataType.Header | 26);
                byte[] value = BitConverter.GetBytes((float)info.Value).Revert();
                Array.Copy(value, 0, data, 1, value.Length);
            }else if(info.Value is HalfFloat){
                data = new byte[3];
                data[0] = (byte)(info.DataType.Header | 25);
                byte[] value = HalfFloat.Encode((HalfFloat)info.Value);
                Array.Copy(value, 0, data, 1, value.Length);
            }else if (info.Value is byte){
                data = new byte[2];
                data[0] = (byte)(info.DataType.Header | 24);
                data[1] = (byte)info.Value;
            }else if(info.Value is Undefined){
                data = new byte[1];
                data[0] = (byte)(info.DataType.Header | 23);
            }else if(info.Value is null){
                data = new byte[1];
                data[0] = (byte)(info.DataType.Header | 22);
            }else if (info.Value.Equals(true)){
                data = new byte[1];
                data[0] = (byte)(info.DataType.Header | 21);
            }else if (info.Value.Equals(false)){
                data = new byte[1];
                data[0] = (byte)(info.DataType.Header | 20);
            }
            else
            {
                byte value = byte.Parse(info.Value.ToString());
                if (value >= 0 && value <= 19)
                {
                    data = new byte[1];
                    data[0] = (byte)(info.DataType.Header | value);
                }
                //超范围的数据，截取为0
                else
                {
                    data = new byte[] { info.DataType.Header };
                }
            }
            return data;
        }

        public override CBORData Parse(byte[] data)
        {
            CBORData di = new CBORData();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;

            ////低5位
            byte indicator = (byte)(head & 0b00011111);
            di.Indicator = indicator;
            //uint8
            if (indicator <= 19)
            {
                di.Value = indicator;
                di.Length = 1;
                //uint8
                di.PackSize = 1;
            }
            else if (indicator == 20)
            {
                di.Value = false;
                di.Length = 1;
                di.PackSize = 1;
            }
            else if (indicator == 21)
            {
                di.Value = true;
                di.Length = 1;
                di.PackSize = 1;
            }
            else if (indicator == 22)
            {
                di.Value = null;
                di.Length = 1;
                di.PackSize = 1;
            }
            else if (indicator == 23)
            {
                //undefined
                di.Value = Undefined.Value;
                di.Length = 1;
                di.PackSize = 1;
            }
            else if (indicator == 24)
            {
                //byte
                di.Value = data[1];
                di.Length = 1;
                di.PackSize = 2;
            }
            else if (indicator == 25)
            {
                //half float->single float
                byte[] arrFloat = new byte[2];
                Array.Copy(data, 1, arrFloat, 0, 2);
                di.Value = HalfFloat.Decode(arrFloat);
                di.Length = 1;
                di.PackSize = 3;
            }
            else if (indicator == 26)
            {
                //single float
                byte[] arrFloat = new byte[4];
                Array.Copy(data, 1, arrFloat, 0, 4);
                Array.Reverse(arrFloat);
                di.Value = BitConverter.ToSingle(arrFloat, 0);
                di.Length = 1;
                di.PackSize = 5;
            }
            else if (indicator == 27)
            {
                //double
                byte[] arrFloat = new byte[8];
                Array.Copy(data, 1, arrFloat, 0, 8);
                Array.Reverse(arrFloat);
                di.Value = BitConverter.ToSingle(arrFloat, 0);
                di.Length = 1;
                di.PackSize = 9;
            }
            return di;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="di"></param>
        /// <returns></returns>
        public override string ToString(CBORData di)
        {
            if(di.Value is null)
            {
                return "null";
            }
            else if(di.Value is byte)
            {
                return "simple(" + di.Value.ToString() + ")";
            }else{
                return di.Value.ToString();
            }
        }
    }
    /// <summary>
    /// 特殊标记项类型
    /// </summary>
    public enum TagItemType
    {
        UTCTime = 0,
        UnixTimestamp = 1,
        UnsignedBigNum = 2,
        NegativeBigNum = 3,
        DecimalFraction = 4,
        BigFloat = 5,
        Base64UrlZeroPadding = 21,
        Base64 = 22,
        Base16Hex = 23,
        ByteString=24,
        Uris = 32,
        Base64Url = 33,
        Base64Text = 34,
        RegularExpression = 35,
        Mime = 36,
        SelfDescribe= 55799
    }
}
