﻿using System;
using System.Net.NetworkInformation;
using Mozi.SSDP;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //开启SSDP服务
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var r in interfaces)
            {
                if (r.SupportsMulticast && r.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    foreach (var ip in r.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            Console.WriteLine("interface:{0}:{1}", r.Name, ip.Address);
                            SSDPService ssdp = new SSDPService();
                            ssdp.MulticastAddress = "239.255.255.250";
                            ssdp.BindingAddress = ip.Address;
                            ssdp.OnNotifyAliveReceived += Ssdp_OnNotifyAliveReceived;
                            ssdp.OnSearchReceived += Ssdp_OnSearchReceived;
                            ssdp.OnNotifyByebyeReceived += Ssdp_OnNotifyByebyeReceived;
                            ssdp.OnNotifyUpdateReceived += Ssdp_OnNotifyUpdateReceived;
                            ssdp.OnResponseMessageReceived += Ssdp_OnResponseMessageReceived;
                            ssdp.AllowLoopbackMessage = true;
                            ssdp.Activate();
                        }
                    }
                }
            }
            Console.ReadLine();
        }

        private static void Ssdp_OnResponseMessageReceived(object sender, HttpResponse resp, string host)
        {
            Console.WriteLine("Response from {0}", host);
        }

        private static void Ssdp_OnNotifyUpdateReceived(object sender, UpdatePackage pack, string host)
        {
            Console.WriteLine("Notify update from {0}", host);
        }

        private static void Ssdp_OnNotifyByebyeReceived(object sender, ByebyePackage pack, string host)
        {
            Console.WriteLine("Notify byebye from {0}", host);
        }

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

        private static void Ssdp_OnNotifyAliveReceived(object sender, AlivePackage pack, string host)
        {
            Console.WriteLine("Notify alive from {0}", host);
        }
    }
}
