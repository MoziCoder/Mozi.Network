namespace Mozi.StateService
{
    /// <summary>
    /// <see cref="HeartBeatService"/>的单例封装
    /// <code>
    ///     StateServiceHost stateHost = StateServiceHost.Instance;
    ///     stateHost.StateChangeNotifyImmediately = true;
    ///     stateHost.ApplyDevice("spos", TerminalConfig.Instance.NodeCode+TerminalConfig.Instance.TerminalCode, App.GetVersionName());
    ///     //配置心跳网关终结点信息
    ///     stateHost.SetHost("{host}", 13453);
    ///     stateHost.Activate();
    /// </code>
    /// </summary>
    /// <remarks>
    ///  如果此封装不能满足应用需求，可自行封装
    /// </remarks>
    public class StateServiceHost
    {

        private static StateServiceHost _host;

        /// <summary>
        /// 单实例
        /// </summary>
        public static StateServiceHost Instance
        {
            get { return _host ?? (_host = new StateServiceHost()); }
        }

        private readonly HeartBeatService _service = new HeartBeatService();
        /// <summary>
        /// 切换状态是否立即发送状态信息
        /// </summary>
        public bool StateChangeNotifyImmediately
        {
            get { return _service.StateChangeNotifyImmediately; }
            set { _service.StateChangeNotifyImmediately = value; }
        }
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool Initialized { get; set; }

        private StateServiceHost()
        {

        }
        /// <summary>
        /// 设置服务器地址
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void SetHost(string host, ushort port)
        {
            _service.Port = port;
            _service.RemoteHost = host;
            Initialized = true;
        }
        /// <summary>
        /// 设置心跳间隔
        /// </summary>
        /// <param name="millseconds"></param>
        public void SetInterval(int millseconds)
        {
            _service.Interval = millseconds;
        }
        /// <summary>
        /// 配置本地设备信息
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="deviceId"></param>
        /// <param name="appVersion"></param>
        public void ApplyDevice(string deviceName, string deviceId, string appVersion)
        {
            _service.ApplyDevice(deviceName, deviceId, appVersion);
        }
        /// <summary>
        /// 设置本地工作状态
        /// </summary>
        /// <param name="state"></param>
        public void SetState(ClientLifeState state)
        {
            _service.SetState(state);
        }
        /// <summary>
        /// 激活心跳
        /// </summary>
        public void Activate()
        {
            _service.Init();
            _service.Activate();
        }
        /// <summary>
        /// 反激活心跳
        /// </summary>
        public void Inactivate()
        {
            _service.Inactivate();
        }
        /// <summary>
        /// 状态设置为在线
        /// </summary>
        public void Alive()
        {
            SetState(ClientLifeState.Alive);
        }
        /// <summary>
        /// 状态设置为离线
        /// </summary>
        public void Leave()
        {
            SetState(ClientLifeState.Byebye);
        }
        /// <summary>
        /// 状态设置为繁忙
        /// </summary>
        public void Busy()
        {
            SetState(ClientLifeState.Busy);
        }
        /// <summary>
        /// 状态设置为空闲
        /// </summary>
        public void Idle()
        {
            SetState(ClientLifeState.Idle);
        }
    }
}
