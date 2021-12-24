using System;

namespace Mozi.IoT.Test
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

            CoAPClient cc = new CoAPClient();
            cc.SetPort(12341);
            cc.Start();
            //CoAPPackage cp = new CoAPPackage();
            //cp.Code = CoAPRequestMethod.Get;
            //cp.Token = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            //cp.MessageType = CoAPMessageType.Confirmable;
            //cp.SetOption(CoAPOptionDefine.UriPath, "sensor");
            //cp.SetOption(CoAPOptionDefine.UriPath, "summit");
            //cp.SetContentType(ContentFormat.TextPlain);
            //cc.SendMessage("127.0.0.1", 5683, cp);
            cc.Get("coap://www.boshentech.com/sensor/test");
            Console.ReadLine();
        }
    }
}
