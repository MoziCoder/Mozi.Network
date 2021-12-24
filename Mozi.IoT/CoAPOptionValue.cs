using System;

namespace Mozi.IoT
{
    /// <summary>
    /// 选项值>=0 bytes
    /// 空 字节数组 数字 ASCII/UTF-8字符串
    /// </summary>
    public abstract class OptionValue
    {
        public abstract object Value { get; set; }
        public abstract byte[] Pack { get; set; }
        public abstract int Length { get; }
    }
    /// <summary>
    /// 空选项值
    /// </summary>
    public class EmptyOptionValue : OptionValue
    {
        public override object Value { get { return null; } set { } }
        public override byte[] Pack
        {
            get => new byte[0];set { }
        }
        public override int Length => 0;
    }
    /// <summary>
    /// 字节数组选项值
    /// </summary>
    public class ArrayByteOptionValue : OptionValue
    {
        private byte[] _pack;

        public override object Value { get => _pack; set => _pack = (byte[])value; }

        public override byte[] Pack { get => _pack; set => _pack = value; }

        public override int Length => _pack!=null?_pack.Length:0;
    }
    /// <summary>
    /// uint选项值，.Net的数值类型与网络包数据包排序不同，故字节数组会进行数组翻转
    /// </summary>
    public class UnsignedIntegerOptionValue : OptionValue
    {
        private byte[] _pack;

        public override object Value
        {
            get
            {
                byte[] data = new byte[4];
                if (_pack != null)
                {
                    Array.Copy(_pack, 0, data, data.Length - _pack.Length, _pack.Length);
                    return BitConverter.ToUInt32(data.Revert(), 0);
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                uint num = (uint)value;
                byte[] data = BitConverter.GetBytes(num);

                if (num < 256) //2~8
                {
                    _pack = new byte[1] { data[3] };
                }
                else if (num < 65536) //2~16
                {
                    _pack = new byte[2] { data[2], data[3] }.Revert();
                }
                else if (num < 16777216) //2~24
                {
                    _pack = new byte[3] { data[1], data[2], data[3] }.Revert();
                }
                else
                {
                    _pack = data;
                }
            }
        }

       /// <summary>
       /// 高位在前，低位在后，且去掉所有高位0x00字节
       /// </summary>
       public override byte[] Pack { get => _pack; set => _pack = value; }

        public override int Length => _pack != null ? _pack.Length : 0;
    }
    /// <summary>
    /// string选项值
    /// </summary>
    public class StringOptionValue : OptionValue
    {
        private byte[] _pack;

        public override object Value
        {
            get => System.Text.Encoding.UTF8.GetString(_pack);
            set => _pack = System.Text.Encoding.UTF8.GetBytes((string)value);
        }

        public override byte[] Pack { get => _pack; set => _pack = value; }

        public override int Length => _pack != null ? _pack.Length : 0;
    }
    /// <summary>
    /// 分块选项 数据结构 适用Block1 Block2 长度为可变长度，可为8bits 16bits 24bits
    /// </summary>
    public class BlockOptionValue : OptionValue
    {
        /// <summary>
        /// 块内位置 占位4-20bits 4 12 20
        /// </summary>
        public uint Num { get; set; }
        /// <summary>
        /// 是否最后一个包 占位1bit 倒数第4位
        /// </summary>
        public bool MoreFlag { get; set; }
        /// <summary>
        /// 数据包总大小 占位3bits 低3位为其储存区间 值大小为1-6，表值范围16bytes-1024bytes 
        /// </summary>
        public ushort Size { get; set; }
        /// <summary>
        /// 字节流
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

                Size = (ushort)Math.Pow(2, (((byte)(value[value.Length-1] << 5)) >> 5) + 4);
                MoreFlag = (value[value.Length-1] & 8) == 8;
                byte[] data = new byte[4];
                Array.Copy(value.Revert(), 0, data, data.Length - value.Length, value.Length);
                Num = BitConverter.ToUInt32(data, 0);
            }
        }
        /// <summary>
        /// 属性赋值器无效，因为BlockValue不是由单一要素构造
        /// </summary>
        public override object Value { get => Size;set { } }

        public override int Length => Pack != null ? Pack.Length : 0;

        public override string ToString()
        {
            return Pack == null ? "null" : string.Format("{0},Num:{1},M:{2},SZX:{3}(bytes)", "Block", Num, MoreFlag ? 1 : 0, Size);
        }
    }
    //RFC8974
    ///// <summary>
    ///// Extended-Token-Length Option 长度0-3 bytes
    ///// </summary>
    //internal class ExtendedTokenLengthOptionValue : UnsignedIntegerOptionValue
    //{
    //    public override object Value
    //    {
    //        get { return base.Value; }
    //        set
    //        {
    //            if ((uint)value< 8){
    //                base.Value = 8;
    //            }else if((uint)value> 65804)
    //            {
    //                base.Value = 65804;
    //            }
    //            else
    //            {
    //                base.Value = value;
    //            }
    //        }
    //    }
    //}
}
