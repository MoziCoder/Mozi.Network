using System;
using System.Collections.Generic;
using System.Threading;

namespace Mozi.IoT.Test.Net5
{
    class Program
    {
        static void Main(string[] args)
        {
            ////服务端
            //CoAPServer cs = new CoAPServer();
            //cs.Start();
            //Console.ReadLine();

            //客户端
            List<CoAPClient> ccs = new List<CoAPClient>();
            for (int i = 0; i < 64; i++)
            {
                CoAPClient cc = new CoAPClient();
                //本地端口
                cc.SetPort(12340 + i);
                cc.Start();
                ccs.Add(cc);
            }
            //CoAPPackage cp = new CoAPPackage();
            //cp.Code = CoAPRequestMethod.Get;
            //cp.Token = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            //cp.MessageType = CoAPMessageType.Confirmable;
            //cp.SetOption(CoAPOptionDefine.UriPath, "sensor");
            //cp.SetOption(CoAPOptionDefine.UriPath, "summit");
            //cp.SetContentType(ContentFormat.TextPlain);
            //cc.SendMessage("127.0.0.1", 5683, cp);
            foreach (CoAPClient cc in ccs)
            {
                for(int i = 0; i < 10; i++)
                {
                    var td = (new Thread(x =>
                    {
                        for(int j = 0; j < 80000; j++)
                        {
                            //cc.Get("coap://127.0.0.1/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test/sensor/test");
                            cc.Get("coap://127.0.0.1/sensor", CoAPMessageType.Confirmable);
                        }
                    }));
                    td.Priority = ThreadPriority.Normal;
                    td.Start();
                }
            }



            //CoAPClient cc = new CoAPClient();
            ////本地端口
            //cc.SetPort(12340);
            //cc.Start();
            //cc.Get("coap://100.100.0.105/sensor/getinfo", CoAPMessageType.Confirmable);

            Console.ReadLine();
        }
    }
}
