using Mozi.HttpEmbedded;
using System.Collections.Generic;

namespace Mozi.Live.RTSP
{

    public abstract class AbsTransportProperty
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
        /// <summary>
        /// DeliveryMode=multicast时可用此参数
        /// </summary>
        public int TTL { get; set; }
        /// <summary>
        /// Synchronization source
        /// </summary>
        public string SSRC { get; set; }
        /// <summary>
        /// 模式 PLAY|RECORD  RECORD为保留值
        /// </summary>
        public string Mode { get; set; }
        /// <summary>
        /// 交错 指示信道数
        /// </summary>
        public string Interleaved { get; set; }

        public bool IsPlay
        {
            get { return string.IsNullOrEmpty(Mode) || Mode == "PLAY"; }
        }
    }

    public class RTSPTransportPropertyV1 : AbsTransportProperty
    {
        /// <summary>
        /// 
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// 媒体源地址，不设置则为本机地址
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 是覆盖还是追加的媒体文件
        /// </summary>
        public bool Append { get; set; }
        public string Port { get; set; }
        public string ClientPort { get; set; }
        public string ServerPort { get; set; }

        public static RTSPTransportPropertyV1 Parse(string data)
        {
            RTSPTransportPropertyV1 trans = new RTSPTransportPropertyV1();
            string[] items = data.Split(new char[] { ';' });
            trans.Protocol = ProtocolVersion.Get<ProtocolVersion>(items[0]);
            trans.DeliveryMode = items[1];
            for (int i = 2; i < items.Length; i++)
            {
                string it = items[i];
                if (it.Equals("append"))
                {
                    trans.Append = true;
                }
                else
                {
                    string[] kp = it.Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (kp.Length >= 2)
                    {
                        string key = kp[0].Trim();
                        string v = kp[1].Trim().Trim(new char[] { '"' });
                        if (key == "destination")
                        {
                            trans.Destination = v;
                        } else if (key == "interleaved")
                        {
                            trans.Interleaved = v;
                        }
                        else if (key == "ttl") {
                            trans.TTL = int.Parse(v);
                        } else if (key == "port") {
                            trans.Port = v;
                        } else if (key == "client_port") {
                            trans.ClientPort = v;
                        } else if (key == "server_port") {
                            trans.ServerPort = v;
                        } else if (key == "ssrc") {
                            trans.SSRC = v;
                        }
                        else if (key == "source") {
                            trans.Source = v;
                        }
                        else if (key == "mode") {
                            trans.Mode = v;
                        }
                        else if (key == "layers") {
                            trans.Layers = int.Parse(v);
                        }
                    }
                }
            }
            return trans;
        }

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
            if (!string.IsNullOrEmpty(Destination))
            {
                ls.Add($"destination={Destination}");
            }
            if (!string.IsNullOrEmpty(Interleaved))
            {
                ls.Add($"interleaved={Interleaved}");
            }
            if (Append)
            {
                ls.Add("append");
            }
            if (TTL > 0 && "multicast".Equals(DeliveryMode))
            {
                ls.Add($"ttl={TTL}");
            }

            if (!string.IsNullOrEmpty(Port))
            {
                ls.Add($"port={Port}");
            }
            if (!string.IsNullOrEmpty(ClientPort)){
                ls.Add($"client_port={ClientPort}");
            }
            if (!string.IsNullOrEmpty(ServerPort))
            {
                ls.Add($"server_port={ServerPort}");
            }

            if (!string.IsNullOrEmpty(SSRC))
            {
                ls.Add($"ssrc={SSRC}");
            }
            if (!string.IsNullOrEmpty(Source))
            {
                ls.Add($"source={Source}");
            }
            if (string.IsNullOrEmpty(Mode))
            {
                ls.Add($"mode=\"PLAY\"");
            }
            else
            {
                ls.Add($"mode=\"{Mode}\"");
            }
            if (Layers > 0)
            {
                ls.Add($"layers={Layers}");
            }
            return string.Join(";", ls);
        }
    }

    /// <summary>
    /// Transport属性值格式
    /// </summary>
    public class RTSPTransportProperty:AbsTransportProperty
    {
        /// <summary>
        /// 客户机端口
        /// </summary>
        public string ClientPort { get; set; }
        /// <summary>
        /// 目标地址信息
        /// </summary>
        public string DestAddr { get; set; }
        /// <summary>
        /// 源地址信息
        /// </summary>
        public string SrcAddr { get; set; }
        /// <summary>
        /// 安全连接 仅SAVP and SAVPF。B64值
        /// </summary>
        public string MIKEY { get; set; }
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

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static RTSPTransportProperty Parse(string data)
        {
            RTSPTransportProperty trans = new RTSPTransportProperty();
            string[] items = data.Split(new char[] { ';' });
            for (int i = 2; i < items.Length; i++)
            {
                string it = items[i];
            }
            return trans;
        }

        /// <summary>
        /// 转为字符串格式
        /// </summary>
        /// <returns></returns>
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
}
