using Mozi.HttpEmbedded;

namespace Mozi.Live.RTP
{
    public class RTSPVersion
    {
        public static readonly ProtocolVersion Version10 = new ProtocolVersion("RTSP","1.0");
        public static readonly ProtocolVersion Version20 = new ProtocolVersion("RTSP","2.0");
    }
}
