using Mozi.HttpEmbedded.Generic;
using System.Collections.Generic;

namespace Mozi.Live.RTP
{
    /// <summary>
    /// RTCP SDES报文
    /// </summary>
    public class RTCPSourceDescriptionPackage : AbsRTCPPackage
    {
        public List<RTCPSourceDescriptionChunk> Chunks { get; set; }
    }
    public class RTCPSourceDescriptionChunk
    {
        /// <summary>
        /// 源标识符 同步源|贡献源
        /// </summary>
        public int Identifier { get; set; }
        /// <summary>
        /// 1bit
        /// </summary>
        public RTCPSourceDescriptionType ItemType { get; set; }
        /// <summary>
        /// 数据长度
        /// </summary>
        public byte Length { get; set; }
        /// <summary>
        /// 值 ASCII类型
        /// </summary>
        public string Value { get; set; }
    }

    public class RTCPSourceDescriptionType : AbsClassEnum
    {
        private string _name = "";
        private byte _typeValue;

        public static RTCPSourceDescriptionType CName = new RTCPSourceDescriptionType("CNAME", 1);
        public static RTCPSourceDescriptionType UserName = new RTCPSourceDescriptionType("NAME", 2);
        public static RTCPSourceDescriptionType EMail = new RTCPSourceDescriptionType("EMAIL", 3);
        public static RTCPSourceDescriptionType Phone = new RTCPSourceDescriptionType("PHONE", 4);
        public static RTCPSourceDescriptionType Location = new RTCPSourceDescriptionType("LOC", 5);
        public static RTCPSourceDescriptionType Tool = new RTCPSourceDescriptionType("TOOL", 6);
        public static RTCPSourceDescriptionType Notice = new RTCPSourceDescriptionType("NOTE", 7);
        public static RTCPSourceDescriptionType PrivateExtension = new RTCPSourceDescriptionType("PRIV", 8);

        /// <summary>
        /// 消息类型值
        /// </summary>
        public byte Value
        {
            get
            {
                return _typeValue;
            }
        }
        /// <summary>
        /// 消息类型名
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
        /// <summary>
        /// 唯一标识符号
        /// </summary>
        protected override string Tag
        {
            get
            {
                return _typeValue.ToString();
            }
        }

        internal RTCPSourceDescriptionType(string name, byte typeValue)
        {
            _name = name;
            _typeValue = typeValue;
        }
    }
}
