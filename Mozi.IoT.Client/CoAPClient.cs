using System;
using Mozi.IoT.Cache;
using Mozi.IoT.Encode;

// CoAP拥塞机制由请求方进行自主控制，故请求方需要实现拥塞控制算法

namespace Mozi.IoT
{
    /// <summary>
    /// CoAP客户端
    /// </summary>
    public class CoAPClient : CoAPPeer
    {
        private bool _randomPort = true;
        
        private CoAPTransmissionConfig _transConfig = new CoAPTransmissionConfig();

        private MessageCacheManager _cacheManager;

        private ulong _packetReceived;

        //private ushort _remotePort = CoAPProtocol.Port;
        //private string _remotehost = "";

        ///// <summary>
        ///// 远端服务器地址
        ///// </summary>
        //public string RemoteAddress { get { return _remotehost; } protected set { _remotehost = value; } }

        ///// <summary>
        ///// 远端服务器端口
        ///// </summary>
        //public ushort RemotePort { get { return _remotePort; } protected set { _remotePort = value; } }

        public CoAPClient() 
        {
            _cacheManager = new MessageCacheManager(this);
            //配置本地服务口地址
        }
        /// <summary>
        /// 设置本地端口，默认为<see cref=" CoAPProtocol.Port"/>
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public CoAPClient SetPort(int port)
        {
            BindPort = port;
            _randomPort = false;
            return this;
        }
        /// <summary>
        /// 数据接收完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void Socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            _packetReceived++;
            //try
            //{
            CoAPPackage pack = CoAPPackage.Parse(args.Data, CoAPPackageType.Response);

            Console.WriteLine($"Package answered{_packetReceived}");
            //pack2 = new CoAPPackage()
            //{
            //    Version = 1,
            //    MessageType = CoAPMessageType.Acknowledgement,
            //    Token = pack.Token,
            //    MesssageId = pack.MesssageId,
            //};

            ////判断是否受支持的方法
            //if (IsSupportedRequest(pack))
            //{
            //    if (pack.MessageType == CoAPMessageType.Confirmable || pack.MessageType == CoAPMessageType.Acknowledgement)
            //    {
            //        pack2.Code = CoAPResponseCode.Content;
            //    }
            //}
            //else
            //{
            //    pack2.Code = CoAPResponseCode.MethodNotAllowed;
            //}

            ////检查分块

            ////检查内容类型

            ////}
            ////catch (Exception ex)
            ////{
            ////    Console.WriteLine(ex.Message);
            ////}
            ////finally
            ////{
            //if (pack2 != null)
            //{
            //    _socket.SendTo(pack2.Pack(), args.IP, args.Port);
            //}
            ////}
        }
        /// <summary>
        /// 发送请求消息,此方法为高级方法。
        /// 1,如果对协议不够了解，请不要调用。
        /// 2,此方法不会调用DNS解析域名，DOMAIN地址请先转换为IP地址，然后填充到“Uri-Host”选项中
        /// 3,MessageId由拥塞管理器生成
        /// 参见<see cref="CoAPPeer.SendMessage(string, int, CoAPPackage)"/>
        /// </summary>
        /// <param name="pack"></param>
        /// <returns>MessageId</returns>
        public override ushort SendMessage(string host,int port,CoAPPackage pack)
        {
            if (pack.MesssageId == 0)
            {
                pack.MesssageId = _cacheManager.GenerateMessageId();
            }
            return base.SendMessage(host, port, pack);
        }

        /// <summary>
        /// 注入URL相关参数,domain,port,paths,queries
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cp"></param>
        private void PackUrl(ref UriInfo uri,ref CoAPPackage cp)
        {
            //注入域名
            if (!string.IsNullOrEmpty(uri.Domain))
            {
                cp.SetOption(CoAPOptionDefine.UriHost, uri.Domain);
            }
            //注入端口号
            if (uri.Port > 0 && !(uri.Port == CoAPProtocol.Port || uri.Port == CoAPProtocol.SecurePort))
            {
                cp.SetOption(CoAPOptionDefine.UriPort, (uint)uri.Port);
            }

            //注入路径
            for (int i = 0; i < uri.Paths.Length; i++)
            {
                cp.SetOption(CoAPOptionDefine.UriPath, uri.Paths[i]);
            }
            //注入查询参数
            for (int i = 0; i < uri.Queries.Length; i++)
            {
                cp.SetOption(CoAPOptionDefine.UriQuery, uri.Queries[i]);
            }
        }

        ///<summary>
        /// Get提交 填入指定格式的URI，如果是域名，程序会调用DNS进行解析
        ///</summary>
        /// <param name="url">
        ///     <list type="table">
        ///         <listheader>URI格式:{host}-IPV4地址,IPV6地址,Domain域名;{path}-路径,请使用REST样式路径;{query}为查询参数字符串</listheader>
        ///         <item><term>格式1：</term>coap://{host}[:{port}]/{path}</item>
        ///         <item><term>格式2：</term>coap://{host}[:{port}]/{path}[?{query}]</item>
        ///         <item><term>格式3：</term>coap://{host}[:{port}]/{path1}[/{path2}]...[/{pathN}][?{query}]</item> 
        /// </list>
        /// </param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <returns>MessageId</returns>
        public ushort Get(string url,CoAPMessageType msgType)
        {

            CoAPPackage cp = new CoAPPackage
            {
                Code = CoAPRequestMethod.Get,
                Token = CacheControl.GenerateToken(8),
                MesssageId = _cacheManager.GenerateMessageId(),
                MessageType = msgType ?? CoAPMessageType.Confirmable
            };

            UriInfo uri = UriInfo.Parse(url);

            if (!string.IsNullOrEmpty(uri.Url))
            {
                PackUrl(ref uri,ref cp);
                //发起通讯
                if (!string.IsNullOrEmpty(uri.Host))
                {
                    SendMessage(uri.Host, uri.Port == 0 ? CoAPProtocol.Port : uri.Port, cp);
                }
                else
                {
                    throw new Exception($"DNS无法解析指定的域名:{uri.Domain}");
                }
            }
            else
            {
                throw new Exception($"本地无法解析指定的链接地址:{url}");
            }
            return cp.MesssageId;
        }
        /// <summary>
        /// Get方法，默认消息类型为<see cref="CoAPMessageType.Confirmable"/>
        /// </summary>
        /// <param name="url"></param>
        /// <returns>MessageId</returns>
        public ushort Get(string url)
        {
            return Get(url, CoAPMessageType.Confirmable);
        }
        /// <summary>
        /// Post方法
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType)"/></param>
        /// <param name="msgType"></param>
        /// <param name="contentType"></param>
        /// <param name="postBody"></param>
        /// <returns></returns>
        public ushort Post(string url, CoAPMessageType msgType, ContentFormat contentType,byte[] postBody)
        {
            CoAPPackage cp = new CoAPPackage
            {
                Code = CoAPRequestMethod.Post,
                Token = CacheControl.GenerateToken(8),
                MesssageId = _cacheManager.GenerateMessageId(),
                MessageType = msgType ?? CoAPMessageType.Confirmable
            };

            UriInfo uri = UriInfo.Parse(url);

            if (!string.IsNullOrEmpty(uri.Url))
            {
                PackUrl(ref uri, ref cp);
                
                cp.SetContentType(contentType);

                cp.Payload = postBody;
                
                //发起通讯
                if (!string.IsNullOrEmpty(uri.Host))
                {
                    SendMessage(uri.Host, uri.Port == 0 ? CoAPProtocol.Port : uri.Port, cp);
                }
                else
                {
                    throw new Exception($"DNS无法解析指定的域名:{uri.Domain}");
                }
            }
            else
            {
                throw new Exception($"本地无法解析指定的链接地址:{url}");
            }
            return cp.MesssageId;
        }
        
        //分块提交
        internal ushort PostBlock(string url, CoAPMessageType msgType, ContentFormat contentType, byte[] postBody)
        {
            throw new NotImplementedException();
        }
        public ushort Post(string url, CoAPMessageType msgType, ContentFormat contentType, string text)
        {
            return Post(url, msgType, contentType, StringEncoder.Encode(text));
        }
        //TODO 是否会出现安全问题
        private void Put(string url)
        {

        }
        //TODO 是否会出现安全问题
        private void Delete(string url)
        {

        }
    }
}
