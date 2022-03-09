using Mozi.HttpEmbedded;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.SSDP
{
    /// <summary>
    /// 订阅请求包
    /// </summary>
    public class SubscribePackage : AbsAdvertisePackage
    {
        public TargetDesc NT { get; set; }
        public string CALLBACK { get; set; }
        public string SID { get; set; }
        public string TIMEOUT { get; set; }
        public string UserAgent { get; set; }
        /// <summary>
        /// a, b, c这种分割字符
        /// </summary>
        public string StateVar { get; set; }
        /// <summary>
        /// <para>
        /// NT设置无效，会被统一设置为<see cref="SSDPType.Event"/>
        /// </para>
        /// </summary>
        /// <returns></returns>
        public override TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add("HOST", $"{HOST}");
            headers.Add("NT",SSDPType.Event.ToString());
            headers.Add("USER-AGENT", UserAgent);
            headers.Add("TIMEOUT", "Second-"+TIMEOUT);
            headers.Add("CALLBACK", CALLBACK);
            headers.Add("STATEVAR", StateVar);
            return headers;
        }
        /// <summary>
        /// NT字段会被转化为SID字段，请给NT赋值
        /// </summary>
        /// <returns></returns>
        public TransformHeader GetUnsubscribe()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add("HOST", $"{HOST}");
            headers.Add("SID",NT.ToString());
            return headers;
        }
    }

    //NOTIFY* HTTP/1.0
    //HOST: 239.255.255.246:7900 *** note the port number is different than SSDP***
    //CONTENT-TYPE: text/xml; charset="utf-8"
    //USN: Unique Service Name for the publisher
    //SVCID: ServiceID from SCPD
    //NT: upnp:event
    //NTS: upnp:propchange
    //SEQ: monotonically increasing sequence count
    //LVL: event importance
    //BOOTID.UPNP.ORG: number increased each time device sends an initial announce or update message
    //CONTENT-LENGTH: bytes in body 
    //<?xml version="1.0"?>
    //<e:propertyset xmlns:e="urn:schemas-upnp-org:event-1-0">
    //    <e:property> <variableName>new value</variableName></e:property>
    //    <!-- Other variable names and values(if any) go here. -->
    //</e:propertyset>
    /// <summary>
    /// 事件请求包
    /// </summary>
    public class EventPackage : AbsAdvertisePackage
    {
        public string ContentType { get; set; }
        public USNDesc USN { get; set; }
        public string SVCID { get; set; }
        public TargetDesc NT { get; set; }
        public SSDPType NTS { get; set; }
        public int SEQ { get; set; }
        public int LVL { get; set; }
        public int BootId {get;set;}
        public int ContentLength { get; set; }
        /// <summary>
        /// delivery path
        /// </summary>
        public Property[] PropertySet { get; set; }

        //public override TransformHeader GetHeaders()
        //{
        //    TransformHeader headers = new TransformHeader();
        //    headers.Add("HOST", $"{HOST}");
        //    headers.Add("NT", SSDPType.Event.ToString());
        //    headers.Add("NTS", SSDPType.PropChange.ToString());
        //    headers.Add("CONTENT-TYPE","text/xml; charset=\"utf-8\"");
        //    return headers;
        //}
        public string CreateBody()
        {
            string doc= "<?xml version=\"1.0\"?><e:propertyset xmlns:e=\"urn:schemas-upnp-org:event-1-0\">";
            foreach(var p in PropertySet)
            {
                doc += "<e:property>";
                foreach(var v in p.Variables)
                {
                    doc += string.Format("<{0}>{1}</{0}>", v.VariableName,v.Value);
                }
                doc += "</e:property>";
            }
            doc += "</e:propertyset>";
            return doc;
        }
    }

    public class Property
    {
        public Variable[] Variables { get; set; }
    }

    public class Variable
    {
        public string VariableName { get; set; }
        public string Value { get; set; }
    }
    /// <summary>
    /// 事件级别
    /// </summary>
    public class EventLevel : AbsClassEnum
    {

        public static EventLevel Emergency =new EventLevel("upnp","emergency");
        public static EventLevel Fault=new EventLevel("upnp","fault");
        public static EventLevel Warning = new EventLevel("upnp","warning");
        public static EventLevel Info = new EventLevel("upnp","info");
        public static EventLevel Debug = new EventLevel("upnp","debug");
        public static EventLevel General = new EventLevel("upnp","general");

        private string _levelName = "";
        private string _domain = "";

        public override string ToString()
        {
            return _domain + ":/" + _levelName;
        }
        protected override string Tag
        {
            get { return _domain +":/"+ _levelName; }
        }

        public EventLevel(string domain,string levelName)
        {
            _domain = domain;
            _levelName = levelName;
        }
    }
}
