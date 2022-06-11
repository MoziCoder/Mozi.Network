using Mozi.HttpEmbedded;
using Mozi.Live.RTSP;
using System.Linq;

namespace Mozi.Live
{
    
    //TODO 实现UDP传输信道
    //TODO 实现控制服务端的API

    /// <summary>
    /// RTSP服务器
    /// </summary>
    public class RTSPServer: HttpServer
    {
        private int _port = RTSPProtocol.RTSPPort;

        private string[] _features = { "play.basic", "play.scale", "play.speed", "setup.rtp.rtcp.mux" };

        /// <summary>
        /// 工作端口，默认为<see cref="RTSPProtocol.RTSPPort"/>
        /// </summary>
        public override int Port { get => _port; protected set => _port = value; }

        /// <summary>
        /// 
        /// </summary>
        public RTSPServer()
        {
            Version = RTSPVersion.Version20;
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
            ////不添加默认的头信息
            context.Response.DontAddAutoHeader = true;
            RequestMethod method = context.Request.Method;

            if (method == RTSPMethod.OPTIONS)
            {

                return HandleRequestOptions(ref context);

            }
            else if(method== RTSPMethod.SETUP)
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
            context.Response.AddHeader(RTSPHeaderProperty.CSeq, context.Request.Headers.GetValue(RTSPHeaderProperty.CSeq));
            return StatusCode.NotAcceptable;
        }

        /// <summary>
        /// 响应Option请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestOptions(ref HttpContext context)
        {
            context.Response.AddHeader(RTSPHeaderProperty.Public, MethodPublic.Select(x=>x.Name).ToArray());
            context.Response.AddHeader(RTSPHeaderProperty.Supported, string.Join(";",_features));
            // Sends 200 OK
            return RTSPStatus.OK;
        }
        private StatusCode HandleRequestSetup(ref HttpContext context)
        {
            context.Response.AddHeader(RTSPHeaderProperty.AcceptRanges, "npt");
            context.Response.AddHeader(RTSPHeaderProperty.Session, CacheControl.GenerateRandom(16)+";timeout=60");
            context.Response.AddHeader(RTSPHeaderProperty.MediaProperties, "No-Seeking, Time-Progressing, Time-Duration=0.0");
            return RTSPStatus.OK;
        }

        private StatusCode HandleRequestDescribe(ref HttpContext context)
        {
            context.Response.AddHeader(RTSPHeaderProperty.ContentBase, context.Request.Path);
            context.Response.AddHeader(RTSPHeaderProperty.ContentType, "application/sdp");
            return RTSPStatus.OK;
        }
        private StatusCode HandleRequestPlay(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
        private StatusCode HandleRequestPause(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
        private StatusCode HandleRequestTeardown(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
        private StatusCode HandleRequestSetParameter(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
        private StatusCode HandleRequestGetParameter(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
    }

    /// <summary>
    /// RTP数据传输服务端
    /// </summary>
    public class RTPServer
    {

    }
    /// <summary>
    /// RTP数据传输控制服务端
    /// </summary>
    public class RTCPServer
    {

    }
}
