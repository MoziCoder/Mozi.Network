using System;
using System.Net.Sockets;

namespace Mozi.HttpEmbedded
{
    public delegate void ServerStart(object sender, ServerArgs args);

    public delegate void AfterServerStop(object sender, ServerArgs args);

    public delegate void ClientDisConnect(object sender, ClientConnectArgs ars);

    public delegate void ClientConnect(object sender, ClientConnectArgs args);

    public delegate void ReceiveStart(object sender, DataTransferArgs args);

    public delegate void ReceiveEnd(object sender, DataTransferArgs args);

    public class ServerArgs : EventArgs
    {
        public int BindPort { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
    }

    public class ClientConnectArgs : EventArgs
    {
        /// <summary>
        /// 会话标识符
        /// </summary>
        public string Id { get; set; }
        public string IP { get; set; }
        /// <summary>
        /// 远端端口
        /// </summary>
        public int RemotePort = 0;

        /// <summary>
        /// 连接时间  
        /// </summary>  
        public DateTime ConnectTime { get; set; }

        private Socket _socket;

        public Socket Client
        {
            get { return _socket; }
            set { _socket = value; }
        }

    }

    public class DataTransferArgs : EventArgs
    {
        /// <summary>
        /// 会话标识符
        /// </summary>
        public string Id { get; set; }
        public byte[] Data { get; set; }
        //IPV4
        public string IP { get; set; }
        public int Port { get; set; }
        public Socket Socket { get; set; }
        public Socket Client { get; internal set; }
        public StateObject State { get; internal set; }

        ~DataTransferArgs()
        {
            Data = null;
            State = null;
        }
    }
    /// <summary>
    /// 客户端访问
    /// </summary>
    public class ClientAccessArgs : EventArgs
    {
        /// <summary>
        /// 会话标识符
        /// </summary>
        public string Id { get; set; }
        public string Path { get; set; }
        public string Query { get; set; }
        public string IP { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
