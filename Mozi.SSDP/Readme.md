# Mozi.SSDP

Mozi.SSDP是一个基于.Net开发的SSDP服务组件，目标是为.Net应用程序提供完善的SSDP服务功能。 项目对UDP Socket进行封装，并遵循UPNP/2.0，实现了UPNP2.0规范中的大部分功能。

## 功能特性

1. 发现-在线
	- 在线通知
	- 离线通知
	- 搜索
	- 更新

3. 设备和服务描述

3. 控制

4. 事件

## 项目地址

- [Github][github]

- [Gitee][gitee]

- [CSDN][codechina]

## 程序下载

~~~shell

	dotnet add package Mozi.SSDP --version 1.2.5

~~~
## 项目依赖  

[Mozi.HttpEmbedded][httpembedded] > 1.2.5

## 版权说明
	本项目采用MIT开源协议，引用请注明出处。欢迎复制，引用和修改。意见建议疑问请联系软件作者，或提交ISSUE。

## 用例说明

~~~csharp

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

~~~
### By [Jason][1] on Feb. 5,2020

[1]:mailto:brotherqian@163.com
[gitee]:https://gitee.com/myui_admin/mozi.git
[github]:https://github.com/MoziCoder/Mozi.HttpEmbedded.git
[codechina]:https://codechina.csdn.net/mozi/mozi.httpembedded.git
[httpembedded]:https://gitee.com/myui_admin/mozi.git