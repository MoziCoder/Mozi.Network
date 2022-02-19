using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
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
                            ssdp.OnNotifyAliveReceived += SSDP_OnNotifyAliveReceived;
                            ssdp.OnSearchReceived += SSDP_OnSearchReceived;
                            ssdp.OnNotifyByebyeReceived += SSDP_OnNotifyByebyeReceived;
                            ssdp.OnNotifyUpdateReceived += SSDP_OnNotifyUpdateReceived;
                            ssdp.OnResponseMessageReceived += SSDP_OnResponseMessageReceived;
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
        protected void SSDP_OnResponseMessageReceived(object sender, HttpResponse resp, string host)
        {
            Console.WriteLine("Response from {0}", host);
        }
        /// <summary>
        /// update通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        protected void SSDP_OnNotifyUpdateReceived(object sender, UpdatePackage pack, string host)
        {
            Console.WriteLine("Notify update from {0}", host);
        }
        /// <summary>
        /// byebye通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        protected void SSDP_OnNotifyByebyeReceived(object sender, ByebyePackage pack, string host)
        {
            Console.WriteLine("Notify byebye from {0}", host);
        }
        /// <summary>
        /// m-search消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        protected void SSDP_OnSearchReceived(object sender, SearchPackage pack, string host)
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
        protected void SSDP_OnNotifyAliveReceived(object sender, AlivePackage pack, string host)
        {
            Console.WriteLine("Notify alive from {0}", host);
        }

    }
}
