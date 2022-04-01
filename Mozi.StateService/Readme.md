# Mozi.StateService 心跳服务

## 项目介绍

Mozi.StateService是一个基于.Net开发的UDP心跳组件，基于UDP Socket开发，主要面向终端检活场景。一般的TCP/UPD心跳包，仅仅包含特殊的字节片段，没有业务承载能力。 

## 特点
    自行设计了一套紧凑的心跳协议，该协议仅仅包含必备要素。包括：协议版本，设备名，设备号，终端程序版本，终端用户名。

## 功能模块

- HeartBeatService
    心跳客户端  
    终端调用此组件，定时向服务器发送在线通知。

- HeartBeatGateway
    心跳网关  
    接收终端心跳信息，并检查终端在线状态管理。网关负责接收心跳数据，统计心跳数据，并转发数据到订阅者

- HearBeatSubScriber
    心跳订阅者  
    向服务器订阅心跳信息，订阅者为已知订阅者，客户端不可随意订阅，订阅者必须由网关主动添加到订阅者列表。

## 项目地址

- [Github][github]

- [Gitee][gitee]

- [CSDN][codechina]

## 程序下载

~~~shell

	dotnet add package Mozi.StateService --version 1.3.8

~~~
## 使用说明

~~~csharp

        //网关
        static readonly HeartBeatGateway hg = new HeartBeatGateway();
        //订阅者
        static readonly HeartBeatSubscriber hbs = new HeartBeatSubscriber();

        static void Main(string[] args)
        {

            //服务网关

            hg.AddSubscriber(new Subscriber() { Host = "{host}", Port = 13452 });
            hg.AddSubscriber(new Subscriber() { Host = "{host}", Port = 13452 });

            hg.OnClientOnlineStateChange += Hg_OnClientStateChange;
            hg.OnClientMessageReceived += Hg_OnClientMessageReceived;
            hg.OnClientUserChange += Hg_OnClientUserChange;
            hg.OnClientJoin += Hg_OnClientJoin;
            hg.OnClientLifeStateChange += Hg_OnClientLifeStateChange;
            hg.Start(13453);

            //订阅者

            hbs.OnClientOnlineStateChange += Hg_OnClientStateChange;
            hbs.OnClientMessageReceived += Hg_OnClientMessageReceived;
            hbs.OnClientUserChange += Hg_OnClientUserChange;
            hbs.OnClientJoin += Hg_OnClientJoin;
            hbs.OnClientLifeStateChange += Hg_OnClientLifeStateChange;
            hbs.Start(13452);

            //客户端 为不干扰主线程运行，建议对HeartBeatService进行再次封装.可直接调用包内 StateServiceHost运行单例模式

            HeartBeatService stateHost =new HeartBeatService();
            stateHost.StateChangeNotifyImmediately = true;
            stateHost.ApplyDevice("Mozi", "0000-0001", "1.3.7");
            stateHost.RemoteHost = "{host}";
            stateHost.Port = 13453;
            stateHost.Activate();

            Console.ReadLine();

        }
        /// <summary>
        /// 终端心跳状态变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="clientInfo"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        private static void Hg_OnClientLifeStateChange(object sender, ClientAliveInfo clientInfo, ClientLifeState oldState, ClientLifeState newState)
        {
            
        }

        /// <summary>
        /// 新增终端事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="clientInfo"></param>
        private static void Hg_OnClientJoin(object sender, ClientAliveInfo clientInfo)
        {
           
        }
        /// <summary>
        /// 终端登录用户变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="client"></param>
        /// <param name="oldUser"></param>
        /// <param name="newUser"></param>
        private static void Hg_OnClientUserChange(object sender, ClientAliveInfo client, string oldUser, string newUser)
        {
            
        }
        /// <summary>
        /// 终端消息接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="client"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        private static void Hg_OnClientMessageReceived(object sender, ClientAliveInfo client,string host,int port)
        {
            Console.WriteLine("{4:MMdd HH:mm:ss}|N:{0},ID:{1},S:{2},V:{3},{5},{6}", client.DeviceName, client.DeviceId, client.State, client.AppVersion, client.BeatTime, host,client.UserName);

            //对终端累计忙时进行统计，此处需要终端配合进行[闲/忙]切换

        }

        private static void Hg_OnClientStateChange(object sender, ClientAliveInfo clientInfo, ClientOnlineState oldState, ClientOnlineState newState)
        {
            Console.Title = hg.Clients.Count.ToString();
        }
    }

~~~

## 版权说明
	本项目采用MIT开源协议，引用请注明出处。欢迎复制，引用和修改。复制请注明出处，引用请附带证书。意见建议疑问请联系软件作者，或提交ISSUE。

### By [Jason][1] on Jun. 5,2021

[1]:mailto:brotherqian@163.com
[gitee]:https://gitee.com/myui_admin/mozi.git
[github]:https://github.com/MoziCoder/Mozi.Network.git
[codechina]:https://codechina.csdn.net/mozi/mozi.httpembedded.git