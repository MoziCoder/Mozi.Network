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

	dotnet add package Mozi.StateService --version 1.2.6

~~~
## 使用说明

~~~csharp

        static HeartBeatGateway hg = new HeartBeatGateway();

        static void Main(string[] args)
        {
            //开启状态服务
            HeartBeatService state = new HeartBeatService()
            {
                Port = 13453,
                RemoteHost = $"{port}"
            };

            state.ApplyDevice("Mozi", "80018001", "1.2.5");
            state.SetState(ClientLifeState.Alive);
            state.Init();
            state.Activate();

            //切换终端状态
            state.SetState(ClientLifeState.Idle);

            //心跳服务网关
            hg.OnClientStateChange += Hg_OnClientStateChange;
            hg.Start(13453);
            Console.ReadLine();
        }
~~~
### By [Jason][1] on Jun. 5,2021

[1]:mailto:brotherqian@163.com
[gitee]:https://gitee.com/myui_admin/mozi.git
[github]:https://github.com/MoziCoder/Mozi.HttpEmbedded.git
[codechina]:https://codechina.csdn.net/mozi/mozi.httpembedded.git