using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Mozi.IoT.Test
{
    class Program
    {
        
        private static ulong _packetReceived=0;
        private static ulong _bytesReceived = 0;
        private static ulong pps = 0;
        static void Main(string[] args)
        {
            //Stopwatch sp = new Stopwatch();
            //////服务端
            //CoAPServer cs = new CoAPServer();
            //cs.RequestReceived += new MessageTransmit((host, port, pack) =>
            //{
            //    //Console.WriteLine($"Rev count:{PacketReceivedCount},current:{args.Data.Length}bytes,total:{TotalReceivedBytes}bytes");
            //    Console.WriteLine($"From:[{host}:{port}]");
            //    Console.WriteLine(pack.ToString());
            //    Console.Title = string.Format("elapsed:{2},count:{0},pps:{3},bytes:{1}", cs.PacketReceivedCount, cs.TotalReceivedBytes,FormatSeconds(sp.ElapsedMilliseconds),pps);
            //});

            //Timer timer = new Timer((x) =>
            //{
            //    pps = cs.PacketReceivedCount - _packetReceived;

            //    _packetReceived = cs.PacketReceivedCount;
            //    _bytesReceived = cs.TotalReceivedBytes;
            //});
            //timer.Change(0, 1000);
            //sp.Start();
            //cs.Start();
            //Console.ReadLine();

            //客户端 多线程并发
            List<CoAPClient> ccs = new List<CoAPClient>();
            for (int i = 0; i < 4; i++)
            {
                CoAPClient cc = new CoAPClient();
                //本地端口
                cc.SetPort(10 + i);
                cc.Start();
                ccs.Add(cc);
            }
            //电脑主机如果性能不行，不要尝试下面的方法
            foreach (CoAPClient cc in ccs)
            {
                var td = (new Thread(x =>
                {
                    for (int i = 0; i < 1000000; i++)
                    {
                        //cc.Get("coap://127.0.0.1/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test");
                        cc.Get("coap://127.0.0.1/sensor", CoAPMessageType.Confirmable);
                        Thread.Sleep(1);
                    }
                }));
                td.Priority = ThreadPriority.Highest;
                td.Start();
            }

            ////客户端调用 基本
            //CoAPClient cc = new CoAPClient();
            ////本地端口
            //cc.SetPort(12340);
            //cc.Start();
            //cc.Get("coap://127.0.0.1/sensor/getinfo", CoAPMessageType.Confirmable);

            ////客户端调用 高级方法 需要熟悉协议
            //CoAPPackage cp = new CoAPPackage();
            //cp.Code = CoAPRequestMethod.Get;
            //cp.Token = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            //cp.MessageType = CoAPMessageType.Confirmable;
            //cp.SetOption(CoAPOptionDefine.UriPath, "sensor");
            //cp.SetOption(CoAPOptionDefine.UriPath, "summit");
            //cp.SetContentType(ContentFormat.TextPlain);
            //cc.SendMessage("127.0.0.1", 5683, cp);

            Console.ReadLine();

        }

        private static string FormatSeconds(long ms)
        {
            var seconds = ms / 1000;
            var day = seconds / 3600 / 24;
            var hour = (seconds - (day * 3600 * 24)) / 3600;
            var minute = (seconds - (day * 3600 * 24) - hour * 3600) / 60;
            var second = seconds - day * 3600 * 24 - hour * 3600 - minute * 60;
            return string.Format("{0}天,{1:00}:{2:00}:{3:00}", day, hour, minute, second);
        }
    }
}
