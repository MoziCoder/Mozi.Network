using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Mozi.Telnet
{
    public class NegotiatePack
    {
        public TelnetCommand Head = TelnetCommand.IAC;
        public virtual TelnetCommand Command { get; set; }
        public Options Option { get; set; }

        public bool IsSub = false;
        public static NegotiatePack Parse(byte[] data)
        {
            NegotiatePack np = null;

            if (data[0] == (byte)TelnetCommand.IAC)
            {
                byte cmd = data[1];
                byte option = data[2];
                //带子选项的协商
                if (cmd == (byte)TelnetCommand.SB)
                {
                    np = new NegotiateSubPack();
                    np.IsSub = true;
                    np.Option = (Options)Enum.Parse(typeof(Options), option.ToString());
                    int indEnd = -1;

                    indEnd = Array.IndexOf(data, (byte)TelnetCommand.IAC, 1);
                    byte[] parameter = new byte[indEnd - 3];
                    Array.Copy(data, 3, parameter, 0, parameter.Length);
                    var np2 = (NegotiateSubPack)np;
                    np2.Parameter = System.Text.ASCIIEncoding.ASCII.GetString(parameter);
                }
                else
                {
                    np = new NegotiatePack();
                    np.Command = (TelnetCommand)Enum.Parse(typeof(TelnetCommand), cmd.ToString());
                    np.Option = (Options)Enum.Parse(typeof(Options), option.ToString());
                }
            }

            return np;
        }
        public virtual byte[] Pack()
        {
            byte[] data = new byte[3];
            data[0] = (byte)Head;
            data[1] = (byte)Command;
            data[2] = (byte)Option;
            return data;
        }
    }

    public class NegotiateSubPack:NegotiatePack
    {
        public string Parameter { get; set; }
        public TelnetCommand Foot = TelnetCommand.IAC;
        public TelnetCommand CommandEnd = TelnetCommand.SE;

        public override byte[] Pack()
        {
            List<byte> data = new List<byte>();

            byte[] dtHead = new byte[3];
            dtHead[0] = (byte)Head;
            dtHead[1] = (byte)TelnetCommand.SB;
            dtHead[2] = (byte)Option;

            data.AddRange(dtHead);

            byte[] parameter = System.Text.Encoding.ASCII.GetBytes(Parameter);
            data.AddRange(parameter);
            byte[] dtFoot = new byte[3];
            dtFoot[0] = (byte)Foot;
            dtFoot[1] = (byte)CommandEnd;

            data.AddRange(dtFoot);
            return data.ToArray();
        }
    }
    public delegate void NegotiateEvent(Socket so,NegotiatePack np);
    public delegate void AuthEvent(Socket so, UserInfo info);

    public class TelnetServer
    {
        private int _port = 23;

        private int _maxLoginAttempts = 3;

        private string _loginMessage = "Mozi.TelnetServer for .Net\r\n Version:1.1.1";
        private string _terminalType = "";

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

        public event NegotiateEvent OnNegotiate;
        public event AuthEvent OnAuth;

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
            Console.WriteLine(BitConverter.ToString(args.Data));
            //string welcome = "";
            //协商部分
            if (args.Data[0] == (byte)TelnetCommand.IAC)
            {
                NegotiatePack np=NegotiatePack.Parse(args.Data);
                Negotiate(args.Socket,np);
            }
            //数据部分
            else
            {

            }
        }

        private void _sc_OnReceiveStart(object sender, DataTransferArgs args)
        {
            
        }

        private void _sc_OnClientConnect(object sender, ClientConnectArgs args)
        {
            args.Client.Send(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.DO, (byte)Options.TERMTYPE });
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

        public void Negotiate(Socket so,NegotiatePack np)
        {
            if (!np.IsSub)
            {

            }
            else
            {

            }

            if (OnNegotiate != null)
            {
                OnNegotiate(so, np);
            }
        }
}
