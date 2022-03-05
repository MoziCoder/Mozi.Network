using Mozi.HttpEmbedded;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.Live.RTP
{
    class RTCPServer
    {

    }
    /// +---------------+-----------+--------+-------------+-------------+
    /// | method        | direction | object | Server req. | Client req. |
    /// +---------------+-----------+--------+-------------+-------------+
    /// | DESCRIBE      | C -> S    | P, S    | recommended | recommended |
    /// |               |           |        |             |             |
    /// | GET_PARAMETER | C -> S    | P, S    | optional    | optional    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P, S    | optional    | optional    |
    /// |               |           |        |             |             |
    /// | OPTIONS       | C -> S    | P, S    | required    | required    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P, S    | optional    | optional    |
    /// |               |           |        |             |             |
    /// | PAUSE         | C -> S    | P, S    | required    | required    |
    /// |               |           |        |             |             |
    /// | PLAY          | C -> S    | P, S    | required    | required    |
    /// |               |           |        |             |             |
    /// | PLAY_NOTIFY   | S -> C    | P, S    | required    | required    |
    /// |               |           |        |             |             |
    /// | REDIRECT      | S -> C    | P, S    | optional    | required    |
    /// |               |           |        |             |             |
    /// | SETUP         | C -> S    | S      | required    | required    |
    /// |               |           |        |             |             |
    /// | SET_PARAMETER | C -> S    | P, S    | required    | optional    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P, S    | optional    | optional    |
    /// |               |           |        |             |             |
    /// | TEARDOWN      | C -> S    | P, S    | required    | required    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P      | required    | required    |
    /// +---------------+-----------+--------+-------------+-------------+
    /// <summary>
    /// RTSP请求方法
    /// </summary>
    public class RTSPMethod
    {
        public static RequestMethod DESCRIBE = new RequestMethod("DESCRIBE");
        public static RequestMethod GET_PARAMETER = new RequestMethod("GET_PARAMETER");
        public static RequestMethod OPTIONS = new RequestMethod("OPTIONS");
        public static RequestMethod PAUSE = new RequestMethod("PAUSE");
        public static RequestMethod PLAY = new RequestMethod("PLAY");
        public static RequestMethod PLAY_NOTIFY = new RequestMethod("PLAY_NOTIFY");
        public static RequestMethod REDIRECT = new RequestMethod("REDIRECT");
        public static RequestMethod SETUP = new RequestMethod("SETUP");
        public static RequestMethod SET_PARAMETER = new RequestMethod("SET_PARAMETER");
        public static RequestMethod TEARDOWN = new RequestMethod("TEARDOWN");
    }

    public class RTSPVersion : AbsClassEnum
    {
        public static readonly RTSPVersion Version10 = new RTSPVersion("1.0");
        public static readonly RTSPVersion Version20 = new RTSPVersion("2.0");

        public string Version { get { return _vervalue; } }

        protected override string Tag { get { return _vervalue; } }

        private string _vervalue = "";

        private RTSPVersion(string vervalue)
        {
            _vervalue = vervalue;
        }
        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("RTSP/{0}", _vervalue);
        }
    }

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

    public class RTSPStatusCode
    {
        public static StatusCode Continue                                  = new StatusCode( 100   ,"Continue"); //all                      
        public static StatusCode OK                                        = new StatusCode( 200   ,"OK"); //all                      
        public static StatusCode MovedPermanently                          = new StatusCode( 301   ,"Moved Permanently"); //all                      
        public static StatusCode Found                                     = new StatusCode( 302   ,"Found"); //all                      
        public static StatusCode SeeOther                                  = new StatusCode( 303   ,"See Other"); //n/a                      
        public static StatusCode NotModified                               = new StatusCode( 304   ,"Not Modified"); //all                      
        public static StatusCode UseProxy                                  = new StatusCode( 305   ,"Use Proxy"); //all                      
        public static StatusCode BadRequest                                = new StatusCode( 400   ,"Bad Request"); //all                      
        public static StatusCode Unauthorized                              = new StatusCode( 401   ,"Unauthorized"); //all                      
        public static StatusCode PaymentRequired                           = new StatusCode( 402   ,"Payment Required"); //all                      
        public static StatusCode Forbidden                                 = new StatusCode( 403   ,"Forbidden"); //all                      
        public static StatusCode NotFound                                  = new StatusCode( 404   ,"Not Found"); //all                      
        public static StatusCode MethodNotAllowed                          = new StatusCode( 405   ,"Method Not Allowed"); //all                      
        public static StatusCode NotAcceptable                             = new StatusCode( 406   ,"Not Acceptable"); //all                      
        public static StatusCode ProxyAuthenticationRequired               = new StatusCode( 407   ,"Proxy Authentication Required"); //all                      
        public static StatusCode RequestTimeout                            = new StatusCode( 408   ,"Request Timeout"); //all                      
        public static StatusCode Gone                                      = new StatusCode( 410   ,"Gone"); //all                      
        public static StatusCode PreconditionFailed                        = new StatusCode( 412   ,"Precondition Failed"); //DESCRIBE, SETUP          
        public static StatusCode RequestMessageBodyTooLarge                = new StatusCode( 413   ,"Request Message Body Too Large"); //all                      
        public static StatusCode RequestURITooLong                         = new StatusCode( 414   ,"Request-URI Too Long"); //all                      
        public static StatusCode UnsupportedMediaType                      = new StatusCode( 415   ,"Unsupported Media Type"); //all                      
        public static StatusCode ParameterNotUnderstood                    = new StatusCode( 451   ,"Parameter Not Understood"); //SET_PARAMETER,           
        public static StatusCode reserved                                  = new StatusCode( 452   ,"reserved"); //n/a                      
        public static StatusCode NotEnoughBandwidth                        = new StatusCode( 453   ,"Not Enough Bandwidth"); //SETUP                    
        public static StatusCode SessionNotFound                           = new StatusCode( 454   ,"Session Not Found"); //all                      
        public static StatusCode MethodNotValid                            = new StatusCode( 455   ,"Method Not Valid in This State"); //all                      
        public static StatusCode HeaderFieldNotValid                       = new StatusCode( 456   ,"Header Field Not Valid for Resource"); //all                      
        public static StatusCode InvalidRange                              = new StatusCode( 457   ,"Invalid Range"); //PLAY, PAUSE              
        public static StatusCode ParameterIsReadOnly                       = new StatusCode( 458   ,"Parameter Is Read-Only"); //SET_PARAMETER      
        public static StatusCode AggregateOperationNotAllowed              = new StatusCode( 459   ,"Aggregate Operation Not Allowed"); //all                     
        public static StatusCode OnlyAggregateOperationAllowed             = new StatusCode( 460   ,"Only Aggregate Operation Allowed"); //all                     
        public static StatusCode UnsupportedTransport                      = new StatusCode( 461   ,"Unsupported Transport"); //all                     
        public static StatusCode DestinationUnreachable                    = new StatusCode( 462   ,"Destination Unreachable"); //all                     
        public static StatusCode DestinationProhibited                     = new StatusCode( 463   ,"Destination Prohibited"); //SETUP              
        public static StatusCode DataTransportNotReadyYet                  = new StatusCode( 464   ,"Data Transport Not Ready Yet"); //PLAY               
        public static StatusCode NotificationReasonUnknown                 = new StatusCode( 465   ,"Notification Reason Unknown"); //PLAY_NOTIFY        
        public static StatusCode KeyManagementError                        = new StatusCode( 466   ,"Key Management Error"); //all                
        public static StatusCode ConnectionAuthorizationRequired           = new StatusCode( 470   ,"Connection Authorization  Required"); //all                
        public static StatusCode ConnectionCredentialsNotAccepted          = new StatusCode( 471   ,"Connection Credentials Not Accepted"); //all                
        public static StatusCode FailuretoEstablishSecureConnection        = new StatusCode( 472   ,"Failure to Establish Secure Connection"); //all                
        public static StatusCode InternalServerError                       = new StatusCode( 500   ,"Internal Server Error"); //all                
        public static StatusCode NotImplemented                            = new StatusCode( 501   ,"Not Implemented"); //all                     
        public static StatusCode BadGateway                                = new StatusCode( 502   ,"Bad Gateway"); //all                     
        public static StatusCode ServiceUnavailable                        = new StatusCode( 503   ,"Service Unavailable"); //all                
        public static StatusCode GatewayTimeout                            = new StatusCode( 504   ,"Gateway Timeout"); //all                
        public static StatusCode RTSPVersionNotSupported                   = new StatusCode( 505   ,"RTSP Version Not Supported"); //all                
        public static StatusCode OptionNotSupported                        = new StatusCode( 551   ,"Option Not Supported"); //all                     
        public static StatusCode ProxyUnavailable                          = new StatusCode( 553, "Proxy Unavailable"); //all                      
    }
}
