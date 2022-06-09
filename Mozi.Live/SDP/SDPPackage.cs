using Mozi.HttpEmbedded.Generic;
using System.Collections.Generic;

namespace Mozi.Live.SDP
{
    //Session description
    //  v=  (protocol version)
    //  o= (originator and session identifier)
    //  s=  (session name)
    //  i= *(session information)
    //  u=* (URI of description)
    //  e=* (email address)
    //  p= *(phone number)
    //  c=* (connection information -- not required if included in all media descriptions)
    //  b=* (zero or more bandwidth information lines)
    //  One or more time descriptions:
    //    ("t=", "r=" and "z=" lines; see below)
    //  k=* (obsolete)
    //  a= *(zero or more session attribute lines)
    //  Zero or more media descriptions
    //
    //Time description
    //  t= (time the session is active)
    //  r=* (zero or more repeat times)
    //  z=* (optional time zone offset line)

    //Media description, if present
    //   m= (media name and transport address)
    //   i=* (media title)
    //   c= *(connection information -- optional if included at session level)
    //   b=* (zero or more bandwidth information lines)
    //   k=* (obsolete)
    //   a= *(zero or more media attribute lines)

    /// <summary>
    /// 会话级描述信息
    /// </summary>
    public class SDPSessionDescription
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 视频源
        /// </summary>
        public SDPOrigin Origin { get; set; }
        /// <summary>
        /// 会话名称
        /// </summary>
        public string SessionName { get; set; }
        /// <summary>
        /// 会话信息
        /// </summary>
        public string SessionInformation { get; set; }
        /// <summary>
        /// 会话描述地址
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// 会话责方联系邮箱 
        /// 规范：
        /// <list type="table">
        ///     <item>Jason&#060;brotherqian&#064;163.com&#062;</item>
        ///     <item>brotherqian@163.com(Jason)</item>
        /// </list>
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 会话责方联系电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 链接地址信息  <![CDATA[<base multicast address>[/<ttl>]/<number of addresses>]]>
        /// </summary>
        public SDPConnection Connection { get; set; }
        /// <summary>
        /// 频带信息
        /// </summary>
        public List<SDPBandWidth> BandWidths { get; set; }
        /// <summary>
        /// 已过时，不再使用
        /// </summary>
        public string EncryptKeys { get; set; }
        /// <summary>
        /// 时间描述信息
        /// </summary>
        public List<SDPTimeDescription> TimeDescriptions { get; set; }
        /// <summary>
        /// 附加属性
        /// </summary>
        public List<SDPAttribute> Attributes { get; set; }
        /// <summary>
        /// 会话及时间描述信息
        /// </summary>
        public SDPTimeDescription TimeDescription { get; set; }

        public override string ToString()
        {
            List<string> sb = new List<string>
            {
                $"v={Version}",
                $"o={Origin}",
                $"s={SessionName??"-"}",
                $"i={SessionInformation}",
                $"u={Uri}"
            };

            if (string.IsNullOrEmpty(Email))
            {
                sb.Add($"e={Email}");
            }
            if (string.IsNullOrEmpty(Phone))
            {
                sb.Add($"e={Phone}");
            }
            if (Connection != null)
            {
                sb.Add($"c={Connection}");
            }
            if (BandWidths != null)
            {
                foreach (var BandWidth in BandWidths)
                {
                    sb.Add($"b={BandWidth}");
                }
            }
            foreach (var tp in TimeDescriptions)
            {
                sb.Add(tp.ToString());
            }

            if (Attributes != null)
            {
                foreach (var att in Attributes)
                {
                    sb.Add($"a={att}");
                }
            }
            return string.Join("\r\n", sb);
        }
    }
    /// <summary>
    /// 时间描述信息
    /// </summary>
    public class SDPTimeDescription
    {
        /// <summary>
        /// 会话的媒体时间区间 起算为1900-01-01T00:00:00Z
        /// </summary>
        public SDPTimeActive TimeActive { get; set; }
        /// <summary>
        /// 重复信息
        /// </summary>
        public List<SDPTimeRepeat> TimeRepeats { get; set; }
        /// <summary>
        /// 转为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> ls = new List<string>();
            ls.Add($"t={TimeActive}");
            if (TimeRepeats != null)
            {
                foreach (var r in TimeRepeats)
                {
                    ls.Add($"r={r.RepeatTime}");
                    if (r.TimezoneAdjust != null)
                    {
                        ls.Add($"z={r.TimezoneAdjust}");
                    }
                }
            }
            return string.Join("\r\n", ls);
        }

    }
    /// <summary>
    /// 时间重复信息，时区适配信息是重复信息的附加字段，故此处设计在一起
    /// </summary>
    public class SDPTimeRepeat
    {
        /// <summary>
        /// 重复播放信息
        /// </summary>
        public SDPRepeatTime RepeatTime { get; set; }
        //TODO 此处还有问题
        /// <summary>
        /// 时区适配信息，该字段与RepeatTime紧密结合
        /// </summary>
        public string TimezoneAdjust { get; set; }
    }

    /// <summary>
    /// 媒体描述信息
    /// </summary>
    public class SDPMediaDescription
    {
        /// <summary>
        /// 媒体摘要信息
        /// </summary>
        public SDPMedia Media { get; set; }
        /// <summary>
        /// 媒体标题
        /// </summary>
        public string MediaTitle { get; set; }
        /// <summary>
        /// 媒体链接信息，这个取决于会话级别是否进行了设置
        /// </summary>
        public SDPConnection Connection { get; set; }
        /// <summary>
        /// 频带信息
        /// </summary>
        public List<SDPBandWidth> BandWidths { get; set; }
        /// <summary>
        /// 已过时，不再使用
        /// </summary>
        public string EncryptKeys { get; set; }
        /// <summary>
        /// 附加属性
        /// </summary>
        public List<SDPAttribute> Attributes { get; set; }
        /// <summary>
        /// 转为SDP字符串格式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> ls = new List<string>();
            ls.Add($"m={Media}");
            ls.Add($"i={MediaTitle}");
            if (Connection != null)
            {
                ls.Add($"c={Connection}");
            }
            if (BandWidths != null)
            {
                foreach (var BandWidth in BandWidths)
                {
                    ls.Add($"b={BandWidth}");
                }
            }
            if (Attributes != null)
            {
                foreach (var a in Attributes)
                {
                    ls.Add($"a={a}");
                }
            }
            return string.Join("\r\n", ls);
        }
    }
    /// <summary>
    /// 媒体描述信息
    /// </summary>
    public class SDPMedia
    {
        /// <summary>
        /// 媒体类型
        /// </summary>
        public SDPMediaType MediaType { get; set; }
        /// <summary>
        /// 开始端口号
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 端口数量
        /// </summary>
        public int PortCount { get; set; }
        // udp: denotes that the data is transported directly in UDP with no additional framing.
        // RTP/AVP: denotes RTP[RFC3550] used under the RTP Profile for Audio and Video Conferences with Minimal Control[RFC3551] running over UDP.
        // RTP/SAVP: denotes the Secure Real-time Transport Protocol[RFC3711] running over UDP.
        // RTP/SAVPF: denotes SRTP with the Extended SRTP Profile for RTCP-Based Feedback [RFC5124] running over UDP.
        /// <summary>
        /// 传输协议类型
        /// </summary>
        public string Protocol { get; set; }
        /// <summary>
        /// 传输中荷载的媒体格式，与RTP包相关
        /// </summary>
        public int FormatType { get; set; }
    }

    //参考RFC8866
    /// <summary>
    /// SDP数据包，UTF-8字符串格式
    /// </summary>
    public class SDPPackage 
    {
        /// <summary>
        /// 会话信息
        /// </summary>
        public SDPSessionDescription Session { get; set; }

        /// <summary>
        /// 媒体信息
        /// </summary>
        public List<SDPMediaDescription> Medias { get; set; }
        /// <summary>
        /// 转为SDP格式文本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> sb = new List<string>();
            sb.Add(Session.ToString());
            if (Medias != null)
            {
                foreach (var md in Medias)
                {
                    sb.Add(md.ToString());
                }
            }
            return string.Join("\r\n", sb);
        }
    }
    /// <summary>
    /// 会话发布者信息
    /// </summary>
    public class SDPOrigin
    {
        private string netType = "IN";
        private string addressType = "IP4";

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public long SessionId { get; set; }
        /// <summary>
        /// 会话版本
        /// </summary>
        public long SessionVersion { get; set; }
        /// <summary>
        /// 网络类型
        /// </summary>
        public string NetType { get => netType; set => netType = value; }
        /// <summary>
        /// 地址类型
        /// </summary>
        public string AddressType { get => addressType; set => addressType = value; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// SDP字符串格式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{UserName} {SessionId} {SessionVersion} {NetType} {AddressType} {Address}";
        }
        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static SDPOrigin Parse(string desc)
        {
            SDPOrigin ori = new SDPOrigin();
            string[] descs = desc.Split(new char[] { ' ' });
            if (descs.Length >= 6)
            {
                ori.UserName = descs[0];
                ori.SessionId = long.Parse(descs[1]);
                ori.SessionVersion = long.Parse(descs[2]);
                ori.NetType = descs[3];
                ori.AddressType = descs[4];
                ori.Address = descs[5];
            }
            return ori;
        }
    }

    /// <summary>
    /// 链接地址信息
    /// </summary>
    public class SDPConnection
    {
        private string netType = "IN";
        private uint portCount = 1;
        private string addressType = "IP4";

        /// <summary>
        /// 网络类型 默认为IN-Internet
        /// </summary>
        public string NetType { get => netType; set => netType = value; }
        /// <summary>
        /// 地址类型 IP4-IPV4 IP6-IPV6
        /// </summary>
        public string AddressType { get => addressType; set => addressType = value; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 生存时间
        /// </summary>
        public uint TTL { get; set; }
        /// <summary>
        /// 端口数量 可选
        /// </summary>
        public uint PortCount { get => portCount; set => portCount = value; }
        /// <summary>
        /// 转为字符串表达式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (PortCount == 1)
            {
                return $"{NetType} {AddressType} {Address}/{TTL}";
            }
            else
            {
                return $"{NetType} {AddressType} {Address}/{TTL}/{PortCount}";
            }
        }
    }

    /// <summary>
    /// 频带信息
    /// </summary>
    public class SDPBandWidth
    {
        private string prefix = "AS";
        /// <summary>
        /// 前缀
        /// </summary>
        public string Prefix { get => prefix; set => prefix = value; }
        /// <summary>
        /// 单位kbps
        /// </summary>
        public int BandWidth { get; set; }
        /// <summary>
        /// 转为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Prefix}:{BandWidth}";
        }
    }
    /// <summary>
    /// 时间区间 时间戳格式
    /// </summary>
    public class SDPTimeActive
    {
        //2208988800

        /// <summary>
        /// 开始时间
        /// </summary>
        public ulong StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public ulong StopTime { get; set; }

        public override string ToString()
        {
            return $"{StartTime} {StopTime}";
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SDPRepeatTime
    {
        /// <summary>
        /// 间隔
        /// </summary>
        public ulong Interval { get; set; }
        /// <summary>
        /// 延续时间
        /// </summary>
        public ulong Duration { get; set; }
        /// <summary>
        /// 偏移时间
        /// </summary>
        public long Offset { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public ulong StartTime { get; set; }
        /// <summary>
        /// 此处有两种写法 604800 3600 0 90000|7d 1h 0 25h
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Interval} {Duration} {Offset} {StartTime}";
        }
    }

    /// <summary>
    /// SDP属性
    /// </summary>
    public class SDPAttribute
    {
        /// <summary>
        /// 属性类型
        /// </summary>
        public SDPAttributes Attribute { get; set; }
        /// <summary>
        /// 属性值
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// 转为键值对表达式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Attribute.Name))
            {
                return Value.ToString();
            }
            else
            {
                return $"{Attribute}:{Value}";
            }
        }
    }

    /// <summary>
    /// SDP属性合集
    /// </summary>
    public class SDPAttributes : AbsClassEnum
    {
        /// <summary>
        /// 会话类别 [session]
        /// </summary>
        public SDPAttributes Category = new SDPAttributes("cat");
        /// <summary>
        /// 会话关键字 [session]
        /// </summary>
        public SDPAttributes Keywords = new SDPAttributes("keywds");
        /// <summary>
        /// 创建会话的工具 [session]
        /// </summary>
        public SDPAttributes tool = new SDPAttributes("tool");
        /// <summary>
        /// 资源的时长,ms [session]
        /// </summary>
        public SDPAttributes PacketTime = new SDPAttributes("ptime");
        /// <summary>
        /// 数据包中装载的最大媒体时长 [media]
        /// </summary>
        public SDPAttributes MaxPacketTime = new SDPAttributes("maxptime");
        /// <summary>
        /// 媒体摘要信息<see cref="RtpMap"/> [media]
        /// </summary>
        public SDPAttributes RtpMap = new SDPAttributes("rtpmap");
        /// <summary>
        /// 媒体指向 [session,media]
        /// </summary>
        public SDPAttributes MediaDirection = new SDPAttributes("");
        /// <summary>
        /// 媒体画面的方向 [media]
        /// </summary>
        public SDPAttributes Orientation = new SDPAttributes("orient");
        /// <summary>
        /// 会话类型 [session]
        /// </summary>
        public SDPAttributes ConferenceType = new SDPAttributes("type");
        /// <summary>
        /// 字符集 [session]
        /// </summary>
        public SDPAttributes CharacterSet = new SDPAttributes("charset");
        /// <summary>
        /// 语言 [session, media]
        /// </summary>
        public SDPAttributes SDPLanguage = new SDPAttributes("sdplang");
        /// <summary>
        /// 语言 [session, media]
        /// </summary>
        public SDPAttributes Language = new SDPAttributes("lang");
        /// <summary>
        /// 帧率 [media]
        /// </summary>
        public SDPAttributes FrameRate = new SDPAttributes("framerate");
        /// <summary>
        /// 质量 [media]
        /// </summary>
        public SDPAttributes Quality = new SDPAttributes("quality");
        /// <summary>
        /// 格式参数 [media]
        /// </summary>
        public SDPAttributes FormatParameters = new SDPAttributes("fmtp");


        private string _name;
        /// <summary>
        /// 属性名
        /// </summary>
        public string Name { get => _name; set => _name = value; }

        public SDPAttributes(string name)
        {
            _name = name;
        }
        protected override string Tag => _name;
    }
    /// <summary>
    /// 会话类型
    /// </summary>
    public class SDPAttributeConference : AbsClassEnum
    {
        /// <summary>
        /// broadcast
        /// </summary>
        public SDPAttributeConference Broadcast = new SDPAttributeConference("broadcast");
        /// <summary>
        /// meeting
        /// </summary>
        public SDPAttributeConference Meeting = new SDPAttributeConference("meeting");
        /// <summary>
        /// moderated
        /// </summary>
        public SDPAttributeConference Moderated = new SDPAttributeConference("moderated");
        /// <summary>
        /// test
        /// </summary>
        public SDPAttributeConference Test = new SDPAttributeConference("test");
        /// <summary>
        /// h332
        /// </summary>
        public SDPAttributeConference H332 = new SDPAttributeConference("H332");

        private string _name;

        /// <summary>
        /// 会话类型名
        /// </summary>
        public string Name { get => _name; set => _name = value; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public SDPAttributeConference(string name)
        {
            _name = name;
        }
        protected override string Tag => _name;
    }

    public class SDPRTPMap
    {
        public byte PayloadType { get; set; }
        public string EncodingName { get; set; }
        public int ClockRate { get; set; }
        /// <summary>
        /// audio声道，video无此参数
        /// </summary>
        public int Channels { get; set; }

        public override string ToString()
        {
            if (Channels > 0)
            {
                return $"{PayloadType} {EncodingName}/{ClockRate}/{Channels}";
            }
            else
            {
                return $"{PayloadType} {EncodingName}/{ClockRate}";
            }
        }
    }
    /// <summary>
    /// SDP媒体类型
    /// </summary>
    public class SDPMediaType : AbsClassEnum
    {

        /// <summary>
        /// audio
        /// </summary>
        public SDPMediaType Audio = new SDPMediaType("audio");
        /// <summary>
        /// video
        /// </summary>
        public SDPMediaType Video = new SDPMediaType("video");
        /// <summary>
        /// text
        /// </summary>
        public SDPMediaType Text = new SDPMediaType("text");
        /// <summary>
        /// application
        /// </summary>
        public SDPMediaType Application = new SDPMediaType("application");
        /// <summary>
        /// message
        /// </summary>
        public SDPMediaType Message = new SDPMediaType("message");
        /// <summary>
        /// control
        /// </summary>
        public SDPMediaType Control = new SDPMediaType("control");
        /// <summary>
        /// data
        /// </summary>
        public SDPMediaType Data = new SDPMediaType("data");

        private string _name;
        /// <summary>
        /// 
        /// </summary>
        public string Name { get => _name; set => _name = value; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public SDPMediaType(string name)
        {
            _name = name;
        }
        protected override string Tag => _name;
    }

    public class SDPMediaFormatParameter
    {

    }

    public enum SDPMediaDirection
    {
        RecvOnly = 1,
        SendRecv = 2,
        SendOnly = 3,
        InActive = 4
    }

}
