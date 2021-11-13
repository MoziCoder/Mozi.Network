using System;

namespace Mozi.Telnet.Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            TelnetServer ts = new TelnetServer();
            ts.SetPort(23).Start();
            Console.ReadLine();
        }
    }
}
