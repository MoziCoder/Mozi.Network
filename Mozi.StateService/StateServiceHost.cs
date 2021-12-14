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
    public class StateServiceHost
    {

        private static StateServiceHost _host;

        public static StateServiceHost Instance
        {
            get { return _host ?? (_host = new StateServiceHost()); }
        }

        private readonly HeartBeatService _service = new HeartBeatService();

        public bool StateChangeNotifyImmediately
        {
            get { return _service.StateChangeNotifyImmediately; }
            set { _service.StateChangeNotifyImmediately = value; }
        }
        public bool Initialized { get; set; }

        private StateServiceHost()
        {

        }

        public void SetHost(string host, int port)
        {
            _service.Port = port;
            _service.RemoteHost = host;
            Initialized = true;
        }

        public void SetInterval(int millseconds)
        {
            _service.Interval = millseconds;
        }

        public void ApplyDevice(string deviceName, string deviceId, string appVersion)
        {
            _service.ApplyDevice(deviceName, deviceId, appVersion);
        }

        public void SetState(ClientLifeState state)
        {
            _service.SetState(state);
        }

        public void Activate()
        {
            _service.Init();
            _service.Activate();
        }

        public void Inactivate()
        {
            _service.Inactivate();
        }

        public void Alive()
        {
            SetState(ClientLifeState.Alive);
        }

        public void Leave()
        {
            SetState(ClientLifeState.Byebye);
        }

        public void Busy()
        {
            SetState(ClientLifeState.Busy);
        }
        public void Idle()
        {
            SetState(ClientLifeState.Idle);
        }
    }
}
