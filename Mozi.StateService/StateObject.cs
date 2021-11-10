using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Mozi.StateService
{
    /// <summary>
    /// 通讯缓冲对象
    /// </summary>
    public class StateObject : IDisposable
    {
        public string Id { get; set; }   //连接标识符
        public Socket WorkSocket = null;
        public int RemotePort = 0;
        public const int BufferSize = 1024;
        public List<byte> Data = new List<byte>();
        public byte[] Buffer = new byte[BufferSize * 4];
        private bool disposedValue;

        public string IP { get; set; }

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
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    Buffer = null;
                    Data = null;
                }
                //TODO 释放非托管对象
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
