# Mozi.IoT.Server 物联网网关

Mozi.IoT.Server是一个物联网标准通讯网关(CoAP协议)，该组件与Mozi.IoT共同构成IoT网关功能。

###　服务启动范例

~~~csharp
    class Program
    {
        static void Main(string[] args)
        {
            //服务端
            CoAPServer cs = new CoAPServer();
            cs.Start();
            Console.ReadLine();
        }
    }
~~~