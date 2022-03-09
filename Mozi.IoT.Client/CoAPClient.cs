using System;
using System.Collections.Generic;
using Mozi.IoT.Cache;
using Mozi.IoT.Encode;

// CoAP拥塞机制由请求方进行自主控制，故请求方需要实现拥塞控制算法

namespace Mozi.IoT
{
    public delegate void ResponseReceived(string host, int port, CoAPPackage resp);

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

        public ResponseReceived onResponse;

        public CoAPClient()
        {
            _cacheManager = new MessageCacheManager(this);
            _socket = new UDPSocketIOCP();
            _socket.AfterReceiveEnd += Socket_AfterReceiveEnd;
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

            Console.WriteLine($"Request answered{_packetReceived}");

            if (pack != null)
            {
                onResponse(args.IP, args.Port, pack);
            }
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
        public override ushort SendMessage(string host, int port, CoAPPackage pack)
        {
            if (pack.MesssageId == 0)
            {
                pack.MesssageId = _cacheManager.GenerateMessageId();
            }
            return base.SendMessage(host, port, pack);
        }

        /// <summary>
        /// 发送CoAP数据包,此方法为高级方法,如果对协议不够了解，请不要调用
        /// </summary>
        /// <param name="url">地址中的要素会被分解注入到Options中,参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="msgId"></param>
        /// <param name="token"></param>
        /// <param name="method"></param>
        /// <param name="options"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public ushort SendMessage(string url, CoAPMessageType msgType, ushort msgId, byte[] token, CoAPRequestMethod method, IList<CoAPOption> options, byte[] payload)
        {
            CoAPPackage cp = new CoAPPackage
            {
                Code = method,
                Token = token,
                MesssageId = msgId,
                MessageType = msgType ?? CoAPMessageType.Confirmable
            };

            UriInfo uri = UriInfo.Parse(url);

            if (!string.IsNullOrEmpty(uri.Url))
            {

                if (cp.Code == CoAPRequestMethod.Post || cp.Code == CoAPRequestMethod.Put)
                {
                    cp.Payload = payload;
                }
                //注入URI信息
                cp.SetUri(uri);
                //发起通讯
                if (!string.IsNullOrEmpty(uri.Host))
                {
                    if (options != null)
                    {
                        foreach (var opt in options)
                        {
                            cp.SetOption(opt.Option, opt.Value);
                        }
                    }
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
        /// 发送CoAP数据包
        /// </summary>
        /// <param name="url"></param>
        /// <param name="msgType"></param>
        /// <param name="method"></param>
        /// <param name="options"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public ushort SendMessage(string url, CoAPMessageType msgType, CoAPRequestMethod method, IList<CoAPOption> options, byte[] payload)
        {
            return SendMessage(url, msgType, _cacheManager.GenerateMessageId(), _cacheManager.GenerateToken(8),method, options, payload);
        }
        ///<summary>
        /// Get方法 填入指定格式的URI，如果是域名，程序会调用DNS进行解析
        ///</summary>
        /// <param name="url">
        ///     地址中的要素会被分解注入到Options中
        ///     <list type="table">
        ///         <listheader>URI格式:{host}-IPV4地址,IPV6地址,Domain域名;{path}-路径,请使用REST样式路径;{query}为查询参数字符串</listheader>
        ///         <item><term>格式1：</term>coap://{host}[:{port}]/{path}</item>
        ///         <item><term>格式2：</term>coap://{host}[:{port}]/{path}[?{query}]</item>
        ///         <item><term>格式3：</term>coap://{host}[:{port}]/{path1}[/{path2}]...[/{pathN}][?{query}]</item> 
        ///     </list>
        ///      
        /// </param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="options">选项值集合。可设置除<see cref="CoAPOptionDefine.UriHost"/>，<see cref="CoAPOptionDefine.UriPort"/>，<see cref="CoAPOptionDefine.UriPath"/>，<see cref="CoAPOptionDefine.UriQuery"/>之外的选项值</param>
        /// <returns>MessageId</returns>
        public ushort Get(string url,CoAPMessageType msgType,IList<CoAPOption> options)
        {
            return SendMessage(url, msgType ?? CoAPMessageType.Confirmable, _cacheManager.GenerateMessageId(), _cacheManager.GenerateToken(8), CoAPRequestMethod.Get, options, null);
        }
        /// <summary>
        /// Get方法<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/>
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <returns>MessageId</returns>
        public ushort Get(string url, CoAPMessageType msgType)
        {
            return Get(url, msgType, null);
        }
        /// <summary>
        /// Get方法，默认消息类型为<see cref="CoAPMessageType.Confirmable"/>
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <returns>MessageId</returns>
        public ushort Get(string url)
        {
            return Get(url, CoAPMessageType.Confirmable);
        }
        /// <summary>
        /// Post方法
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="contentType"></param>
        /// <param name="options"></param>
        /// <param name="payload"></param>
        /// <returns>MessageId</returns>
        public ushort Post(string url, CoAPMessageType msgType, ContentFormat contentType, IList<CoAPOption> options, byte[] payload)
        {
            if (options == null)
            {
                options = new List<CoAPOption>();
            }

            options.Add(new CoAPOption() { Option = CoAPOptionDefine.ContentFormat, Value = new UnsignedIntegerOptionValue() { Value = contentType.Num } });
            return SendMessage(url, msgType ?? CoAPMessageType.Confirmable, _cacheManager.GenerateMessageId(), _cacheManager.GenerateToken(8), CoAPRequestMethod.Delete, options, payload);

        }
        /// <summary>
        /// Post方法,<see cref="Post(string, CoAPMessageType, ContentFormat, IList{CoAPOption}, byte[])"/>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="contentType"></param>
        /// <param name="payload"></param>
        /// <returns>MessageId</returns>
        public ushort Post(string url, CoAPMessageType msgType, ContentFormat contentType,  byte[] payload)
        {
            return Post(url, msgType, contentType, null, payload);
        }
        /// <summary>
        /// Post方法,<see cref="Post(string, CoAPMessageType, ContentFormat, IList{CoAPOption}, byte[])"/>
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="contentType"></param>
        /// <param name="options"></param>
        /// <param name="text"></param>
        /// <returns>MessageId</returns>
        public ushort Post(string url, CoAPMessageType msgType, ContentFormat contentType, IList<CoAPOption> options, string text)
        {
            return Post(url, msgType, contentType, options, StringEncoder.Encode(text));
        }
        /// <summary>
        /// Post方法,<see cref="Post(string, CoAPMessageType, ContentFormat, IList{CoAPOption}, string)"/>
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="contentType"></param>
        /// <param name="text"></param>
        /// <returns>MessageId</returns>
        public ushort Post(string url, CoAPMessageType msgType, ContentFormat contentType,string text)
        {
            return Post(url, msgType, contentType,null, text);
        }
        /// <summary>
        /// PUT方法 不安全
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="contentType"></param>
        /// <param name="options"></param>
        /// <param name="payload"></param>
        /// <returns>MessageId</returns>
        public ushort Put(string url, CoAPMessageType msgType, ContentFormat contentType, IList<CoAPOption> options, byte[] payload)
        {
            if (options == null)
            {
                options = new List<CoAPOption>();
            }

            options.Add(new CoAPOption() { Option = CoAPOptionDefine.ContentFormat, Value = new UnsignedIntegerOptionValue() { Value = contentType.Num } });
            return SendMessage(url, msgType ?? CoAPMessageType.Confirmable, _cacheManager.GenerateMessageId(), _cacheManager.GenerateToken(8), CoAPRequestMethod.Delete, options, payload);
        }
        /// <summary>
        /// PUT方法
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="contentType"></param>
        /// <param name="payload"></param>
        /// <returns>MessageId</returns>
        public ushort Put(string url, CoAPMessageType msgType, ContentFormat contentType,  byte[] payload)
        {
           return Put(url, msgType, contentType, null,payload);
        }
        /// <summary>
        /// DELETE方法，不推荐使用 不安全
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="options"></param>
        /// <returns>MessageId</returns>
        public ushort Delete(string url, CoAPMessageType msgType,  IList<CoAPOption> options)
        {
            return SendMessage(url, msgType ?? CoAPMessageType.Confirmable, _cacheManager.GenerateMessageId(), _cacheManager.GenerateToken(8), CoAPRequestMethod.Delete,options, null);
        }
        /// <summary>
        /// DELETE方法
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <returns>MessageId</returns>
        public ushort Delete(string url, CoAPMessageType msgType)
        {
            return Delete(url, msgType, null);
        }
        //分块提交
        internal ushort PostBlock(string url, CoAPMessageType msgType, ContentFormat contentType, byte[] payload)
        {
            throw new NotImplementedException();
        }
    }
}
