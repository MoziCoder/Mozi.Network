using Mozi.Live.RTP;
using System;

namespace Mozi.Live
{
    /// <summary>
    /// RTP数据传输控制服务端
    /// </summary>
    public class RTCPServer
    {
        /// <summary>
        /// 最大数据包尺寸 包含所有头信息和有效荷载 Byte
        /// </summary>
        private int _maxTransferPackSize = 512;

        private ulong _packetSendCount, _totalSendBytes, _packetReceived = 0, _totalReceivedBytes;

        protected UDPSocketIOCP _socket;

        protected int _bindPort = 3005;
        /// <summary>
        /// 数据包接收事件，字节流数据包
        /// </summary>
        public RTCPPackageReceive DatagramReceived;
        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get { return _bindPort; } protected set { _bindPort = value; } }
        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime StartTime { get; private set; }
        /// <summary>
        /// 服务器运行状态
        /// </summary>
        public bool Running
        {
            get; set;
        }
        /// <summary>
        /// 最大数据包尺寸 包含所有头信息和有效荷载
        /// </summary>
        internal int MaxTransferPackSize { get => _maxTransferPackSize; set => _maxTransferPackSize = value; }
        /// <summary>
        /// 累计接收到的包的数量
        /// </summary>
        public ulong PacketReceivedCount { get => _packetReceived; }
        /// <summary>
        /// 累计接收的字节数
        /// </summary>
        public ulong TotalReceivedBytes { get => _totalReceivedBytes; }
        /// <summary>
        /// 累计发出的包的数量
        /// </summary>
        public ulong PacketSendCount => _packetSendCount;
        /// <summary>                                                               
        /// 累计发出的字节数                                                                
        /// </summary>
        public ulong TotalSendBytes => _totalSendBytes;

        public RTCPServer()
        {
            _socket = new UDPSocketIOCP();
            _socket.AfterReceiveEnd += Socket_AfterReceiveEnd;
        }
        /// <summary>
        /// 以指定端口启动<see cref="F:Port"/>，如果不配置端口则使用默认端口
        /// </summary>
        public void Start()
        {
            Start(_bindPort);
        }
        /// <summary>
        /// 启动本端服务 默认5683端口
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            _bindPort = port;
            _socket.Start(_bindPort);
            StartTime = DateTime.Now;
            Running = true;
        }
        /// <summary>
        /// 端口下线
        /// </summary>
        public void Shutdown()
        {
            _socket.Shutdown();
            StartTime = DateTime.MinValue;
            Running = false;
        }
        /// <summary>
        /// 数据接收完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <remarks>继承类如果覆盖该事件，则可以接管数据处理</remarks>
        protected virtual void Socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            _packetReceived++;
            _totalReceivedBytes += args.Data != null ? (uint)args.Data.Length : 0;
            if (DatagramReceived != null)
            {
                DatagramReceived(args.IP, args.Port, args.Data);
            }
        }
        /// <summary>
        /// 发送请求消息,此方法为高级方法。
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="pack"></param>
        /// <returns>MessageId</returns>
        /// <remarks>  
        /// </remarks>
        public virtual void SendMessage(string host, int port, AbsRTCPPackage pack)
        {
            byte[] buffer = pack.GetBuffer();
            _totalSendBytes += (ulong)buffer.Length;
            _packetSendCount++;
            _socket.SendTo(buffer, host, port);
        }
    }

    /// <summary>
    /// 包传递回调
    /// </summary>
    /// <param name="host">主机地址</param>
    /// <param name="port">主机端口</param>
    /// <param name="data">字节流</param>
    public delegate void RTCPPackageReceive(string host, int port, byte[] data);
}
