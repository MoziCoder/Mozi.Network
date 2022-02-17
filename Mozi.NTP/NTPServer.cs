using System;

namespace Mozi.NTP
{

    public class TimeSyncArgs:DataTransferArgs
    {
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

        private ushort _port = NTPProtocol.Port;
        /// <summary>
        /// 服务端端口
        /// </summary>
        public ushort Port { get { return _port; } }

        public DateTime StartTime { get; private set; }
        /// <summary>
        /// 系统时钟源
        /// </summary>
        public IClock Clock { get; set; }

        public  TimePackageReceive OnTimePackageReceived;
        /// <summary>
        /// 服务器运行状态
        /// </summary>
        public bool Running
        {
            get; set;
        }
        public NTPServer()
        {
            _socket = new UDPSocket();
            _socket.AfterReceiveEnd += Socket_AfterReceiveEnd;
            Clock = new SystemClock();
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
        public void Start(ushort port)
        {
            _port = port;
            _socket.Start(_port);
            StartTime = DateTime.Now;
            Running = true;
        }
        /// <summary>
        /// 网关下线
        /// </summary>
        public void Shutdown()
        {
            Running = false;
            _socket.Shutdown();
            StartTime = DateTime.MinValue;
        }
        /// <summary>
        /// 数据接收完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void Socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            try
            {
                DateTime dtNow = Clock.GetTime();

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

                        ReferenceTime = new TimeStamp() { UniversalTime = dtNow },
                        OriginTime = np.TransmitTime,
                        ReceiveTime = np.LocalReceiveTime,
                        TransmitTime = new TimeStamp() { UniversalTime = Clock.GetTime() },
                    };
                    // s.rootdisp < --p.epsilon_r + p.epsilon + p.psi + PHI * (s.t - p.t)+ |THETA| 
                    // p.epsilon_r=s.precision+p.precision+PHI*(t4-t1)
                    // p.epsilon=

                    //δ=(Ti−Ti−3)−(Ti−1−Ti−2) ,
                    //θ=((Ti−2−Ti−3)+(Ti−1−Ti))/2 ,
                    //ε=(1<< sys.precision)+φ(Ti−Ti−3) 
                    // sys.rootdispersion ←Ε+ max(εξ+ |Θ|, NTP.MINDISPERSE);

                    //s.jitter = LOG2D(s.precision);
                    //s.rootdelay = p->rootdelay + p->delay;
                    //dtemp = SQRT(SQUARE(p->jitter) + SQUARE(s.jitter));
                    //dtemp += max(p->disp + PHI * (c.t - p->t) +fabs(p->offset), MINDISP);
                    //s.rootdisp = p->rootdisp + dtemp;
                    //offset = (LFP2D(r->rec - r->org) + LFP2D(r->dst - r->xmt)) / 2;

                    //double temp = Math.Sqrt(Math.Pow(np.PrecisionSecond, 2) + Math.Pow(npr.PrecisionSecond, 2));
                    //temp += Math.Max(np.RootDispersion.Seconds + (NTPProtocol.PHI * (npr.TransmitTime.UniversalTime - np.ReferenceTime.UniversalTime).TotalSeconds)+Math.Abs((np.ReceiveTime.UniversalTime-np.OriginTime.UniversalTime).TotalSeconds+(npr.ReceiveTime.UniversalTime-npr.TransmitTime.UniversalTime).TotalSeconds/2), NTPProtocol.MinDispersion);
                    //npr.RootDispersion = new ShortTime() { Seconds = temp };

                    //TODO 这里算法应该是有问题 10为常数

                    npr.RootDispersion = new ShortTime() { Seconds = (double)(10+ npr.PrecisionSecond + (NTPProtocol.PHI * (npr.TransmitTime.UniversalTime - np.ReferenceTime.UniversalTime).TotalSeconds)) };
                    Array.Copy(ClockIdentifier.LOCL.Pack, npr.ReferenceIdentifier, npr.ReferenceIdentifier.Length);
                    _socket.SendTo(npr.Pack(), args.IP, args.Port);
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
