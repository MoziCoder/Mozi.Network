# Mozi.SSDP

### 项目简介

Mozi.SSDP是一个基于.Net开发的SSDP组件，目标是为.Net应用程序提供完善的SSDP/UPNP服务功能。 项目对UDP Socket进行封装，并遵循UPNP/2.0(UPnP Device Architecture 2.0)，实现了UPNP2.0规范中的大部分功能。

## 特点

1. 精巧
2. 高度可控
3. 依赖少，仅依赖于HTTP服务器组件[HttpEmbedded][httpembedded]

## 功能

1. 发现-在线
	- 在线通知
	- 离线通知
	- 搜索
	- 更新

3. 设备和服务描述
    内含设备和服务描述文档，使用过程中请自行填写和发布

3. 控制
    
4. 事件

## 项目地址

- [Github][github]

- [Gitee][gitee]

- [CSDN][codechina]

## 程序下载

~~~shell

	dotnet add package Mozi.SSDP --version 1.4.3

~~~
## 项目依赖  

[Mozi.HttpEmbedded][httpembedded] > 1.4.3

## 用例说明

~~~csharp

    /// <summary>
    /// 发现服务范例
    /// </summary>
    /// <remarks>
    /// 这是一个范例，如果这个范例不能满足应用需求，可参照范例进行修改
    /// </remarks>
    public class SSDPHost:ISSDPService
    {
        private static SSDPHost _host;

        private readonly List<SSDPService> _services = new List<SSDPService>();

        public static SSDPHost Instance
        {
            get { return _host ?? (_host = new SSDPHost()); }
        }

        private SSDPHost()
        {
            //开启SSDP服务
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var r in interfaces)
            {
                //遍历所有可用网卡，过滤临时地址
                if (r.SupportsMulticast && r.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    foreach (var ip in r.GetIPProperties().UnicastAddresses)
                    {
                        //排除未正确获取IP的网卡
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ip.Address.ToString().StartsWith("169.254"))
                        {
                            SSDPService ssdp = new SSDPService();
                            ssdp.MulticastAddress = "239.255.255.250";
                            ssdp.BindingAddress = ip.Address;
                            Console.WriteLine("binding start:{0},{1}", ip.Address, r.Name);
                            ssdp.AllowLoopbackMessage = true;
                            //初始化并加入多播组

                            _services.Add(ssdp);

                        }
                    }
                }
            }
        }
        /// <summary>
        /// 设置组播地址
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void SetMulticastAddress(string ip,int port)
        {
            foreach (var sd in _services)
            {
                sd.ApplyMulticastAddress(ip, port);
            }
        }

        //public void ApplyDevice()
        //{
        //    foreach (var sd in _services)
        //    {
        //        sd.PackDefaultAlive
        //    }
        //}
        /// <summary>
        /// M-SEARCH,此处建议发3次包，避免终端没有收到信息
        /// </summary>
        /// <param name="st"></param>
        public void Search(SearchPackage st)
        {
            foreach (var service in _services)
            {
                service.Search(st);
            }
        }
        /// <summary>
        /// M-SEARCH,此处建议发3次包，避免终端没有收到信息
        /// </summary>
        /// <param name="st"></param>
        public void Search(TargetDesc st)
        {
            foreach (var service in _services)
            {
                service.Search(st);
            }
        }
        /// <summary>
        /// M-SEARCH,此处建议发3次包，避免终端没有收到信息
        /// </summary>
        /// <param name="st"></param>
        public void Search(string st)
        {
            foreach (var service in _services)
            {
                service.Search(st);
            }
        }
        /// <summary>
        /// 激活服务，开始侦听广播信息
        /// </summary>
        public void Activate()
        {
            foreach (var service in _services)
            {
                service.Activate();
            }
        }
        /// <summary>
        /// 停用服务，不再接收广播消息
        /// </summary>
        public void Inactivate()
        {
            foreach (var service in _services)
            {
                service.Inactivate();
            }
        }
        /// <summary>
        /// 广播在线信息
        /// </summary>
        public void StartAdvertise()
        {
            foreach (var service in _services)
            {
                service.StartAdvertise();
            }
        }
        /// <summary>
        /// 停止广播在线信息
        /// </summary>
        public void StopAdvertise()
        {
            foreach (var service in _services)
            {
                service.StopAdvertise();
            }
        
        }
        /// <summary>
        /// 收到POST请求时触发
        /// </summary>
        /// <param name="dlg"></param>
        public void SetPostReceived(PostReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnPostReceived += dlg;
            }
        }
        /// <summary>
        /// HTTP响应事件，即有HTTP/1.1 200 OK类似的HTTP响应包时触发
        /// </summary>
        /// <param name="dlg"></param>
        public void SetHttpResponsed(HttpResponsed dlg)
        {
            foreach(var service in _services)
            {
                service.OnHttpResponsed += dlg;
            }
        }
        /// <summary>
        /// 设置事件 M-SEARCH响应
        /// </summary>
        /// <param name="dlg"></param>
        public void SetSearchResponsed(SearchResponsed dlg)
        {
            foreach (var service in _services)
            {
                service.OnSearchResponsed += dlg;
            }
        }
        /// <summary>
        /// 设置事件 Notify upnp:update
        /// </summary>
        /// <param name="dlg"></param>
        public void SetNotifyUpdateReceived(NotifyUpdateReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnNotifyUpdateReceived += dlg;
            }
        }
        /// <summary>
        /// 设置事件 Notify ssdp:alive
        /// </summary>
        /// <param name="dlg"></param>
        public void SetNotifyAliveReceived(NotifyAliveReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnNotifyAliveReceived += dlg;
            }
        }
        /// <summary>
        /// 设置事件 Notify ssdp:byebye
        /// </summary>
        /// <param name="dlg"></param>
        public void SetNotifyByebyeReceived(NotifyByebyeReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnNotifyByebyeReceived += dlg;
            }
        }
        /// <summary>
        /// 设置事件 M-SEARCH
        /// </summary>
        /// <param name="dlg"></param>
        public void SetSearchReceived(SearchReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnSearchReceived += dlg;
            }
        }
        /// <summary>
        /// 设置订阅事件
        /// </summary>
        /// <param name="dlg"></param>
        public void SetSubscribeReceived(SubscribeReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnSubscribeReceived += dlg;
            }
        }
        /// <summary>
        /// 设置取消订阅事件
        /// </summary>
        /// <param name="dlg"></param>
        public void SetUnSubscribeReceived(UnSubscribedReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnUnSubscribedReceived += dlg;
            }
        }
        /// <summary>
        /// 设置控制信息接收事件
        /// </summary>
        /// <param name="dlg"></param>
        public void SetControlActionReceived(ControlActionReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnControlActionReceived += dlg;
            }
        }
        /// <summary>
        /// 设置事件 Control
        /// </summary>
        /// <param name="service"></param>
        /// <param name="pk"></param>
        internal void ControlAction(SSDPService service, ControlActionPackage pk)
        {
            service.ControlAction(pk);
        }

        //public void ControlQuery(SSDPService service,ControlQueryPackage pk)
        //{
        //    service.ControlAction(pk);
        //}

        /// <summary>
        /// 响应搜索
        /// </summary>
        /// <param name="service"></param>
        /// <param name="pk"></param>
        public void EchoSearch(SSDPService service,SearchResponsePackage pk)
        {
             service.EchoSearch(pk);
        }
        /// <summary>
        /// 在线通知
        /// </summary>
        /// <param name="service"></param>
        /// <param name="pk"></param>
        public void NotifyAlive(SSDPService service, AlivePackage pk)
        {
             service.NotifyAlive(pk);
        }
        /// <summary>
        /// 发送在线通知 简易方法，请参考方法原型<see cref="SSDPService.NotifyAlive(string)"/>
        /// </summary>
        /// <param name="service"></param>
        /// <param name="nt"></param>
        public void NotifyAlive(SSDPService service,string nt)
        {
            service.NotifyAlive(nt);
        }
        /// <summary>
        /// 离线通知
        /// </summary>
        /// <param name="service"></param>
        /// <param name="pk"></param>
        public void NotifyLeave(SSDPService service, ByebyePackage pk)
        {
             service.NotifyLeave(pk);
        }
        /// <summary>
        /// 发送在线通知 简易方法，请参考方法原型<see cref="SSDPService.NotifyLeave(string)"/>
        /// </summary>
        /// <param name="service"></param>
        /// <param name="nt"></param>
        public void NotifyLeave(SSDPService service,string nt)
        {
            service.NotifyLeave(nt);
        }
        /// <summary>
        /// 升级通知
        /// </summary>
        /// <param name="service"></param>
        /// <param name="pk"></param>
        public void NotifyUpdate(SSDPService service, UpdatePackage pk)
        {
             service.NotifyUpdate(pk);
        }
        /// <summary>
        /// 升级通知 简易方法，请参考方法原型<see cref="SSDPService.NotifyUpdate(string)"/>
        /// </summary>
        /// <param name="service"></param>
        /// <param name="nt"></param>
        public void NotifyUpdate(SSDPService service, string nt)
        {
            service.NotifyUpdate(nt);
        }
        /// <summary>
        /// Subscribe订阅 Subscribe with NT and CALLBACK 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="publishPath"></param>
        /// <param name="timeout"></param>
        /// <param name="callbackurl"></param>
        /// <param name="statevar"></param>
        public void Subscribe(SSDPService service, string publishPath, int timeout, string callbackurl, string statevar)
        {
             service.Subscribe(publishPath, timeout, callbackurl, statevar);
        }
        /// <summary>
        /// Subscribe订阅 Subscribe with sid
        /// </summary>
        /// <param name="service"></param>
        /// <param name="publishPath"></param>
        /// <param name="timeout"></param>
        /// <param name="sid"></param>
        public void Subscribe(SSDPService service, string publishPath, int timeout, string sid)
        {
            service.Subscribe(publishPath, timeout, sid);
        }
        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="service"></param>
        /// <param name="pk"></param>
        public void UnSubscribe(SSDPService service, UnSubscribedPackage pk)
        {
             service.UnSubscribe(pk);
        }
    }

       static void Main(string[] args)
        {
            SSDPHost host = SSDPHost.Instance;

            //设置组播地址和端口
            //host.SetMulticastAddress("239.255.255.251", 1901);

            //绑定事件
            host.SetNotifyAliveReceived(SSDP_OnNotifyAliveReceived);
            host.SetNotifyByebyeReceived(SSDP_OnNotifyByebyeReceived);
            host.SetNotifyUpdateReceived(SSDP_OnNotifyUpdateReceived);
            host.SetSearchResponsed(SSDP_OnSearchResponsed);
            host.SetSearchReceived(SSDP_OnSearchReceived);
            host.SetSubscribeReceived(SSDP_OnSubscribeReceived);
            host.SetUnSubscribeReceived(SSDP_OnUnSubscribeReceived);
            host.SetPostReceived(SSDP_OnPostReceived);
            host.SetControlActionReceived(SSDP_OnControlActionReceived);

            //启用服务
            host.Activate();

            //搜索指定的设备
            host.Search(TargetDesc.Parse("urn:mozi.org:device:simplehost:1"));

            //host.Search(TargetDesc.Parse("ssdp:all"));
            Console.ReadLine();
        }

        private static void SSDP_OnControlActionReceived(object sender, ControlActionPackage pack, string host)
        {
            //CONTROL ACTION 
        }
        /// <summary>
        /// 所有的POST请求都会在这里触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="req"></param>
        /// <param name="host"></param>
        private static void SSDP_OnPostReceived(object sender, HttpRequest req, string host)
        {
            
        }

        /// <summary>
        /// 消息响应事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="resp"></param>
        /// <param name="host"></param>
        protected static void SSDP_OnSearchResponsed(object sender, HttpResponse resp, string host)
        {
            Console.WriteLine("Response search from {0}", host);
        }

        /// <summary>
        /// update通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        protected static void SSDP_OnNotifyUpdateReceived(object sender, UpdatePackage pack, string host)
        {
            Console.WriteLine("Notify update from {0}", host);
        }
        /// <summary>
        /// byebye通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        protected static void SSDP_OnNotifyByebyeReceived(object sender, ByebyePackage pack, string host)
        {
            Console.WriteLine("Notify byebye from {0}", host);
        }
        /// <summary>
        /// m-search消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        protected static void SSDP_OnSearchReceived(object sender, SearchPackage pack, string host)
        {
            var service = (SSDPService)sender;

            Console.WriteLine("Search from {0},looking for {1}", host, pack.ST.ToString());
            //service.EchoSearch(search);
        }
        /// <summary>
        /// alive消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        protected static void SSDP_OnNotifyAliveReceived(object sender, AlivePackage pack, string host)
        {
            Console.WriteLine("Notify alive from {0}", host);
        }

        protected static void SSDP_OnUnSubscribeReceived(object sender, UnSubscribedPackage pack, string host)
        {
            
        }

        protected static void SSDP_OnSubscribeReceived(object sender, HttpRequest pack, string host)
        {
            
        }
    }

~~~
## 版权说明

本项目采用MIT开源协议，引用请注明出处。欢迎复制，引用和修改。复制请注明出处，引用请附带证书。意见建议疑问请联系软件作者，或提交ISSUE。

### By [Jason][1] on Feb. 5,2020

[1]:mailto:brotherqian@163.com
[gitee]:https://gitee.com/myui_admin/mozi.git
[github]:https://github.com/MoziCoder/Mozi.Network.git
[codechina]:https://codechina.csdn.net/mozi/mozi.httpembedded.git
[httpembedded]:https://gitee.com/myui_admin/mozi.git