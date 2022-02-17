using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    /// <summary>
    /// SSDP请求方法
    /// </summary>
    public class RequestMethodUPnP 
    {
        public static RequestMethod MSEARCH = new RequestMethod("M-SEARCH");
        public static RequestMethod NOTIFY = new RequestMethod("NOTIFY");
        public static RequestMethod SUBSCRIBE = new RequestMethod("SUBSCRIBE");
        public static RequestMethod UNSUBSCRIBE = new RequestMethod("UNSUBSCRIBE");
    }
}
