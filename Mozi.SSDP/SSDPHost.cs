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
}
