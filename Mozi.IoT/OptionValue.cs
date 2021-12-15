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

    public class EmptyOptionValue : OptionValue
    {
        public override object Value { get { return null; } set { } }

        public override byte[] Pack
        {
            get => new byte[0];set { }
        }

        public override int Length => 0;
    }

    public class ArrayByteOptionValue : OptionValue
    {
        private byte[] _pack;

        public override object Value { get => _pack; set => _pack = (byte[])value; }

        public override byte[] Pack { get => _pack; set => _pack = value; }

        public override int Length => _pack!=null?_pack.Length:0;
    }
    /// <summary>
    /// uint选项值，.Net的数值类型与网络包数据类型不同，故字节数组会进行翻转
    /// </summary>
    public class UnsignIntegerOptionValue : OptionValue
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

                if (num < 256)
                {
                    _pack = new byte[1] { data[3] };
                }
                else if (num < 65536)
                {
                    _pack = new byte[2] { data[2], data[3] }.Revert();
                }
                else if (num < 16777216)
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
    /// 分块选项 数据结构 适用Block1 Block2 总长度uint24
    /// </summary>
    public class BlockOptionValue : OptionValue
    {
        /// <summary>
        /// 块内位置 占位4-20bits
        /// </summary>
        public uint Num { get; set; }
        /// <summary>
        /// 是否最后一个包 占位1bit
        /// </summary>
        public bool MoreFlag { get; set; }
        /// <summary>
        /// 数据包总大小 占位3bits 值大小为1-6，表值范围16bytes-1024bytes
        /// </summary>
        public ushort Size { get; set; }
        /// <summary>
        /// 
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

                Size = (ushort)Math.Pow(2, (((byte)(value[0] << 5)) >> 5) + 4);
                MoreFlag = (value[0] & 8) == 8;
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
            return Pack == null ? "null" : String.Format("{0},Num:{1},M:{2},SZX:{3}(bytes)", "Block", Num, MoreFlag ? 1 : 0, Size);
        }
    }
}
