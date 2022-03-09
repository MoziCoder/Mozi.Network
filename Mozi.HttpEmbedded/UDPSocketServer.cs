using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Mozi.HttpEmbedded
{
    //TODO 实现SOCKET UDP
    /// <summary>
    /// 异步单线程
    /// </summary>
    public class UDPSocketServer
    {
        //private static SocketServer _mSocketServer;

        protected int _iport = 80;

        protected int _maxListenCount = 100;
        protected readonly ConcurrentDictionary<string, Socket> _socketDocker;
        protected Socket _sc;
        private long _errorCount = 0;
        /// <summary>
        /// 接收错误计数
        /// </summary>
        public long ReceiveErrorCount
        {
            get
            {
                return _errorCount;
            }
        }
        /// <summary>
        /// 服务器启动事件
        /// </summary>
        public  ServerStart OnServerStart;
        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public  ClientConnect OnClientConnect;
        /// <summary>
        /// 客户端断开连接时间
        /// </summary>
        public  ClientDisConnect AfterClientDisConnect;
        /// <summary>
        /// 数据接收开始事件
        /// </summary>
        public  ReceiveStart OnReceiveStart;
        /// <summary>
        /// 数据接收完成事件
        /// </summary>
        public  ReceiveEnd AfterReceiveEnd;
        /// <summary>
        /// 服务器停用事件
        /// </summary>
        public  AfterServerStop AfterServerStop;

        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get { return _iport; }
        }
        public Socket SocketMain
        {
            get { return _sc; }
        }

        public UDPSocketServer()
        {
            _socketDocker = new ConcurrentDictionary<string, Socket>();
        }

        //TODO 测试此处是否有BUG
        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="port"></param>
        public void StartServer(int port)
        {
            _iport = port;
            if (_sc == null)
            {
                _sc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
            }
            else
            {
                _sc.Close();
            }
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, _iport);
            //允许端口复用
            _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _sc.Bind(endpoint);
            _sc.Listen(_maxListenCount);
            //回调服务器启动事件
            if (OnServerStart != null)
            {
                OnServerStart(this, new ServerArgs() { BindPort=_iport,StartTime = DateTime.Now, StopTime = DateTime.MinValue });
            }
            _sc.BeginAccept(new AsyncCallback(CallbackAccept), _sc);
        }
        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void StopServer()
        {
            _socketDocker.Clear();
            try
            {
                _sc.Shutdown(SocketShutdown.Both);
                _sc.Close();
                if (AfterServerStop != null)
                {
                    AfterServerStop(_sc, null);
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// 开始连接回调
        /// </summary>
        /// <param name="iar"></param>
        protected void CallbackAccept(IAsyncResult iar)
        {
            Socket server = (Socket)iar.AsyncState;
            Socket client = server.EndAccept(iar);
            //接受新连接传入
            server.BeginAccept(CallbackAccept, server);

            if (OnClientConnect != null)
            {
                OnClientConnect.BeginInvoke(this, new ClientConnectArgs()
                {
                    Client = client
                }, null, null);
            }
            StateObject so = new StateObject()
            {
                WorkSocket = client,
                Id = Guid.NewGuid().ToString(),
                IP = ((IPEndPoint)client.RemoteEndPoint).Address.ToString(),
                RemotePort = ((IPEndPoint)client.RemoteEndPoint).Port,
            };
            _socketDocker.TryAdd(so.Id, client);
            try
            {
                client.BeginReceive(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, CallbackReceive, so);
                if (OnReceiveStart != null)
                {
                    OnReceiveStart.BeginInvoke(this, new DataTransferArgs(), null, null);
                }
            }
            catch (Exception ex)
            {
                _errorCount++;
            }
        }
        /// <summary>
        /// 接收数据回调
        /// </summary>
        /// <param name="iar"></param>
        protected void CallbackReceive(IAsyncResult iar)
        {
            StateObject so = (StateObject)iar.AsyncState;
            Socket client = so.WorkSocket;
            if (client.Connected)
            {
                int iByteRead = client.EndReceive(iar);

                if (iByteRead > 0)
                {
                    //置空数据缓冲区
                    so.ResetBuffer(iByteRead);
                    if (client.Available > 0)
                    {
                        //Thread.Sleep(10);
                        client.BeginReceive(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, CallbackReceive, so);
                    }
                    else
                    {
                        InvokeAfterReceiveEnd(so, client);
                    }
                }
                else
                {
                    InvokeAfterReceiveEnd(so, client);
                }
            }
            else
            {
                InvokeAfterReceiveEnd(so, client);
            }
        }
        private void InvokeAfterReceiveEnd(StateObject so, Socket client)
        {
            RemoveClientSocket(so);
            if (AfterReceiveEnd != null)
            {
                AfterReceiveEnd.BeginInvoke(this,
                    new DataTransferArgs()
                    {
                        Data = so.Data.ToArray(),
                        IP = so.IP,
                        Port = so.RemotePort,
                        Socket = so.WorkSocket
                    }, null, null);
            }
        }
        //TODO 此处开启Socket状态监听，对断开的链接进行关闭销毁
        private void RemoveClientSocket(StateObject so)
        {
            Socket client;
            _socketDocker.TryRemove(so.Id, out client);
        }

        /// <summary>
        /// 向指定地址发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void SendTo(byte[] buffer, string host, int port)
        {
            _sc.SendTo(buffer, new IPEndPoint(IPAddress.Parse(host), port));
        }
    }
}
