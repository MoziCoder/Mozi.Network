using System;

namespace Mozi.IoT.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //服务端
            CoAPServer cs = new CoAPServer();
            cs.RequestReceived += new MessageTransmit((a, b, c) =>
            {
                //Console.WriteLine($"Rev count:{PacketReceivedCount},current:{args.Data.Length}bytes,total:{TotalReceivedBytes}bytes");
                Console.WriteLine($"From:[{a}:{b}]");
                Console.WriteLine(c.ToString());
            });
            cs.Start();
            Console.ReadLine();

            ////客户端 多线程并发
            //List<CoAPClient> ccs = new List<CoAPClient>();
            //for (int i = 0; i < 32; i++)
            //{
            //    CoAPClient cc = new CoAPClient();
            //    //本地端口
            //    cc.SetPort(10 + i);
            //    cc.Start();
            //    ccs.Add(cc);
            //}
            ////电脑主机如果性能不行，不要尝试下面的方法
            //foreach (CoAPClient cc in ccs)
            //{

            //    var td = (new Thread(x =>
            //    {
            //        for (int i = 0; i < 1000000; i++)
            //        {
            //            //cc.Get("coap://127.0.0.1/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test");
            //            cc.Get("coap://127.0.0.1/sensor", CoAPMessageType.Confirmable);
            //        }
            //    }));
            //    td.Priority = ThreadPriority.Highest;
            //    td.Start();
            //}

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
    }
}
