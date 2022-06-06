using Mozi.HttpEmbedded.Generic;

namespace Mozi.SSDP
{
    /// <summary>
    /// SSDP消息类型
    /// </summary>
    public class SSDPType : AbsClassEnum
    {
        /// <summary>
        /// ssdp:discover
        /// </summary>
        public static SSDPType Discover = new SSDPType("ssdp","discover");
        /// <summary>
        /// ssdp:all
        /// </summary>
        public static SSDPType All = new SSDPType("ssdp", "all");
        /// <summary>
        /// ssdp:alive
        /// </summary>
        public static SSDPType Alive = new SSDPType("ssdp", "alive");
        /// <summary>
        /// ssdp:byebye
        /// </summary>
        public static SSDPType Byebye = new SSDPType("ssdp", "byebye");
        /// <summary>
        /// ssdp:update
        /// </summary>
        public static SSDPType Update = new SSDPType("ssdp", "update");
        /// <summary>
        /// upnp:event
        /// </summary>
        public static SSDPType Event = new SSDPType("upnp", "event");
        /// <summary>
        /// upnp:rootdevice
        /// </summary>
        public static SSDPType RootDevice = new SSDPType("upnp", "rootdevice");
        /// <summary>
        /// upnp:propchange
        /// </summary>
        public static SSDPType PropChange = new SSDPType("upnp", "propchange");

        private readonly string _name;

        private readonly string _domain;

        //discover all alive byebye
        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="name"></param>
        public SSDPType(string domain,string name)
        {
            _domain = domain;
            _name = name;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{_domain}:{_name}";
        }
        /// <summary>
        /// 
        /// </summary>
        protected override string Tag {
            get { return _domain + ":"+_name; }
        }
    }
}
