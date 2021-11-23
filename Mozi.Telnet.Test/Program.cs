using System;

namespace Mozi.Telnet.Test
{
    /// <summary>
    /// Telnet调用范例
    /// </summary>
    static class Program
    {
        static void Main(string[] args)
        {
            TelnetServer ts = new TelnetServer();
            //设置用户
            ts.AddUser("admin", "admin");
            //指令注册
            ts.AddCommand<Shell>();
            //配置端口及启动服务
            ts.SetPort(23).Start();
            short a=0b10000000;
            Console.WriteLine(a << 1);
            Console.ReadLine();
        }
    }
}
