using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Mozi.Telnet
{
    public class NegotiatePack
    {
        public TelnetCommand Head = TelnetCommand.IAC;
        public virtual TelnetCommand Command { get; set; }
        public TelnetSubOptions Option { get; set; }

        public bool IsSub = false;
        public static NegotiatePack[] Parse(byte[] data)
        {

            List<NegotiatePack> packs = new List<NegotiatePack>();

            int indStart = 0;
            do
            {
                NegotiatePack np = null;
                if ((data.Length >= indStart + 3) && data[indStart] == (byte)TelnetCommand.IAC)
                {
                    byte cmd = data[indStart + 1];
                    byte option = data[indStart + 2];
                    //带子选项的协商
                    if (cmd == (byte)TelnetCommand.SB)
                    {
                        np = new NegotiateSubPack();
                        np.IsSub = true;
                        np.Option = (TelnetSubOptions)Enum.Parse(typeof(TelnetSubOptions), option.ToString());
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
                        np.Option = (TelnetSubOptions)Enum.Parse(typeof(TelnetSubOptions), option.ToString());
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

    public struct ClientWindowSize
    {
        public ushort Width;
        public ushort Height;
    }
    //TODO Telnet功能还需要进一步完善，但基本功能已具备
    /// <summary>
    /// Telnet服务端
    /// </summary>
    public class TelnetServer
    {
        private ushort _port = 23;

        private ClientWindowSize _clientSize = new ClientWindowSize() { Width = 300, Height = 200 };
        private string _serverName = "MoziTelnetServer";
        private string _welcomeMessage = "TelnetServer {0} for .Net Platform\r\nDeveloped by MoziCoder workgroup\r\n";
        private string _loginMessage = "Login successed.\r\nPlease enter the command that you want to execute,or  'help' to list commands\r\n";
        private string _username = "Username:", _password = "Password:";

        private readonly SocketServer _sc = new SocketServer();

        private SessionManager _sm = new SessionManager();
        private Authenticator _auth = new Authenticator();
        private List<ITelnetShellCommand> _commands = new List<ITelnetShellCommand>();

        /// <summary>
        /// 指令集合
        /// </summary>
        protected internal List<ITelnetShellCommand> Commands
        {
            get
            {
                return _commands;
            }
            set
            {
                _commands = value;
            }
        }

        /// <summary>
        /// 最大会话数
        /// </summary>
        public int MaxSessions
        {
            get { return _sm.MaxSessions; }
            set { _sm.MaxSessions = value; }
        }
        /// <summary>
        /// 登陆最大尝试次数
        /// </summary>
        public int MaxLoginTryCount
        {
            get { return _sm.MaxLoginTryCount; }
            set { _sm.MaxLoginTryCount = value; }
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
        /// <summary>
        /// 时区
        /// </summary>
        public string Timezone { get; private set; }
        /// <summary>
        /// 服务器运行状态
        /// </summary>
        public bool Running
        {
            get; set;
        }
        /// <summary>
        /// 选项协商事件
        /// </summary>
        public  NegotiateEvent OnNegotiate;
        /// <summary>
        /// 用户鉴权事件
        /// </summary>
        public  AuthEvent OnAuth;
        /// <summary>
        /// 用户认证成功后，发送指令事件
        /// </summary>
        public  CommandReceived OnCommand;
        /// <summary>
        /// 用户认证成功后触发
        /// </summary>
        public  DataEvent OnData;

        public TelnetServer()
        {
            StartTime = DateTime.MinValue;
            this.Timezone = string.Format("UTC{0:+00;-00;}:00", TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours);
            //配置默认服务器名
            _welcomeMessage = string.Format(_welcomeMessage, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            _sc.OnServerStart += _sc_OnServerStart;
            _sc.OnClientConnect += _sc_OnClientConnect;
            _sc.OnReceiveStart += _sc_OnReceiveStart;
            _sc.AfterReceiveEnd += Socket_AfterReceiveEnd;
            _sc.AfterServerStop += _sc_AfterServerStop;
            Help help = new Help(this);
            _commands.Add(help);
        }
        public void AddUser(string username, string password)
        {
            _auth.SetUser(username, password);
        }

        private void _sc_AfterServerStop(object sender, ServerArgs args)
        {

        }

        //TODO 后续数据无法接收，查找原因
        protected virtual void Socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            if (args.Data.Length == 0)
            {
                return;
            }
            Console.WriteLine(BitConverter.ToString(args.Data));
            //协商部分
            if (args.Data[0] == (byte)TelnetCommand.IAC)
            {
                var nps = NegotiatePack.Parse(args.Data);
                foreach (var np in nps)
                {
                    Negotiate(args.Id, args.Socket, np);
                }
            }
            //数据部分
            else
            {
                //CTRL+C 中断连接
                if (args.Data[0] == 0x03)
                {
                    _sm.Remove(args.Id);
                    CloseSocket(args.Socket);
                }
                else
                {
                    var session = _sm.GetSession(args.Id);
                    if (session.User.IsValid == false)
                    {
                        session.InputBuffer += System.Text.Encoding.Default.GetString(args.Data);
                        //\r\n
                        if (args.Data.Length >= 2 && args.Data[0] == 0x0D && args.Data[1] == 0x0A)
                        {
                            //用户名
                            if (string.IsNullOrEmpty(session.User.UserName))
                            {
                                session.User.UserName = session.InputBuffer.Trim();
                                session.ResetInput();
                                Echo(args.Client, "\r\nPassword:");
                            }

                            if (string.IsNullOrEmpty(session.User.Password))
                            {
                                session.User.Password = session.InputBuffer.Trim();
                                session.ResetInput();
                            }

                            if (!string.IsNullOrEmpty(session.User.UserName) && !string.IsNullOrEmpty(session.User.Password))
                            {
                                session.User.TryCount++;

                                //开始验证用户密码
                                if (_auth.Check(session.User.UserName, session.User.Password))
                                {
                                    session.User.LoginTime = DateTime.Now;
                                    session.User.IsValid = true;
                                    Echo(args.Socket, "\r\nLogin successed.\r\nPlease Enter help to list commands supported by server side.\r\n");
                                }
                                else
                                {
                                    if (session.User.TryCount >= _sm.MaxLoginTryCount)
                                    {
                                        Echo(args.Socket, "\r\nTrying too much times.");
                                        CloseSocket(args.Socket);
                                    }
                                    else
                                    {
                                        session.User.Retry();
                                        Echo(args.Socket, "\r\nUserName:");
                                    }
                                }
                                if (OnAuth != null)
                                {
                                    OnAuth(args.Socket, session);
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(session.User.UserName))
                            {
                                Echo(args.Socket, args.Data);
                            }
                            else
                            {
                                Echo(args.Socket, "*");
                            }
                        }
                    }
                    else
                    {
                        Echo(args.Socket, args.Data);
                        session.InputBuffer += System.Text.Encoding.Default.GetString(args.Data);
                        
                        if (args.Data.Length >= 2 && args.Data[0] == 0x0D && args.Data[1] == 0x0A){
                            if (session.InputBuffer.Trim().Length > 0)
                            {
                                Execute(args.Socket, session);
                            }
                            else
                            {
                                session.ResetInput();
                            }
                        }

                        if (OnData != null)
                        {
                            OnData(args.Socket, session, args.Data);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 指令需要阻塞式执行
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="session"></param>
        private void Execute(Socket sc,Session session)
        {
            string[] pattern = session.InputBuffer.Trim().Split(new char[] { ' ' },StringSplitOptions.RemoveEmptyEntries);
            session.ResetInput();
            if (pattern.Length > 0) {

                var cmd = pattern[0];
                var isHelp = cmd.EndsWith("?");
                cmd = cmd.TrimEnd(new char[] { '?', '/' });
                if ( _commands.Exists(x => x.Name.ToLower().Equals(cmd)))
                {

                    string[] arrArgs = new string[pattern.Length - 1];
                    if (arrArgs.Length > 0)
                    {
                        Array.Copy(pattern, 1, arrArgs, 0, arrArgs.Length);
                    }
                    var instance = _commands.Find(x => x.Name.ToLower().Equals(cmd));
                    string message = "";
                    if (!isHelp)
                    {
                        instance.Invoke(ref message, arrArgs);
                    }
                    else
                    {
                        message = instance.Descript();
                    }
                    Echo(sc, message);
                }
                else
                {
                    Echo(sc, "The command is not exists.Please retype the correct words.\r\n");   
                }
                //if (OnCommand != null)
                //{
                //    OnCommand(sc, System.Text.Encoding.Default.GetString(args.Data));
                //}
            }
        }

        private void _sc_OnReceiveStart(object sender, DataTransferArgs args)
        {

        }

        private void _sc_OnClientConnect(object sender, ClientConnectArgs args)
        {

            //List<byte> data = new List<byte>();

            if (!_sm.Full)
            {

                args.Client.Send(new NegotiatePack() { Command = TelnetCommand.WILL, Option = TelnetSubOptions.ECHO }.Pack());
                args.Client.Send(new NegotiatePack() { Command = TelnetCommand.WILL, Option = TelnetSubOptions.SGA }.Pack());
                args.Client.Send(new NegotiatePack() { Command = TelnetCommand.DO, Option = TelnetSubOptions.TERMTYPE }.Pack());
                args.Client.Send(new NegotiatePack() { Command = TelnetCommand.DO, Option = TelnetSubOptions.NAWS }.Pack());

                //发送连接欢迎信息
                args.Client.Send(System.Text.Encoding.Default.GetBytes(_welcomeMessage));
                //发送协商内容

                //args.Client.Send(new NegotiatePack() { Command = TelnetCommand.WILL, Option = Options.LINEMODE }.Pack());
                args.Client.Send(new NegotiatePack() { Command = TelnetCommand.DO, Option = TelnetSubOptions.AUTH }.Pack());
                Echo(args.Client, "\r\nAuthorization needed");
                Echo(args.Client, "\r\nUsername:");
                //发送鉴权要求

                //开启会话管理
                var session = _sm.GetSession(args.Id);
            }
            else
            {
                Echo(args.Client, "Server side session pool is full.Request refused.");
                CloseSocket(args.Client);
            }
        }

        private void _sc_OnServerStart(object sender, ServerArgs args)
        {

        }

        /// <summary>
        /// 配置服务端口 默认端口为23
        /// <para>
        /// 在调用<see cref="Start"/>之前设置参数
        /// </para>
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public TelnetServer SetPort(ushort port)
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
            _sc.Start(_port);
            Running = true;
        }
        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Shutdown()
        {
            Running = false;
            _sc.Shutdown();
        }

        private void Echo(Socket sc, byte[] data)
        {
            if (sc.Connected)
            {
                sc.Send(data);
            }
        }
        private void Echo(Socket sc, string data)
        {
            Echo(sc, System.Text.Encoding.Default.GetBytes(data));
        }
        private void CloseSocket(Socket sc)
        {
            if (sc.Connected)
            {
                sc.Close();
            }
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
        public virtual void Negotiate(string sessionId, Socket so, NegotiatePack np)
        {
            if (!np.IsSub)
            {
                // IAC，SB，24，1，IAC，SE
                if (np.Command == TelnetCommand.WILL)
                {
                    switch (np.Option)
                    {
                        case TelnetSubOptions.TERMTYPE:
                            {
                                NegotiatePack np2 = new NegotiatePack();
                                np2.Command = TelnetCommand.DO;
                                np2.Option = TelnetSubOptions.TERMTYPE;
                                so.Send(np.Pack());

                                NegotiateSubPack nsp = new NegotiateSubPack();
                                nsp.Option = TelnetSubOptions.TERMTYPE;
                                nsp.Parameter = new byte[] { 0x01 };
                                so.Send(nsp.Pack());
                            }
                            break;
                        case TelnetSubOptions.NAWS:
                            {
                                NegotiatePack np2 = new NegotiatePack();
                                np2.Command = TelnetCommand.DO;
                                np2.Option = TelnetSubOptions.NAWS;
                                so.Send(np2.Pack());
                                NegotiateSubPack nsp = new NegotiateSubPack();

                                nsp.Option = TelnetSubOptions.NAWS;
                                List<byte> data = new List<byte>();
                                data.AddRange(BitConverter.GetBytes(_clientSize.Width));
                                data.AddRange(BitConverter.GetBytes(_clientSize.Height));
                                nsp.Parameter = data.ToArray();
                                so.Send(nsp.Pack());
                            }
                            break;
                    }

                } else if (np.Command == TelnetCommand.DO) {

                } else if (np.Command == TelnetCommand.WONT) {

                } else if (np.Command == TelnetCommand.DONT) {

                }

                if (np.Command == TelnetCommand.BRK)
                {
                    CloseSocket(so);
                    _sm.Remove(sessionId);
                }
            }
            else
            {
                if (np.Option == TelnetSubOptions.TERMTYPE)
                {
                    NegotiateSubPack nsp = new NegotiateSubPack();
                    nsp.Option = TelnetSubOptions.TERMTYPE;
                    nsp.Parameter = System.Text.Encoding.Default.GetBytes(_serverName);
                    so.Send(nsp.Pack());
                }
            }

            if (OnNegotiate != null)
            {
                OnNegotiate(so, np);
            }
        }

        //public void AddCommand<ITelnetCommand>()
        //{

        //}
        /// <summary>
        /// 增加命令，命令必须继承自<see cref="ITelnetCommand"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddCommand<T>() where T : ITelnetShellCommand
        {
            var ins = Activator.CreateInstance(typeof(T));
            if (!_commands.Exists(x => x.Name.ToLower() == typeof(T).Name))
            {
                _commands.Add((ITelnetShellCommand)ins);
            }
        }
    }
    public class SessionManager
    {
        private int _maxSessions = 10;
        private int _maxLoginTryCount = 3;

        /// <summary>
        /// 最大会话数
        /// </summary>
        public int MaxSessions
        {
            get { return _maxSessions; }
            set { _maxSessions = value; }
        }
        /// <summary>
        /// 登陆最大尝试次数
        /// </summary>
        public int MaxLoginTryCount
        {
            get { return _maxLoginTryCount; }
            set { _maxLoginTryCount = value; }
        }
        /// <summary>
        /// 会话数
        /// </summary>
        public int Count
        {
            get { return _session.Count; }
        }
        /// <summary>
        /// 会话是否满队
        /// </summary>
        public bool Full
        {
            get { return _session.Count >= MaxSessions; }
        }
        //会话合集
        private List<Session> _session = new List<Session>();

        public SessionManager()
        {

        }

        public Session GetSession(string sessionId)
        {
            if (_session.Exists(x => x.Id == sessionId))
            {
                return _session.Find(x => x.Id == sessionId);
            }
            else
            {
                if (_session.Count < _maxSessions)
                {
                    lock (_session)
                    {
                        var se = new Session()
                        {
                            Id = sessionId,
                            StartTime = DateTime.Now,
                            User=new UserInfo()
                        };
                        _session.Add(se);
                        return se;
                    }
                }
                else
                {
                    throw new Exception("Server side session pool is full.");
                }
            }
        }

        public int Remove(string sessionId)
        {
            return _session.RemoveAll(x => x.Id.Equals(sessionId));
        }
    }
}
