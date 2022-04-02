using Mozi.Encode.CBOR;
using System;

namespace Mozi.Encode.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //通过类型实例化进行构造

            //unsigned integer pack
            CBORData di_uint1 = new CBORData(CBORDataType.UnsignedInteger,12);
            CBORData di_uint2 = new CBORData(CBORDataType.UnsignedInteger, 123);
            CBORData di_uint3 = new CBORData(CBORDataType.UnsignedInteger, 12345);
            CBORData di_uint4 = new CBORData(CBORDataType.UnsignedInteger, 12345678);
            CBORData di_uint5 = new CBORData(CBORDataType.UnsignedInteger, 1234567891011);

            //CBORDataInfo di = new CBORDataInfo(12);
            //CBORDataInfo di5 = new CBORDataInfo(1234567891011);

            byte[] data_uint1 = CBOREncoder.Encode(di_uint1);
            byte[] data_uint2 = CBOREncoder.Encode(di_uint2);
            byte[] data_uint3 = CBOREncoder.Encode(di_uint3);
            byte[] data_uint4 = CBOREncoder.Encode(di_uint4);
            byte[] data_uint5 = CBOREncoder.Encode(di_uint5);

            Console.WriteLine(Hex.To(data_uint1));
            Console.WriteLine(Hex.To(data_uint2));
            Console.WriteLine(Hex.To(data_uint3));
            Console.WriteLine(Hex.To(data_uint4));
            Console.WriteLine(Hex.To(data_uint5));

            //negative integer pack
            CBORData di_nint1 = new CBORData(CBORDataType.NegativeInteger, -12);
            CBORData di_nint2 = new CBORData(CBORDataType.NegativeInteger, -123);
            CBORData di_nint3 = new CBORData(CBORDataType.NegativeInteger, -12345);
            CBORData di_nint4 = new CBORData(CBORDataType.NegativeInteger, -12345678);
            CBORData di_nint5 = new CBORData(CBORDataType.NegativeInteger, -1234567891011);

            byte[] data_nint1 = CBOREncoder.Encode(di_nint1);
            byte[] data_nint2 = CBOREncoder.Encode(di_nint2);
            byte[] data_nint3 = CBOREncoder.Encode(di_nint3);
            byte[] data_nint4 = CBOREncoder.Encode(di_nint4);
            byte[] data_nint5 = CBOREncoder.Encode(di_nint5);

            Console.WriteLine(Hex.To(data_nint1));
            Console.WriteLine(Hex.To(data_nint2));
            Console.WriteLine(Hex.To(data_nint3));
            Console.WriteLine(Hex.To(data_nint4));
            Console.WriteLine(Hex.To(data_nint5));

            //hex array pack
            CBORData di_hexarray1 = new CBORData(CBORDataType.StringArray, "010203040506");
            CBORData di_hexarray2 = new CBORData(CBORDataType.StringArray,new CBORData[] { new CBORData(CBORDataType.StringArray, "010203040506"), new CBORData(CBORDataType.StringArray, "010203040506") });
            di_hexarray2.IsIndefinite = true;
            byte[] data_hexarray1 = CBOREncoder.Encode(di_hexarray1);
            byte[] data_hexarray2 = CBOREncoder.Encode(di_hexarray2);
            Console.WriteLine(Hex.To(data_hexarray1));
            Console.WriteLine(Hex.To(data_hexarray2));

            //hex array parse
            Console.WriteLine(CBOREncoder.Decode(data_hexarray2).ToString());

            //string text pack
            CBORData di_stringtext1 = new CBORData(CBORDataType.StringText, "010203040506");
            CBORData di_stringtext2 = new CBORData(CBORDataType.StringText, new CBORData[] { new CBORData(CBORDataType.StringText, "010203040506"), new CBORData(CBORDataType.StringText, "010203040506") });
            di_stringtext2.IsIndefinite = true;
            byte[] data_stringtext1 = CBOREncoder.Encode(di_stringtext1);
            byte[] data_stringtext2 = CBOREncoder.Encode(di_stringtext2);
            Console.WriteLine(Hex.To(data_stringtext1));
            Console.WriteLine(Hex.To(data_stringtext2));

            Console.WriteLine(CBOREncoder.Decode(data_stringtext2).ToString());
            Console.WriteLine(Hex.To(BitConverter.GetBytes(100000.0f)));
            HalfFloat f = new HalfFloat(1.0f);
            HalfFloat f2 = HalfFloat.Decode(new byte[] { 62, 00 });

            //array pack
            //array parse 


            //keypair pack
            //keypair parse 
            Console.WriteLine(CBOREncoder.Decode(new byte[] { 0xa2, 0x61, 0x61, 0x01, 0x61, 0x62, 0x82, 0x02, 0x03 }).ToString());

            //tagitem pack
            //tagitem parse 

            //simplefloat pack
            //simplefloat parse 


            //parse from text 

            //unsigned integer 

            //negative integer

            //hex array

            //string text

            //array 

            //keypair

            //tagitem 

            //simplefloat


            //字符串解析到类型
            CBOREncoder.Parse("(_ h'0102', h'030405')");

            Console.Read();
        }
    }
}
