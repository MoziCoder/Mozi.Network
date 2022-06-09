using Mozi.HttpEmbedded;

namespace Mozi.Live.RTP
{
    /// <summary>
    /// RTSP头属性
    /// </summary>
    public class RTSPHeaderProperty
    {
        //General  Description
        public static HeaderProperty AcceptRanges = new HeaderProperty("Accept-Ranges");
        public static HeaderProperty CacheControl = new HeaderProperty("Cache-Control");
        public static HeaderProperty Connection = new HeaderProperty("Connection");
        public static HeaderProperty CSeq = new HeaderProperty("CSeq");
        public static HeaderProperty Date = new HeaderProperty("Date");
        public static HeaderProperty MediaProperties = new HeaderProperty("Media-Properties");
        public static HeaderProperty MediaRange = new HeaderProperty("Media-Range");
        public static HeaderProperty PipelinedRequests = new HeaderProperty("Pipelined-Requests");
        public static HeaderProperty ProxySupported = new HeaderProperty("Proxy-Supported");
        public static HeaderProperty Range = new HeaderProperty("Range");
        public static HeaderProperty RTPInfo = new HeaderProperty("RTP-Info");
        public static HeaderProperty Scale = new HeaderProperty("Scale");
        public static HeaderProperty SeekStyle = new HeaderProperty("Seek-Style");
        public static HeaderProperty Server = new HeaderProperty("Server");
        public static HeaderProperty Session = new HeaderProperty("Session");
        public static HeaderProperty Speed = new HeaderProperty("Speed");
        public static HeaderProperty Supported = new HeaderProperty("Supported");
        public static HeaderProperty Timestamp = new HeaderProperty("Timestamp");
        public static HeaderProperty Transport = new HeaderProperty("Transport");
        public static HeaderProperty UserAgent = new HeaderProperty("User-Agent");
        public static HeaderProperty Via = new HeaderProperty("Via");

        //Request Description
        public static HeaderProperty Accept = new HeaderProperty("Accept");
        public static HeaderProperty AcceptCredentials = new HeaderProperty("Accept-Credentials");
        public static HeaderProperty AcceptEncoding = new HeaderProperty("Accept-Encoding");
        public static HeaderProperty AcceptLanguage = new HeaderProperty("Accept-Language");
        public static HeaderProperty Authorization = new HeaderProperty("Authorization");
        public static HeaderProperty Bandwidth = new HeaderProperty("Bandwidth");
        public static HeaderProperty Blocksize = new HeaderProperty("Blocksize");
        public static HeaderProperty From = new HeaderProperty("From");
        public static HeaderProperty IfMatch = new HeaderProperty("If-Match");
        public static HeaderProperty IfModifiedSince = new HeaderProperty("If-Modified-Since");
        public static HeaderProperty IfNoneMatch = new HeaderProperty("If-None-Match");
        public static HeaderProperty NotifyReason = new HeaderProperty("Notify-Reason");
        public static HeaderProperty ProxyAuthorization = new HeaderProperty("Proxy-Authorization");
        public static HeaderProperty ProxyRequire = new HeaderProperty("Proxy-Require");
        public static HeaderProperty Referrer = new HeaderProperty("Referrer");
        public static HeaderProperty RequestStatus = new HeaderProperty("Request-Status");
        public static HeaderProperty Require = new HeaderProperty("Require");
        public static HeaderProperty TerminateReason = new HeaderProperty("Terminate-Reason");

        //Response Description
        public static HeaderProperty AuthenticationInfo    = new HeaderProperty("Authentication-Info");
        public static HeaderProperty ConnectionCredentials = new HeaderProperty("Connection-Credentials");
        public static HeaderProperty Location               = new HeaderProperty("Location");
        public static HeaderProperty MTag                   = new HeaderProperty("MTag");
        public static HeaderProperty ProxyAuthenticate     = new HeaderProperty("Proxy-Authenticate");
        public static HeaderProperty Public                 = new HeaderProperty("Public");
        public static HeaderProperty RetryAfter            = new HeaderProperty("Retry-After");
        public static HeaderProperty Unsupported            = new HeaderProperty("Unsupported");
        public static HeaderProperty WWWAuthenticate       = new HeaderProperty("WWW-Authenticate");
        //Message Body Description
        public static HeaderProperty Allow = new HeaderProperty("Allow");
        public static HeaderProperty ContentBase     = new HeaderProperty("Content-Base");
        public static HeaderProperty ContentEncoding = new HeaderProperty("Content-Encoding");
        public static HeaderProperty ContentLanguage = new HeaderProperty("Content-Language");
        public static HeaderProperty ContentLength   = new HeaderProperty("Content-Length");
        public static HeaderProperty ContentLocation = new HeaderProperty("Content-Location");
        public static HeaderProperty ContentType     = new HeaderProperty("Content-Type");
        public static HeaderProperty Expires = new HeaderProperty("Expires");
        public static HeaderProperty LastModified    = new HeaderProperty("Last-Modified");
    }
}
