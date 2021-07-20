using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 协议类型
    /// </summary>
    public class ProtocolType : AbsClassEnum
    {
        public static ProtocolType HTTP = new ProtocolType("HTTP");
        public static ProtocolType HTTPS = new ProtocolType("HTTPS");

        private string _protocol;

        public string Protocol
        {
            get { return _protocol; }
            set { _protocol = value; }
        }

        protected override string Tag { get { return _protocol; } }

        public static System.Net.Sockets.ProtocolType Tcp { get; internal set; }

        private ProtocolType(string typeName)
        {
            _protocol = typeName;
        }
    }
}