using System;
using System.Collections.Generic;

// UDP使用对等模式工作，客户机和服务器地位对等，且CoAP协议定义的客户机和服务器也是对等关系，角色可以随时互换。
// 服务端一般承载较大的并发压力和更复杂的业务逻辑，同时需要更强的算力。客户机则多用于信息采集，数据上报，资料下载等轻量型计算。
// 基于上述原因，还是应从实现上对客户机和服务器进行角色区分。

namespace Mozi.IoT
{

    //TODO 即时响应ACK，延迟响应CON,消息可即时响应也可处理完成后响应，延迟消息需要后端缓存支撑
    //TODO 拥塞算法
    //TODO 安全认证
    //TODO 消息缓存
    //TODO 分块传输 RFC 7959
    //TODO 对象安全
    //TODO 映射CoAP到Http
    //TODO 观察者模式 观察者模式功能比较弱，是否考虑不实现？
    //TODO Extend Token Length RFC8974

    // CoAP基于UDP,可工作的C/S模式，多播，单播，任播（IPV6）
    // 
    // C/S模式
    //     URI格式:
    //     coap://{host}:{port}/{path}[?{query}]
    // 默认端口号为5683
    //     coaps://{host}:{port}/{path}[?{query}]
    // 默认端口号为5684
    // 
    // 多播模式:
    //     IPV4:224.0.1.187
    //     IPV6:FF0X::FD
    // 
    // 消息重传
    // When SendTimeOut between {ACK_TIMEOUT} and (ACK_TIMEOUT * ACK_RANDOM_FACTOR)  then 
    //     TryCount=0
    // When TryCount <{MAX_RETRANSMIT} then 
    //     TryCount++ 
    //     SendTime*=2
    // When TryCount >{MAX_RETRANSMIT} then 
    //     Send(Rest)

    /// <summary>
    /// CoAP对等端
    /// </summary>
    public class CoAPPeer
    {
        /// <summary>
        /// 最大数据包尺寸 包含所有头信息和有效荷载 Byte
        /// </summary>
        private int _maxTransferPackSize = 512;

        private int _blockSize = 128;

        private ulong _packetSendCount, _totalSendBytes, _packetReceived = 0, _totalReceivedBytes;

        protected UDPSocketIOCP _socket;

        protected int BindPort = CoAPProtocol.Port;
        /// <summary>
        /// 最小分块大小,单位Byte
        /// </summary>
        public const int MinBlockSize = 16;
        /// <summary>
        /// 最大分块大小,单位Byte
        /// </summary>
        public const int MaxBlockSize = 2048;
        /// <summary>
        /// 当前端默认采用块大小,默认值为128bytes,单位Byte
        /// </summary>
        /// <remarks>在通讯两方没有进行协商的情况下，默认采用此值作为分块大小。取值区间为{<see cref="MinBlockSize"/>~<see cref="MaxBlockSize"/>}</remarks>
        public int BlockSize { get { return _blockSize; } set { _blockSize = value; } }
        /// <summary>s
        /// 受支持的请求方法
        /// </summary>
        protected List<CoAPCode> SupportedRequest = new List<CoAPCode> { CoAPRequestMethod.Get, CoAPRequestMethod.Post, CoAPRequestMethod.Put, CoAPRequestMethod.Delete };
        /// <summary>
        /// 数据包接收事件，字节流数据包
        /// </summary>
        public PackageReceive DatagramReceived;
        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get { return BindPort; } protected set { BindPort = value; } }
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

        public CoAPPeer()
        {
            _socket = new UDPSocketIOCP();
            _socket.AfterReceiveEnd += Socket_AfterReceiveEnd;
        }
        /// <summary>
        /// 以指定端口启动<see cref="F:Port"/>，如果不配置端口则使用默认端口
        /// </summary>
        public void Start()
        {
            Start(BindPort);
        }
        /// <summary>
        /// 启动本端服务 默认5683端口
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            BindPort = port;
            _socket.Start(BindPort);
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
        /// 是否受支持的请求方法<see cref="CoAPRequestMethod"/>
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        protected bool IsSupportedRequest(CoAPPackage pack)
        {
            return SupportedRequest.Contains(pack.Code);
        }
        /// <summary>
        /// 发送请求消息,此方法为高级方法。
        /// </summary>
        /// <param name="pack"></param>
        /// <returns>MessageId</returns>
        /// <remarks>
        ///     1,如果对协议不够了解，请不要调用。
        ///     2,DOMAIN地址请先转换为IP地址，然后填充到“Uri-Host”选项中
        ///     3,MessageId值由调用方生成并控制   
        /// </remarks>
        public virtual ushort SendMessage(string host, int port, CoAPPackage pack)
        {
            byte[] buffer = pack.Pack();
            _totalSendBytes += (ulong)buffer.Length;
            _packetSendCount++;
            _socket.SendTo(buffer, host, port);
            return pack.MesssageId;
        }
    }
    /// <summary>
    /// 包传递回调
    /// </summary>
    /// <param name="host">主机地址</param>
    /// <param name="port">主机端口</param>
    /// <param name="data">字节流</param>
    public delegate void PackageReceive(string host, int port, byte[] data);

    /// <summary>
    /// 消息回调
    /// </summary>
    /// <param name="host">主机地址</param>
    /// <param name="port">主机端口</param>
    /// <param name="rp">协议包</param>
    public delegate void MessageTransmit(string host, int port, CoAPPackage rp);
}
