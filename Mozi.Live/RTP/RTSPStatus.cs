using Mozi.HttpEmbedded;

namespace Mozi.Live.RTP
{
    /// <summary>
    /// RTSP状态码
    /// </summary>
    public class RTSPStatus
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
        public static StatusCode ProxyUnavailable                          = new StatusCode( 553   ,"Proxy Unavailable"); //all                      
    }
}
