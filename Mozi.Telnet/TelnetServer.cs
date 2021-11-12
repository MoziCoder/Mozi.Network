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
        public static NegotiatePack[] Parse(byte[] data)
        {

            List<NegotiatePack> packs = new List<NegotiatePack>();

            int indStart = 0;
            do
            {
                NegotiatePack np = null;
                if ((data.Length >= indStart + 3)&&data[indStart] == (byte)TelnetCommand.IAC)
                {
                    byte cmd = data[indStart+1];
                    byte option = data[indStart + 2];
                    //带子选项的协商
                    if (cmd == (byte)TelnetCommand.SB)
                    {
                        np = new NegotiateSubPack();
                        np.IsSub = true;
                        np.Option = (Options)Enum.Parse(typeof(Options), option.ToString());
                        int indEnd = Array.IndexOf(data, (byte)TelnetCommand.IAC, indStart + 1);
                        byte[] parameter = new byte[indEnd - 3];
                        Array.Copy(data, 3, parameter, 0, parameter.Length);
                        var np2 = (NegotiateSubPack)np;
                        np2.Parameter = parameter;
                        indStart += indEnd + 2;
                    }
                    else
                    {
                        np = new NegotiatePack();
                        np.Command = (TelnetCommand)Enum.Parse(typeof(TelnetCommand), cmd.ToString());
                        np.Option = (Options)Enum.Parse(typeof(Options), option.ToString());
                        indStart += 3;
                    }
                    packs.Add(np);
                }
                else
                {
                    indStart = -1;
                }
            } while (indStart > 0);

            return packs.ToArray();
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

    public class NegotiateSubPack : NegotiatePack
    {
        public byte[] Parameter { get; set; }
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

            byte[] parameter = Parameter;
            data.AddRange(parameter);
            byte[] dtFoot = new byte[2];
            dtFoot[0] = (byte)Foot;
            dtFoot[1] = (byte)CommandEnd;

            data.AddRange(dtFoot);
            return data.ToArray();
        }
    }

    public delegate void NegotiateEvent(Socket so, NegotiatePack np);
    public delegate void AuthEvent(Socket so, UserInfo info);
    public delegate void SessionStart(Session se);
    public delegate void SessionStop(Session se);
    public delegate void CommandReceived();

    public struct ClientWindowSize
    {
        public ushort Width;
        public ushort Height;
    }

    /// <summary>
    /// Telnet服务端
    /// </summary>
    public class TelnetServer
    {
        private int _port = 23;

        private int _maxLoginAttempts = 3;
        private int _maxSessions = 10;

        private ClientWindowSize _clientSize = new ClientWindowSize() { Width = 300, Height = 200 };

        private string _welcomeMessage = "TelnetServer {0} for .Net Platform\r\nDeveloped by MoziCoder workgroup\r\n";
        private string _loginMessage = "Login successed.\r\nPlease enter the command that you want to execute,or  'help' to list commands\r\n";
        private string _username = "Username:", _password = "Password:";

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

        /// <summary>
        /// 连接时的服务端信息
        /// </summary>
        public string WelcomeMessage
        {
            get { return _welcomeMessage; }
            set { _welcomeMessage = value; }
        }
        /// <summary>
        /// 登陆成功后的显示信息
        /// </summary>
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
        /// <summary>
        /// 客户端窗体大小
        /// </summary>
        public ClientWindowSize ClientSize
        {
            get { return _clientSize; }
            set { _clientSize = value; }
        }
        public DateTime StartTime { get; private set; }
        public String Timezone { get; private set; }

        public event NegotiateEvent OnNegotiate;
        public event AuthEvent OnAuth;

        public TelnetServer()
        {
            StartTime = DateTime.MinValue;
            this.Timezone = String.Format("UTC{0:+00;-00;}:00", TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours);
            //配置默认服务器名
            _welcomeMessage = String.Format(_welcomeMessage, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
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
            //协商部分
            if (args.Data[0] == (byte)TelnetCommand.IAC)
            {
                var nps = NegotiatePack.Parse(args.Data);
                foreach (var np in nps)
                {
                    Negotiate(args.Socket, np);
                }
            }
            //数据部分
            else
            {
                args.Socket.Send(args.Data);
            }
        }

        private void _sc_OnReceiveStart(object sender, DataTransferArgs args)
        {

        }

        private void _sc_OnClientConnect(object sender, ClientConnectArgs args)
        {
            //发送连接欢迎信息
            args.Client.Send(System.Text.Encoding.ASCII.GetBytes(_welcomeMessage));
            //发送协商内容
            args.Client.Send(new NegotiatePack() {  Command = TelnetCommand.DO, Option = Options.TERMTYPE  }.Pack());
            args.Client.Send(new NegotiatePack() { Command = TelnetCommand.DO, Option = Options.ECHO }.Pack());
            args.Client.Send(new NegotiatePack() { Command = TelnetCommand.DO, Option = Options.SGA }.Pack());
            //发送鉴权要求
            //args.Client.Send(new NegotiatePack() { Command = TelnetCommand.DO, Option = Options.AUTH }.Pack());
        }

        private void _sc_OnServerStart(object sender, ServerArgs args)
        {

        }

        private Session GetSession(string sessionId)
        {
            return _session.Find(x => x.Id == sessionId);
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
        /// <summary>
        /// 发 收
        /// WILL DO   发送者想激活某选项，接受者接收该选项请求
        /// WILL DONT 发送者想激活某选项，接受者拒绝该选项请求
        /// DO WILL 发送者希望接收者激活某选项，接受者接受该请求
        /// DO DONT 发送者希望接收6者激活某选项，接受者拒绝该请求
        /// WONT DONT 发送者希望使某选项无效，接受者必须接受该请求
        /// DONT WONT 发送者希望对方使某选项无效，接受者必须接受该请求
        /// </summary>
        /// <param name="so"></param>
        /// <param name="np"></param>
        public virtual void Negotiate(Socket so, NegotiatePack np)
        {
            if (!np.IsSub)
            {
                // IAC，SB，24，1，IAC，SE
                if (np.Command == TelnetCommand.WILL)
                {
                    switch (np.Option)
                    {
                        case Options.TERMTYPE:
                            {
                                NegotiateSubPack nsp = new NegotiateSubPack();          
                                nsp.Option = Options.TERMTYPE;
                                nsp.Parameter = new byte[] { 0x01 };
                                so.Send(nsp.Pack());
                            }
                            break;
                        case Options.NAWS:
                            {
                                NegotiatePack np2 = new NegotiatePack();
                                np2.Command = TelnetCommand.DO;
                                np2.Option = Options.NAWS;
                                so.Send(np2.Pack());
                                NegotiateSubPack nsp = new NegotiateSubPack();
 
                                nsp.Option = Options.NAWS;
                                List<byte> data = new List<byte>();
                                data.AddRange(BitConverter.GetBytes(_clientSize.Width));
                                data.AddRange(BitConverter.GetBytes(_clientSize.Height));
                                nsp.Parameter = data.ToArray();
                                so.Send(nsp.Pack());
                            }
                            break;
                    }

                }else if(np.Command==TelnetCommand.DO){

                }else if (np.Command == TelnetCommand.WONT){

                }else if (np.Command == TelnetCommand.DONT){

                }
            }
            else
            {
                if (np.Option == Options.TERMTYPE)
                {
                    NegotiateSubPack nsp = new NegotiateSubPack();
                    nsp.Option = Options.TERMTYPE;
                    nsp.Parameter = new byte[] { 0x00, (byte)'M', (byte)'Z' };
                    so.Send(nsp.Pack());
                }
            }

            if (OnNegotiate != null)
            {
                OnNegotiate(so, np);
            }
        }
    }
}
