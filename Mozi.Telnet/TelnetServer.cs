using System;
using System.Collections.Generic;

namespace Mozi.Telnet
{

    public delegate void AuthEvent(UserInfo info);

    public class TelnetServer
    {
        private int _port = 23;

        private int _maxLoginAttempts = 3;

        private string _loginMessage = "Mozi.TelnetServer for .Net\r\n Version:1.1.1";
        private string _serverName;
        private SocketServer _sc = new SocketServer();

        //会话合集
        private List<Session> _session = new List<Session>();

        public int MaxLoginAttempts
        {
            get
            {
                return _maxLoginAttempts;
            }
            set
            {
                _maxLoginAttempts = value;
            }
        }

        public string LoginMessage
        {
            get
            {
                return _loginMessage;
            }
            set
            {
                _loginMessage = value;
            }
        }

        public object StartTime { get; private set; }
        public object Timezone { get; private set; }

       
        public event AuthEvent onAuth;

        public TelnetServer()
        {
            StartTime = DateTime.MinValue;
            this.Timezone = String.Format("UTC{0:+00;-00;}:00", System.TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours);
            //配置默认服务器名
            _serverName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "/" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            _sc.OnServerStart += _sc_OnServerStart;
            _sc.OnClientConnect += _sc_OnClientConnect;
            _sc.OnReceiveStart += _sc_OnReceiveStart;
            _sc.AfterReceiveEnd += _sc_AfterReceiveEnd;
            _sc.AfterServerStop += _sc_AfterServerStop;
        }

        private void _sc_AfterServerStop(object sender, ServerArgs args)
        {
            
        }

        private void _sc_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            Negotiate(args);
            Console.WriteLine(BitConverter.ToString(args.Data));
            //string welcome = "";           
        }

        private void _sc_OnReceiveStart(object sender, DataTransferArgs args)
        {
            
        }

        private void _sc_OnClientConnect(object sender, ClientConnectArgs args)
        {
            args.Client.Send(new byte[] { 255, 253, 24 });
        }

        private void _sc_OnServerStart(object sender, ServerArgs args)
        {
            
        }
        /// <summary>
        /// 配置服务端口 
        /// <para>
        /// 在调用<see cref="Start"/>之前设置参数
        /// </para>
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public TelnetServer SetPort(int port)
        {
            _port = port;
            return this;
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start()
        {
            StartTime = DateTime.Now;
            _sc.StartServer(_port);
        }
        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Shutdown()
        {
            _sc.StopServer();
        }

        //protected bool Auth()
        //{

        //}

        public void Negotiate(DataTransferArgs args)
        {

        }
    }
}
