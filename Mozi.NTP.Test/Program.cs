using System;

namespace Mozi.NTP.Test
{
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
}
