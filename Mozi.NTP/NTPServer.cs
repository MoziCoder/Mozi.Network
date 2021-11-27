using System;

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

        private int _port = NTPProtocol.ProtocolPort;
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
                    NTPPackage npr = new NTPPackage()
                    {
                        LeapIndicator=0,
                        VersionNumber=3,
                        Mode=4,
                        Stratum=0,
                        Pool=10,
                        //Precision=Dateti
                    };
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
