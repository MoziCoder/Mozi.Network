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

	dotnet add package Mozi.SSDP --version 1.2.6

~~~
## 项目依赖  

[Mozi.HttpEmbedded][httpembedded] > 1.2.5

## 用例说明

~~~csharp

    /// <summary>
    /// 发现服务范例
    /// </summary>
    /// <remarks>
    /// 这是一个范例，如果这个范例不能满足应用需求，可参照范例进行修改
    /// </remarks>
    public class SSDPHost
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
                            ssdp.PackDefaultSearch.ST = new TargetDesc()
                            {
                                Domain = ssdp.Domain,
                                ServiceType = ServiceCategory.Device,
                                ServiceName = "simplehost",
                                Version = 1
                            };
                            ssdp.MulticastAddress = "239.255.255.250";
                            ssdp.BindingAddress = ip.Address;
                            Console.WriteLine("binding start:{0},{1}", ip.Address, r.Name);
                            ssdp.OnNotifyAliveReceived += Ssdp_OnNotifyAliveReceived;
                            ssdp.OnSearchReceived += Ssdp_OnSearchReceived;
                            ssdp.OnNotifyByebyeReceived += Ssdp_OnNotifyByebyeReceived;
                            ssdp.OnNotifyUpdateReceived += Ssdp_OnNotifyUpdateReceived;
                            ssdp.OnResponseMessageReceived += Ssdp_OnResponseMessageReceived;
                            ssdp.AllowLoopbackMessage = true;
                            //初始化并加入多播组

                            _services.Add(ssdp);

                        }
                    }
                }
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
        /// 消息响应事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="resp"></param>
        /// <param name="host"></param>
        protected void Ssdp_OnResponseMessageReceived(object sender, HttpResponse resp, string host)
        {
            Console.WriteLine("Response from {0}", host);
        }
        /// <summary>
        /// update通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        protected void Ssdp_OnNotifyUpdateReceived(object sender, UpdatePackage pack, string host)
        {
            Console.WriteLine("Notify update from {0}", host);
        }
        /// <summary>
        /// byebye通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        protected void Ssdp_OnNotifyByebyeReceived(object sender, ByebyePackage pack, string host)
        {
            Console.WriteLine("Notify byebye from {0}", host);
        }
        /// <summary>
        /// m-search消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        protected void Ssdp_OnSearchReceived(object sender, SearchPackage pack, string host)
        {
            SearchResponsePackage search = new SearchResponsePackage();
            var service = (SSDPService)sender;
            search.HOST = string.Format("{0}:{1}", service.MulticastAddress, service.MulticastPort);
            search.CacheTimeout = 3600;
            search.USN = service.USN;
            search.ST = pack.ST;
            search.Server = service.Server;
            //ssdp:all
            if (search.ST.IsAll)
            {
                Console.WriteLine("Search from {0},looking for ssdp:all", host);
            }
            //upnp:rootdevice
            else if (search.ST.IsRootDevice)
            {
                Console.WriteLine("Search from {0},looking for upnp:rootdevice", host);
            }
            //urn:schema-upnp-org:device:deviceName:version
            else
            {
                Console.WriteLine("Search from {0},looking for {1}", host, search.ST.ToString());
            }
            //service.EchoSearch(search);
        }
        /// <summary>
        /// alive消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        protected void Ssdp_OnNotifyAliveReceived(object sender, AlivePackage pack, string host)
        {
            Console.WriteLine("Notify alive from {0}", host);
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