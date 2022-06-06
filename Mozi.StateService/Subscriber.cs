using System;

namespace Mozi.StateService
{
    /// <summary>
    /// 订阅者信息
    /// </summary>
    public class Subscriber
    {
        /// <summary>
        /// 域
        /// </summary>
        public string Domain { get; set; }
        /// <summary>
        /// 主机地址
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 是否禁止接收
        /// </summary>
        public bool IsForbidden { get; set; }
        /// <summary>
        /// 订阅时间
        /// </summary>
        public DateTime SubscribeTime { get; set; }
    }
}
