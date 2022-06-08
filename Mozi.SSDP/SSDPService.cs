using System;
using System.Net;
using System.Threading;
using Mozi.HttpEmbedded;
using Mozi.HttpEmbedded.Encode;

namespace Mozi.SSDP
{
    //ST:Search Target
    //NT:Notification Type
    //NTS:Notification Sub Type
    //USN:Unique Service Name
    //MX: Maximum wait time in seconds. Should be between 1 and 120 inclusive
    /// <summary>
    /// Notify ssdp:alive接收委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="pack"></param>
    /// <param name="host"></param>
    public delegate void NotifyAliveReceived(object sender,AlivePackage pack,string host);
    /// <summary>
    /// Notify ssdp:byebye接收委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="pack"></param>
    /// <param name="host"></param>
    public delegate void NotifyByebyeReceived(object sender, ByebyePackage pack, string host);
    /// <summary>
    /// Notify upnp:update 接收委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="pack"></param>
    /// <param name="host"></param>
    public delegate void NotifyUpdateReceived(object sender, UpdatePackage pack, string host);
    /// <summary>
    /// M-SEARCH 接收委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="pack"></param>
    /// <param name="host"></param>
    public delegate void SearchReceived(object sender,SearchPackage pack,string host);
    /// <summary>
    /// M-SEARCH 响应委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="resp"></param>
    /// <param name="host"></param>
    public delegate void SearchResponsed(object sender, SearchResponsePackage resp, string host);
    /// <summary>
    /// POST 接收委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="req"></param>
    /// <param name="host"></param>
    public delegate void PostReceived(object sender, HttpRequest req, string host);
    /// <summary>
    /// HTTP 响应委托 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="resp"></param>
    /// <param name="host"></param>
    public delegate void HttpResponsed(object sender, HttpResponse resp, string host);
    /// <summary>
    /// 接收到任何消息时都会触发
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void MessageReceived(object sender, DataTransferArgs args);

    //public delegate void SubscribeReceived(object sender, HttpRequest pack,string host);
    //public delegate void UnSubscribedReceived(object sender, UnSubscribedPackage pack, string host);
    //public delegate void ControlActionReceived(object sender,ControlActionPackage pack,string host);
    //internal delegate void EventMessageReceive(object sender, EventPackage pack, string host);

    //TODO 进一步完善SSDP协议并进行良好的封装

    /// <summary>
    /// SSDP协议实现
    /// <para>
    ///     功能包含：发现，在线广播，离线广播
    /// </para>
    /// <para>
    /// UPNP 4大部分
    /// --Discovery
    ///     --Notify:alive
    ///     --Notify:bybye
    ///     --Search
    /// --Description
    ///     --Device Description
    ///     --Service Description
    /// --Control
    ///     --Action
    ///     --Query
    /// --Eventing
    ///     --Subscribe
    ///     --UnSubscribe
    /// </para>
    /// </summary>
    public class SSDPService : ISSDPService
    {
        private bool _initialized = false;

        private UDPSocket _socket;
        private Timer _timer;
        private IPEndPoint _remoteEP;

        private string _multicastGroupAddress = SSDPProtocol.MulticastAddress;
        private int _multicastGroupPort = SSDPProtocol.MulticastPort;
        private IPAddress _bindingAddress = IPAddress.Any;

        private string _server = "Mozi/1.4.3 UPnP/2.0 Mozi.SSDP/1.4.3";
        //本地服务唯一信息
        private USNDesc _uniqueServiceName = new USNDesc()
        {
            DeviceId= UUID.Generate(),
            Domain="mozicoder.org",
            ServiceName="simplehost",
            ServiceType=ServiceCategory.Device,
            Version=1
        };
        private bool autoEchoSearch=false;

        private SSDPCacheCollection _sc = SSDPCacheCollection.Instance;

        /// <summary>
        /// 设备描述文档地址
        /// </summary>
        private string _deviceDescPath = "";

        #region
        /// <summary>
        /// 服务器描述值
        /// </summary>
        public string Server { get { return _server; } set { _server = value; } }
        /// <summary>
        /// 缓存的设备信息
        /// </summary>
        public SSDPCacheCollection Cache
        {
            get
            {
                return _sc;
            }
        }
        /// <summary>
        /// 设备描述信息地址
        /// </summary>
        public string DescriptionLocation { get { return _deviceDescPath; } set { _deviceDescPath = value; } }
        /// <summary>
        /// 程序默认的域信息，用于绑定NT,ST,USN，以及设备/服务描述文档中的相关信息
        /// </summary>
        public string Domain
        {
            get
            {
                return _uniqueServiceName.Domain;
            }
            set
            {
                _uniqueServiceName.Domain = value;
            }
        }
        #endregion

        /// <summary>
        /// 广播消息周期,单位：毫秒
        /// </summary>
        public int NotificationPeriod = 30 * 1000;
        ///// <summary>
        ///// 查询周期
        ///// </summary>
        //public int DiscoverPeriod = 30 * 1000;
        /// <summary>
        /// 缓存时间
        /// </summary>
        public int CacheTimeout = 3600;
        /// <summary>
        /// 唯一标识符 UUID
        /// </summary>
        public string UniqueID
        {
            get => _uniqueServiceName.DeviceId; set => _uniqueServiceName.DeviceId = value;
        }
        /// <summary>
        /// 本地服务唯一信息
        /// </summary>
        public USNDesc UniqueServiceName
        {
            get
            {
                return _uniqueServiceName;
            }
            set
            {
                _uniqueServiceName = value;
                PackDefaultAlive.USN = value;
                PackDefaultByebye.USN = value;
            }
        }
        /// <summary>
        /// 自动响应搜索请求
        /// </summary>
        public bool AutoEchoSearch
        {
            get => autoEchoSearch; set => autoEchoSearch = value;
        }
        /// <summary>
        /// 是否接受回环地址消息
        /// <para>
        /// 激活定时服务前启用此参数
        /// </para>
        /// </summary>
        public bool AllowLoopbackMessage { get; set; }

        //224.0.0.0～224.0.0.255为预留的组播地址（永久组地址），地址224.0.0.0保留不做分配，其它地址供路由协议使用；
        //224.0.1.0～224.0.1.255是公用组播地址，可以用于Internet；
        //224.0.2.0～238.255.255.255为用户可用的组播地址（临时组地址），全网范围内有效；
        //239.0.0.0～239.255.255.255为本地管理组播地址，仅在特定的本地范围内有效。
        /// <summary>
        /// 组播地址
        /// <para>
        /// 标准地址为 <see cref="SSDPProtocol.MulticastAddress"/> | <see cref="SSDPProtocol.MulticastAddressIPv6"/>
        /// 239.0.0.0～239.255.255.255为本地管理组播地址，仅在特定的本地范围内有效。
        /// </para>
        /// </summary>
        public string MulticastAddress
        {
            get
            {
                return _multicastGroupAddress;
            }
            set
            {
                //值校验
                _multicastGroupAddress = value;
                ApplyMulticastAddress(MulticastAddress, MulticastPort);
            }
        }
        /// <summary>
        /// 组播端口
        /// <para>
        /// 标准端口为 <see cref="SSDPProtocol.MulticastPort"/>
        /// </para>
        /// </summary>
        public int MulticastPort
        {
            get
            {
                return _multicastGroupPort;
            }
            set
            {
                _multicastGroupPort = value;
                ApplyMulticastAddress(MulticastAddress, MulticastPort);
            }
        }
        /// <summary>
        /// 绑定的本地IP地址
        /// </summary>
        public IPAddress BindingAddress
        {
            get
            {
                return _bindingAddress;
            }
            set
            {
                _bindingAddress = value;
            }
        }
        /// <summary>
        /// 默认在线消息包
        /// </summary>
        public AlivePackage PackDefaultAlive = new AlivePackage()
        {
            CacheTimeout = 3600,
            Location = "",
            Server = "",
            NT = TargetDesc.All,
            USN = new USNDesc() { IsRootDevice = true },
        };
        /// <summary>
        /// 默认离线消息包
        /// </summary>
        public ByebyePackage PackDefaultByebye = new ByebyePackage()
        {
            NT = TargetDesc.All,
        };
        /// <summary>
        /// 服务器运行状态
        /// </summary>
        public bool Running
        {
            get; set;
        }
        /// <summary>
        /// 是否正在进行公告广播
        /// </summary>
        public bool OnAdvertise
        {
            get; set;
        }
        /// <summary>
        /// 搜索程序定义的默认类型
        /// urn:{domain}:device:1
        /// </summary>
        public TargetDesc DeviceDefault = new TargetDesc();
        /// <summary>
        /// 收到ssdp:alive时触发
        /// </summary>
        public NotifyAliveReceived OnNotifyAliveReceived;
        /// <summary>
        /// 收到ssdp:byebye时触发
        /// </summary>
        public NotifyByebyeReceived OnNotifyByebyeReceived;
        /// <summary>
        /// 收到m-search时触发
        /// </summary>
        public SearchReceived OnSearchReceived;
        /// <summary>
        /// 收到upnp:update时触发
        /// </summary>
        public NotifyUpdateReceived OnNotifyUpdateReceived;
        /// <summary>
        /// 收到HTTP/1.1 200 OK,且头属性中带有"ST"字段 时触发
        /// </summary>
        public SearchResponsed OnSearchResponsed;
        /// <summary>
        /// 收到HTTP/1.2 {status} {statusname} 触发
        /// </summary>
        public HttpResponsed OnHttpResponsed;
        /// <summary>
        /// 收到所有POST请求都会触发
        /// </summary>
        public PostReceived OnPostReceived;

        /// <summary>
        /// 原始数据包解析
        /// <para>如果内置的解析结果不能满足应用需求，可以使用该事件进行数据解析</para>
        /// </summary>
        public MessageReceived OnMessageReceived;
        ///// <summary>
        ///// 订阅消息触发
        ///// </summary>
        //public SubscribeReceived OnSubscribeReceived;
        ///// <summary>
        ///// 取消订阅触发
        ///// </summary>
        //public UnSubscribedReceived OnUnSubscribedReceived;
        ///// <summary>
        ///// 控制信息接收触发
        ///// </summary>
        //public ControlActionReceived OnControlActionReceived;
        /// <summary>
        /// 构造函数
        /// <para>
        /// 从实例创建开始就可以开始接受组播数据
        /// </para>
        /// </summary>
        public SSDPService()
        {
            _socket = new UDPSocket();
            _socket.AfterReceiveEnd += Socket_AfterReceiveEnd;
            _remoteEP = new IPEndPoint(IPAddress.Parse(SSDPProtocol.MulticastAddress), SSDPProtocol.MulticastPort);

            _timer = new Timer(TimeoutCallback, null, Timeout.Infinite, Timeout.Infinite);

            //初始化数据包
            PackDefaultAlive.USN = UniqueServiceName;
            PackDefaultAlive.Server = _server;
            PackDefaultByebye.USN = UniqueServiceName;

        }
        /// <summary>
        /// 设置组播地址和端口号
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public void ApplyMulticastAddress(string address, int port)
        {
            _multicastGroupAddress = address;
            _multicastGroupPort = port;
            _remoteEP = new IPEndPoint(IPAddress.Parse(address), port);
            var host = string.Format("{0}:{1}", address, port);
            PackDefaultAlive.HOST = host;
            PackDefaultByebye.HOST = host;
        }
        /// <summary>
        /// 数据接收时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void Socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            //DONE 如何进行多包分割？

            HandleMessage(args);
            if (OnMessageReceived != null)
            {
                OnMessageReceived(this, args);
            }
            Console.WriteLine("==**************{0}*************==", args.IP);
            Console.WriteLine("{1}", args.IP, System.Text.Encoding.UTF8.GetString(args.Data));
            Console.WriteLine("==***************************************==");

        }
        /// <summary>
        /// 包解析
        /// </summary>
        private void HandleMessage(DataTransferArgs args)
        {
            try
            {
                HttpRequest request = HttpRequest.Parse(args.Data);
                RequestMethod method = request.Method;
                //Notify
                if (method == RequestMethodUPnP.NOTIFY)
                {
                    var nts = request.Headers.GetValue("NTS");

                    //TODO notify event

                    //ssdp:alive
                    if (nts == SSDPType.Alive.ToString())
                    {
                        var pack = AlivePackage.Parse(request);
                        if (pack != null && OnNotifyAliveReceived != null)
                        {
                            OnNotifyAliveReceived(this, pack, args.IP);
                        }
                    }
                    //ssdp:byebye
                    else if (nts == SSDPType.Byebye.ToString())
                    {
                        var pack = ByebyePackage.Parse(request);
                        if (pack != null && OnNotifyByebyeReceived != null)
                        {
                            OnNotifyByebyeReceived(this, pack, args.IP);
                        }
                    //upnp:update
                    }
                    else if (nts == SSDPType.Update.ToString())
                    {

                        var pack = UpdatePackage.Parse(request);
                        if (pack != null && OnNotifyUpdateReceived != null)
                        {
                            OnNotifyUpdateReceived(this, pack, args.IP);
                        }
                    //ST: upnp:event NTS: upnp:propchange
                    }
                    //else if (nts == SSDPType.PropChange.ToString()){
                    //    var 
                    //}
                }
                //MS-SEARCH
                else if (method == RequestMethodUPnP.MSEARCH)
                {
                    var pack = SearchPackage.Parse(request);
                    if (pack != null && OnSearchReceived != null)
                    {
                        OnSearchReceived(this, pack, args.IP);
                        //TODO 按UPNP规范自动响应搜索请求
                        if (autoEchoSearch)
                        {
                            var st = pack.ST;

                            if (st.IsAll||(st.IsRootDevice && UniqueServiceName.IsRootDevice)|| UniqueID.Equals(st.DeviceId)|| UniqueServiceName.ToURN().Equals(st.ToString()))
                            {
                                ResponseSearch(new SearchResponsePackage()
                                {
                                    CacheTimeout = CacheTimeout,
                                    ST = pack.ST,
                                    Location = _deviceDescPath,
                                    Server = _server,
                                    USN=UniqueServiceName
                                });
                            }
                        }
                    }
                }
                ////TODO SUBSCRIBE
                ////event SUBSCRIBE
                //else if (method == RequestMethodUPnP.SUBSCRIBE)
                //{
                //    if (OnSubscribeReceived != null)
                //    {
                //        OnSubscribeReceived(this, request, args.IP);
                //    }
                //}
                ////TODO UNSCRIBE
                ////event UNSUBSCRIBE
                //else if (method == RequestMethodUPnP.UNSUBSCRIBE)
                //{
                //    var pack = UnSubscribedPackage.Parse(request);
                //    if (pack != null && OnUnSubscribedReceived!=null)
                //    {
                //        OnUnSubscribedReceived(this, pack, args.IP);
                //    }
                //}
                //Control
                else if (method == RequestMethod.POST)
                {
                    //if(request.Headers.Contains("SOAPACTION"))
                    //{
                    //    ControlActionPackage pack = ControlActionPackage.Parse(request);
                    //    if (pack != null && OnControlActionReceived != null)
                    //    {
                    //        OnControlActionReceived(this, pack, args.IP);
                    //    }
                    //}
                    if (OnPostReceived != null)
                    {
                        OnPostReceived(this, request, args.IP);
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                try
                {
                    HttpResponse resp = HttpResponse.Parse(args.Data);
                    //成功
                    if (resp.Status == StatusCode.Success)
                    {
                        //判断包类型
                        //M-SEARCH response
                        var usn=resp.Headers.GetValue("USN");
                        if (usn != null)
                        {
                            var usndesc = USNDesc.Parse(usn);
                            var cachecontrol = resp.Headers.GetValue("CACHE-CONTROL");
                            var cc = cachecontrol.Split(new char[] { '=' });
                            _sc.Add(new SSDPCache
                            {
                                Host = $"{args.IP}:{_multicastGroupPort}",
                                USN = usndesc,
                                AddTime = DateTime.Now.ToUniversalTime(),
                                Server = resp.Headers.GetValue("SERVER"),
                                Location = resp.Headers.GetValue("LOCATION"),
                                Expiration = cc.Length > 1 ? int.Parse(cc[1]) : -1
                            });
                        }
                    }
                    else
                    {

                    }
                    if (resp.Headers.Contains(SSDPHeader.St.PropertyName))
                    {
                        SearchResponsePackage sr = SearchResponsePackage.Parse(resp);
                        if (sr!=null&&OnSearchResponsed != null)
                        {
                            OnSearchResponsed(this, sr, args.IP);
                        }
                    }
                    //HTTP响应触发 http/1.1 200 OK
                    if (OnHttpResponsed != null)
                    {
                        OnHttpResponsed(this, resp, args.IP);
                    }
                }
                catch (Exception ex2)
                {
                    int a = 1;
                }
            }
        }
        /// <summary>
        /// 激活服务
        /// <para>
        /// 使服务器加入到多播组中，并侦听消息
        /// </para>
        /// </summary>
        /// <returns></returns>
        public void Activate()
        {
            _socket.AllowLoopbackMessage = AllowLoopbackMessage;
            _socket.BindingAddress = BindingAddress;
            _socket.Start(_multicastGroupAddress, _multicastGroupPort);
            _initialized = true;
            Running = true;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public void Inactivate()
        {
            Running = false;
            _socket.Shutdown();
        }
        /// <summary>
        /// 广播ssdp:alive信息 定时发送ssdp:alive广播
        /// </summary>
        /// <returns></returns>
        public void StartAdvertise()
        {
            //是否接受回环消息
            _timer.Change(0, NotificationPeriod);
            OnAdvertise = true;
        }
        /// <summary>
        /// 停止广播
        /// </summary>
        /// <returns></returns>
        public void StopAdvertise()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            OnAdvertise = false;
        }
        //M-SEARCH * HTTP/1.1
        //S: uuid:ijklmnop-7dec-11d0-a765-00a0c91e6bf6
        //Host: 239.255.255.250:1900
        //MAN: "ssdp:discover"
        //ST: ge:fridge
        //      -ssdp:all 搜索所有设备和服务
        //      -upnp:rootdevice 仅搜索网络中的根设备
        //      -uuid:device-UUID 查询UUID标识的设备
        //      -urn:schemas-upnp-org:device:device-Type:version 查询device-Type字段指定的设备类型，设备类型和版本由UPNP组织定义。
        //      -urn:schemas-upnp-org:service:service-Type:version 查询service-Type字段指定的服务类型，服务类型和版本由UPNP组织定义。
        //MX: {seconds}
        //      --{seconds} in [1,120]

        /// <summary>
        /// 发送查询消息
        /// </summary>
        /// <param name="pk"></param>
        public void Search(SearchPackage pk)
        {
            HttpRequest request = new HttpRequest();
            request.SetPath("*").SetMethod(RequestMethodUPnP.MSEARCH);
            request.SetHeaders(pk.GetHeaders());
            byte[] data = request.GetBuffer();
            SendMessage(data);
        }
        /// <summary>
        /// 查找设备简化方法，原型参看<see cref="Search(SearchPackage)"/>,此处建议发3次包，避免终端没有收到信息
        /// </summary>
        /// <param name="desc"></param>
        public void Search(TargetDesc desc)
        {
            SearchPackage pk = new SearchPackage();
            pk.MX = 1;
            pk.UserAgent = _server;
            pk.ST = desc;

            HttpRequest request = new HttpRequest();
            request.SetPath("*").SetMethod(RequestMethodUPnP.MSEARCH);
            request.SetHeaders(pk.GetHeaders());
            byte[] data = request.GetBuffer();
            SendMessage(data);
        }
        /// <summary>
        /// 发送查询消息,参见<see cref="TargetDesc"/>
        /// <list type="table">
        /// <listheader>查询目标字符串格式约定如下</listheader>
        ///     -ssdp:all 搜索所有设备和服务
        ///      -upnp:rootdevice 仅搜索网络中的根设备
        ///      -uuid:device-UUID 查询UUID标识的设备
        ///      -urn:schemas-upnp-org:device:device-Type:version 查询device-Type字段指定的设备类型，设备类型和版本由UPNP组织定义。
        ///      -urn:schemas-upnp-org:service:service-Type:version 查询service-Type字段指定的服务类型，服务类型和版本由UPNP组织定义。
        ///</list>
        /// </summary>
        /// <param name="st"></param>
        public void Search(string st)
        {
            TargetDesc desc = null;
            try
            {
                desc = TargetDesc.Parse(st);
            }
            catch
            {

            }

            if (desc != null)
            {
                Search(desc);
            }
            else
            {
                throw new Exception($"ST:{st},查询目标字符串不符合定义规范，请查看本方法的注释以了解命名规范");
            }
        }
        //NOTIFY * HTTP/1.1     
        //HOST: 239.255.255.250:1900    
        //CACHE-CONTROL: max-age = {seconds}   
        //      --{seconds}>=1800s
        //LOCATION: URL for UPnP description for root device
        //NT: search target
        //      --upnp:rootdevice
        //      --uuid:device-UUID
        //      --urn:schemas-upnp-org:device:deviceType:v
        //      --urn:schemas-upnp-org:service:serviceType:v
        //      --urn:domain-name:device:deviceType:v
        //      --urn:domain-name:service:serviceType:v
        //NTS: ssdp:alive 
        //SERVER: OS/version UPnP/1.0 product/version 
        //USN:
        //      --uuid:device-UUID
        //      --uuid:device-UUID::upnp:rootdevice 
        //      --uuid:device-UUID::urn:schemas-upnp-org:device:deviceType:v
        //      --uuid:device-UUID::urn:schemas-upnp-org:service:serviceType:v
        //      --uuid:device-UUID::urn:domain-name:device:deviceType:v
        //      --uuid:device-UUID::urn:domain-name:service:serviceType:v

        /// <summary>
        /// 发送存在通知
        /// </summary>
        public void NotifyAlive(AlivePackage pk)
        {
            HttpRequest request = new HttpRequest();
            request.SetPath("*").SetMethod(RequestMethodUPnP.NOTIFY);
            request.SetHeaders(pk.GetHeaders());
            byte[] data = request.GetBuffer();
            SendMessage(data);
        }
        /// <summary>
        /// 发送在线通知，简易方法
        /// </summary>
        /// <param name="nt">
        /// 字符串格式，请符合<see cref="TargetDesc"/>的字符串格式规范
        /// </param>
        public void NotifyAlive(string nt)
        {
            AlivePackage pack = new AlivePackage()
            {
                HOST = $"{_multicastGroupAddress}:{_multicastGroupPort}",
                CacheTimeout = CacheTimeout,
                Location = _deviceDescPath,
                NT = TargetDesc.Parse(nt),
                Server = _server,
                USN = UniqueServiceName
            };
            NotifyAlive(pack);
        }

        //NOTIFY * HTTP/1.1     
        //HOST:    239.255.255.250:1900
        //NT: search target
        //NTS: ssdp:byebye
        //USN: advertisement UUID
        //      --uuid:device-UUID 
        //      --uuid:device-UUID::upnp:rootdevice 
        //      --uuid:device-UUID::urn:schemas-upnp-org:device:deviceType:v
        //      --uuid:device-UUID::urn:schemas-upnp-org:service:serviceType:v
        //      --uuid:device-UUID::urn:domain-name:device:deviceType:v
        //      --uuid:device-UUID::urn:domain-name:service:serviceType:v

        /// <summary>
        /// 离线通知
        /// </summary>
        public void NotifyLeave(ByebyePackage pk)
        {
            HttpRequest request = new HttpRequest();
            request.SetPath("*").SetMethod(RequestMethodUPnP.NOTIFY);
            request.SetHeaders(pk.GetHeaders());
            byte[] data = request.GetBuffer();
            SendMessage(data);
        }
        /// <summary>
        /// 离线通知 简易方法
        /// </summary>
        /// <param name="nt">字符串格式，请符合<see cref="TargetDesc"/>的字符串格式规范</param>
        public void NotifyLeave(string nt)
        {
            ByebyePackage pack = new ByebyePackage()
            {
                HOST = $"{_multicastGroupAddress}:{_multicastGroupPort}",
                NT = TargetDesc.Parse(nt),
                USN = UniqueServiceName
            };
            NotifyLeave(pack);
        }
        /// <summary>
        /// update信息
        /// </summary>
        /// <param name="pk"></param>
        public void NotifyUpdate(UpdatePackage pk)
        {
            HttpRequest request = new HttpRequest();
            request.SetPath("*").SetMethod(RequestMethodUPnP.NOTIFY);
            request.SetHeaders(pk.GetHeaders());
            byte[] data = request.GetBuffer();
            SendMessage(data);
        }
        /// <summary>
        /// 离线通知 简易方法
        /// </summary>
        /// <param name="nt">字符串格式，请符合<see cref="TargetDesc"/>的字符串格式规范</param>
        public void NotifyUpdate(string nt)
        {
            UpdatePackage pack = new UpdatePackage()
            {
                HOST = $"{_multicastGroupAddress}:{_multicastGroupPort}",
                CacheTimeout = CacheTimeout,
                Location = _deviceDescPath,
                NT = TargetDesc.Parse(nt),
                Server = _server,
                USN = UniqueServiceName
            };
            NotifyUpdate(pack);
        }

        internal void NotifyEvent()
        {

        }

        //HTTP/1.1 200 OK
        //CACHE-CONTROL: max-age = seconds until advertisement expires
        //DATE: when reponse was generated
        //EXT:
        //LOCATION: URL for UPnP description for root device
        //SERVER: OS/Version UPNP/1.0 product/version
        //ST: ge:fridge
        //      -ssdp:all 搜索所有设备和服务
        //      -upnp:rootdevice 仅搜索网络中的根设备
        //      -uuid:device-UUID 查询UUID标识的设备
        //      -urn:schemas-upnp-org:device:device-Type:version 
        //      -urn:schemas-upnp-org:service:service-Type:version 
        //USN: advertisement UUID 
        //      --uuid:device-UUID
        //      --uuid:device-UUID::upnp:rootdevice 
        //      --uuid:device-UUID::urn:schemas-upnp-org:device:deviceType:v
        //      --uuid:device-UUID::urn:schemas-upnp-org:service:serviceType:v
        //      --uuid:device-UUID::urn:domain-name:device:deviceType:v
        //      --uuid:device-UUID::urn:domain-name:service:serviceType:v

        /// <summary>
        /// 响应 MS-SEARCH 查找
        /// </summary>
        /// <param name="pk"></param>
        [Obsolete("方法已过时，请使用ResponseSearch(SearchResponsePackage pk)", error:true)]
        public void EchoSearch(SearchResponsePackage pk)
        {
            ResponseSearch(pk);
        }
        /// <summary>
        /// 响应 MS-SEARCH 查找
        /// </summary>
        /// <param name="pk"></param>
        public void ResponseSearch(SearchResponsePackage pk)
        {
            HttpResponse resp = new HttpResponse();

            resp.SetHeaders(pk.GetHeaders());
            resp.SetStatus(StatusCode.Success);
            Response(resp);
        }
        /// <summary>
        /// 响应请求，此方法用于响应POST请求
        /// </summary>
        /// <param name="resp"></param>
        public void Response(HttpResponse resp)
        {
            resp.DontAddAutoHeader = true;
            byte[] data = resp.GetBuffer(true);
            SendMessage(data);
        }
        //POST path of control URL HTTP/1.1 
        //HOST: host of control URL:port of control URL
        //CONTENT-LENGTH: bytes in body
        //CONTENT-TYPE: text/xml; charset="utf-8" 
        //SOAPACTION: "urn:schemas-upnp-org:service:serviceType:v#actionName" 
        //<?xml version = "1.0" ?>
        //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"  s:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/"> 
        //      <s:Body> 
        //          <u:actionName xmlns:u="urn:schemas-upnp-org:service:serviceType:v"> 
        //              <argumentName>in arg value</argumentName> 
        //              other in args and their values go here, if any
        //          </u:actionName>
        //      </s:Body> 
        //</s:Envelope>

        /// <summary>
        /// 控制信息 HTTP
        /// </summary>
        /// <param name="controlPath"></param>
        /// <param name="pk"></param>
        /// <param name="callback"></param>
        public void ControlAction(string controlPath,ControlActionPackage pk,RequestComplete callback)
        {
            HttpRequest request = new HttpRequest();
            //如果POST被拒绝，则使用M-POST
            request.SetBody(StringEncoder.Encode(pk.Body.CreateDocument()));
            request.SetHeaders(pk.GetHeaders());
            request.SetHeader("CONTENT-LENGTH", request.Body.Length.ToString());
            HttpClient hc = new HttpClient();
            hc.Send(controlPath, request, callback);
        }

        //POST path of control URL HTTP/1.1 
        //HOST: host of control URL:port of control URL
        //CONTENT-LENGTH: bytes in body
        //CONTENT-TYPE: text/xml; charset="utf-8" 
        //SOAPACTION: "urn:schemas-upnp-org:control-1-0#QueryStateVariable" 
        //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/" s:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/"> 
        //      <s:Body> 
        //          <u:QueryStateVariable xmlns:u="urn:schemas-upnp-org:control-1-0"> 
        //              <u:varName>variableName</u:varName> 
        //          </u:QueryStateVariable> 
        //      </s:Body> 
        //</s:Envelope>
        /// <summary>
        /// UPNP/2.0已废除Control Query
        /// </summary>
        private void ControlQuery(ControlActionPackage pk)
        {

        }

        //SUBSCRIBE publisher path HTTP/1.1 
        //HOST: publisher host:publisher port
        //CALLBACK: <delivery URL> 
        //NT: upnp:event
        //TIMEOUT: Second-requested subscription duration

        //SUBSCRIBE publisher path HTTP/1.1 
        //HOST: publisher host:publisher port
        //SID: uuid:subscription UUID
        //TIMEOUT: Second-requested subscription duration

        //NOTIFY delivery path HTTP/1.1 
        //HOST: delivery host:delivery port
        //CONTENT-TYPE: text/xml
        //CONTENT-LENGTH: Bytes in body
        //NT: upnp:event
        //NTS: upnp:propchange
        //SID: uuid:subscription-UUID
        //SEQ: event key
        //<?xml version="1.0"?>
        //<e:propertyset xmlns:e="urn:schemas-upnp-org:event-1-0"> 
        //<e:property> 
        //  <variableName>new value</variableName> 
        //</e:property> 
        //  Other variable names and values(if any) go here.
        //</e:propertyset>

        /// <summary>
        /// Subscribe订阅  Subscribe with NT and CALLBACK
        /// </summary>
        /// <param name="publishPath"></param>
        /// <param name="timeout"></param>
        /// <param name="callbackurl">回调地址</param>
        /// <param name="statevar">可为空</param>
        /// <param name="callback"></param>
        internal void Subscribe(string publishPath, int timeout, string callbackurl, string statevar,RequestComplete callback)
        {
            HttpRequest request = new HttpRequest();
            UriInfo uri=UriInfo.Parse(publishPath);
            request.SetPath(publishPath).SetMethod(RequestMethodUPnP.SUBSCRIBE);
            request.SetHeader("HOST", $"{uri.Domain??uri.Host}:{(uri.Port>0?uri.Port:80)}");
            request.SetHeader("NT", SSDPType.Event.ToString());
            request.SetHeader("CALLBACK",callbackurl);
            request.SetHeader("TIMEOUT", timeout.ToString());
            if (string.IsNullOrEmpty(statevar))
            {
                request.SetHeader("STATEVAR", statevar);
            }
            HttpClient hc = new HttpClient();
            hc.Send(publishPath, request, callback);
        }
        /// <summary>
        /// Subscribe订阅  Subscribe with SID
        /// </summary>
        /// <param name="publishPath"></param>
        /// <param name="timeout"></param>
        /// <param name="sid">对端UDN值,即uuid:{uuid}</param>
        /// <param name="callback"></param>
        public void Subscribe(string publishPath, int timeout, string sid,RequestComplete callback)
        {
            HttpRequest request = new HttpRequest();
            UriInfo uri = UriInfo.Parse(publishPath);
            request.SetPath(publishPath).SetMethod(RequestMethodUPnP.SUBSCRIBE);
            request.SetHeader("HOST", $"{uri.Domain ?? uri.Host}:{(uri.Port > 0 ? uri.Port : 80)}");
            request.SetHeader("SID", $"uuid:{sid}");
            request.SetHeader("TIMEOUT", timeout.ToString());
            HttpClient hc = new HttpClient();
            hc.Send(publishPath, request, callback);
        }
        //UNSUBSCRIBE publisher path HTTP/1.1 
        //HOST: publisher host:publisher port
        //SID: uuid:subscription UUID

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="publishPath"></param>
        /// <param name="sid">对端UDN值,即uuid:{uuid}</param>
        /// <param name="callback"></param>
        public void UnSubscribe(string publishPath,string sid,RequestComplete callback)
        {
            HttpRequest request = new HttpRequest();
            UriInfo uri = UriInfo.Parse(publishPath);
            request.SetHeader("HOST", $"{uri.Domain ?? uri.Host}:{(uri.Port > 0 ? uri.Port : 80)}");
            request.SetHeader("SID", $"uuid:{sid}");
            HttpClient hc = new HttpClient();
            hc.Send(publishPath, request, callback);
        }

        /// <summary>
        /// 发送报文消息
        /// </summary>
        /// <param name="data"></param>
        protected void SendMessage(byte[] data)
        {
            _socket.SocketMain.SendTo(data, _remoteEP);
        }
        /// <summary>
        /// 设置描述文档地址
        /// </summary>
        /// <param name="path"></param>
        internal void SetDeviceDescriptionPath(string path)
        {
            _deviceDescPath = path;
        }
        /// <summary>
        /// 定时器回调函数
        /// </summary>
        /// <param name="state"></param>
        private void TimeoutCallback(object state)
        {
            if (_initialized)
            {
                //Search(PackDefaultSearch);
                NotifyAlive(PackDefaultAlive);
            }
        }
    }

    public class SSDPProtocol
    {
        /// <summary>
        /// SSDP组播地址
        /// </summary>
        public const string MulticastAddress = "239.255.255.250";
        /// <summary>
        /// SSDP组播地址IPV6
        /// </summary>
        public const string MulticastAddressIPv6 = "FF0x::C";
        /// <summary>
        /// SSDP组播端口
        /// </summary>
        public const int MulticastPort = 1900;
        /// <summary>
        /// 事件组播地址
        /// </summary>
        public const string EventMulticastAddress = "239.255.255.246";
        /// <summary>
        /// 事件组播端口
        /// </summary>
        public const int EventMulticastPort = 7900;

    }
}
