using System;
using System.Net;
using System.Net.Sockets;

namespace Mozi.StateService
{
    /// <summary>
    /// UDP套接字
    /// </summary>
    public class UDPSocket
    {
        protected int _iport = 13453;

        protected Socket _sc;

        private EndPoint _endPoint;

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
            get { return _iport; }
        }
        public Socket SocketMain
        {
            get { return _sc; }
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
        /// 启动服务器
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            _iport = port;
            if (_sc == null)
            {
                _sc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //设置此处防止转发消息包时出现 UDP 远程主机未连接的问题
                const uint IOC_IN = 0x80000000;
                int IOC_VENDOR = 0x18000000;
                int SIO_UDP_CONNRESET = (int)(IOC_IN | IOC_VENDOR | 12);
                _sc.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, new byte[4]);
            }
            else
            {
                _sc.Close();
            }
            _endPoint = new IPEndPoint(IPAddress.Any, _iport);
            //允许端口复用
            _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _sc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, 32);
            _sc.Bind(_endPoint);

            //回调服务器启动事件
            UDPStateObject so = new UDPStateObject()
            {
                WorkSocket = _sc,
                Id = Guid.NewGuid().ToString(),
                //IP = ((System.Net.IPEndPoint)client.RemoteEndPoint).Address.ToString(),
                //RemotePort = ((System.Net.IPEndPoint)client.RemoteEndPoint).Port,
                RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0)
            };

            if (OnServerStart != null)
            {
                OnServerStart(this, new ServerArgs() {BindPort = _iport, StartTime = DateTime.Now, StopTime = DateTime.MinValue });
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
                var ex2 = ex;
            }
        }
        /// <summary>
        /// 接收数据回调
        /// </summary>
        /// <param name="iar"></param>
        protected void CallbackReceived(IAsyncResult iar)
        {
            try
            {
                UDPStateObject so = (UDPStateObject)iar.AsyncState;
                Socket client = so.WorkSocket;

                EndPoint remote = (IPEndPoint)so.RemoteEndPoint;

                int iByteRead = client.EndReceiveFrom(iar, ref remote);

                if (iByteRead > 0)
                {
                    //置空数据缓冲区
                    so.ResetBuffer(iByteRead);
                    if (client.Available > 0)
                    {
                        so.RemoteEndPoint = remote;
                        client.BeginReceiveFrom(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, ref so.RemoteEndPoint, new AsyncCallback(CallbackReceived), so);
                    }
                    else
                    {
                        InvokeAfterReceiveEnd(so, client, (IPEndPoint)remote);
                    }
                }
                else
                {
                    InvokeAfterReceiveEnd(so, client, (IPEndPoint)remote);
                }
            }
            catch
            {

            }
        }
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
                //IP = ((IPEndPoint)remote).Address.ToString(),
                RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0)
            };
            _sc.BeginReceiveFrom(stateobject.Buffer, 0, stateobject.Buffer.Length, SocketFlags.None, ref stateobject.RemoteEndPoint, new AsyncCallback(CallbackReceived), stateobject);
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
