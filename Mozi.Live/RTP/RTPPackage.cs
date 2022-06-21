using System;
using System.Collections.Generic;

namespace Mozi.Live.RTP
{
    /// <summary>
    /// RTP数据包头
    /// </summary>
    public class RTPPackageHeader
    {
        /// <summary>
        /// 版本 2bits
        /// </summary>
        public byte Version { get; set; }
        /// <summary>
        /// 是否含有末尾填充 1bit
        /// </summary>
        public bool Padding { get; set; }
        /// <summary>
        /// 是否含有扩展头信息 1bit
        /// </summary>
        public bool Extension { get; set; }
        /// <summary>
        /// 贡献来源标识符数量 4bits
        /// </summary>
        public byte CSRCcount { get; set; }
        /// <summary>
        /// 1bit
        /// </summary>
        public bool Marker { get; set; }
        /// <summary>
        /// 荷载类型 7bits
        /// </summary>
        public byte PalyloadType { get; set; }
        /// <summary>
        /// 包序号 16bits
        /// </summary>
        public ushort SequenceNumber { get;set; }
        /// <summary>
        /// 采样包的开始时间戳 32bits
        /// </summary>
        public int Timestamp { get; set; }
        /// <summary>
        /// 同步源编号 32bits
        /// </summary>
        public int SSRC { get; set; }
        /// <summary>
        /// 贡献源标识符 每项32bits 最多 15项
        /// </summary>
        public List<int> CSRCList { get; set; }

    }
    /// <summary>
    /// RTP数据包
    /// </summary>
    public class RTPPackage
    {
        public RTPPackageHeader Header { get; set; }
        /// <summary>
        /// 荷载部分
        /// </summary>
        public byte[] Payload { get; set; }

        internal byte[] GetBuffer()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// RTP头扩展
    /// </summary>
    public class RTPExtension
    {
        /// <summary>
        /// 扩展标识 16bits
        /// </summary>
        public ushort Profile { get; set; }
        /// <summary>
        /// 值长度 16bits
        /// </summary>
        public ushort Length { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public byte[] Extension { get; set; }
    }
}
