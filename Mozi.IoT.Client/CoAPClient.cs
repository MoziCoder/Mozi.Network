using System;
using System.Collections.Generic;
using System.IO;
using Mozi.IoT.Cache;
using Mozi.IoT.Encode;

//TODO CoAP协议约定拥塞机制由请求方进行自主控制，故请求方需要实现拥塞控制算法

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

        /// <summary>
        /// 服务端回应请求
        /// </summary>
        public MessageTransmit Response;
        /// <summary>
        /// 发起请求
        /// </summary>
        public MessageTransmit Request;

        private byte[] _token;

        /// <summary>
        /// 统一通信Token
        /// </summary>
        public byte[] Token { get => _token; set => _token = value; }

        public CoAPClient()
        {
            _cacheManager = new MessageCacheManager(this);
            _token = _cacheManager.GenerateToken(8);
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
            base.Socket_AfterReceiveEnd(sender, args);
            CoAPPackage pack = CoAPPackage.Parse(args.Data, CoAPPackageType.Response);

            if (pack != null)
            {
                if (Response != null)
                {
                    Response(args.IP, args.Port, pack);
                }
            }
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
            ushort mgId = base.SendMessage(host, port, pack);
            if (Request != null)
            {
                Request(host, port, pack);
            }
            return mgId;
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
                throw new Exception($"分析链接地址:{url}时出错，请检查URL地址是否合法");
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
            return SendMessage(url, msgType, _cacheManager.GenerateMessageId(), _token, method, options, payload);
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
        public ushort Get(string url, CoAPMessageType msgType, IList<CoAPOption> options)
        {
            return SendMessage(url, msgType ?? CoAPMessageType.Confirmable, _cacheManager.GenerateMessageId(), _token, CoAPRequestMethod.Get, options, null);
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
            return SendMessage(url, msgType ?? CoAPMessageType.Confirmable, _cacheManager.GenerateMessageId(), _token, CoAPRequestMethod.Post, options, payload);

        }
        /// <summary>
        /// Post方法,<see cref="Post(string, CoAPMessageType, ContentFormat, IList{CoAPOption}, byte[])"/>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="contentType"></param>
        /// <param name="payload"></param>
        /// <returns>MessageId</returns>
        public ushort Post(string url, CoAPMessageType msgType, ContentFormat contentType, byte[] payload)
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
        public ushort Post(string url, CoAPMessageType msgType, ContentFormat contentType, string text)
        {
            return Post(url, msgType, contentType, null, text);
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
            return SendMessage(url, msgType ?? CoAPMessageType.Confirmable, _cacheManager.GenerateMessageId(), _token, CoAPRequestMethod.Put, options, payload);
        }
        /// <summary>
        /// PUT方法
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="contentType"></param>
        /// <param name="payload"></param>
        /// <returns>MessageId</returns>
        public ushort Put(string url, CoAPMessageType msgType, ContentFormat contentType, byte[] payload)
        {
            return Put(url, msgType, contentType, null, payload);
        }
        /// <summary>
        /// DELETE方法，不推荐使用 不安全
        /// </summary>
        /// <param name="url">地址格式参见<see cref="Get(string, CoAPMessageType, IList{CoAPOption})"/></param>
        /// <param name="msgType">消息类型，默认为<see cref="CoAPMessageType.Confirmable"/></param>
        /// <param name="options"></param>
        /// <returns>MessageId</returns>
        public ushort Delete(string url, CoAPMessageType msgType, IList<CoAPOption> options)
        {
            return SendMessage(url, msgType ?? CoAPMessageType.Confirmable, _cacheManager.GenerateMessageId(), _token, CoAPRequestMethod.Delete, options, null);
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
        internal ushort PostBlock(string url, CoAPMessageType msgType, ContentFormat contentType, int blockSize, byte[] body)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 文件上传，功能还在调试，暂时不用
        /// </summary>
        /// <param name="url"></param>
        /// <param name="msgType"></param>
        /// <param name="contentType"></param>
        /// <param name="path"></param>
        public void PostFile(string url, CoAPMessageType msgType, ContentFormat contentType, string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            uint blockSize = (uint)BlockSize;
            //分片
            int blockCount = (int)Math.Ceiling((double)fs.Length / blockSize);

            CoAPOption cp = new CoAPOption() { Option = CoAPOptionDefine.Size1, Value = new UnsignedIntegerOptionValue { Value = fs.Length } };
            for (int i = 0; i < blockCount - 1; i++)
            {
                byte[] payload = new byte[blockSize];
                fs.Read(payload, 0, payload.Length);
                List<CoAPOption> opts = new List<CoAPOption>();
                CoAPOption opt = new CoAPOption()
                {
                    Option = CoAPOptionDefine.Block1,
                    Value = new BlockOptionValue
                    {
                        Size = (ushort)blockSize,
                        MoreFlag = true,
                        Num = (uint)i,
                    }
                };
                opts.Add(opt);
                opts.Add(cp);
                Post(url, CoAPMessageType.Confirmable, ContentFormat.Stream, opts, payload);
            }
            byte[] last = new byte[blockSize];
            int iReadCount = fs.Read(last, 0, last.Length);
            List<CoAPOption> opts2 = new List<CoAPOption>();
            CoAPOption opt2 = new CoAPOption()
            {
                Option = CoAPOptionDefine.Block1,
                Value = new BlockOptionValue
                {
                    Size = (ushort)blockSize,
                    MoreFlag = false,
                    Num = (uint)(blockCount - 1),
                }
            };
            byte[] lastPayload = new byte[iReadCount];
            Array.Copy(last, lastPayload, lastPayload.Length);
            opts2.Add(opt2);
            opts2.Add(cp);
            Post(url, CoAPMessageType.Confirmable, ContentFormat.Stream, opts2, lastPayload);
            fs.Close();
        }
    }
}
