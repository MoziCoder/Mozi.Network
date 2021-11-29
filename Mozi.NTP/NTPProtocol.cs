using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mozi.NTP
{
    //RFC1305 1992 NTP Version3 
    //RFC5905 2010 NTP Version4 修订 RFC7822,8573,9109  

    //参考RFC NTPv4实现 并向下兼容NTPv3

    //T(t) = T(t_0) + R(t_0)(t-t_0) + 1/2 * D(t_0)(t-t_0)^2 + e,

    /// <summary>
    /// NTP协议采取定长数据包格式
    /// </summary>
    public class NTPPackage
    {
        /// <summary>
        /// LI（Leap Indicator）：2bits，值为“11”时表示告警状态，时钟未被同步。为其他值时NTP本身不做处理。
        /// 0     - no warning                             
        /// 1     - last minute of the day has 61 seconds  
        /// 2     - last minute of the day has 59 seconds  
        /// 3     - unknown(clock unsynchronized)
        /// </summary>
        public byte LeapIndicator { get; set; }
        /// <summary>
        /// VN（Version Number）：3bits，表示NTP的版本号，目前的最新版本为4。
        /// </summary>
        public byte VersionNumber { get; set; }
        /// <summary>
        /// Mode：3bits，表示NTP的工作模式。不同的值所表示的含义分别是：
        /// 0-未定义
        /// 1-主动对等体模式
        /// 2-被动对等体模式
        /// 3-客户模式
        /// 4-服务器模式
        /// 5-广播模式或组播模式
        /// 6-此报文为NTP控制报文
        /// 7-预留给内部使用。
        /// </summary>
        public byte Mode { get; set; }
        ///<summary>
        ///Stratum：8bits,系统时钟的层数，取值范围为1～16，它定义了时钟的准确度。层数为1的时钟准确度最高，准确度从1到16依次递减，层数为16的时钟处于未同步状态，不能作为参考时钟。
        /// 0      - unspecified or invalid                              
        /// 1      - primary server(e.g., equipped with a GPS receiver) 
        /// 2-15   - secondary server(via NTP)                          
        /// 16     - unsynchronized                                      
        /// 17-255 - reserved
        /// </summary>
        public byte Stratum { get; set; }             //
        /// <summary>
        /// Poll：8bits,轮询时间，即两个连续NTP报文之间的时间间隔。
        /// 值为log2N的值，取值范围为v3 6-10 v4 4-17
        /// </summary>
        public byte Pool { get; set; }               
        public ushort PoolInterval
        {
            get
            {
                return (ushort)Math.Pow(2, Pool);
            }
        }
        /// <summary>
        /// Precision：时钟精度,8bits,系统时钟的精度。
        /// </summary>
        public byte Precision { get; set; }  
        public double PrecisionSecond
        {
            get 
            {
                return Math.Round(1d / (1L << -Precision), 6,MidpointRounding.AwayFromZero);
            }
            set
            {
                Precision = (byte)(-(256 ^ ((int)Math.Log(1d / value, 2))));
            }
        }
        /// <summary>
        /// Root Delay：32bits,本地到主参考时钟源的往返时间。
        /// </summary>
        public ShortTime RootDelay = new ShortTime();
        /// <summary>
        /// Root Dispersion：32bits,系统时钟相对于主参考时钟的最大误差。
        /// 取值范围 v3 0.01s-16s v4 0.005-16s
        /// </summary>
        public ShortTime RootDispersion = new ShortTime();

        ///<summary>
        /// Reference Identifier：8bits,参考时钟源的标识。长度不足时左向填充0
        /// 
        /// 时钟层数为0-1时，取如下标识符号，时钟层数>1时，取4位时钟主机IP
        /// V3 
        /// 
        /// 0 - DCN routing protocol
        /// 0 - NIST public modem
        /// 0 - TSP time protocol
        /// 0 - DTS Digital Time Service
        /// 1 - ATOM    Atomic clock (calibrated)
        /// 1 - VLF VLF radio (OMEGA, etc.)
        /// 1 - callsign    Generic radio
        /// 1 - LORC    LORAN-C radionavigation
        /// 1 - GOES    GOES UHF environment satellite
        /// 1 - GPS GPS UHF satellite positioning
        /// 
        /// V4
        /// GOES - Geosynchronous Orbit Environment Satellite              
        /// GPS  - Global Position System                                  
        /// GAL  - Galileo Positioning System                              
        /// PPS  - Generic pulse-per-second                                
        /// IRIG - Inter-Range Instrumentation Group                       
        /// WWVB - LF Radio WWVB Ft.Collins, CO 60 kHz                     
        /// DCF  - LF Radio DCF77 Mainflingen, DE 77.5 kHz                 
        /// HBG  - LF Radio HBG Prangins, HB 75 kHz                        
        /// MSF  - LF Radio MSF Anthorn, UK 60 kHz                         
        /// JJY  - LF Radio JJY Fukushima, JP 40 kHz, Saga, JP 60 kHz      
        /// LORC - MF Radio LORAN C station, 100 kHz                       
        /// TDF  - MF Radio Allouis, FR 162 kHz                            
        /// CHU  - HF Radio CHU Ottawa, Ontario                            
        /// WWV  - HF Radio WWV Ft. Collins, CO                            
        /// WWVH - HF Radio WWVH Kauai, HI                                 
        /// NIST - NIST telephone modem                                    
        /// ACTS - NIST telephone modem                                    
        /// USNO - USNO telephone modem                                    
        /// PTB  - European telephone modem
        /// </summary>
        public readonly byte[] ReferenceIdentifier = new byte[4]; 
        /// <summary>
        /// Reference Timestamp：64bits,系统时钟最后一次被设定或更新的时间。
        /// </summary>
        public TimeStamp ReferenceTime = new TimeStamp();  
        /// <summary>
        /// Originate Timestamp：64bits,NTP请求报文离开发送端时发送端的本地时间。
        /// </summary>
        public TimeStamp OriginateTime = new TimeStamp();   
        /// <summary>
        /// Receive Timestamp：64bits,NTP请求报文到达接收端时接收端的本地时间。
        /// </summary>
        public TimeStamp ReceiveTime = new TimeStamp();     
        /// <summary>
        /// Transmit Timestamp：64bits,应答报文离开应答者时应答者的本地时间。
        /// </summary>
        public TimeStamp TransmitTime = new TimeStamp();
        /// <summary>
        /// Destination Timestamp:64bits
        /// NTPv4扩展
        /// </summary>
        public TimeStamp DestinationTime = new TimeStamp();   // 

        /// <summary>
        /// 验证信息 
        /// V3版本采用DES加密算法
        /// 前32字节的密码标识，后64为密文
        /// 1, 3, 5模式时，服务端决定密码；2，4模式客户机决定密码
        /// V4
        /// </summary>
        public byte[] Authenticator;   //Authenticator：96bits,验证信息。

        public byte[] Pack()
        {
            List<byte> data = new List<byte>();
            byte head = 0b00000000;
            //默认11未同步
            head = (byte)(head | (LeapIndicator << 6));
            head = (byte)(head | (VersionNumber << 3));
            head = (byte)(head | Mode);
            data.Add(head);
            data.Add(Stratum);
            data.Add(Pool);
            data.Add(Precision);
            data.AddRange(BitConverter.GetBytes(RootDelay.Integer).Revert());
            data.AddRange(BitConverter.GetBytes(RootDelay.Fraction).Revert());
            data.AddRange(BitConverter.GetBytes(RootDispersion.Integer).Revert());
            data.AddRange(BitConverter.GetBytes(RootDispersion.Fraction).Revert());
            data.AddRange(ReferenceIdentifier);
            data.AddRange(BitConverter.GetBytes(ReferenceTime.Seconds).Revert());
            data.AddRange(BitConverter.GetBytes(ReferenceTime.Fraction).Revert());
            data.AddRange(BitConverter.GetBytes(OriginateTime.Seconds).Revert());
            data.AddRange(BitConverter.GetBytes(OriginateTime.Fraction).Revert());
            data.AddRange(BitConverter.GetBytes(ReceiveTime.Seconds).Revert());
            data.AddRange(BitConverter.GetBytes(ReceiveTime.Fraction).Revert());
            data.AddRange(BitConverter.GetBytes(TransmitTime.Seconds).Revert());
            data.AddRange(BitConverter.GetBytes(TransmitTime.Fraction).Revert());

            if (Authenticator != null)
            {
                byte[] auth = new byte[12];
                Array.Copy(Authenticator, auth, Authenticator.Length < 12 ? auth.Length : 12);
                data.AddRange(Authenticator);
            }
            return data.ToArray();
        }

        public static NTPPackage Parse(byte[] data)
        {
            NTPPackage np = new NTPPackage();
            byte head = data[0];
            np.LeapIndicator = (byte)(head >> 6);
            np.VersionNumber = (byte)((byte)(head << 2) >> 5);
            np.Mode = (byte)((byte)(head << 5) >> 5);
            np.Stratum = data[1];
            //求幂
            np.Pool = data[2];
            np.Precision = data[3];

            byte[] arrRootDelay = new byte[2], arrRootDelayFrac = new byte[2], arrRootDispersion = new byte[2], arrRootDispersionFrac = new byte[2];
            Array.Copy(data, 4, arrRootDelay, 0, 2);
            Array.Copy(data, 6, arrRootDelayFrac, 0, 2);
            Array.Copy(data, 8, arrRootDispersion, 0, 2);
            Array.Copy(data, 10, arrRootDispersionFrac, 0, 2);

            np.RootDelay.Integer = BitConverter.ToUInt16(arrRootDelay.Revert(), 0);
            np.RootDelay.Fraction = BitConverter.ToUInt16(arrRootDelayFrac.Revert(), 0);
            np.RootDispersion.Integer = BitConverter.ToUInt16(arrRootDispersion.Revert(), 0);
            np.RootDispersion.Fraction = BitConverter.ToUInt16(arrRootDispersionFrac.Revert(), 0);

            Array.Copy(data, 12, np.ReferenceIdentifier, 0, 4);

            byte[] arrRefSec = new byte[4], arrRefMicro = new byte[4], arrOriSec = new byte[4], arrOriMicro = new byte[4], arrRecSec = new byte[4], arrRecMicro = new byte[4], arrTranSec = new byte[4], arrTranMicro = new byte[4];

            Array.Copy(data, 16, arrRefSec, 0, 4);
            Array.Copy(data, 20, arrRefMicro, 0, 4);
            Array.Copy(data, 24, arrOriSec, 0, 4);
            Array.Copy(data, 28, arrOriMicro, 0, 4);
            Array.Copy(data, 32, arrRecSec, 0, 4);
            Array.Copy(data, 36, arrRecMicro, 0, 4);
            Array.Copy(data, 40, arrTranSec, 0, 4);
            Array.Copy(data, 44, arrTranMicro, 0, 4);

            np.ReferenceTime.Seconds = BitConverter.ToUInt32(arrRefSec.Revert(), 0);
            np.ReferenceTime.Fraction = BitConverter.ToUInt32(arrRefMicro.Revert(), 0);

            np.OriginateTime.Seconds = BitConverter.ToUInt32(arrOriSec.Revert(), 0);
            np.OriginateTime.Fraction = BitConverter.ToUInt32(arrOriMicro.Revert(), 0);

            np.ReceiveTime.Seconds = BitConverter.ToUInt32(arrRecSec.Revert(), 0);
            np.ReceiveTime.Fraction = BitConverter.ToUInt32(arrRecMicro.Revert(), 0);

            np.TransmitTime.Seconds = BitConverter.ToUInt32(arrTranSec.Revert(), 0);
            np.TransmitTime.Fraction = BitConverter.ToUInt32(arrTranMicro.Revert(), 0);

            if (data.Length > 48)
            {
                np.Authenticator = new byte[12];
                Array.Copy(data, 48, np.Authenticator, 0, data.Length >= 60 ? 12 : data.Length - 48);
            }
            return np;
        }
    }
    /// <summary>
    /// 时间戳起点为1900-01-01
    /// 日期最大校准精度为0.1微秒
    /// </summary>
    /// <summary>
    /// 32bit整数+32bits小数/2^32,整数部分正常取值，小数部分/4294967295进行换算
    /// </summary>
    public struct TimeStamp
    {
        /// <summary>
        /// 秒部分
        /// </summary>
        public uint Seconds;
        /// <summary>
        /// 小数部分
        /// </summary>
        public uint Fraction;
        public const long FractionSecondRate = (long)1 + uint.MaxValue;

        /// <summary>
        /// 时间设置统一使用UTC+0:00
        /// </summary>
        public DateTime UniversalTime
        {
            get
            {
                DateTime dateTimeStart = new DateTime(1900, 1, 1);
                dateTimeStart = dateTimeStart.AddSeconds(Seconds);
                //100纳秒=0.1微秒 
                dateTimeStart=dateTimeStart.AddTicks((long)((double)Fraction / FractionSecondRate * 1e7));
                return dateTimeStart;
            }
            set
            {
                DateTime dateTimeStart = new DateTime(1900, 1, 1);
                var dtDiff = (value - dateTimeStart);
                Seconds = (uint)dtDiff.TotalSeconds;
                Fraction = (uint)(((double)(dtDiff.Ticks/1e7)-Seconds)*1e7*FractionSecondRate);
            }
        }

    };

    /// <summary>
    /// 短时间格式
    /// 16bits整数+16bit小数/2^16 整数部分正常取值，小数部分/65536进行换算
    /// </summary>
    public struct ShortTime
    {
        public ushort Integer;
        public ushort Fraction { get; set; }
        public decimal Seconds
        {
            get
            {
                return Integer + Math.Round((decimal)Fraction / FractionSecondRate, 4, MidpointRounding.AwayFromZero);
            }
            set
            {
                Integer = (ushort)((ushort)(Seconds * 10) / 10);
                Fraction = (ushort)((Seconds - Integer) * FractionSecondRate);
            }
        }
        public const int FractionSecondRate = ushort.MaxValue + 1;

    }
    /// <summary>
    /// 协议版本
    /// </summary>
    public enum NTPVersion
    {
        Ver1 = 0x01,
        Ver2 = 0x02,
        Ver3 = 0x03,
        Ver4 = 0x04
    }
    public class NTPProtocol
    {
        public static int ProtocolPort = 123;
        /// <summary>
        /// 组播地址
        /// </summary>
        public static string MulticastAddress = "224.0.0.1";

        public int LeapIndicator { get; set; }

    }

    public enum NTPWorkMode
    {
        [Description("")]
        Undefined = 0,
        ActiveP2P = 1,
        PassiveP2P = 2,
        Client = 3,
        Server = 4,
        Broadcast = 5,
        ControlDatagraph = 6,
        Reserved = 7
    }
    public static class Others
    {

        /// <summary>
        /// 数据翻转
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Revert(this byte[] data)
        {
            byte[] d2 = new byte[data.Length];
            Array.Copy(data, d2, d2.Length);
            Array.Reverse(d2);
            return d2;
        }
    }
}
