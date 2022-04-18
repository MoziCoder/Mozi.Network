using Mozi.HttpEmbedded.Generic;

namespace Mozi.SSDP
{
    /// <summary>
    /// SSDP消息类型
    /// </summary>
    public class SSDPType : AbsClassEnum
    {
        public static SSDPType Discover = new SSDPType("ssdp","discover");
        public static SSDPType All = new SSDPType("ssdp", "all");
        public static SSDPType Alive = new SSDPType("ssdp", "alive");
        public static SSDPType Byebye = new SSDPType("ssdp", "byebye");
        public static SSDPType Update = new SSDPType("ssdp", "update");
        public static SSDPType Event = new SSDPType("upnp", "event");
        public static SSDPType RootDevice = new SSDPType("upnp", "rootdevice");
        public static SSDPType PropChange = new SSDPType("upnp", "propchange");

        private readonly string _name;

        private readonly string _domain;

        //discover all alive byebye
        public SSDPType(string domain,string name)
        {
            _domain = domain;
            _name = name;
        }
        public override string ToString()
        {
            return $"{_domain}:{_name}";
        }
        protected override string Tag {
            get { return _domain + ":"+_name; }
        }
    }
}
