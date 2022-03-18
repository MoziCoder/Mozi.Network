using Mozi.NTP.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mozi.NTP
{

    //TODO 加密验证部分未处理
    //TODO KOD包未处理
    //TODO NTP控制包未实现

    //RFC1305 1992 NTP Version3 
    //RFC5905 2010 NTP Version4 修订 RFC7822,8573,9109  

    //参考RFC NTPv4实现 并向下兼容NTPv3
    //| org  | T1      | origin timestamp   |
    //| rec  | T2      | receive timestamp  |
    //| xmt  | T3      | transmit timestamp |
    //| dst  | T4      | destination timestamp
    //| t    | t       | packet time

    //T(t) = T(t_0) + R(t_0)(t-t_0) + 1/2 * D(t_0)(t-t_0)^2 + e,
    //NTP控制报文
    public class NTPControlPackage
    {

    }
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
        //TODO 此处算法是否有问题？
        public double PrecisionSecond
        {
            get
            {
                return Math.Round(1d / (1L << -Precision), 6, MidpointRounding.AwayFromZero);
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
        /// 1 - call sign    Generic radio
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
        public TimeStamp OriginTime = new TimeStamp();
        /// <summary>
        /// Receive Timestamp：64bits,NTP请求报文到达接收端时接收端的本地时间。
        /// </summary>
        public TimeStamp ReceiveTime = new TimeStamp();
        /// <summary>
        /// Transmit Timestamp：64bits,应答报文离开应答者时应答者的本地时间。
        /// </summary>
        public TimeStamp TransmitTime = new TimeStamp();
        /// <summary>
        /// 验证信息 
        /// V3版本采用DES加密算法
        /// 前32字节的密码标识，后64为密文
        /// 1, 3, 5模式时，服务端决定密码；2，4模式客户机决定密码
        /// V4
        /// </summary>
        public byte[] Authenticator;   //Authenticator：96bits,验证信息。

        internal TimeStamp LocalReceiveTime = new TimeStamp();

        public byte[] Pack()
        {
            double skew=0d;
            //TODO 系统最大时钟误差有问题
            //if (LeapIndicator == 0x11 || (DateTime.UtcNow - ReferenceTime.UniversalTime).TotalSeconds > NTPProtocol.MaxAge)
            //{
            //    skew = NTPProtocol.MaxSkew;
            //}
            //else
            //{
            //    skew = (DateTime.UtcNow - ReferenceTime.UniversalTime).TotalSeconds / 2;
            //}
            //r.rho-客户机精度  s.rho服务器精度 
            //epsilon(t_0) = r.rho + s.rho + PHI * (T4 - T1)

            //RootDispersion.Seconds = (decimal)Math.Round((1 << Precision) + skew,5);
            //if (sys.leap = 112or(sys.clock – sys.reftime) > NTP.MAXAGE)
            //        skew← NTP.MAXSKEW;
            //else
            //    skew←φ(sys.clock − sys.reftime);
            //pkt.rootdispersion ←sys.rootdispersion + (1 << sys.precision) + skew;
            // | s.leap < --p.leap |
            //| s.stratum < --p.stratum + 1 |
            //| s.offset < --THETA |
            //| s.jitter < --PSI |
            //| s.rootdelay < --p.delta_r + delta |
            //| s.rootdisp < --p.epsilon_r + p.epsilon + |p.psi + PHI * (s.t - p.t) |+ | THETA |
            //| s.refid < --p.refid |
            //| s.reftime < --p.reftime |
            //| s.t < --p.t

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
            data.AddRange(RootDelay.Pack);
            data.AddRange(RootDispersion.Pack);
            data.AddRange(ReferenceIdentifier);
            data.AddRange(ReferenceTime.Pack);
            data.AddRange(OriginTime.Pack);
            data.AddRange(ReceiveTime.Pack);
            data.AddRange(TransmitTime.Pack);

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
            //赋值本地接收时间
            np.LocalReceiveTime.UniversalTime = DateTime.Now.ToUniversalTime();

            byte head = data[0];
            np.LeapIndicator = (byte)(head >> 6);
            np.VersionNumber = (byte)((byte)(head << 2) >> 5);
            np.Mode = (byte)((byte)(head << 5) >> 5);
            np.Stratum = data[1];

            //求幂
            np.Pool = data[2];
            np.Precision = data[3];

            byte[] arrRootDelay = new byte[4], arrRootDispersion = new byte[4];

            Array.Copy(data, 4, arrRootDelay, 0, 2);
            Array.Copy(data, 8, arrRootDispersion, 0, 2);

            np.RootDelay.Pack = arrRootDelay;
            np.RootDispersion.Pack = arrRootDispersion;

            Array.Copy(data, 12, np.ReferenceIdentifier, 0, 4);

            byte[] arrRef = new byte[8], arrOri = new byte[8], arrRec = new byte[8], arrTran = new byte[8];

            Array.Copy(data, 16, arrRef, 0, 8);
            Array.Copy(data, 24, arrOri, 0, 8);
            Array.Copy(data, 32, arrRec, 0, 8);
            Array.Copy(data, 40, arrTran, 0, 8);

            np.ReferenceTime.Pack = arrRef;
            np.OriginTime.Pack = arrOri;
            np.ReceiveTime.Pack = arrRec;
            np.TransmitTime.Pack = arrTran;

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

        public byte[] Pack
        {
            get
            {
                byte[] data = new byte[8];
                Array.Copy(BitConverter.GetBytes(Seconds).Revert(), data, 4);
                Array.Copy(BitConverter.GetBytes(Fraction).Revert(), 0, data, 4, 4);
                return data;
            }
            set
            {
                byte[] data = new byte[8];
                Array.Copy(value, data, value.Length < 8 ? value.Length : 8);
                byte[] di = new byte[4], df = new byte[4];
                Array.Copy(data, di, 4);
                Array.Copy(data, 4, df, 0, 4);
                Seconds = BitConverter.ToUInt32(di.Revert(), 0);
                Fraction = BitConverter.ToUInt32(df.Revert(), 0);
            }
        }

        /// <summary>
        /// 时间设置统一使用UTC+0:00
        /// </summary>
        public DateTime UniversalTime
        {
            get
            {
                if (Seconds == 0 && Fraction == 0)
                {
                    return NTPProtocol.JAN_1970;
                }
                else
                {
                    DateTime dateTimeStart = new DateTime(1900, 1, 1);
                    dateTimeStart = dateTimeStart.AddSeconds(Seconds);
                    //100纳秒=0.1微秒 
                    dateTimeStart = dateTimeStart.AddTicks((long)((double)Fraction / FractionSecondRate * 1e7));
                    return dateTimeStart;
                }
            }
            set
            {
                DateTime dateTimeStart = new DateTime(1900, 1, 1);
                var dtDiff = (value - dateTimeStart);
                Seconds = (uint)dtDiff.TotalSeconds;
                Fraction = (uint)(((double)(dtDiff.Ticks / 1e7) - Seconds) * 1e7 * FractionSecondRate);
            }
        }
        public override string ToString()
        {
            return UniversalTime.ToString("yyyy-MM-dd HH:mm:ss.fffffffff UTC");
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

        public byte[] Pack
        {
            get
            {
                byte[] data = new byte[4];
                Array.Copy(BitConverter.GetBytes(Integer).Revert(), data, 2);
                Array.Copy(BitConverter.GetBytes(Fraction).Revert(), 0, data, 2, 2);
                return data;
            }

            set
            {
                byte[] data = new byte[4];
                Array.Copy(value, data, value.Length < 4 ? value.Length : 4);
                byte[] di = new byte[2], df = new byte[2];
                Array.Copy(data, di, 2);
                Array.Copy(data, 2, df, 0, 2);
                Seconds = BitConverter.ToUInt16(di.Revert(), 0);
                Fraction = BitConverter.ToUInt16(df.Revert(), 0);
            }
        }

        public double Seconds
        {
            get
            {
                return Integer + Math.Round((double)Fraction / FractionSecondRate, 4, MidpointRounding.AwayFromZero);
            }
            set
            {
                Integer = (ushort)((ushort)(value * 10) / 10);
                Fraction = (ushort)((value - Integer) * FractionSecondRate);
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
    /// <summary>
    /// V3版本常量
    /// </summary>
    public class NTPProtocol
    {
        public const int Port = 123;
        /// <summary>
        /// 组播地址
        /// </summary>
        public const string MulticastAddress = "224.0.0.1";

        public static DateTime JAN_1970 = new DateTime(1970, 01, 01);

        /// <summary>
        /// Version Number
        /// </summary>
        public const int Version = 3;
        ///Max Stratum
        public const int MaxStratum = 15;
        ///Max Clock Age in seconds
        public const int MaxAge = 86400;
        ///Max Skew in seconds
        public const int MaxSkew = 1;
        ///Max Distance in seconds
        public const int MaxDistance = 1;
        //V3版本中取值范围为6-10，V4版本中Pool取值范围为4-17
        ///Min Polling Interval power of 2 in seconds
        public const int MinPool = 6;// (64 sec)
        ///Max Polling Interval power of 2 in seconds
        public const int MaxPool = 10;// (1024 sec)
        /// <summary>
        /// 时钟频率 Hz
        /// </summary>
        public const int ClockRate = 1000;
        ///Min Select Clocks
        public const int MinClock = 3;
        ///Max Select Clocks
        public const int MaxClock = 10;
        ///Min Dispersion in seconds
        public const double MinDispersion = 0.01d;
        ///Max Dispersion in seconds
        public const double MaxDispersion = 16d;
        /// Reachability Reg Size
        public const int Window = 8;
        ///Filter Size
        public const int Shift = 8;
        ///Filter Weight
        public const double Filter = 1 / 2;
        ///Select Weight
        public const double Select = 3 / 4;

        public const double PHI = 15e-6;
    }
    /// <summary>
    /// V4版本中的常量
    /// </summary>
    public class NTPProtocolV4:NTPProtocol
    {
        public const int TTLMax = 8;       /* max ttl manycast */
        public const int Beacon = 15;      /* max interval between beacons */
        public const int NSTAGE = 8;       /* clock register stages */
        public const int NMAX = 50;      /* maximum number of peers */
        public const int NSANE = 1;       /* % minimum intersection survivors */
        public const int NMIN = 3;      /* % minimum cluster survivors */
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

    public class ClockIdentifier : AbsClassEnum
    {
        public static ClockIdentifier GOES = new ClockIdentifier("GOES", "Geostationary Orbit Environment Satellite");
        public static ClockIdentifier GPS = new ClockIdentifier("GPS\0", "Global Position System");
        public static ClockIdentifier GAL = new ClockIdentifier("GAL\0", "Galileo Positioning System");
        public static ClockIdentifier PPS = new ClockIdentifier("PPS\0", "Generic pulse-per-second");
        public static ClockIdentifier IRIG = new ClockIdentifier("IRIG", "Inter-Range Instrumentation Group");
        public static ClockIdentifier WWVB = new ClockIdentifier("WWVB", "LF Radio WWVB Ft. Collins, CO 60 kHz");
        public static ClockIdentifier DCF = new ClockIdentifier("DCF\0", "LF Radio DCF77 Mainflingen, DE 77.5 kHz");
        public static ClockIdentifier HBG = new ClockIdentifier("HBG\0", "LF Radio HBG Prangins, HB 75 kHz");
        public static ClockIdentifier MSF = new ClockIdentifier("MSF\0", "LF Radio MSF Anthorn, UK 60 kHz");
        public static ClockIdentifier JJY = new ClockIdentifier("JJY\0", "LF Radio JJY Fukushima, JP 40 kHz, Saga, JP 60 kHz");
        public static ClockIdentifier LORC = new ClockIdentifier("LORC", "MF Radio LORAN C station, 100 kHz");
        public static ClockIdentifier TDF = new ClockIdentifier("TDF\0", "MF Radio Allouis, FR 162 kHz");
        public static ClockIdentifier CHU = new ClockIdentifier("CHU\0", "HF Radio CHU Ottawa, Ontario");
        public static ClockIdentifier WWV = new ClockIdentifier("WWV\0", "HF Radio WWV Ft. Collins, CO");
        public static ClockIdentifier WWVH = new ClockIdentifier("WWVH", "HF Radio WWVH Kauai, HI");
        public static ClockIdentifier NIST = new ClockIdentifier("NIST", "NIST telephone modem");
        public static ClockIdentifier ACTS = new ClockIdentifier("ACTS", "NIST telephone modem");
        public static ClockIdentifier USNO = new ClockIdentifier("USNO", "USNO telephone modem");
        public static ClockIdentifier PTB = new ClockIdentifier("PTB\0", "European telephone modem");
        public static ClockIdentifier LOCL = new ClockIdentifier("LOCL", "uncalibrated local clock");
        public static ClockIdentifier CESM = new ClockIdentifier("CESM", "calibrated Cesium clock");
        public static ClockIdentifier RBDM = new ClockIdentifier("RBDM", "calibrated Rubidium clock");
        public static ClockIdentifier OMEG = new ClockIdentifier("OMEG", "OMEGA radionavigation system");
        public static ClockIdentifier DCN = new ClockIdentifier("DCN\0", "DCN routing protocol");
        public static ClockIdentifier TSP = new ClockIdentifier("TSP\0", "TSP time protocol");
        public static ClockIdentifier DTS = new ClockIdentifier("DTS\0", "Digital Time Service");
        public static ClockIdentifier ATOM = new ClockIdentifier("ATOM", "Atomic clock (calibrated)");
        public static ClockIdentifier VLF = new ClockIdentifier("VLF\0", "VLF radio (OMEGA,, etc.)");
        public static ClockIdentifier FREE = new ClockIdentifier("FREE", "(Internal clock)");
        public static ClockIdentifier INIT = new ClockIdentifier("INIT", "(Initialization)");
        public static ClockIdentifier Unknown = new ClockIdentifier("\0\0\0\0", "NULL");

        public string Name { get; set; }
        public string Description { get; set; }
        protected override string Tag { get { return Name; } }

        public byte[] Pack
        {
            get
            {
                byte[] data = new byte[4];
                Array.Copy(System.Text.Encoding.ASCII.GetBytes(Name), data, Name.Length > 4 ? 4 : Name.Length);
                return data;
            }
        }

        public ClockIdentifier(string name, string desc)
        {
            Name = name;
            Description = desc;
        }

        //public static ClockIdentifier Parse()
        //{

        //}
    }
}
