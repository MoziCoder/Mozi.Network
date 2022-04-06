using System;

// CoAP拥塞机制由请求方进行自主控制，故请求方需要实现拥塞控制算法。
// 拥塞机制交由请求方控制往往会导致服务端遭受流量冲击，因此服务端需要实现一定的约束机制，保证服务正常。

//TODO 服务端的性能需要进一步优化，每秒约能处理2000个数据包，还不具备超高并发能力
//TODO 分块包重组

namespace Mozi.IoT
{
    //TODO 即时响应ACK，延迟响应CON,消息可即时响应也可处理完成后响应，延迟消息需要后端缓存支撑
    //TODO 拥塞算法
    //TODO 安全认证
    //TODO 消息缓存
    //TODO 分块传输 RFC 7959
    //TODO 对象安全

    // CoAP基于UDP,可工作的C/S模式，多播，单播，任播（IPV6）
    // 
    // C/S模式
    //      URI格式:
    //      coap://{host}:{port}/{path}[?{query}]
    // 默认端口号为5683
    //      coaps://{host}:{port}/{path}[?{query}]
    // 默认端口号为5684
    // 
    // 多播模式:
    //      IPV4:224.0.1.187
    //      IPV6:FF0X::FD
    // 
    // 消息重传
    // When SendTimeOut between {ACK_TIMEOUT} and (ACK_TIMEOUT * ACK_RANDOM_FACTOR)  then 
    //     TryCount=0
    // When TryCount <{MAX_RETRANSMIT} then 
    //     TryCount++ 
    //     SendTime*=2
    // When TryCount >{MAX_RETRANSMIT} then 
    //     Send(Rest)
    //     

    /// <summary>
    /// CoAP服务端　默认5683端口
    /// </summary>
    public class CoAPServer : CoAPPeer
    {
        private Cache.MessageCacheManager _cm;

        /// <summary>
        /// 接收到请求
        /// </summary>
        public MessageTransmit RequestReceived;
        /// <summary>
        /// 发起响应请求
        /// </summary>
        public MessageTransmit Responsed;

        private bool _proxyPassed = false;

        private uint maxBodySize=20*1024*1024;

        /// <summary>
        /// 服务端能处理的最大POST资源大小 单位byte
        /// </summary>
        public uint MaxBodySize { get => maxBodySize; set => maxBodySize = value; }
        /// <summary>
        /// 服务器根目录
        /// </summary>
        public string Root = AppDomain.CurrentDomain.BaseDirectory;

        public CoAPServer()
        {
            _cm = new Cache.MessageCacheManager(this);
        }

        //TODO 通过CGI规范转发到WEB解释程序

        /// <summary>
        /// 设置此方法后，所有请求将转至后端HTTP服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        internal void SetProxyPass(string ip, ushort port)
        {
            _proxyPassed = true;
        }
        /// <summary>
        /// 数据接收完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void Socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            base.Socket_AfterReceiveEnd(sender, args);

            if (DatagramReceived != null)
            {
                DatagramReceived(args.IP, args.Port, args.Data);
            }

            CoAPContext ctx = new CoAPContext();
            ctx.ClientAddress = args.IP;
            ctx.ClientPort = args.Port;

            try
            {
                ctx.Request = CoAPPackage.Parse(args.Data, CoAPPackageType.Request);
                if (RequestReceived != null)
                {
                    RequestReceived(args.IP, args.Port, ctx.Request);
                }
                //_cm.Request(args.IP, req);
            }
            catch (Exception)
            {

            }

            try
            {
                //判断是否受支持的方法
                if (IsSupportedRequest(ctx.Request))
                {
                    //接入后端方法
                    ctx.Response = ResourceManager.Default.Invoke(ctx);
                    ctx.Response.Token = ctx.Request.Token;
                    ctx.Response.MesssageId = ctx.Request.MesssageId;
                }
                else
                {
                    ctx.Response = new CoAPPackage()
                    {
                        Code = CoAPResponseCode.MethodNotAllowed,
                        MessageType = CoAPMessageType.Reset
                    };
                }
            }
            catch (Exception ex)
            {
                if (ctx.Response != null)
                {
                    ctx.Response.Code = CoAPResponseCode.BadGateway;
                    ctx.Response.MessageType = CoAPMessageType.Reset;
                }
                else
                {
                    ctx.Response = new CoAPPackage()
                    {
                        Version = 1,
                        MessageType = CoAPMessageType.Reset,
                        Code = CoAPResponseCode.BadGateway,
                    };
                }
            }

            SendMessage(args.IP, args.Port, ctx.Response);

            if (Responsed != null)
            {
                Responsed(args.IP, args.Port, ctx.Response);
            }
        }
    }
}
