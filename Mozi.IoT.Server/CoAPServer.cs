using System;

namespace Mozi.IoT
{
    //TODO 即时响应ACK，延迟响应CON,消息可即时响应也可处理完成后响应，延迟消息需要后端缓存支撑
    //TODO 拥塞算法
    //TODO 安全认证
    //TODO 消息缓存
    //TODO 分块传输 RFC 7959
    //TODO 对象安全

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
    /// <summary>
    /// CoAP服务端
    /// </summary>
    public class CoAPServer:CoAPPeer
    {
        private ulong _packageReceived = 0, _totalFlowsize;

        public CoAPServer()
        {
            
        }

        /// <summary>
        /// 设置此方法后，所有请求将转至后端HTTP服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        internal void SetProxyPass(string ip,ushort port)
        {

        }
        /// <summary>
        /// 数据接收完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void Socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            CoAPPackage pack2=null;
            _packageReceived++;
            _totalFlowsize = _totalFlowsize+(uint)args.Data.Length;
            Console.WriteLine($"Received package:{_packageReceived} pic,current:{args.Data.Length}bytes,total:{_totalFlowsize}bytes");

            //try
            //{
                CoAPPackage pack = CoAPPackage.Parse(args.Data,true);

                pack2 = new CoAPPackage()
                {
                    Version = 1,
                    MessageType = CoAPMessageType.Acknowledgement,
                    Token = pack.Token,
                    MesssageId = pack.MesssageId,
                };

                //判断是否受支持的方法
                if (IsSupportedRequest(pack))
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

                //检查分块

                //检查内容类型

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            //finally
            //{
                if (pack2 != null)
                {
                     _socket.SendTo(pack2.Pack(), args.IP, args.Port);
                }
            //}
        }
    }
}
