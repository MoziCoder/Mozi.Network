using Mozi.HttpEmbedded.Generic;
using System.Collections.Generic;

namespace Mozi.Live.RTP
{
    //参考RFC8866
    /// <summary>
    /// 
    /// </summary>
    public class SDPPackage
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
        public SDPOrigin Origin { get; set; }
        public string SessionName { get; set; }
        public string SessionInformation { get; set; }
        public string Uri { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<SDPConnection> Connections { get; set; }
        public SDPBandWidth BandWidth { get; set; }
        public SDPTimeActive TimeActive { get; set; }
        public SDPRepeatTime RepeatTimes { get; set; }
        public string TimezoneAdjust { get; set; }
        /// <summary>
        /// 已过时，不再使用
        /// </summary>
        public string EncryptKeys { get; set; }
        public string Attribute { get; set; }
        public string MediaDescription { get; set; }
        public override string ToString()
        {
            List<string> sb = new List<string>();

            sb.Add($"v={Version}");
            sb.Add($"o={Origin}");
            sb.Add($"s={SessionName}");
            sb.Add($"i={SessionInformation}");
            sb.Add($"u={Uri}");

            if (string.IsNullOrEmpty(Email))
            {
                sb.Add($"e={Email}");
            }
            if (string.IsNullOrEmpty(Phone))
            {
                sb.Add($"e={Phone}");
            }
            foreach (var Connection in Connections)
            {
                sb.Add($"c={Connection}");
            }
            sb.Add($"b={BandWidth}");
            sb.Add($"t={TimeActive}");
            sb.Add($"r={RepeatTimes}");

            return string.Join("\r\n", sb);
        }
    }

    public class SDPOrigin
    {
        private string netType="IN";
        private string addressType="IP4";

        public string UserName { get; set; }
        public long SessionId { get; set; }
        public long SessionVersion { get; set; }
        public string NetType { get => netType; set => netType = value; }
        public string AddressType { get => addressType; set => addressType = value; }
        public string Address { get; set; }

        public override string ToString()
        {
            return $"{UserName} {SessionId} {SessionVersion} {NetType} {AddressType} {Address}";
        }

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

    public class SDPConnection
    {
        private string netType="IN";
        private uint portCount=1;
        private string addressType="IP4";

        public string NetType { get => netType; set => netType = value; }
        public string AddressType { get => addressType; set => addressType = value; }
        public string Address { get; set; }

        public uint TTL { get; set; }
        public uint PortCount { get => portCount; set => portCount = value; }

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

    public class SDPBandWidth
    {
        private string prefix="AS";

        public string Prefix { get => prefix; set => prefix = value; }
        /// <summary>
        /// 单位kbps
        /// </summary>
        public int BandWidth { get; set; }

        public override string ToString()
        {
            return $"{Prefix}:{BandWidth}";
        }
    }
    //时间戳
    public class SDPTimeActive
    {
        //2208988800
        public ulong StartTime { get; set; }
        public ulong StopTime { get; set; }

        public override string ToString()
        {
            return $"{StartTime} {StopTime}";
        }
    }

    public class SDPRepeatTime
    {
        public ulong Interval { get; set; }
        public ulong Duration { get; set; }

        public long Offset { get; set; }

        public ulong StartTime { get; set; }
    }

    public class SDPMediaDescription
    {
        
    }

    public class SDPMediaAttribute
    {
        public SDPMediaAttributes Attribute  { get; set; }
        public object Value { get; set; }

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

    public class SDPMediaAttributes : AbsClassEnum
    {

        public SDPMediaAttributes Category = new SDPMediaAttributes("cat");
        public SDPMediaAttributes Keywords = new SDPMediaAttributes("keywds");
        public SDPMediaAttributes tool = new SDPMediaAttributes("tool");
        public SDPMediaAttributes PacketTime = new SDPMediaAttributes("ptime");
        public SDPMediaAttributes MaxPacketTime = new SDPMediaAttributes("maxptime");
        public SDPMediaAttributes RtpMap = new SDPMediaAttributes("rtpmap");
        public SDPMediaAttributes MediaDirection = new SDPMediaAttributes("");
        public SDPMediaAttributes Orientation = new SDPMediaAttributes("orient");
        public SDPMediaAttributes ConferenceType  = new SDPMediaAttributes("type");
        public SDPMediaAttributes CharacterSet  = new SDPMediaAttributes("charset");
        public SDPMediaAttributes SDPLanguage  = new SDPMediaAttributes("sdplang");
        public SDPMediaAttributes Language = new SDPMediaAttributes("lang");
        public SDPMediaAttributes FrameRate  = new SDPMediaAttributes("framerate");
        public SDPMediaAttributes Quality = new SDPMediaAttributes("quality");
        public SDPMediaAttributes FormatParameters = new SDPMediaAttributes("fmtp");

        private string _name;

        public string Name { get => _name; set => _name = value; }

        public SDPMediaAttributes(string name)
        {
            _name = name;
        }
        protected override string Tag => _name;
    }

    public class SDPMediaAttributeConference : AbsClassEnum
    {
        public SDPMediaAttributeConference Broadcast = new SDPMediaAttributeConference("broadcast");
        public SDPMediaAttributeConference Meeting = new SDPMediaAttributeConference("meeting");
        public SDPMediaAttributeConference Moderated = new SDPMediaAttributeConference("moderated");
        public SDPMediaAttributeConference Test = new SDPMediaAttributeConference("test");
        public SDPMediaAttributeConference H332 = new SDPMediaAttributeConference("H332");

        private string _name;


        public string Name { get => _name; set => _name = value; }

        public SDPMediaAttributeConference(string name)
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

    public class SDPMediaType : AbsClassEnum
    {
        public SDPMediaType Audio   = new SDPMediaType("audio");
        public SDPMediaType Video   = new SDPMediaType("video");
        public SDPMediaType Text    = new SDPMediaType("text");
        public SDPMediaType Application = new SDPMediaType("application");
        public SDPMediaType Message = new SDPMediaType("message");
        public SDPMediaType Control = new SDPMediaType("control");
        public SDPMediaType Data = new SDPMediaType("data");

        private string _name;
        public string Name { get => _name; set => _name = value; }

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
        RecvOnly=1,
        SendRecv=2,
        SendOnly=3,
        InActive=4
    }

}
