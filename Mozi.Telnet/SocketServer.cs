using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Mozi.Telnet
{
    //TODO 加入定时器并利用POLL判断远端是否断开
    //TODO 实现链接复用
    //TODO 解决接收文件内存占用过大，无法及时释放的问题
    /// <summary>
    /// 异步单线程
    /// </summary>
    public class SocketServer
    {
        protected int _iport = 80;

        protected int _maxListenCount = 65535;
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

        public SocketServer()
        {
            _socketDocker = new ConcurrentDictionary<string, Socket>();
        }

        //TODO 测试此处是否有BUG
        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            _iport = port;
            if (_sc == null)
            {
                _sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                OnServerStart(this, new ServerArgs() {BindPort = _iport, StartTime = DateTime.Now, StopTime = DateTime.MinValue });
            }
            _sc.BeginAccept(new AsyncCallback(CallbackAccept), _sc);
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Shutdown()
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
            //接受新连接传入
            server.BeginAccept(CallbackAccept, server);

            Socket client = server.EndAccept(iar);


            StateObject so = new StateObject()
            {
                WorkSocket = client,
                Id = Guid.NewGuid().ToString(),
                IP = ((IPEndPoint)client.RemoteEndPoint).Address.ToString(),
                ConnectTime = DateTime.Now,
                RemotePort = ((IPEndPoint)client.RemoteEndPoint).Port,
            };
            if (OnClientConnect != null)
            {
                //TODO .NetCore不再支持异步委托，需要重新实现
                OnClientConnect(this, new ClientConnectArgs()
                {
                    Id=so.Id,
                    IP=so.IP,
                    ConnectTime=so.ConnectTime,
                    RemotePort=so.RemotePort,
                    Client = client
                });
            }
            _socketDocker.TryAdd(so.Id, client);
            try
            {

                client.BeginReceive(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, CallbackReceived, so);
                if (OnReceiveStart != null)
                {
                    OnReceiveStart.Invoke(this, new DataTransferArgs() { Id = so.Id, IP = so.IP, Socket = server, Port = so.RemotePort, Client = client, State = so });
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
        internal void CallbackReceived(IAsyncResult iar)
        {
            StateObject so = (StateObject)iar.AsyncState;
            Socket client = so.WorkSocket;
            if (client.Connected)
            {
                try
                {
                    int iByteRead = client.EndReceive(iar);

                    if (iByteRead > 0)
                    {
                        //置空数据缓冲区
                        so.ResetBuffer(iByteRead);
                        if (client.Available > 0)
                        {
                            client.BeginReceive(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, CallbackReceived, so);
                        }
                        else
                        {
                            InvokeAfterReceiveComplete(so, client);
                        }
                    }
                    else
                    {
                        InvokeAfterReceiveComplete(so, client);
                    }
                }
                catch (SocketException se)
                {

                }
                finally
                {

                }
            }
            else
            {
                InvokeAfterReceiveComplete(so, client);
            }
        }

        private void InvokeAfterReceiveComplete(StateObject so, Socket client)
        {
            try
            {
                RemoveClientSocket(so);
                if (AfterReceiveEnd != null)
                {
                    AfterReceiveEnd(this,
                        new DataTransferArgs()
                        {
                            Id=so.Id,
                            Data = so.Data.ToArray(),
                            IP = so.IP,
                            Port = so.RemotePort,
                            Socket = so.WorkSocket,
                            Client = client,
                            State = so
                        });
                }
                //循环等待消息
                if (so.WorkSocket.Connected)
                {
                    StateObject so2 = new StateObject()
                    {
                        WorkSocket = so.WorkSocket,
                        Id = so.Id,
                        IP = so.IP,
                        ConnectTime = so.ConnectTime,
                        RemotePort = so.RemotePort,
                    };
                    so.WorkSocket.BeginReceive(so2.Buffer, 0, so2.Buffer.Length, SocketFlags.None, CallbackReceived, so2);
                }
                else
                {
                    so.WorkSocket.Close();
                }
            }
            finally
            {

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
