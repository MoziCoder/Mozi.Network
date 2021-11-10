using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 通讯缓冲对象
    /// </summary>
    public class StateObject
    {
        /// <summary>
        /// 会话标识符
        /// </summary>
        public string Id { get; set; }   //连接标识符
        public Socket WorkSocket = null;
        
        public const int BufferSize = 1024;
        public List<byte> Data = new List<byte>();
        public byte[] Buffer = new byte[BufferSize * 4];

        public string IP { get; set; }

        public int RemotePort = 0;

        /// <summary>
        /// 连接时间  
        /// </summary>  
        public DateTime ConnectTime { get; set; }
        //TODO 此处会造成内存占用过大的问题
        //TODO 此处没有完整处理包体，会有多读取的冗余数据
        public void ResetBuffer(int count)
        {
            byte[] data = new byte[count > 0 ? count : Buffer.Length];
            Array.Copy(Buffer, data, data.Length);
            Data.AddRange(data);
            Buffer = new byte[BufferSize];
        }
        ~StateObject()
        {
            Data.Clear();
            Buffer = null;
            Data = null;
        }
    }
}
