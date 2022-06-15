using Mozi.Live;
using System;

namespace Mozi.RTSP.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            RTSPServer _server = new RTSPServer();
            //_server.Request = (host,port,request) =>
            //{
            //    Console.WriteLine("==**************{0}*************==", host);
            //    Console.WriteLine("{0}", System.Text.Encoding.UTF8.GetString(request.GetBuffer()));
            //    Console.WriteLine("==***************************************==");
            //};
            _server.RequestHandled = (host,port,context) =>
            {
                Console.WriteLine("{0}", System.Text.Encoding.UTF8.GetString(context.Request.GetBuffer()));
                Console.WriteLine("");
                Console.WriteLine("{0}", System.Text.Encoding.UTF8.GetString(context.Response.GetBuffer()));
                Console.WriteLine("");
            };
            //_server.SetPort(554);
            _server.Start();
            Console.ReadLine();
        }
    }
}
