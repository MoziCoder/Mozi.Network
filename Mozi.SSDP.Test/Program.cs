using Mozi.HttpEmbedded;
using System;

namespace Mozi.SSDP.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SSDPHost host = SSDPHost.Instance;
            host.SetNotifyAliveReceived(SSDP_OnNotifyAliveReceived);
            host.SetNotifyByebyeReceived(SSDP_OnNotifyByebyeReceived);
            host.SetSearchResponsed(SSDP_OnSearchResponsed);
            host.SetSearchReceived(SSDP_OnSearchReceived);
            host.SetNotifyUpdateReceived(SSDP_OnNotifyUpdateReceived);
            host.Activate();
            host.Search(TargetDesc.Parse("ssdp:all"));
            host.Search(TargetDesc.Parse("ssdp:all"));
            host.Search(TargetDesc.Parse("ssdp:all"));
            Console.ReadLine();
        }

        /// <summary>
        /// 消息响应事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="resp"></param>
        /// <param name="host"></param>
        protected static void SSDP_OnSearchResponsed(object sender, HttpResponse resp, string host)
        {
            Console.WriteLine("Response from {0}", host);
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

            //SearchResponsePackage search = new SearchResponsePackage();
            
            //search.HOST = string.Format("{0}:{1}", service.MulticastAddress, service.MulticastPort);
            //search.CacheTimeout = 3600;
            //search.USN = service.USN;
            //search.ST = pack.ST;
            //search.Server = service.Server;

            //ssdp:all
            if (pack.ST.IsAll)
            {
                Console.WriteLine("Search from {0},looking for ssdp:all", host);
            }
            //upnp:rootdevice
            else if (pack.ST.IsRootDevice)
            {
                Console.WriteLine("Search from {0},looking for upnp:rootdevice", host);
            }
            //urn:schema-upnp-org:device:deviceName:version
            else
            {
                Console.WriteLine("Search from {0},looking for {1}", host, pack.ST.ToString());
            }
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
    }

}
