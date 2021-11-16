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
            ts.AddUser("admin", "admin");
            ts.AddCommand<Shell>();
            ts.SetPort(23).Start();
            Console.ReadLine();
        }
    }
}
