using System;

namespace Mozi.StateService.Test
{
    class Program
    {
        //网关
        static readonly HeartBeatGateway hg = new HeartBeatGateway();
        //订阅者
        static readonly HeartBeatSubscriber hbs = new HeartBeatSubscriber();

        static void Main(string[] args)
        {

            ////开启状态服务
            //HeartBeatService state = new HeartBeatService()
            //{
            //    Port = 13453,
            //    RemoteHost = $"100.100.0.111"
            //};

            //state.ApplyDevice("Mozi", "80018001", "1.2.3");
            //state.SetState(ClientLifeState.Alive);
            //state.Init();
            //state.Activate();
            //state.SetState(ClientLifeState.Idle);

            //服务网关

            hg.AddSubscriber(new Subscriber() { Host = "100.100.0.171", Port = 13452 });
            hg.AddSubscriber(new Subscriber() { Host = "100.100.0.105", Port = 13452 });
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
            stateHost.RemoteHost = "127.0.0.1";
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
}
