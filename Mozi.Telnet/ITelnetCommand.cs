namespace Mozi.Telnet
{
    public interface ITelnetCommand
    {
        /// <summary>
        /// 命令名
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 指令标题
        /// </summary>
        string Title { get; }
        /// <summary>
        /// 命令描述
        /// </summary>
        /// <returns></returns>
        string Descript();
        /// <summary>
        /// 命令调用过程
        /// </summary>
        /// <param name="args"></param>
        bool Invoke(ref string message, params string[] args);
    }



    public class Shell : ITelnetCommand
    {
        public string Name => this.GetType().Name;

        public string Title => "系统指令调用入口";

        public string Descript()
        {
            return "";
        }

        public bool Invoke(ref string message,params string[] args)
        {
            return false;   
        }
    }
    /// <summary>
    /// 内置指令
    /// </summary>
    internal class Help : ITelnetCommand
    {
        private TelnetServer _ts;

        public Help(TelnetServer ts)
        {
            _ts = ts;
        }

        public string Name => this.GetType().Name;

        public string Title => "帮助";

        public string Descript()
        {
            return $"{Title}\r\n列出所有命令\r\n";
        }

        public bool Invoke(ref string message,params string[] args)
        {
            message = "";
            foreach(var r in _ts.Commands)
            {
                message += r.Name.ToLower().PadRight(20);
                message += r.Title + "\r\n";
            }      
            return true;
        }
    }
}
