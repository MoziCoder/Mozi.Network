using System;
using System.Collections.Generic;

namespace Mozi.IoT.Encode
{
    class CBOREncoder
    {
        public static List<CBORDataInfo> Decode(byte[] data)
        {
            List<CBORDataInfo> infos = new List<CBORDataInfo>();
            int ind = -1;
            while (ind < data.Length)
            {
                ind++;

            }
        }

        public static byte[] Encode(List<CBORDataInfo> data)
        {

        }
    }
    public delegate CBORDataInfo CBORDataParser(byte[] data);

    public delegate byte[] CBORDataPacker(CBORDataInfo data);

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
            }else if (indicator == 24){
                di.Value = data[0];
                di.Length = 1;

            }//uint16
            else if (indicator == 25){
                di.Value = BitConverter.ToUInt16(data.Revert(), 0);
                di.Length = 2;
                //uint32
            }else if (indicator == 26){
                di.Value = BitConverter.ToUInt32(data.Revert(), 0);
                di.Length = 4;
                //uint64
            }else if(indicator==27){
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
                di.Value = (sbyte)(-1-indicator);
                di.Length = 1;
            }
            //uint8
            else if (indicator == 24)
            {
                di.Value = (sbyte)(-1-data[0]);
                di.Length = 1;
            }//uint16+
            else if (indicator == 25)
            {
                di.Value = (int)(-1-BitConverter.ToUInt16(data.Revert(), 0));
                di.Length = 2;
            }//uint32
            else if (indicator == 26)
            {
                di.Value = (long)(-1-BitConverter.ToUInt32(data.Revert(), 0));
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
            }else if(indicator==24){
                lenArr = (byte)data[1];
                offset = 1;
            }else if (indicator == 25){
                lenArr = BitConverter.ToUInt16(data, 1);
                offset = 2;
            }else if (indicator == 26){
                lenArr = BitConverter.ToUInt32(data, 1);
                offset = 4;
            }else if (indicator == 27){
                
                lenArr = (long)BitConverter.ToUInt64(data, 1);
                offset = 8;
            }else if (indicator == 31){
                //查找结束符号
                lenArr = Array.LastIndexOf(data, 0xff);
                lenArr = lenArr - 1;
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
            else if(indicator==24){
                lenArr = (byte)data[1];
                offset = 1;
            }else if (indicator == 25){
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
                    lenArr = (byte)data[1];
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
                }
                int[] item = new int[lenArr];
                for(int i = 0; i < lenArr; i++)
                {
                    //最多可表示 254
                    item[i] = data[offset + 1 + i];
                }
                value.Add(item);
                offset = (int)(offset + lenArr)+1;
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// CBOR数据信息
    /// </summary>
    public class CBORDataInfo
    {
        public CBORDataType DataType { get; set; }
        public long Length { get; set; }
        public object Value { get; set; }
        public byte[] Data { get; set; }
    }
}
