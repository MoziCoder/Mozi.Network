using System;

namespace Mozi.Telnet.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TelnetServer ts = new TelnetServer();
            ts.SetPort(23);
            ts.Start();
            Console.Read();
        }
    }
}
