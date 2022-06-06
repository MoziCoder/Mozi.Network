using Mozi.Live;
using System;

namespace Mozi.RTSP.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            RTSPServer _server = new RTSPServer();
            //_server.SetPort(554);
            _server.Start();
            Console.ReadLine();
        }
    }
}
