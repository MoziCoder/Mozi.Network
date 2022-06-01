using Mozi.HttpEmbedded;
using Mozi.Live.RTP;

namespace Mozi.Live
{
    public class RTSPServer:HttpEmbedded.HttpServer
    {
        private int _port = RTSPProtocol.RTSPPort;

        public override int Port { get => _port; protected set => _port = value; }

        public RTSPServer()
        {
            //定义允许的方法
            MethodPublic = new RequestMethod[] { RTSPMethod.OPTIONS, RTSPMethod.DESCRIBE, RTSPMethod.PLAY, RTSPMethod.PAUSE, RTSPMethod.SETUP, RTSPMethod.TEARDOWN, RTSPMethod.SET_PARAMETER, RTSPMethod.GET_PARAMETER };
        }

        protected override StatusCode HandleRequest(ref HttpContext context)
        {
            context.Response.Version = RTP.RTSPVersion.Version20;
            ////不添加默认的头信息
            //context.Response.DontAddAutoHeader = true;
            RequestMethod method = context.Request.Method;

            if (method == RequestMethod.OPTIONS)
            {
                return HandleRequestOptions(ref context);
            }
            context.Response.AddHeader(RTSPHeaderProperty.CSeq, context.Request.Headers.GetValue(RTSPHeaderProperty.CSeq));
            return StatusCode.NotAcceptable;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestOptions(ref HttpContext context)
        {
            foreach (RequestMethod verb in MethodPublic)
                context.Response.AddHeader("Public", verb.Name);
            // Sends 200 OK
            return RTSPStatus.OK;
        }

        private StatusCode HandleRequestDescribe(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
        private StatusCode HandleRequestPLAY(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
        private StatusCode HandleRequestPAUSE(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
        private StatusCode HandleRequestSETUP(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
        private StatusCode HandleRequestTEARDOWN(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
        private StatusCode HandleRequestSET_PARAMETER(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
        private StatusCode HandleRequestGET_PARAMETER(ref HttpContext context)
        {
            return RTSPStatus.Forbidden;
        }
    }
}
