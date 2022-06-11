using Mozi.HttpEmbedded.Generic;
using System.Collections.Generic;

namespace Mozi.Live.RTP
{
    public abstract class AbsRTCPPackage
    {
        /// <summary>
        /// 2bits
        /// </summary>
        public byte Version { get; set; }
        /// <summary>
        /// 1bit
        /// </summary>
        public bool Padding { get; set; }
        /// <summary>
        /// 5bits
        /// </summary>
        public byte ReportCount { get; set; }
        /// <summary>
        /// 8bits
        /// </summary>
        public RTCPPackageType PackageType { get; set; }
        /// <summary>
        /// 16bits  整包长度-1
        /// </summary>
        public int Length { get; set; }
    }

    public class RTCPSenderPackage:AbsRTCPPackage
    {
        /// <summary>
        /// 源标识符 同步源|贡献源
        /// </summary>
        public int Identifier { get; set; }
        /// <summary>
        /// 64bits 高位在前，低位在后
        /// </summary>
        public ulong SendTime { get; set; }
        public int Offset { get; set; }
        public int PacketCount { get; set; }
        public int PayloadLength { get; set; }
        public List<RTCPReportBlock> Reports { get; set; }
        public byte[] Extension { get; set; }
    }
    public class RTCPReceiverPackage:AbsRTCPPackage
    {
        /// <summary>
        /// 源标识符 同步源|贡献源
        /// </summary>
        public int Identifier { get; set; }
        /// <summary>
        /// 64bits 高位在前，低位在后
        /// </summary>
        public ulong SendTime { get; set; }
        public int Offset { get; set; }
        public int PacketCount { get; set; }
        public int PayloadLength { get; set; }
        public List<RTCPReportBlock> Reports { get; set; }
        public byte[] Extension { get; set; }
    }

    public class RTCPReportBlock
    {
        /// <summary>
        /// 32bits
        /// </summary>
        public int Identifier { get; set; }
        /// <summary>
        /// 8bits
        /// </summary>
        public byte FractionLost { get; set; }
        /// <summary>
        /// 24bits
        /// </summary>
        public int PacketLost { get; set; }
        /// <summary>
        /// 32bits
        /// </summary>
        public int ExtendedHighestSequenceNumber { get; set; }
        /// <summary>
        /// 32bits
        /// </summary>
        public int InterArrivalJitter {get;set;}
        /// <summary>
        /// LSR 32bits 
        /// </summary>
        public int LastReportTime { get; set; }
        /// <summary>
        /// DLSR 32bits
        /// </summary>
        public int DelaySinceLastReport { get; set; }
    }

    public class RTCPGoodbyePackage : AbsRTCPPackage
    {
        /// <summary>
        /// 源标识符 同步源|贡献源
        /// </summary>
        public List<int> Identifiers { get; set; }
        public byte ReasonLength { get; set; }
        public string Reason { get; set; }
    }

    public class RTCPAppPackage : AbsRTCPPackage
    {
        /// <summary>
        /// 源标识符 同步源|贡献源
        /// </summary>
        public int Identifier { get; set; }
        /// <summary>
        /// 32bits
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// n*32bits
        /// </summary>
        public byte[] Data { get; set; }
    }

    public class RTCPPackageType : AbsClassEnum
    {
        private string _name = "";
        private byte _typeValue;
        /// <summary>
        /// 包类型 SR
        /// </summary>
        public static RTCPPackageType SenderReport = new RTCPPackageType("SR", 200);
        /// <summary>
        /// 包类型 RR
        /// </summary>
        public static RTCPPackageType ReceiverReport = new RTCPPackageType("RR", 201);
        /// <summary>
        /// 包类型 SDES
        /// </summary>
        public static RTCPPackageType SourceDescription = new RTCPPackageType("SDES", 202);
        /// <summary>
        /// 包类型 BYE
        /// </summary>
        public static RTCPPackageType Goodbye = new RTCPPackageType("BYE", 203);
        /// <summary>
        /// 包类型 APP
        /// </summary>
        public static RTCPPackageType APP = new RTCPPackageType("APP", 204);
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

        internal RTCPPackageType(string name, byte typeValue)
        {
            _name = name;
            _typeValue = typeValue;
        }
    }
}
