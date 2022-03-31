using Mozi.Encode.Generic;
using System;

namespace Mozi.Encode.CBOR
{
    //.Net4不支持半精度类型，故需要另行实现

    //TODO 半精度类型还需要多测试
    /// <summary>
    /// 半精度类型
    /// </summary>
    /// <remarks>1(sign)+5(exponent)+10(fraction)</remarks>
    public class HalfFloat
    {
        private byte sign=0;
        private byte exponent=0;
        private ushort fraction=0;

        public static float MaxValue=(float)(1.1111111111f *Math.Pow(2,15)); 
        public static float MinValue=(float)(-1*Math.Pow(2,-14));

        /// <summary>
        /// 正无穷
        /// </summary>
        public const float PostiveInfinity = 1F / 0F;
        /// <summary>
        /// 负无穷
        /// </summary>
        public const float NegativeInfinity = -1F / 0F;
        /// <summary>
        /// 非数字
        /// </summary>
        public const float NaN = 0F / 0F;
        /// <summary>
        /// 用浮点型表示的值
        /// </summary>
        private float Value
        {
            get
            {
                float dec = 0;
                if (exponent == 0 && fraction == 0)
                {
                    dec = 0;
                }
                else if (exponent == 0 && fraction > 0)
                {
                    dec = (float)(exponent * Math.Pow(2, -24));
                }
                else if (exponent == 31 && fraction == 0 && sign == 0)
                {
                    dec = PostiveInfinity;
                }
                else if (exponent == 31 && fraction == 0 && sign == 1)
                {
                    dec = NegativeInfinity;
                }
                else
                {
                    dec= (float)(Math.Pow(-1,sign)*Math.Pow(2, exponent - 15) *(1+ fraction / Math.Pow(2, 10)));
                }
                return dec;
            }
        }

        public HalfFloat(float value)
        {
            
            byte[] data = BitConverter.GetBytes(value).Revert();
            //符号位
            sign = (byte)((byte)(data[0]>>7)&0b00000001);
            //指数位 float指数位为8位取数-127 故要转换成半精度的5位指数位
            sbyte expReal = (sbyte)((sbyte)((sbyte)(data[0] << 1)-0b01111111)+ (byte)(data[1] >> 7));
            expReal+= (1 << (5 - 1)) - 1;
            if (expReal < 0)
            {
                expReal = 0;
            }
            exponent = (byte)(expReal);

            //TODO 更加简化
            fraction = (ushort)(data[1] & 0x7f);
            fraction = (ushort)(fraction << 3);
            fraction += (ushort)(data[2] >> 5);
            if (((data[1] >> (15 - 10 - 1)) & 1) == 1)
                //若被移除的最高位是1，则产生进位。
                fraction += 1;
            if (fraction >= (ushort)Math.Pow(2, 10))
                //若进位后发生尾数溢出，则取消进位
                fraction -= 1;
        }

        public HalfFloat()
        {

        }

        /// <summary>
        /// 半精度
        /// </summary>
        /// <param name="data">2 bytes</param>
        /// <returns></returns>
        public static HalfFloat Decode(byte[] data)
        {
            HalfFloat hf = new HalfFloat();
            hf.sign = (byte)(data[0] >> 7);
            byte[] fract = new byte[2];
            fract[1] = (byte)(data[0] & 0b00000011);
            fract[0] = data[1];
            hf.fraction = BitConverter.ToUInt16(fract, 0);
            hf.exponent = (byte)(((byte)(data[0] & 0b01111100))>>2);
            return hf;
        }

        public static byte[] Encode(HalfFloat data)
        {
            byte[] half = new byte[2] { 0x00, 0x00 };

            half[0] = (byte)(half[0]|((byte)(data.sign & 0b00000001)<<7));
            half[0] = (byte)(half[0] | ((byte)(data.exponent & 0b00011111) << 2));

            byte[] fract = BitConverter.GetBytes(data.fraction).Revert();
            half[0] = (byte)(half[0] | (fract[0]&0b00000011));
            half[1] = (byte)(half[1] | fract[1]);
            return half;
        }

        public static bool operator>(HalfFloat thi, HalfFloat that)
        {
            return thi.Value > that.Value;
        }

        public static bool operator<(HalfFloat thi, HalfFloat that)
        {
            return thi.Value < that.Value;
        }

        public static bool operator==(HalfFloat thi, HalfFloat that)
        {
            return thi.Value == that.Value;
        }

        public static bool operator!=(HalfFloat thi, HalfFloat that)
        {
            return thi.Value != that.Value;
        }

        public override bool Equals(object that)
        {
            return (that!=null)&&(that is HalfFloat)&&this.Value == ((HalfFloat)that).Value;
        }
        public override int GetHashCode()
        {
            return sign*(exponent*fraction);
        }
    }
}
