using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Mozi.StateService
{
    //TODO 是否考虑建立双向心跳服务，类似于握手
    //TODO 是否考虑设置一个备份服务器地址

    /// <summary>
    /// 状态服务,心跳发送方
    /// <para>
    /// 状态服务仅能承载很少的业务功能，主要面向终端检活这种应用场景，协议包封装的很小，可以支撑数量庞大的终端监测功能。
    /// </para>
    /// </summary>
    public class HeartBeatService
    {
        
        private ushort _port = DefaultPort;

        private string _host = "127.0.0.1";

        protected Socket _sc;

        private bool active = false;

        private readonly Timer _timeLooper;

        private int _interval = 30 * 1000;

        private readonly HeartBeatPackage _sp = new HeartBeatPackage
        {
            DeviceName = "Mozi",
            DeviceId = "00010001",
            StateName = 0x31,
            Version =0x31,
            AppVersion = "1.0.0",
            UserName="",
        };

        private  IPEndPoint _remoteEndPoint;
        /// <summary>
        /// 套接字是否已初始化
        /// </summary>
        private bool _socketInitialized = false;

        public const int DefaultPort = 13453; 

        public HeartBeatService()
        {
            InitRemoteEndpoint();
            _timeLooper = new Timer(TimerCallbackInvoker, this, Timeout.Infinite, Timeout.Infinite);
        }

        ~HeartBeatService()
        {
            if (_timeLooper != null)
            {
                _timeLooper.Dispose();
            }

            if (_sc != null)
            {
                _sc.Dispose();
            }
        }
        /// <summary>
        ///服务器地址 
        /// </summary>
        public string RemoteHost
        {
            get { return _host; }
            set {
                _host = value;
                InitRemoteEndpoint();
            }
        }
        /// <summary>
        /// 状态变更实时通知
        /// <para>
        /// 开启此参数会立即向服务器发出数据包
        /// </para>
        /// </summary>
        public bool StateChangeNotifyImmediately { get; set; }
        /// <summary>
        /// 登录用户变更即时通知
        /// <para>
        /// 开启此参数会立即向服务器发出数据包
        /// </para>
        /// </summary>
        public bool UserChangeNotifyImmediately { get; set; }
        /// <summary>
        /// 网关服务端口
        /// </summary>
        public ushort Port
        {
            get { return _port; }
            set { 
                _port = value;
                InitRemoteEndpoint();
            }
        }
        /// <summary>
        /// 设置心跳周期 默认30秒
        /// </summary>
        public int Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }
        public Socket SocketMain
        {
            get { return _sc; }
        }
        /// <summary>
        /// 激活心跳
        /// </summary>
        public void Activate()
        {
            active = true;
            _timeLooper.Change(0, _interval);
        }
        /// <summary>
        /// 停止心跳
        /// </summary>
        public void Inactivate()
        {
            active = false;
            _timeLooper.Change(Timeout.Infinite, Timeout.Infinite);
        }
        /// <summary>
        /// 定时回调
        /// </summary>
        /// <param name="sender"></param>
        private void TimerCallbackInvoker(object sender)
        {
            if (active)
            {
                SendPack();
            }
            else
            {

            }
        }
        /// <summary>
        /// 配置终端信息
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="deviceId"></param>
        /// <param name="appVersion"></param>
        /// <returns></returns>
        public HeartBeatService ApplyDevice(string deviceName, string deviceId,string appVersion)
        {
            _sp.DeviceName = deviceName;
            _sp.DeviceId = deviceId;
            _sp.AppVersion = appVersion;
            return this;
        }
        /// <summary>
        /// 设置用户名
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public HeartBeatService SetUserName(string userName)
        {
            var oldUserName = _sp.UserName;
            _sp.UserName = userName;
            if (oldUserName != _sp.UserName && UserChangeNotifyImmediately)
            {
                SendPack();
            }
            return this;
        }
        /// <summary>
        /// 设置终端状态，此方法会实时发送客户机状态到网关
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public HeartBeatService SetState(ClientLifeState stateName)
        {
            var oldState = _sp.StateName;
            _sp.StateName = (byte)stateName;
            if (oldState != _sp.StateName&&StateChangeNotifyImmediately)
            {
                SendPack();
            }
            return this;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="port"></param>
        public void Init()
        {
            if (_sc == null)
            {
                _sc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            }
            else
            {
                _sc.Close();
            }
            //允许端口复用
            _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _sc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, 100);
        }
        /// <summary>
        /// 初始化终结点
        /// </summary>
        private void InitRemoteEndpoint()
        {
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(_host), _port);
            _socketInitialized = true;
        }
        /// <summary>
        /// 发送数据包
        /// </summary>
        private void SendPack()
        {
            if (!string.IsNullOrEmpty(_host)&&_socketInitialized)
            {
                _sc.SendTo(_sp.Pack(), _remoteEndPoint);
            }
        }
        /// <summary>
        /// 终端状态 Alive
        /// </summary>
        public void Alive()
        {
            SetState(ClientLifeState.Alive);
        }
        /// <summary>
        /// 终端状态 Leave
        /// </summary>
        public void Leave()
        {
            SetState(ClientLifeState.Byebye);
        }
        /// <summary>
        /// 终端状态 Busy
        /// </summary>
        public void Busy()
        {
            SetState(ClientLifeState.Busy);
        }
        /// <summary>
        /// 终端状态 Idle
        /// </summary>
        public void Idle()
        {
            SetState(ClientLifeState.Idle);
        }
    }

}
