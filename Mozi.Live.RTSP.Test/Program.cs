using Mozi.HttpEmbedded;
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
                Console.WriteLine($"{context.Request.RequestLineString}=>{context.Response.StatusLineString}");

                HttpClient hc = new HttpClient();
                hc.SetAuthorization(new HttpEmbedded.Auth.BasicAuth());
                hc.SetUser("admin", "admin");
                hc.Post("http://100.100.0.171:2343/log",System.Text.Encoding.UTF8.GetString(context.Request.GetBuffer()));
                //Console.WriteLine("{0}", );
                //Console.WriteLine("");
                hc.Post("http://100.100.0.171:2343/log", System.Text.Encoding.UTF8.GetString(context.Response.GetBuffer()));
                //Console.WriteLine("");
            };
            //_server.SetPort(554);
            _server.Start();
            Console.ReadLine();
        }
    }
}
