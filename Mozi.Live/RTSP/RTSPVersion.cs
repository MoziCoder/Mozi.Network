using Mozi.HttpEmbedded;

namespace Mozi.Live.RTSP
{
    /// <summary>
    /// RTSP协议版本
    /// </summary>
    public class RTSPVersion
    {
        /// <summary>
        /// RTSP/1.0
        /// </summary>
        public static readonly ProtocolVersion Version10 = new ProtocolVersion("RTSP", "1.0");
        /// <summary>
        /// rtsp/2.0
        /// </summary>
        public static readonly ProtocolVersion Version20 = new ProtocolVersion("RTSP", "2.0");
    }
}
