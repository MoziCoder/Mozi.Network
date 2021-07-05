using Mozi.HttpEmbedded.Auth;
using Mozi.HttpEmbedded.Page;
using System;
using System.Threading;
using System.Threading.Tasks;
using Mozi.HttpEmbedded.Common;
using Mozi.StateService;
using System.Reflection;

namespace Mozi.HttpEmbedded.Test
{
    public delegate void TaskExceptionThrowing(object sender, Exception ex);

    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            HttpServer hs = new HttpServer();

            //启用HTTPS 
            //hs.UseHttps().LoadCert(AppDomain.CurrentDomain.BaseDirectory + @"Cert\ServerCert.pfx", "12345678");
            //配置端口并启动服务器
            hs.SetPort(9000).Start();

            //开启认证
            //hs.UseAuth(AuthorizationType.Basic).SetUser("admin", "admin");

            //开启静态文件支持
            hs.UseStaticFiles("");
            hs.SetVirtualDirectory("Config", "Config");
            //开启文件压缩
            hs.UseGzip(new Compress.CompressOption() { 
                MinContentLength=1024,
                CompressLevel=2
            });
            //程序集注入
            //1,此方法会扫描程序集内继承自BaseApi或属性标记为[BasicApi]的类
            //2,Http通讯数据标准默认为xml,使用Router.Default.SetDataSerializer(ISerializer ser)更改序列化类型
            //Router.Default.Register("./test.dll");

            //路由映射
            Router router = Router.Default;
            router.Register(Assembly.GetExecutingAssembly());
            router.Map("services/{controller}/{action}");

            //开启WebDAV
            //hs.UseWebDav("{path}");

            //开启SSDP服务
            SSDP.SSDPService ssdp = new SSDP.SSDPService();
            ssdp.OnNotifyAliveReceived += Ssdp_OnNotifyAliveReceived;
            ssdp.OnSearchReceived += Ssdp_OnSearchReceived;
            ssdp.OnNotifyByebyeReceived += Ssdp_OnNotifyByebyeReceived;
            ssdp.OnNotifyUpdateReceived += Ssdp_OnNotifyUpdateReceived;
            ssdp.OnResponseMessageReceived += Ssdp_OnResponseMessageReceived;
            ssdp.AllowLoopbackMessage = true;
            ssdp.Activate();

            ////开启状态服务
            HeartBeatService state = new HeartBeatService()
            {
                Port = 13453,
                RemoteHost = "100.100.0.111"
            };

            state.ApplyDevice("Mozi", "80018001", "1.2.3");
            state.SetState(ClientLifeState.Alive);
            state.SetUserName("StateService");
            state.Init();
            state.Activate();

            Console.ReadLine();

            //请访问地址 http://{ip}:{port}/admin/index.html

        }

        private static void Ssdp_OnResponseMessageReceived(object sender, HttpResponse resp, string host)
        {
            Console.WriteLine("Response from {0}", host);
        }

        private static void Ssdp_OnNotifyUpdateReceived(object sender, SSDP.UpdatePackage pack, string host)
        {
            Console.WriteLine("Notify update from {0}", host);
        }

        private static void Ssdp_OnNotifyByebyeReceived(object sender, SSDP.ByebyePackage pack, string host)
        {
            Console.WriteLine("Notify byebye from {0}", host);
        }

        private static void Ssdp_OnSearchReceived(object sender, SSDP.SearchPackage pack, string host)
        {
            Console.WriteLine("Search from {0}", host);
            SSDP.SearchResponsePackage sr = new SSDP.SearchResponsePackage();
            var service = (SSDP.SSDPService)sender;
            sr.CacheTimeout = 3600;
            sr.USN = service.USN;
            sr.ST = pack.ST;
            sr.Server = service.Server;
            //service.EchoSearch(sr);
        }

        private static void Ssdp_OnNotifyAliveReceived(object sender, SSDP.AlivePackage pack,string host)
        {
            Console.WriteLine("Notify alive from {0}", host);
        }

        /// <summary>
        /// Task异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            //DONE 对异常进行更详细的记录
            Log.Save("error", "[TASK]" + e.Exception.Message + Environment.NewLine + (e.Exception.StackTrace ?? ""));
        }

        static void Application_ThreadExit(object sender, EventArgs e)
        {
            Log.Save("error", "程序退出");
        }
        /// <summary>
        /// 跨线程调用异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Save("error", e.ExceptionObject.ToString());
        }
        /// <summary>
        /// 主线程未处理异常 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //DONE 对异常进行更详细的记录
            Log.Save("error", e.Exception.Message + Environment.NewLine + (e.Exception.StackTrace ?? ""));
        }
    }
}
