using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    /// <summary>
    /// SSDP请求方法
    /// </summary>
    public class RequestMethodUPnP 
    {
        /// <summary>
        /// M-SEARCH
        /// </summary>
        public static RequestMethod MSEARCH = new RequestMethod("M-SEARCH");
        /// <summary>
        /// NOTIFY
        /// </summary>
        public static RequestMethod NOTIFY = new RequestMethod("NOTIFY");
        /// <summary>
        /// SUBSCRIBE
        /// </summary>
        public static RequestMethod SUBSCRIBE = new RequestMethod("SUBSCRIBE");
        /// <summary>
        /// UNSUBSCRIBE
        /// </summary>
        public static RequestMethod UNSUBSCRIBE = new RequestMethod("UNSUBSCRIBE");
    }
}
