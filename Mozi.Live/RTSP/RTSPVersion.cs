using Mozi.HttpEmbedded;
using System.Collections.Generic;

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
    /// <summary>
    /// RTSP传输协议
    /// </summary>
    public class RTSPTransportProtocol
    {

        public static readonly ProtocolVersion AVP = new ProtocolVersion("RTP", "AVP");
        public static readonly ProtocolVersion AVPUDP = new ProtocolVersion("RTP", "AVP/UDP");
        public static readonly ProtocolVersion AVPF = new ProtocolVersion("RTP", "AVPF");
        public static readonly ProtocolVersion AVPFUDP = new ProtocolVersion("RTP", "AVPF/UDP");
        public static readonly ProtocolVersion SAVP = new ProtocolVersion("RTP", "SAVP");
        public static readonly ProtocolVersion SAVPUDP = new ProtocolVersion("RTP", "SAVP/UDP");
        public static readonly ProtocolVersion SAVPF = new ProtocolVersion("RTP", "SAVPF");
        public static readonly ProtocolVersion SAVPFUDP = new ProtocolVersion("RTP", "SAVPF/UDP");
        public static readonly ProtocolVersion AVPTCP = new ProtocolVersion("RTP", "AVP/TCP");
        public static readonly ProtocolVersion AVPFTCP = new ProtocolVersion("RTP", "AVPF/TCP");
        public static readonly ProtocolVersion SAVPTCP = new ProtocolVersion("RTP", "SAVP/TCP");
        public static readonly ProtocolVersion SAVPFTCP = new ProtocolVersion("RTP", "SAVPF/TCP");
    }
    /// <summary>
    /// Transport属性值格式
    /// </summary>
    public class RTSPTransportProperty
    {
        /// <summary>
        /// 传输协议 <see cref="RTSPTransportProtocol"/>
        /// </summary>
        public ProtocolVersion Protocol { get; set; }
        /// <summary>
        /// unicast/multicast
        /// </summary>
        public string DeliveryMode { get; set; }
        /// <summary>
        /// 广播层数，可选项，如果值小于等于0，则不输出此值 
        /// </summary>
        public int Layers { get; set; }
        public string DestAddr { get; set; }

        public string SrcAddr { get; set; }
        /// <summary>
        /// 模式 PLAY|RECORD  RECORD为保留值
        /// </summary>
        public string Mode { get; set; }
        /// <summary>
        /// 交错 指示信道数
        /// </summary>
        public string Interleaved { get; set; }
        /// <summary>
        /// 安全连接 仅SAVP and SAVPF。B64值
        /// </summary>
        public string MIKEY { get; set; }
        /// <summary>
        /// DeliveryMode=multicast时可用此参数
        /// </summary>
        public int TTL { get; set; }
        /// <summary>
        /// Synchronization source
        /// </summary>
        public string SSRC { get; set; }
        /// <summary>
        /// Multiplexing
        /// </summary>
        public bool Mux { get; set; }
        /// <summary>
        /// active|passive|actpass   holdconn
        /// </summary>
        public string Setup { get; set; }
        /// <summary>
        /// new|existing
        /// </summary>
        public string Connection { get; set; }


        public override string ToString()
        {
            List<string> ls = new List<string>();
            ls.Add(Protocol.ToString());
            if (string.IsNullOrEmpty(DeliveryMode))
            {
                ls.Add("unicast");
            }
            else
            {
                ls.Add(DeliveryMode);
            }
            if (!string.IsNullOrEmpty(DestAddr))
            {
                ls.Add($"dest_addr={ DestAddr}");
            }
            if (!string.IsNullOrEmpty(SrcAddr))
            {
                ls.Add($"src_addr={SrcAddr}");
            }
            if (string.IsNullOrEmpty(Mode))
            {
                ls.Add($"mode=\"PLAY\"");
            }
            else
            {
                ls.Add($"mode=\"{Mode}\"");
            }
            if (!string.IsNullOrEmpty(Interleaved))
            {
                ls.Add($"interleaved={Interleaved}");
            }
            if (Layers > 0)
            {
                ls.Add($"layers={Layers}");
            }
            if (!string.IsNullOrEmpty(MIKEY))
            {
                ls.Add($"MIKEY=\"{MIKEY}\"");
            }
            if (TTL > 0 && "multicast".Equals(DeliveryMode))
            {
                ls.Add($"ttl={TTL}");
            }
            if (!string.IsNullOrEmpty(SSRC))
            {
                ls.Add($"ssrc={SSRC}");
            }
            if (!Mux)
            {
                ls.Add($"RTCP-mux");
            }
            if (string.IsNullOrEmpty(Connection))
            {
                ls.Add($"connection=existing");
            }
            else
            {
                ls.Add($"connection={Connection}");
            }

            if (string.IsNullOrEmpty(Setup))
            {
                ls.Add($"setup={Setup}");
            }
            return string.Join(";", ls);
        }
    }
}
