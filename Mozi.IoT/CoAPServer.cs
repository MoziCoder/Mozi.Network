﻿using System;
using System.Collections.Generic;
using System.Net;

namespace Mozi.IoT
{

    /// <summary>
    /// CoAP基于UDP,可工作的C/S模式，多播，单播，任播（IPV6）
    /// 
    /// C/S模式
    /// URI格式:
    /// coap://{host}:{port}/{path}[?{query}]
    /// 默认端口号为5683
    /// coaps://{host}:{port}/{path}[?{query}]
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
    /// </summary>
    public class CoAPServer
    {
        private readonly UDPSocket _socket;

        private int _port = CoAPProtocol.Port;

        private List<CoAPCode> _supported = new List<CoAPCode> { CoAPRequestCode.Get, CoAPRequestCode.Post, CoAPRequestCode.Put, CoAPRequestCode.Delete }; 

        /// <summary>
        /// 服务端端口
        /// </summary>
        public int Port { get { return _port; } }

        public DateTime StartTime { get; private set; }

        public CoAPServer()
        {
            _socket = new UDPSocket();
            _socket.AfterReceiveEnd += _socket_AfterReceiveEnd;
        }
        /// <summary>
        /// 以默认端口启动<see cref="F:Port"/>
        /// </summary>
        public void Start()
        {
            Start(_port);
        }
        /// <summary>
        /// 启动网关
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            _port = port;
            _socket.Start(_port);
            StartTime = DateTime.Now;
        }
        /// <summary>
        /// 网关下线
        /// </summary>
        public void Shutdown()
        {
            _socket.Shutdown();
            StartTime = DateTime.MinValue;
        }
        /// <summary>
        /// 数据接收完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void _socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            CoAPPackage pack2=null;

            try
            {
                CoAPPackage pack = CoAPPackage.Parse(args.Data,true);

                pack2 = new CoAPPackage() { 
                    Version = 1, 
                    MessageType = CoAPMessageType.Acknowledgement,
                    Token = pack.Token,
                    MesssageId = pack.MesssageId,
                };

                //进一步处理
                if (IsSupportedMethod(pack))
                {
                    if (pack.MessageType==CoAPMessageType.Confirmable||pack.MessageType == CoAPMessageType.Acknowledgement)
                    {
                        pack2.Code = CoAPResponseCode.Content;
                    }
                }
                else
                {
                    pack2.Code = CoAPResponseCode.MethodNotAllowed;
                }
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                if (pack2 != null)
                {
                    args.Socket.SendTo(pack2.Pack(), new IPEndPoint(IPAddress.Parse(args.IP), args.Port));
                }
            }
        }
        /// <summary>
        /// 是否受支持的请求方法<see cref="CoAPRequestCode"/>
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        private bool IsSupportedMethod(CoAPPackage pack)
        {
            return _supported.Contains(pack.Code);
        }
    }

    public class CoAPClient
    {

    }
}
