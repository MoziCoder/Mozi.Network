using System.ComponentModel;

namespace Mozi.NTP
{

    //LI（Leap Indicator）：长度为2比特，值为“11”时表示告警状态，时钟未被同步。为其他值时NTP本身不做处理。
    //VN（Version Number）：长度为3比特，表示NTP的版本号，目前的最新版本为3。
    //Mode：长度为3比特，表示NTP的工作模式。不同的值所表示的含义分别是：0未定义、1表示主动对等体模式、2表示被动对等体模式、3表示客户模式、4表示服务器模式、5表示广播模式或组播模式、6表示此报文为NTP控制报文、7预留给内部使用。
    //Stratum：系统时钟的层数，取值范围为1～16，它定义了时钟的准确度。层数为1的时钟准确度最高，准确度从1到16依次递减，层数为16的时钟处于未同步状态，不能作为参考时钟。
    //Poll：轮询时间，即两个连续NTP报文之间的时间间隔。
    //Precision：系统时钟的精度。
    //Root Delay：本地到主参考时钟源的往返时间。
    //Root Dispersion：系统时钟相对于主参考时钟的最大误差。
    //Reference Identifier：参考时钟源的标识。
    //Reference Timestamp：系统时钟最后一次被设定或更新的时间。
    //Originate Timestamp：NTP请求报文离开发送端时发送端的本地时间。
    //Receive Timestamp：NTP请求报文到达接收端时接收端的本地时间。
    //Transmit Timestamp：应答报文离开应答者时应答者的本地时间。
    //Authenticator：验证信息。

    class NTPProtocol
    {
        public static int ProtocolPort=123;
        /// <summary>
        /// 组播地址
        /// </summary>
        public static string MulticastAddress = "224.0.0.1";

        public int LeapIndicator { get; set; }

    }

    public enum NTPWorkMode
    {
        [Description("")]
        Undefined=0,
        ActiveP2P=1,
        PassiveP2P=2,
        Client=3,
        Server=4,
        Broadcast=5,
        ControlDatagraph=6,
        Reserved=7
    }
}
