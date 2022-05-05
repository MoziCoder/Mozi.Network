using Mozi.HttpEmbedded.Extension;
using System;
using System.Net;

namespace Mozi.HttpEmbedded.Common
{
    /// <summary>
    /// DNS客户端
    /// </summary>
    public class DNSResolver
    {
        /// <summary>
        /// DNS解析指定域名
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static string GetDomainAddress(string domain)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(domain);
                IPAddress[] addresses = entry.AddressList;

                if (addresses.Length > 0)
                {
                    return addresses[0].ToString();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }

    public class UnsignedIntegerOptionValue 
    {
        private byte[] _pack;

        public  object Value
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
                uint num = Convert.ToUInt32(value);
                byte[] data = BitConverter.GetBytes(num);
                if (num < 256) //2~8
                {
                    _pack = new byte[1] { data[0] };
                }
                else if (num < 65536) //2~16
                {
                    _pack = new byte[2] { data[1], data[0] };
                }
                else if (num < 16777216) //2~24
                {
                    _pack = new byte[3] { data[2], data[1], data[0] };
                }
                else
                {
                    _pack = new byte[4] { data[3], data[2], data[1], data[0] };
                }
            }
        }

        /// <summary>
        /// 高位在前，低位在后，且去掉所有高位0x00字节
        /// </summary>
        public  byte[] Pack { get => _pack; set => _pack = value; }

        public  int Length => _pack != null ? _pack.Length : 0;

        public override string ToString()
        {
            if (_pack != null)
            {
                return Value.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}
