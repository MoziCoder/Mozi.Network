using System;
using System.Net.NetworkInformation;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //开启SSDP服务
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            char cd = (char)Mozi.HttpEmbedded.ASCIICode.DIVIDE;
            foreach (var r in interfaces)
            {
                //遍历所有可用网卡，过滤临时地址
                if (r.SupportsMulticast && r.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    foreach (var ip in r.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork&&!ip.Address.ToString().StartsWith("169.254"))
                        {
                            SSDPService ssdp = new SSDPService();
                            ssdp.PackDefaultSearch.ST = new TargetDesc()
                            {
                                Domain = ssdp.Domain,
                                ServiceType=ServiceCategory.Device,
                                ServiceName="simplehost",
                                Version=1
                            };
                            ssdp.MulticastAddress = "239.255.255.250";
                            ssdp.BindingAddress = ip.Address;
                            Console.WriteLine("binding start:{0},{1}",ip.Address,r.Name);
                            ssdp.OnNotifyAliveReceived += Ssdp_OnNotifyAliveReceived;
                            ssdp.OnSearchReceived += Ssdp_OnSearchReceived;
                            ssdp.OnNotifyByebyeReceived += Ssdp_OnNotifyByebyeReceived;
                            ssdp.OnNotifyUpdateReceived += Ssdp_OnNotifyUpdateReceived;
                            ssdp.OnResponseMessageReceived += Ssdp_OnResponseMessageReceived;
                            ssdp.AllowLoopbackMessage = true;
                            //初始化并加入多播组
                            ssdp.Activate();
                            //开始公告消息
                            ssdp.StartAdvertise();
                        }
                    }
                }
            }
            Console.ReadLine();
        }
        /// <summary>
        /// 消息响应事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="resp"></param>
        /// <param name="host"></param>
        private static void Ssdp_OnResponseMessageReceived(object sender, HttpResponse resp, string host)
        {
            Console.WriteLine("Response from {0}", host);
        }
        /// <summary>
        /// update通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        private static void Ssdp_OnNotifyUpdateReceived(object sender, UpdatePackage pack, string host)
        {
            Console.WriteLine("Notify update from {0}", host);
        }
        /// <summary>
        /// byebye通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        private static void Ssdp_OnNotifyByebyeReceived(object sender, ByebyePackage pack, string host)
        {
            Console.WriteLine("Notify byebye from {0}", host);
        }
        /// <summary>
        /// m-search消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pack"></param>
        /// <param name="host"></param>
        private static void Ssdp_OnSearchReceived(object sender, SearchPackage pack, string host)
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
        private static void Ssdp_OnNotifyAliveReceived(object sender, AlivePackage pack, string host)
        {
            Console.WriteLine("Notify alive from {0}", host);
        }
    }
}
