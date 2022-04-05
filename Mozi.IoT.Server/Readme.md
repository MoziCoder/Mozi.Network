# Mozi.IoT.Server 物联网网关

Mozi.IoT.Server是一个物联网标准通讯网关(CoAP协议)，该组件与Mozi.IoT(协议解析与通讯核心)共同构成IoT网关功能。

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

### 资源定义范例

~~~csharp

    /// <summary>
    /// 时间服务 UTC时间
    /// </summary>
    /// <remarks>
    /// 用于客户机查询服务时间或客户机时间校准
    /// </remarks>
    [ResourceDescription(Namespace = "core", Name = "time",Description ="isotime")]
    public class TimeResource : CoAPResource
    {
        public override uint ResourceSize { get => 0; }

        public override CoAPPackage OnGet(CoAPContext ctx)
        {
            CoAPPackage pack = base.OnGet(ctx);
            DateTime dt = DateTime.Now;
            pack.Payload = StringEncoder.Encode(dt.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            pack.Code = CoAPResponseCode.Content;
            return pack;
        }
    }

~~~
## 版权说明
	本项目采用MIT开源协议，引用请注明出处。欢迎复制，引用和修改。复制请注明出处，引用请附带证书。意见建议疑问请联系软件作者，或提交ISSUE。

### By [Jason][1] on Dec. 5,2021

[1]:mailto:brotherqian@163.com