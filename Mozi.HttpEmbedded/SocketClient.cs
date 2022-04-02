using System;
using System.Net;
using System.Net.Sockets;

namespace Mozi.HttpEmbedded
{
    //TODO 加入定时器并利用POLL判断远端是否断开
    //TODO 实现链接复用
    //TODO 解决接收文件内存占用过大，无法及时释放的问题

    //TODO 实现对IPV6的支持
    /// <summary>
    /// 异步单线程
    /// </summary>
    public class SocketClient
    {
        protected int _localPort,_iport = 80;
        protected string _host = "";

        protected Socket _sc;
        private long _errorCount = 0;
        private bool _connected = false;
        private int _connectTimeout = 45;

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
        public int LocalPort
        {
            get
            {
                return _localPort;
            }
        }
        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool Connected
        {
            get
            {
                return _connected;
            }
        }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get { return _iport; }
        }
        /// <summary>
        /// 远程主机地址
        /// </summary>
        public string Host
        {
            get { return _host; }
        }

        public int ConnectTimeout
        {
            get { return _connectTimeout; }
            set
            {
                _connectTimeout = value;
            }
        }
        public int SendTimeout
        {
            get
            {
                return _sc.SendTimeout;
            }
            set
            {
                _sc.SendTimeout = value;
            }
        }
        public Socket SocketMain
        {
            get { return _sc; }
        }

        public SocketClient()
        {
            _sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }
        /// <summary>
        /// 绑定本地端口
        /// </summary>
        /// <param name="port"></param>
        public void SetPort(int port)
        {
            _localPort = port;
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
            _sc.Bind(endpoint);
        }
        ////TODO 测试此处是否有BUG
        ///// <summary>
        ///// 启动服务器
        ///// </summary>
        ///// <param name="port"></param>
        //public void Start(int port)
        //{
        //    _iport = port;
        //    if (_sc == null)
        //    {
        //        _sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
        //    }
        //    else
        //    {
        //        _sc.Close();
        //    }
        //    IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, _iport);
        //    //允许端口复用
        //    _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        //    //_sc.Bind(endpoint);
        //    //_sc.Listen(_maxListenCount);
        //    ////回调服务器启动事件
        //    //_sc.BeginAccept(new AsyncCallback(CallbackAccept), _sc);
        //}
        /// <summary>
        /// 链接远程主机
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public bool Connect(string host,int port)
        {
            _host = host;
            _iport = port;
            IAsyncResult result = _sc.BeginConnect(host, port, null, null);
            result.AsyncWaitHandle.WaitOne(_connectTimeout, false);
            _connected = _sc.Connected;
            try
            {
                result.AsyncWaitHandle.Close();
            }
            finally
            {

            }
            return _connected;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Shutdown()
        {
            try
            {
                _sc.Shutdown(SocketShutdown.Both);
                _sc.Close();
            }
            catch
            {

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
                            //Thread.Sleep(10);
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
                }catch(SocketException se){
                    
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
            }finally{

            }
        }

        /// <summary>
        /// 向指定地址发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void SendTo(byte[] buffer)
        {
            if (_connected)
            {
                _sc.SendTo(buffer, new IPEndPoint(IPAddress.Parse(_host), _iport));
                StateObject so = new StateObject()
                {
                    WorkSocket = _sc,
                    Id = Guid.NewGuid().ToString(),
                    IP = _host,
                    ConnectTime = DateTime.Now,
                    RemotePort = _iport,
                };
                try
                {
                    _sc.BeginReceive(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, CallbackReceived, so);
                    if (OnReceiveStart != null)
                    {
                        OnReceiveStart.Invoke(this, new DataTransferArgs() { Id = so.Id, IP = so.IP, Socket = _sc, Port = so.RemotePort, Client = _sc, State = so });
                    }
                }
                catch (Exception ex)
                {
                    _errorCount++;
                }
            }
            else
            {
                throw new Exception($"与服务的链接已断开：{_host}:{_iport}");
            }
        }
    }
}
