using System;
using System.Net;
using System.Threading;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    //ST:Search Target
    //NT:Notification Type
    //NTS:Notification Sub Type
    //USN:Unique Service Name
    //MX: Maximum wait time in seconds. Should be between 1 and 120 inclusive

    public delegate void NotifyAliveReceived(object sender,AlivePackage pack,string host);
    public delegate void NotifyByebyeReceived(object sender, ByebyePackage pack, string host);
    public delegate void SearchReceived(object sender,SearchPackage pack,string host);
    public delegate void NotifyUpdateReceived(object sender, UpdatePackage pack, string host);
    public delegate void ResponseMessageReceived(object sender, HttpResponse resp, string host);
    public delegate void MessageReceived(object sender, DataTransferArgs args);

    internal delegate void SubscribeReceived(object sender,SubscribePackage pack,string host);
    internal delegate void UnSubscribedReceived(object sender,SubscribePackage pack, string host);
    internal delegate void ControlActionReceived(object sender,ControlActionPackage pack,string host);
    internal delegate void EventMessageReceive(object sender, EventPackage pack, string host);

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
    public class SSDPService
    {
        private bool _initialized = false;

        private UDPSocket _socket;
        private Timer _timer;
        private IPEndPoint _remoteEP;

        private string _multicastGroupAddress = SSDPProtocol.MulticastAddress;
        private int _multicastGroupPort = SSDPProtocol.ProtocolPort;
        private IPAddress _bindingAddress = IPAddress.Any;

        private string _server = "Mozi/1.3.7 UPnP/2.0 Mozi.SSDP/1.3.7";

        /// <summary>
        /// 设备描述文档地址
        /// </summary>
        private string _deviceDescPath = "";

        #region
        /// <summary>
        /// 服务器描述值
        /// </summary>
        public string Server { get { return _server; } set { _server = value; } }
        ///// <summary>
        ///// 设备描述信息地址
        ///// </summary>
        //public string Location { get; set; }
        /// <summary>
        /// 程序默认的域信息，用于绑定NT,ST,USN，以及设备/服务描述文档中的相关信息
        /// </summary>
        public string Domain = "mozi.org";
        /// <summary>
        /// 本地服务基本信息
        /// </summary>
        public USNDesc USN = new USNDesc()
        {
            DeviceId = UUID.Generate(),
            Domain = "mozi.org",
            ServiceType = ServiceCategory.Device,
            ServiceName = "simplehost",
            IsRootDevice=false,
            Version=1,
        };

        #endregion

        /// <summary>
        /// 广播消息周期
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
                ApplyMulticastAddressChange(MulticastAddress, MulticastPort);
            }
        }
        /// <summary>
        /// 组播端口
        /// <para>
        /// 标准端口为 <see cref="SSDPProtocol.ProtocolPort"/>
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
                ApplyMulticastAddressChange(MulticastAddress, MulticastPort);
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
        /// 默认查询包
        /// </summary>
        public SearchPackage PackDefaultSearch = new SearchPackage() 
        {
            MX=3,
            ST= TargetDesc.All,
        };
        /// <summary>
        /// 默认在线消息包
        /// </summary>
        public AlivePackage PackDefaultAlive = new AlivePackage()
        {
            CacheTimeout = 3600,
            Location = "",
            Server = "",
            NT= TargetDesc.All,
            USN=new USNDesc() { IsRootDevice=true },
        };
        /// <summary>
        /// 默认离线消息包
        /// </summary>
        public ByebyePackage PackDefaultByebye = new ByebyePackage() 
        { 
            
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
            get;set;
        }
        /// <summary>
        /// 搜索程序定义的默认类型
        /// urn:{domain}:device:1
        /// </summary>
        public TargetDesc DeviceDefault = new TargetDesc();
        /// <summary>
        /// 收到ssdp:alive时触发
        /// </summary>
        public  NotifyAliveReceived OnNotifyAliveReceived;
        /// <summary>
        /// 收到ssdp:byebye时触发
        /// </summary>
        public  NotifyByebyeReceived OnNotifyByebyeReceived;
        /// <summary>
        /// 收到m-search时触发
        /// </summary>
        public  SearchReceived OnSearchReceived;
        /// <summary>
        /// 收到upnp:update时触发
        /// </summary>
        public  NotifyUpdateReceived OnNotifyUpdateReceived;
        /// <summary>
        /// 收到HTTP/1.1 200 OK时触发
        /// </summary>
        public  ResponseMessageReceived OnResponseMessageReceived;
        /// <summary>
        /// 原始数据包解析
        /// <para>如果内置的解析结果不能满足应用需求，可以使用该事件进行数据解析</para>
        /// </summary>
        public  MessageReceived OnMessageReceived;
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
            _remoteEP = new IPEndPoint(IPAddress.Parse(SSDPProtocol.MulticastAddress), SSDPProtocol.ProtocolPort);

            _timer = new Timer(TimeoutCallback, null, Timeout.Infinite, Timeout.Infinite);

            //初始化数据包
            PackDefaultAlive.USN = new USNDesc() { IsRootDevice = true, DeviceId = UUID.Generate(),Domain="mozi.org" };
            PackDefaultAlive.Server = _server;
            
            PackDefaultByebye.USN = new USNDesc() { IsRootDevice = true, DeviceId = UUID.Generate(),Domain="mozi.org" };

            DeviceDefault.Domain = Domain;
            DeviceDefault.ServiceType = ServiceCategory.Device;
            DeviceDefault.Version = 1;
        }

        private void ApplyMulticastAddressChange(string address,int port)
        {
            _remoteEP = new IPEndPoint(IPAddress.Parse(address), port);
            var host = string.Format("{0}:{1}", address, port);
            PackDefaultAlive.HOST = host;
            PackDefaultByebye.HOST = host;
            PackDefaultSearch.HOST = host;
        }
        /// <summary>
        /// 数据接收时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void Socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            //DONE 如何进行多包分割？

            ParsePackage(args);
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
        private void ParsePackage(DataTransferArgs args)
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
                            OnNotifyAliveReceived(this, pack,args.IP);
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
                    }else if (nts == SSDPType.Update.ToString()){

                        var pack = UpdatePackage.Parse(request);
                        if (pack != null && OnNotifyUpdateReceived != null)
                        {
                            OnNotifyUpdateReceived(this, pack, args.IP);
                        }
                    }
                }
                //MS-SEARCH
                else if(method==RequestMethodUPnP.MSEARCH)
                {
                    var pack = SearchPackage.Parse(request);
                    if (pack != null && OnSearchReceived != null)
                    {
                        OnSearchReceived(this, pack, args.IP);
                    }
                }
                //event SUBSCRIBE
                else if(method==RequestMethodUPnP.SUBSCRIBE)
                {
                    //var pack = SubscribePackage.Parse(request);
                    //if (pack != null && OnSubscribeReceived)
                    //{
                    //    OnSubscribeReceived(this, pack, args.IP);
                    //}
                }
                //event UNSUBSCRIBE
                else if(method==RequestMethodUPnP.UNSUBSCRIBE)
                {
                    //var pack = UnSubscribedPackage.Parse(request);
                    //if (pack != null && OnUnSubscribedReceived)
                    //{
                    //    OnUnSubscribedReceived(this, pack, args.IP);
                    //}
                }
                //Control
                else if(method== RequestMethod.POST)
                {

                }
            }
            catch(Exception ex)
            {
                try
                {
                    HttpResponse resp = HttpResponse.Parse(args.Data);
                    if (OnResponseMessageReceived != null)
                    {
                        OnResponseMessageReceived(this, resp, args.IP);
                    }
                    //成功
                    if (resp.Status == StatusCode.Success)
                    {
                        //判断包类型
                        //M-SEARCH response
                        var st = resp.Headers.GetValue("ST");
                    }
                    else
                    {

                    }
                }
                catch(Exception ex2)
                {

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
        public SSDPService Activate()
        {            
            _socket.AllowLoopbackMessage = AllowLoopbackMessage;
            _socket.BindingAddress = BindingAddress;
            _socket.Start(_multicastGroupAddress,_multicastGroupPort);
            _initialized = true;
            Running = true;
            return this;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public SSDPService Inactivate()
        {
            Running = false;
            _socket.Shutdown();
            return this;
        }
        /// <summary>
        /// 广播ssdp:alive信息 定时发送ssdp:alive广播
        /// </summary>
        /// <returns></returns>
        public SSDPService StartAdvertise()
        {            
            //是否接受回环消息
            _timer.Change(0, NotificationPeriod);
            OnAdvertise = true;
            return this;
        }
        /// <summary>
        /// 停止广播
        /// </summary>
        /// <returns></returns>
        public SSDPService StopAdvertise()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            OnAdvertise = false;
            return this;
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
            SendTo(data);
        }
        /// <summary>
        /// 查找设备简化方法，参看<see cref="Search(SearchPackage)"/>
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
            SendTo(data);
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
            SendTo(data);
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
            SendTo(data);
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
            SendTo(data);
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
        public void EchoSearch(SearchResponsePackage pk)
        {
            HttpResponse resp = new HttpResponse();
            resp.DontAddAutoHeader = true;
            resp.SetHeaders(pk.GetHeaders());
            resp.SetStatus(StatusCode.Success);
            byte[] data = resp.GetBuffer(true);
            SendTo(data);
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
        /// 控制信息
        /// </summary>
        /// <param name="pk"></param>
        internal void ControlAction(ControlActionPackage pk)
        {
            HttpRequest request = new HttpRequest();
            //如果POST被拒绝，则使用M-POST
            request.SetPath(pk.Path).SetMethod(RequestMethod.POST);
            request.SetBody(HttpEmbedded.Encode.StringEncoder.Encode(pk.Body.CreateDocument()));
            request.SetHeader("CONTENT-LENGTH", request.ContentLength);
            request.SetHeaders(pk.GetHeaders());
            byte[] data = request.GetBuffer();
            SendTo(data);
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
        internal void ControlQuery()
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
        /// 订阅
        /// </summary>
        /// <param name="pk"></param>
        internal void Subscribe(SubscribePackage pk)
        {
            HttpRequest request = new HttpRequest();
            request.SetPath(pk.Path).SetMethod(RequestMethodUPnP.SUBSCRIBE);
            request.SetHeaders(pk.GetHeaders());
            byte[] data = request.GetBuffer();
            SendTo(data);
        }

        //UNSUBSCRIBE publisher path HTTP/1.1 
        //HOST: publisher host:publisher port
        //SID: uuid:subscription UUID
        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="pk"></param>
        internal void UnSubscribe(SubscribePackage pk)
        {
            HttpRequest request = new HttpRequest();
            request.SetPath(pk.Path).SetMethod(RequestMethodUPnP.UNSUBSCRIBE);
            request.SetHeaders(pk.GetHeaders());
            byte[] data = request.GetBuffer();
            SendTo(data);
        }

        protected void SendTo(byte[] data)
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

    //NOTIFY* HTTP/1.1     
    //HOST: 239.255.255.250:1900    
    //CACHE-CONTROL: max-age = seconds until advertisement expires    
    //LOCATION: URL for UPnP description for root device
    //NT: search target
    //NTS: ssdp:alive 
    //SERVER: OS/versionUPnP/1.0product/version 
    //USN: advertisement UUI
    /// <summary>
    /// 公告包
    /// </summary>
    public abstract class AbsAdvertisePackage
    {

        private string _host = "";

        public string HOST { 
            get 
            { 
                return _host; 
            }
            set 
            {
                try
                {
                    string[] hostItmes = value.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (hostItmes.Length == 2)
                    {
                        HostIp = hostItmes[0];
                        HostPort = ushort.Parse(hostItmes[1]);
                    }
                    _host = value;
                }catch(Exception ex){

                }
            } 
        }
        public string Path { get; set; }

        public string HostIp { get; private set; }
        public int HostPort { get; private set; }

        public AbsAdvertisePackage()
        {
            _host = string.Format("{0}:{1}",SSDPProtocol.MulticastAddress,SSDPProtocol.ProtocolPort);
            //HostIp = SSDPProtocol.MulticastAddress;
            //HostPort = SSDPProtocol.ProtocolPort;
            Path = "*";
        }

        public virtual TransformHeader GetHeaders()
        {
            throw new NotImplementedException();
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
        public const int ProtocolPort = 1900;
        /// <summary>
        /// 事件组播地址
        /// </summary>
        public const string EventMulticastAddress = "239.255.255.246";
        /// <summary>
        /// 事件组播端口
        /// </summary>
        public const int EventMulticastPort = 7900;

    }


    /// <summary>
    ///     --upnp:rootdevice
    //      --uuid:device-UUID
    //      --urn:schemas-upnp-org:device:deviceType:v
    //      --urn:schemas-upnp-org:service:serviceType:v
    //      --urn:domain-name:device:deviceType:v
    //      --urn:domain-name:service:serviceType:v
    /// </summary>
    public class TargetDesc:USNDesc
    {

        public bool IsAll = false;
        /// <summary>
        /// upnp:rootdevice
        /// </summary>
        public static TargetDesc RootDevice = new TargetDesc { IsRootDevice = true };
        public static TargetDesc All = new TargetDesc { IsAll = true};

        public new string ToString()
        {
            if (IsAll)
            {
                return SSDPType.All.ToString();
            }
            string result;
            if (IsRootDevice)
            {
                result =SSDPType.RootDevice.ToString();
            }
            else
            {
                if (string.IsNullOrEmpty(DeviceId))
                {

                    result = string.Format("urn:{0}:{1}:{2}:{3}", Domain, ServiceType == ServiceCategory.Device ? "device" : "service", ServiceName, Version);
                }
                else
                {
                    result = "uuid:" + DeviceId;
                }
            }
            return result;
        }

        public static new TargetDesc Parse(string data)
        {
            //uuid:device-UUID::urn:domain-name:service:serviceType:v
            TargetDesc desc = new TargetDesc()
            {
                Domain = "",
                IsRootDevice = false,
                DeviceId = "",
                ServiceName = "",
                Version = 0,
            };
            string[] items = data.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                //ssdp:all
                if (data == SSDPType.All.ToString())
                {
                    desc.IsAll = true;
                }
                else
                {
                    //upnp:rootdevice
                    if (data == SSDPType.RootDevice.ToString())
                    {
                        desc.IsRootDevice = true;
                        desc.DeviceId = items[1];
                    }
                    //specific device
                    else if (items[0] == "uuid")
                    {
                        desc.DeviceId = items[1];
                    }
                    else
                    {
                        desc.Domain = items[1];
                        var serviceType = items[2];
                        desc.ServiceType = serviceType == "device" ? ServiceCategory.Device : ServiceCategory.Service;
                        desc.ServiceName = items[3];
                        desc.Version = int.Parse(items[4]);
                    }
                }
            }
            finally
            {

            }
            return desc;
        }
    }

    /// <summary>
    /// --uuid:device-UUID
    /// --uuid:device-UUID::upnp:rootdevice 
    /// --uuid:device-UUID::urn:schemas-upnp-org:device:deviceType:v
    /// --uuid:device-UUID::urn:schemas-upnp-org:service:serviceType:v
    /// --uuid:device-UUID::urn:domain-name:device:deviceType:v
    /// --uuid:device-UUID::urn:domain-name:service:serviceType:v
    /// </summary>
    public class USNDesc
    {
        public string Domain = "schemas-upnp-org";
        public bool IsRootDevice = false;
        public ServiceCategory ServiceType = ServiceCategory.Service;
        public string DeviceId = "";
        public string ServiceName = "simplehost";
        public int Version = 1;
        /// <summary>
        /// 转为USN格式字符串
        /// <list type="number">
        /// <item>--uuid:device-UUID::upnp:rootdevice</item> 
        /// <item>--uuid:device-UUID::upnp:rootdevice </item>
        /// <item>--uuid:device-UUID::urn:schemas-upnp-org:device:deviceType:v</item>
        /// <item>--uuid:device-UUID::urn:schemas-upnp-org:service:serviceType:v</item>
        /// <item>--uuid:device-UUID::urn:domain-name:device:deviceType:v</item>
        /// <item>--uuid:device-UUID::urn:domain-name:service:serviceType:v</item>
        /// </list>
        /// </summary>
        /// <returns></returns>
        public new string ToString()
        {
            var result = "uuid:" + DeviceId;
            if (IsRootDevice)
            {
                result += "::"+ SSDPType.RootDevice.ToString();
            }
            else
            {
                result += string.Format(":urn:{0}:{1}:{2}:{3}", Domain, ServiceType == ServiceCategory.Device ? "device" : "service", ServiceName, Version);
            }
            return result;
        }
        /// <summary>
        /// 包解析
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static USNDesc Parse(string data)
        {
            //uuid:device-UUID::urn:domain-name:service:serviceType:v
            USNDesc desc = new USNDesc()
            {
                Domain = "",
                IsRootDevice = false,
                DeviceId = "",
                ServiceName="",
                Version=0,
            };            
            string[] items = data.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                if (data.Contains(SSDPType.RootDevice.ToString()))
                {
                    desc.IsRootDevice = true;
                    desc.DeviceId = items[1];
                }
                else
                {
                    desc.DeviceId = items[1];
                    desc.Domain = items[3];
                    var serviceType = items[4];
                    desc.ServiceType = serviceType == "device" ? ServiceCategory.Device : ServiceCategory.Service;
                    desc.ServiceName = items[5];
                    desc.Version = int.Parse(items[6]);
                }
            }
            finally
            {

            }
            return desc;
        }
    }

    public enum ServiceCategory
    {
        Device=1,
        Service=2
    }
}
