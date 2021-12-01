using System;

namespace Mozi.NTP.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            NTPServer server = new NTPServer();
            server.Start(123);
            Console.ReadLine();
        }
    }
}
