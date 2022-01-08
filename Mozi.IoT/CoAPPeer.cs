﻿using System;
using System.Collections.Generic;

//UDP使用对等模式工作，客户机和服务器地位对等，且CoAP协议定义的客户机和服务器也是对等关系，角色可以随时互换。
//服务端一般承载较大的并发压力和更复杂的业务逻辑，同时需要更强的算力。客户机则多用于信息采集，数据上报，资料下载等轻量型计算。
//基于上述原因，还是对从协议实现上对客户机和服务器进行角色区分。

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

    /// CoAP基于UDP,可工作的C/S模式，多播，单播，任播（IPV6）
    /// 
    /// C/S模式
    ///     URI格式:
    ///     coap://{host}:{port}/{path}[?{query}]
    /// 默认端口号为5683
    ///     coaps://{host}:{port}/{path}[?{query}]
    /// 默认端口号为5684
    /// 
    /// 多播模式:
    /// IPV4:224.0.1.187
    /// IPV6:FF0X::FD
    /// 
    /// 消息重传
    /// When SendTimeOut between {ACK_TIMEOUT} and (ACK_TIMEOUT * ACK_RANDOM_FACTOR)  then 
    ///     TryCount=0
    /// When TryCount <{MAX_RETRANSMIT} then 
    ///     TryCount++ 
    ///     SendTime*=2
    /// When TryCount >{MAX_RETRANSMIT} then 
    ///     Send(Rest)
    /// <summary>
    /// CoAP对等端
    /// </summary>
    public class CoAPPeer
    {
        protected readonly UDPSocket _socket;

        protected int BindPort = CoAPProtocol.Port;

        /// <summary>
        /// 受支持的请求方法
        /// </summary>
        protected List<CoAPCode> SupportedRequest = new List<CoAPCode> { CoAPRequestMethod.Get, CoAPRequestMethod.Post, CoAPRequestMethod.Put, CoAPRequestMethod.Delete };

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
            get;set;
        }
        public CoAPPeer()
        {
            _socket = new UDPSocket();
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
        /// 启动本端服务
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
        //继承类如果覆盖该事件，则可以接管数据处理
        /// <summary>
        /// 数据接收完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void Socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            
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
        /// 1,如果对协议不够了解，请不要调用。
        /// 2,DOMAIN地址请先转换为IP地址，然后填充到“Uri-Host”选项中
        /// 3,MessageId值由调用方生成并控制
        /// </summary>
        /// <param name="pack"></param>
        /// <returns>MessageId</returns>
        public virtual ushort SendMessage(string host, int port,CoAPPackage pack)
        {
            _socket.SendTo(pack.Pack(), host, port);
            return pack.MesssageId;
        }
    }

    /// <summary>
    /// 消息回调
    /// </summary>
    /// <param name="host"></param>
    /// <param name="msgId"></param>
    /// <param name="rp"></param>
    public delegate void MessageReceive(string host, int port, ushort msgId, CoAPPackage rp);

}
