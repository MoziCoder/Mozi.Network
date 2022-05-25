using Mozi.HttpEmbedded;
using System;

namespace Mozi.SSDP.Test
{
    class Program
    {
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

}
