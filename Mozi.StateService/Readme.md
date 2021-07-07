# Mozi.StateService 心跳服务

## 项目介绍

Mozi.StateService是一个心跳服务组件，基于UDP开发。项目主要面向终端检活场景，包含两个可用组件: 

### HeartBeatService
    心跳客户端  
    终端调用此组件，定时向服务器发送在线通知。

### HeartBeatGateway
    心跳网关  
    接收终端心跳信息，并检查终端在线状态管理。

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

            state.ApplyDevice("Mozi", "80018001", "1.2.4");
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