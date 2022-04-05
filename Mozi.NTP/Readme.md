# Mozi.NTP

NTP授时服务器，目前实现了客户机/服务器模式的网络授时能力，低精度(15ms)互联网授时可直接应用，高精度授时需配合高精度时钟硬件,应用高精度授时场景前请先进行测试验证。

## 用例说明

~~~csharp

	class Program
    {
        static void Main(string[] args)
        {
            NTPServer server = new NTPServer();
            ///默认端口为123 可直接调用server.Start()
            server.Start(123);
            Console.ReadLine();
        }
    }
    //时钟实现范例

    /// <summary>
    /// 时钟实现，需配合硬件时钟，如果直接调用系统时钟，只能获得15.6ms的授时精度
    /// </summary>
    class AtomClock : IClock
    {
        private int _stratum = 1;
        /// <summary>
        /// 时钟编号
        /// </summary>
        public string Id => "00000000-0000-0000-0000-000000000001";

        public ClockIdentifier Name => ClockIdentifier.ATOM;

        public int Stratum { get => _stratum; set => _stratum = value; }

        public double Frequency => 1000;

        public double Precision => 1;

        public DateTime GetTime()
        {
            //实现原子钟调用
            throw new NotImplementedException();
        }

        public void SetTime(DateTime dt)
        {
            throw new NotImplementedException();
        }
    }

~~~
## 版权说明
	本项目采用MIT开源协议，引用请注明出处。欢迎复制，引用和修改。复制请注明出处，引用请附带证书。意见建议疑问请联系软件作者，或提交ISSUE。

### By [Jason][1] on Dec. 7,2021

[1]:mailto:brotherqian@163.com