using System;
using System.Net;
using System.Net.Sockets;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    //TODO 多网卡绑定IPAddress.Any会出现无法接收的问题

    /// <summary>
    /// UDP套接字
    /// </summary>
    public class UDPSocket
    {

        protected int _multicastGroupPort = SSDPProtocol.ProtocolPort;
        protected string _multicastGroupAddress  = SSDPProtocol.MulticastAddress;

        protected Socket _sc;

        //private EndPoint _remoteEndPoint=new IPEndPoint(IPAddress.Any, 0);

        private IPAddress _bindingAddress = IPAddress.Any;
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
        /// 绑定的本地地址
        /// </summary>
        public IPAddress BindingAddress { get { return _bindingAddress; } set { _bindingAddress = value; } }

        /// <summary>
        /// 是否接收本程序发出的数据包
        /// </summary>
        public bool AllowLoopbackMessage { get; set; }


        public UDPSocket()
        {

        }
        /// <summary>
        /// 服务器启动事件
        /// </summary>
        public  ServerStart OnServerStart;
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
            get { return _multicastGroupPort; }
        }
        public Socket SocketMain
        {
            get { return _sc; }
        }
        /// <summary>
        /// 组播地址
        /// </summary>
        public string MulticastAddress
        {
            get { return _multicastGroupAddress; }
        }
        /// <summary>
        /// 组播端口
        /// </summary>
        public int MulticastPort
        {
            get { return _multicastGroupPort; }
        }
        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Shutdown()
        {
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
        /// 加入组播
        /// <para>
        /// 239.0.0.0～239.255.255.255为本地管理组播地址，仅在特定的本地范围内有效。
        /// </para>
        /// </summary>
        /// <param name="multicastGroupAddress"></param>
        public void JoinMulticastGroup(string multicastGroupAddress)
        {
            _multicastGroupAddress = multicastGroupAddress;
            MulticastOption mcastOpt = new MulticastOption(IPAddress.Parse(multicastGroupAddress), _bindingAddress);
            _sc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mcastOpt);
        }
        /// <summary>
        /// 离开组播
        /// </summary>
        /// <param name="multicastGroupAddress"></param>
        public void LeaveMulticastGroup(string multicastGroupAddress)
        {
            MulticastOption mcastOpt = new MulticastOption(IPAddress.Parse(multicastGroupAddress), _bindingAddress);
            _sc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, mcastOpt);
        }
        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="multicastGroupPort"></param>
        public void Start(string multicastGroupAddress,int multicastGroupPort)
        {
            _multicastGroupPort = multicastGroupPort;
            if (_sc == null)
            {
                _sc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
            }
            else
            {
                _sc.Close();
            }
            IPEndPoint endpoint = new IPEndPoint(_bindingAddress, _multicastGroupPort);
            //允许端口复用
            _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _sc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 10);
            _sc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, 0);
            //先绑定 后加入组播，否则XP/2003系统下会报错
            _sc.Bind(endpoint);
            _sc.MulticastLoopback = AllowLoopbackMessage;
            JoinMulticastGroup(multicastGroupAddress);

            //回调服务器启动事件
            UDPStateObject so = new UDPStateObject()
            {
                WorkSocket = _sc,
                Id = Guid.NewGuid().ToString(),
                //IP = ((System.Net.IPEndPoint)client.RemoteEndPoint).Address.ToString(),
                //RemotePort = ((System.Net.IPEndPoint)client.RemoteEndPoint).Port,
                RemoteEndPoint = new IPEndPoint(_bindingAddress, 0)
            };

            if (OnServerStart != null)
            {
                OnServerStart(this, new ServerArgs() { BindPort=_multicastGroupPort,StartTime = DateTime.Now, StopTime = DateTime.MinValue });
            }
            try
            {
                _sc.BeginReceiveFrom(so.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ref so.RemoteEndPoint, CallbackReceived, so);
                if (OnReceiveStart != null)
                {
                    OnReceiveStart(this, new DataTransferArgs());
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
        protected void CallbackReceived(IAsyncResult iar)
        {
            UDPStateObject so = (UDPStateObject)iar.AsyncState;
            Socket client = so.WorkSocket;

            EndPoint remote = (IPEndPoint)so.RemoteEndPoint;

            int iByteRead = client.EndReceiveFrom(iar, ref remote);

            if (iByteRead > 0)
            {
                //置空数据缓冲区
                so.ResetBuffer(iByteRead);
                //if (client.Available > 0)
                //{
                //    so.RemoteEndPoint = remote;
                //    client.BeginReceiveFrom(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, ref so.RemoteEndPoint,new AsyncCallback(CallbackReceived), so);
                //}
                //else
                //{
                    InvokeAfterReceiveEnd(so, client, (IPEndPoint)remote);
                //}
            }
            else
            {
                InvokeAfterReceiveEnd(so, client, (IPEndPoint)remote);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="so"></param>
        /// <param name="client"></param>
        /// <param name="remote"></param>
        
        private void InvokeAfterReceiveEnd(UDPStateObject so, Socket client, EndPoint remote)
        {
            if (AfterReceiveEnd != null)
            {
                AfterReceiveEnd(this,
                    new DataTransferArgs()
                    {
                        Data = so.Data.ToArray(),
                        IP = ((IPEndPoint)remote).Address.ToString(),
                        Port = ((IPEndPoint)remote).Port,
                        Socket = so.WorkSocket
                    });
            }
            UDPStateObject stateobject = new UDPStateObject()
            {
                WorkSocket = _sc,
                Id = Guid.NewGuid().ToString(),
                //IP = ((System.Net.IPEndPoint)client.RemoteEndPoint).Address.ToString(),
                //RemotePort = ((System.Net.IPEndPoint)client.RemoteEndPoint).Port,
                RemoteEndPoint = new IPEndPoint(_bindingAddress, 0)
            };
            _sc.BeginReceiveFrom(stateobject.Buffer, 0, stateobject.Buffer.Length, SocketFlags.None, ref stateobject.RemoteEndPoint, new AsyncCallback(CallbackReceived), stateobject);
        }
    }
}
