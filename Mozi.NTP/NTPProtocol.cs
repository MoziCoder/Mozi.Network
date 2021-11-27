using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mozi.NTP
{
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
    /// <summary>
    /// NTP协议采取定长数据包格式
    /// </summary>
    public class NTPPackage
    {

        public byte LeapIndicator { get; set; }       //LI（Leap Indicator）：2bits，值为“11”时表示告警状态，时钟未被同步。为其他值时NTP本身不做处理。
        public byte VersionNumber { get; set; }       //VN（Version Number）：3bits，表示NTP的版本号，目前的最新版本为3。
        public byte Mode { get; set; }                //Mode：3bits，表示NTP的工作模式。不同的值所表示的含义分别是：0未定义、1表示主动对等体模式、2表示被动对等体模式、3表示客户模式、4表示服务器模式、5表示广播模式或组播模式、6表示此报文为NTP控制报文、7预留给内部使用。
        public byte Stratum { get; set; }             //Stratum：8bits,系统时钟的层数，取值范围为1～16，它定义了时钟的准确度。层数为1的时钟准确度最高，准确度从1到16依次递减，层数为16的时钟处于未同步状态，不能作为参考时钟。
        public byte Pool { get; set; }                //Poll：8bits,轮询时间，即两个连续NTP报文之间的时间间隔。
        public byte Precision { get; set; }           //Precision：8bits,系统时钟的精度。
        public float RootDelay { get; set; }           //Root Delay：32bits,本地到主参考时钟源的往返时间。
        public float RootDispersion { get; set; }      //Root Dispersion：32bits,系统时钟相对于主参考时钟的最大误差。
        public readonly byte[] ReferenceIdentifier = new byte[4]; //Reference Identifier：8bits,参考时钟源的标识。
        public TimeStamp ReferenceTimeStamp = new TimeStamp();  //Reference Timestamp：64bits,系统时钟最后一次被设定或更新的时间。
        public TimeStamp OriginateTimestamp = new TimeStamp();   //Originate Timestamp：64bits,NTP请求报文离开发送端时发送端的本地时间。
        public TimeStamp ReceiveTimestamp = new TimeStamp();     //Receive Timestamp：64bits,NTP请求报文到达接收端时接收端的本地时间。
        public TimeStamp TransmitTimestamp = new TimeStamp();    //Transmit Timestamp：64bits,应答报文离开应答者时应答者的本地时间。
        public byte[] Authenticator;   //Authenticator：96bits,验证信息。

        public byte[] Pack()
        {
            List<byte> data = new List<byte>();
            byte head = 0b00000000;
            head = (byte)(head | (LeapIndicator << 6));
            head = (byte)(head | (VersionNumber << 3));
            head = (byte)(head | Mode);
            data.Add(head);
            data.Add(Stratum);
            data.Add(Pool);
            data.Add(Precision);
            data.AddRange(BitConverter.GetBytes(RootDelay).Revert());
            data.AddRange(BitConverter.GetBytes(RootDispersion).Revert());
            data.AddRange(ReferenceIdentifier);
            data.AddRange(BitConverter.GetBytes(ReferenceTimeStamp.Seconds).Revert());
            data.AddRange(BitConverter.GetBytes(ReferenceTimeStamp.MicroSeconds).Revert());
            data.AddRange(BitConverter.GetBytes(OriginateTimestamp.Seconds).Revert());
            data.AddRange(BitConverter.GetBytes(OriginateTimestamp.MicroSeconds).Revert());
            data.AddRange(BitConverter.GetBytes(ReceiveTimestamp.Seconds).Revert());
            data.AddRange(BitConverter.GetBytes(ReceiveTimestamp.MicroSeconds).Revert());
            data.AddRange(BitConverter.GetBytes(TransmitTimestamp.Seconds).Revert());
            data.AddRange(BitConverter.GetBytes(TransmitTimestamp.MicroSeconds).Revert());
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
            np.Pool = data[2];
            np.Precision = data[3];
            byte[] arrRootDelay = new byte[4], arrRootDispersion = new byte[4];
            Array.Copy(data, 4, arrRootDelay, 0, 4);
            Array.Copy(data, 8, arrRootDispersion, 0, 4);
            np.RootDelay = BitConverter.ToSingle(arrRootDelay.Revert(), 0);
            np.RootDispersion = BitConverter.ToSingle(arrRootDispersion.Revert(), 0);
            Array.Copy(data, 4 + 4 + 4, np.ReferenceIdentifier, 0, 4);

            byte[] arrRefSec = new byte[4], arrRefMicro = new byte[4], arrOriSec = new byte[4], arrOriMicro = new byte[4], arrRecSec = new byte[4], arrRecMicro = new byte[4], arrTranSec = new byte[4], arrTranMicro = new byte[4];
            Array.Copy(data, 16, arrRefSec, 0, 4);
            Array.Copy(data, 20, arrRefMicro, 0, 4);
            Array.Copy(data, 24, arrOriSec, 0, 4);
            Array.Copy(data, 28, arrOriMicro, 0, 4);
            Array.Copy(data, 32, arrRecSec, 0, 4);
            Array.Copy(data, 36, arrRecMicro, 0, 4);
            Array.Copy(data, 40, arrTranSec, 0, 4);
            Array.Copy(data, 44, arrTranMicro, 0, 4);
            
            np.ReferenceTimeStamp.Seconds = BitConverter.ToUInt32(arrRefSec.Revert(),0);
            np.ReferenceTimeStamp.MicroSeconds = BitConverter.ToUInt32(arrRefMicro.Revert(), 0);

            np.OriginateTimestamp.Seconds = BitConverter.ToUInt32(arrOriSec.Revert(), 0);
            np.OriginateTimestamp.MicroSeconds = BitConverter.ToUInt32(arrOriMicro.Revert(), 0);

            np.ReceiveTimestamp.Seconds = BitConverter.ToUInt32(arrRecSec.Revert(), 0);
            np.ReceiveTimestamp.MicroSeconds = BitConverter.ToUInt32(arrRecMicro.Revert(), 0);

            np.TransmitTimestamp.Seconds = BitConverter.ToUInt32(arrTranSec.Revert(), 0);
            np.TransmitTimestamp.MicroSeconds = BitConverter.ToUInt32(arrTranMicro.Revert(), 0);

            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            //dateTimeStart.AddMilliseconds(np.ReceiveTimestamp);
            if (data.Length > 48)
            {
                np.Authenticator = new byte[12];
                Array.Copy(data, 48, np.Authenticator, 0, data.Length >= 60 ? 12 : data.Length - 48);
            }
            return np;
        }
    }
    /// <summary>
    /// 4294.967296
    /// </summary>
    public struct TimeStamp
    {

        public uint Seconds;
        public uint MicroSeconds;
    };

    public static class Others{
        /// <summary>
        /// 数据翻转
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Revert(this byte[] data)
        {
            Array.Reverse(data);
            return data;
        }
    }
}
