using Mozi.HttpEmbedded;
using Mozi.Live.RTP;
using Mozi.Live.RTSP;
using System;
using System.Linq;

namespace Mozi.Live
{

    //TODO 实现UDP传输信道
    //TODO 实现控制服务端的API

    /// <summary>
    /// RTSP2.0 服务器
    /// </summary>
    public class RTSPServer: HttpServer
    {
        private int _port = RTSPProtocol.Port;

        private string[] _features = { "play.basic", "play.scale", "play.speed", "setup.rtp.rtcp.mux" };

        private RTSPTransportProtocol _transport = new RTSPTransportProtocol();

        /// <summary>
        /// 工作端口，默认为<see cref="RTSPProtocol.Port"/>
        /// </summary>
        public override int Port
        {
            get { return _port; } protected set {_port = value;}
        }
        /// <summary>
        /// 
        /// </summary>
        public RTSPServer()
        {
            Version = RTSPVersion.Version20;
            ServerName = "Mozi.Live";
            //定义允许的方法
            MethodPublic = new RequestMethod[] { RTSPMethod.OPTIONS, RTSPMethod.DESCRIBE, RTSPMethod.PLAY, RTSPMethod.PAUSE, RTSPMethod.SETUP, RTSPMethod.TEARDOWN, RTSPMethod.SET_PARAMETER, RTSPMethod.GET_PARAMETER };
        }
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override StatusCode HandleRequest(ref HttpContext context)
        {
            context.Response.Version = RTSPVersion.Version20;
            //不添加默认的头信息
            context.Response.DontAddAutoHeader = true;
            RequestMethod method = context.Request.Method;

            context.Response.AddHeader(RTSPHeaderProperty.CSeq, context.Request.Headers.GetValue(RTSPHeaderProperty.CSeq));
            try
            {
                if (method == RTSPMethod.OPTIONS)
                {
                    return HandleRequestOptions(ref context);
                }
                else if (method == RTSPMethod.SETUP)
                {
                    return HandleRequestSetup(ref context);
                }
                else if (method == RTSPMethod.DESCRIBE)
                {
                    return HandleRequestDescribe(ref context);
                }
                else if (method == RTSPMethod.PLAY)
                {
                    return HandleRequestPlay(ref context);
                }
                else if (method == RTSPMethod.PAUSE)
                {
                    return HandleRequestPause(ref context);
                }
                else if (method == RTSPMethod.TEARDOWN)
                {
                    return HandleRequestTeardown(ref context);
                }
                else if (method == RTSPMethod.GET_PARAMETER)
                {
                    return HandleRequestGetParameter(ref context);
                }
                else if (method == RTSPMethod.SET_PARAMETER)
                {
                    return HandleRequestSetParameter(ref context);
                }
                return RTSPStatus.NotAcceptable;
            }
            catch
            {
                return RTSPStatus.InternalServerError;
            }
        }

        /// <summary>
        /// 响应Option请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestOptions(ref HttpContext context)
        {
            context.Response.AddHeader(RTSPHeaderProperty.Public, MethodPublic.Select(x=>x.Name).ToArray());
            //context.Response.AddHeader(RTSPHeaderProperty.Supported, string.Join(";",_features));
            // Sends 200 OK
            return RTSPStatus.OK;
        }
        /// <summary>
        /// 响应Setup请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestSetup(ref HttpContext context)
        {
            var transport = context.Request.Headers.GetValue(RTSPHeaderProperty.Transport);
            RTSPTransportPropertyV1 trans = RTSPTransportPropertyV1.Parse(transport);

            RTSPTransportPropertyV1 tranResp = new RTSPTransportPropertyV1();
            tranResp.Protocol = RTSPTransportProtocol.AVP;
            tranResp.DeliveryMode = "unicast";
            tranResp.ClientPort = trans.ClientPort;
            tranResp.ServerPort = "3005-3006";

            context.Response.AddHeader(RTSPHeaderProperty.Date, DateTime.Now.ToUniversalTime().ToString("r"));
            context.Response.AddHeader(RTSPHeaderProperty.AcceptRanges, "npt");
            context.Response.AddHeader(RTSPHeaderProperty.Session, CacheControl.GenerateRandom(16)+";timeout=60");
            context.Response.AddHeader(RTSPHeaderProperty.Transport, tranResp.ToString());
            context.Response.AddHeader(RTSPHeaderProperty.MediaProperties, "No-Seeking, Time-Progressing, Time-Duration=0.0");

            return RTSPStatus.OK;

        }
        /// <summary>
        /// 响应Describe请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestDescribe(ref HttpContext context)
        {
            context.Response.AddHeader(RTSPHeaderProperty.ContentBase, context.Request.Path);
            context.Response.AddHeader(RTSPHeaderProperty.ContentType, "application/sdp");
            return RTSPStatus.OK;
        }
        /// <summary>
        /// 响应Play请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestPlay(ref HttpContext context)
        {
            return RTSPStatus.NotAcceptable;
        }
        /// <summary>
        /// 响应Pause请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestPause(ref HttpContext context)
        {
            return RTSPStatus.NotAcceptable;
        }
        /// <summary>
        /// 响应Teardown请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestTeardown(ref HttpContext context)
        {
            return RTSPStatus.NotAcceptable;
        }
        /// <summary>
        /// 响应SetParameter请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestSetParameter(ref HttpContext context)
        {
            return RTSPStatus.NotAcceptable;
        }
        /// <summary>
        /// 响应GetParameter请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestGetParameter(ref HttpContext context)
        {
            return RTSPStatus.NotAcceptable;
        }
        /// <summary>
        /// 发布媒体
        /// </summary>
        /// <param name="path"></param>
        public void PublishMedia(string path)
        {

        }
    }
}
