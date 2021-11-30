using System;
using System.Net;

namespace Mozi.NTP
{

    public class TimeSyncArgs:DataTransferArgs{
        public NTPPackage TimePackage { get; set; }
        
    }
    public delegate void TimePackageReceive(object sender, TimeSyncArgs args);

    //目前仅开发服务器/客户机模式
    /// <summary>
    /// 心跳网关服务器
    /// </summary>
    public class NTPServer
    {
        private readonly UDPSocket _socket;

        private int _port = NTPProtocol.Port;
        /// <summary>
        /// 服务端端口
        /// </summary>
        public int Port { get { return _port; } }

        public DateTime StartTime { get; private set; }

        public event TimePackageReceive OnTimePackageReceived;

        public NTPServer()
        {
            _socket = new UDPSocket();
            _socket.AfterReceiveEnd += _socket_AfterReceiveEnd;
        }
        /// <summary>
        /// 以默认端口启动<see cref="F:Port"/>
        /// </summary>
        public void Start()
        {
            Start(_port);
        }
        /// <summary>
        /// 启动网关
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            _port = port;
            _socket.Start(_port);
            StartTime = DateTime.Now;
        }
        /// <summary>
        /// 网关下线
        /// </summary>
        public void Shutdown()
        {
            _socket.Shutdown();
            StartTime = DateTime.MinValue;
        }
        /// <summary>
        /// 数据接收完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void _socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            try
            {
                DateTime dtNow = DateTime.Now.ToUniversalTime();

                NTPPackage np = NTPPackage.Parse(args.Data);
                TimeSyncArgs ta = new TimeSyncArgs()
                {
                    Client=args.Client,
                    Data=args.Data,
                    IP=args.IP,
                    Port=args.Port,
                    Socket=args.Socket,
                    State=args.State,
                    TimePackage=np
                };
                if (OnTimePackageReceived == null)
                {
                    
                    //theta = T(B) - T(A) = 1 / 2 * [(T2 - T1) + (T3 - T4)]
                    //delta = T(ABA) = (T4 - T1) - (T3 - T2).

                    NTPPackage npr = new NTPPackage()
                    {
                        //TODO 系统闰秒判断
                        LeapIndicator = 0,
                        VersionNumber = np.VersionNumber > (int)NTPVersion.Ver4 ? (byte)NTPVersion.Ver4 : np.VersionNumber,
                        Mode = (int)NTPWorkMode.Server,
                        //时钟层数为1
                        Stratum = 1,
                        Pool = 10,
                        //本地时钟精度 约15.6ms
                        Precision = 250,
                        RootDelay = new ShortTime() { Integer = 0, Fraction = 0 },
                        //RootDispersion = new ShortTime() { Seconds = 10.0156m },
                        ReferenceTime=new TimeStamp() { UniversalTime=dtNow},
                        Origin =np.TransmitTime,
                        ReceiveTime = np.LocalReceiveTime,
                        TransmitTime = new TimeStamp() { UniversalTime = dtNow },
                    };
                    Array.Copy(ClockIdentifier.LOCL.Pack, npr.ReferenceIdentifier, npr.ReferenceIdentifier.Length);
                    args.Socket.SendTo(npr.Pack(), new IPEndPoint(IPAddress.Parse(args.IP), args.Port));
                }
                else
                {
                    OnTimePackageReceived(this, ta);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
