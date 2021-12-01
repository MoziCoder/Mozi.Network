using System;

namespace Mozi.NTP
{
    /// <summary>
    /// Clock UTC时钟
    /// </summary>
    public interface IClock
    {
        string Id { get; }
        ClockIdentifier Name { get; }
        /// <summary>
        /// 时钟层数
        /// </summary>
        int Stratum { get; set; }
        /// <summary>
        /// 时钟运行频率 Hz
        /// </summary>
        double Frequency { get; }
        /// <summary>
        /// 时钟精度 单位ms
        /// </summary>
        double Precision { get; }
        /// <summary>
        /// 获取时钟时间
        /// </summary>
        /// <returns></returns>
        DateTime GetTime();
        /// <summary>
        /// 设置时钟时间
        /// </summary>
        /// <param name="dt"></param>
        void SetTime(DateTime dt);
    }

    public class SystemClock : IClock
    {
        private int _stratum = 1;

        public double Frequency { get  { return 14318180; } }

        public double Precision => 15.6d;

        public string Id => "00000000-0000-0000-0000-000000000000";

        public ClockIdentifier Name => ClockIdentifier.LOCL;

        public int Stratum { get => _stratum; set => _stratum = value; }

        public DateTime GetTime()
        {
            return DateTime.UtcNow;
        }

        public void SetTime(DateTime dt)
        {
            
        }
    }
}
