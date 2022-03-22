﻿using Mozi.Encode.CBOR;
using Mozi.Encode.Generic;
using System;
using System.Collections.Generic;

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
    internal class UnsignedIntegerSerializer : CBORDataSerializer
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

        public override CBORDataInfo Parse(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            di.DataType = DataType;
            byte head = data[0];
            di.Data = data;
            //低5位
            byte indicator = (byte)(head & 00011111);
            di.Indicator = indicator;
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
    internal class NegativeIntegerSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORDataInfo info)
        {
            byte[] data;
            object value = info.Value;
            Int64 valueReal = Int64.Parse(value.ToString());
            UInt64 valueRevert = (ulong)Math.Abs(valueReal + 1);
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
            else if (valueRevert < UInt32.MaxValue)
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

        public override CBORDataInfo Parse(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
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
    internal class StringArraySerializer : CBORDataSerializer
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
                return "h'" + di.Value.ToString() + "'";
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
    /// UTF-8字符串解码编码器  Uint64，Uint32长度在c# 实现中意义不大
    /// </summary>
    internal class StringTextSerialzier : CBORDataSerializer
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
    /// 数组解码编码器 复合类型
    /// </summary>
    internal class ArraySerializer : CBORDataSerializer
    {
        public override CBORDataInfo Parse(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
            di.DataType = DataType;
            di.Data = data;

            //int[] value = new int[];
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
                //value.Add(item);
                offset = (int)(offset + lenArr) + 1;
            }
            //di.Value = value.ToArray();
            //di.Length = value.Count;
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
    /// 键值对解码编码器 复合类型
    /// </summary>
    internal class KeyPairSerializer : CBORDataSerializer
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
    /// 标签项解码编码器 复合类型
    /// </summary>
    internal class TagItemSerializer : CBORDataSerializer
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
            //低5位
            byte indicator = (byte)(head & 00011111);
            di.Indicator = indicator;
            //utc time
            if (indicator == 0)
            {
                StringTextSerialzier serialzier = new StringTextSerialzier();
                byte[] item = new byte[data.Length - 1];
                Array.Copy(data, 1, item, 0, item.Length);
                di.Value = serialzier.Parse(item);
                di.Length = 1;
            }
            //unix timestamp
            else if (indicator == 1)
            {
                di.Value = data[0];
                di.Length = 1;
            }
            //unsigned bignum
            else if (indicator == 2)
            {
                di.Value = BitConverter.ToUInt16(data.Revert(), 0);
                di.Length = 1;
            }
            //negative bignum
            else if (indicator == 3)
            {
                di.Value = BitConverter.ToUInt32(data.Revert(), 0);
                di.Length = 1;
            }
            //decimal Fraction
            else if (indicator == 4)
            {
                di.Value = BitConverter.ToUInt64(data.Revert(), 0);
                di.Length = 1;
            }
            //0xc5    bigfloat(data item "array" follows; see Section 3.4.4)
            //0xc6..0xd4(tag)
            //0xd5..0xd7  expected conversion(data item follows; see Section 3.4.5.2)
            //0xd8..0xdb(more tags; 1 / 2 / 4 / 8 bytes of tag number and then a data item follow)
            return di;
        }

        public override string ToString(CBORDataInfo di)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 简单类型，浮点型解码编码器
    /// </summary>
    internal class SimpleFloatSerializer : CBORDataSerializer
    {
        public override byte[] Pack(CBORDataInfo info)
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

        public override CBORDataInfo Parse(byte[] data)
        {
            CBORDataInfo di = new CBORDataInfo();
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
            }
            else if (indicator == 20)
            {
                di.Value = false;
                di.Length = 1;

            }
            else if (indicator == 21)
            {
                di.Value = true;
                di.Length = 1;
            }
            else if (indicator == 22)
            {
                di.Value = null;
                di.Length = 1;
            }
            else if (indicator == 23)
            {
                //undefined
                di.Value = Undefined.Value;
                di.Length = 1;
            }
            else if (indicator == 24)
            {
                //byte
                di.Value = data[1];
                di.Length = 1;
            }
            else if (indicator == 25)
            {
                //half float->single float
                byte[] arrFloat = new byte[2];
                di.Value = HalfFloat.Decode(arrFloat);
                di.Length = 1;
            }
            else if (indicator == 26)
            {
                //single float
                byte[] arrFloat = new byte[4];
                Array.Copy(data, 1, arrFloat, 0, 4);
                Array.Reverse(arrFloat);
                di.Value = BitConverter.ToSingle(arrFloat, 0);
                di.Length = 1;
            }
            else if (indicator == 27)
            {
                //double
                byte[] arrFloat = new byte[8];
                Array.Copy(data, 1, arrFloat, 0, 8);
                Array.Reverse(arrFloat);
                di.Value = BitConverter.ToSingle(arrFloat, 0);
                di.Length = 1;
            }
            return di;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="di"></param>
        /// <returns></returns>
        public override string ToString(CBORDataInfo di)
        {
            if(di.Value is byte)
            {
                return "simple(" + di.Value.ToString() + ")";
            }else{
                return di.Value.ToString();
            }
        }
    }
}
