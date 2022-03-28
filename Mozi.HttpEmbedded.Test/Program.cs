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
            string pathStatic = "";
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            HttpServer hs = new HttpServer();

            //启用HTTPS 
            //hs.UseHttps().LoadCert(AppDomain.CurrentDomain.BaseDirectory + @"Cert\ServerCert.pfx", "12345678");

            //配置端口并启动服务器 默认80
            hs.SetPort(9090).Start();

            //开启认证
            //hs.UseAuth(AuthorizationType.Basic).SetUser("admin", "admin");

            //设置默认首页 可选 不设置首页为空，打开首页返回默认页面
            hs.SetIndexPage("index.html,index.htm");

            //开启静态文件支持 可选
            hs.UseStaticFiles(pathStatic);
            hs.SetVirtualDirectory("Config", "Config");

            //开启文件压缩 可选 不设置则没有压缩效果
            hs.UseGzip(new Compress.CompressOption() { 
                MinContentLength=1024,
                CompressLevel=2
            });

            //程序集注入
            //1,此方法会扫描程序集内继承自BaseApi或属性标记为[BasicApi]的类
            //2,Http通讯数据标准默认为xml,使用Router.Default.SetDataSerializer(ISerializer ser)更改序列化类型
            //Router.Default.Register("./test.dll");

            //路由映射 可选
            Router router = Router.Default;
            router.Register(Assembly.GetExecutingAssembly());
            router.Map("services/{controller}/{action}");

            //开启WebDAV
            //hs.UseWebDav("{path}");
            ////开启状态服务
            HeartBeatService state = new HeartBeatService()
            {
                Port = 13453,
                RemoteHost = "127.0.0.1"
            };

            state.ApplyDevice("Mozi", "80018001", "1.2.3");
            state.SetState(ClientLifeState.Alive);
            state.SetUserName("StateService");
            state.Init();
            state.Activate();

            HttpClient hc = new HttpClient();
 
            //hc.Get("http://127.0.0.1/index.html", null, new RequestComplete((ctx) =>
            //{
            //    Console.WriteLine(ctx.Response.Body.Length);
            //}));

            //hc.PostFile("http://127.0.0.1/Service/upload", files, new RequestComplete((ctx) => {
            //    Console.WriteLine(ctx.Response.Body.Length);
            //}));
            Console.ReadLine();

            //路径信息

            //    首页
            //      @ip 服务器地址，本机调试使用127.0.0.1
            //      @port 服务器端口 
            //      http://{ip}:{port}
            //    静态文件
            //      @ip 服务器地址，本机调试使用127.0.0.1
            //      @port 服务器端口 
            //      @path 路径名，为静态目录的子级目录名
            //      @filename 文件名
            //      http://{ip}:{port}/{path}/{filename} 
            //    API
            //      @ip 服务器地址，本机调试使用127.0.0.1
            //      @port 服务器端口 
            //      @controller 注册的类名，暂不支持改名
            //      @action 方法名
            //      http://{ip}:{port}/{controller}/{action}

            //内置API 请参看Runtime.cs文件
            
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
