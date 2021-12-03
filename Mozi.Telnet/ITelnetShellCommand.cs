using System.Diagnostics;

namespace Mozi.Telnet
{
    /// <summary>
    /// Telnet调用命令
    /// </summary>
    public interface ITelnetShellCommand
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
        /// <param name="args">参数为空格分割</param>
        bool Invoke(ref string message, params string[] args);
    }


    /// <summary>
    /// 系统指令调用入口，调用能力取决于于运行环境
    /// 调用命令必须继承<see cref="ITelnetShellCommand"/>
    /// </summary>
    public class Shell : ITelnetShellCommand
    {
        public string Name => this.GetType().Name;

        public string Title => "系统指令调用入口";

        public string Descript()
        {
            return "";
        }
        /// <summary>
        /// 测试用
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool Invoke(ref string message,params string[] args)
        {
            Process p = new Process();
            //设置要启动的应用程序
            p.StartInfo.FileName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.System) + "\\cmd.exe"; ;
            //是否使用操作系统shell启动
            p.StartInfo.UseShellExecute = false; 
            // 接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardInput = true;
            //输出信息
            p.StartInfo.RedirectStandardOutput = true;
            // 输出错误
            p.StartInfo.RedirectStandardError = true;
            //不显示程序窗口
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = "/k";
            //启动程序
            p.Start();
            
            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(string.Join(" ",args));
            p.StandardInput.WriteLine("exit");
            p.StandardInput.AutoFlush = true;
            
             //获取输出信息
            message = p.StandardOutput.ReadToEnd();
            //等待程序执行完退出进程
            p.WaitForExit();
            p.Close();
            return true;   
        }
    }
    /// <summary>
    /// 内置指令
    /// </summary>
    internal class Help : ITelnetShellCommand
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
