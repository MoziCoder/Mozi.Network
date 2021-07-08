using System;

namespace Mozi.StateService
{
    /// <summary>
    /// 订阅者信息
    /// </summary>
    public class Subscriber
    {
        public string Domain { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsForbidden { get; set; }
        public DateTime SubscribeTime { get; set; }
    }
}
