﻿using System;

namespace Mozi.StateService.Test
{
    class Program
    {
        static HeartBeatGateway hg = new HeartBeatGateway();
        static void Main(string[] args)
        {
            ////开启状态服务
            //HeartBeatService state = new HeartBeatService()
            //{
            //    Port = 13453,
            //    RemoteHost = "100.100.0.105"
            //};

            //state.ApplyDevice("Mozi.StateService", "80018001", "1.2.3");
            //state.SetState("alive");
            //state.Init();
            //state.Activate();

            hg.OnClientStateChange += Hg_OnClientStateChange;
            hg.Start(13453);
            Console.ReadLine();
        }

        private static void Hg_OnClientStateChange(object sender, ClientAliveInfo clientInfo, ClientState oldState, ClientState newState)
        {
            Console.Title = hg.Clients.Count.ToString();
        }
    }
}
