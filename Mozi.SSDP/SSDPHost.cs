using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Mozi.SSDP
{
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
                            ssdp.AllowLoopbackMessage = true;
                            //初始化并加入多播组

                            _services.Add(ssdp);

                        }
                    }
                }
            }
        }

        //public void ApplyDevice()
        //{
        //    foreach (var sd in _services)
        //    {
        //        sd.PackDefaultAlive
        //    }
        //}

        public void Search(SearchPackage sp)
        {
            foreach (var service in _services)
            {
                service.Search(sp);
            }
        }
        /// <summary>
        /// M-SEARCH
        /// </summary>
        /// <param name="td"></param>
        public void Search(TargetDesc td)
        {
            foreach (var service in _services)
            {
                service.Search(td);
            }
        }

        public void SetMulticastAddress(string address)
        {
            foreach (var service in _services)
            {
                service.MulticastAddress = address;
            }
        }

        public void SetMulticastPort(int port)
        {
            foreach (var service in _services)
            {
                service.MulticastPort = port;
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

        public void SetResponseMessageReceived(ResponseMessageReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnResponseMessageReceived += dlg;
            }
        }

        public void SetNotifyUpdateReceived(NotifyUpdateReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnNotifyUpdateReceived += dlg;
            }
        }
        public void SetNotifyAliveReceived(NotifyAliveReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnNotifyAliveReceived += dlg;
            }
        }
        public void SetNotifyByebyeReceived(NotifyByebyeReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnNotifyByebyeReceived += dlg;
            }
        }
        public void SetSearchReceived(SearchReceived dlg)
        {
            foreach (var service in _services)
            {
                service.OnSearchReceived += dlg;
            }
        }

        public void ControlAction(SSDPService service, ControlActionPackage pk)
        {
            service.ControlAction(pk);
        }

        //public void ControlQuery(SSDPService service,ControlQueryPackage pk)
        //{
        //    service.ControlAction(pk);
        //}

        public void EchoSearch(SSDPService service,SearchResponsePackage pk)
        {
             service.EchoSearch(pk);
        }

        public void NotifyAlive(SSDPService service, AlivePackage pk)
        {
             service.NotifyAlive(pk);
        }

        public void NotifyLeave(SSDPService service, ByebyePackage pk)
        {
             service.NotifyLeave(pk);
        }

        public void NotifyUpdate(SSDPService service, UpdatePackage pk)
        {
             service.NotifyUpdate(pk);
        }

        public void Subscribe(SSDPService service, SubscribePackage pk)
        {
             service.Subscribe(pk);
        }

        public void UnSubscribe(SSDPService service, SubscribePackage pk)
        {
             service.UnSubscribe(pk);
        }
    }
}
