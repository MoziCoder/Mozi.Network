using Mozi.HttpEmbedded;
using Mozi.HttpEmbedded.Encode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Mozi.Http.Client
{
    class Program
    {

        private static bool responsed = false;

        private static bool sendrequest = false;

        private static bool observeMode = false;

        private static string _filePathUpload = "";
        private static bool _needDump = false;
        private static string _filePathDump = "";
        private static int _round = -1;

        private static string _url = "";

        //用信号量取代Action->BeginInvoke，适应.NetCore
        static SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

        static void Main(string[] args)
        {
            ParseRequest(args);
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 处理参数
        /// </summary>
        /// <param name="args"></param>
        static void ParseRequest(IList<string> args)
        {

            //Console.WriteLine(String.Join("|", args));
            var dicArgs = new List<KeyValuePair<string, object>>();
            if (args != null && args.Count > 0)
            {
                if (args.Count > 0)
                {
                    var arg0 = args[0];
                    if (arg0 == "-h" || arg0 == "-help")
                    {
                        //TODO 打印帮助信息
                        PrintHelp();
                    }
                    else if (args.Count > 1)
                    {

                        int observeSeconds = -1;

                        HttpRequest cp = new HttpRequest();
                        cp.SetMethod(RequestMethod.GET);

                        RequestMethod method = RequestMethod.Get<RequestMethod>(arg0);
                        if (Equals(method, null))
                        {
                             method = new RequestMethod(arg0.ToUpper());
                        }
                        else
                        {
                       
                        }
                        cp.SetMethod(method);
                        _url = args[1];

                        int i;

                        string payload = "";

                        if (args.Count >= 3)
                        {
                            for (i = 2; i < args.Count; i++)
                            {
                                var argName = args[i];
                                var argValue = "";

                                //参数
                                if (argName.StartsWith("-"))
                                {

                                    argName = argName.Substring(1);
                                    if (args.Count > i + 1)
                                    {
                                        argValue = args[i + 1];
                                    }
                                    if (argValue.StartsWith("-"))
                                    {
                                        argValue = "";
                                    }
                                    else
                                    {
                                        i++;
                                    }
                                    object argValueReal = null;
                                    //空字符串
                                    if (string.IsNullOrEmpty(argValue))
                                    {

                                        //Hex字符串
                                    }
                                    else if (argValue.StartsWith("0x"))
                                    {
                                        argValueReal = Hex.From(argValue.Substring(2));
                                    }
                                    //整数
                                    else
                                    {
                                        uint intValue;
                                        if (uint.TryParse(argValue, out intValue))
                                        {
                                            argValueReal = intValue;
                                        }
                                        //字符串
                                        else
                                        {
                                            argValueReal = argValue;
                                        }
                                    }
                                    dicArgs.Add(new KeyValuePair<string, object>(argName.ToLower(), argValueReal));
                                }
                                //包体
                                else
                                {
                                    payload = argName;
                                }
                            }
                            //参数头属性
                            foreach (var r in dicArgs)
                            {
                                HeaderProperty optName = HeaderProperty.Get<HeaderProperty>(r.Key);
                                object optValue = r.Value;
                                bool isOption = true;
                                switch (r.Key)
                                {
                                    case "time":
                                        {
                                            isOption = false;
                                            observeSeconds = int.Parse(r.Value.ToString());
                                            if (observeSeconds > 0)
                                            {
                                                observeMode = true;
                                            }
                                            continue;
                                        }
                                    case "file":
                                        {
                                            _filePathUpload = r.Value.ToString();
                                            continue;
                                        }
                                    case "dump":
                                        {
                                            _needDump = true;
                                            _filePathDump = r.Value.ToString();
                                            continue;
                                        }
                                    case "round":
                                        {
                                            int.TryParse(r.Value.ToString(), out _round);
                                            continue;
                                        }
                                }
                                if (isOption)
                                {
                                    if (optValue != null)
                                    {
                                            cp.AddHeader(optName.PropertyName, optValue != null ? optValue.ToString() : "");
                                    }
                                }
                                cp.SetBody(Mozi.HttpEmbedded.Encode.StringEncoder.Encode(payload));
                            }

                        }
                        if (!_needDump)
                        {
                            Execute(observeSeconds, cp, _url);
                        }
                        else
                        {
                            FileStream fs = new FileStream(_filePathDump, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                            StreamWriter sw = new StreamWriter(fs);
                            sw.Write(BitConverter.ToString(cp.GetBuffer()));
                            sw.Flush();
                            sw.Close();
                            fs.Close();
                            Console.WriteLine($"Hex bytes dumped to \"{_filePathDump}\"");
                        }
                    }
                    else
                    {
                        Console.WriteLine("命令参数不正确，请键入 -h或-help 参数打印帮助信息");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("命令参数过少，请键入 -h或-help 参数打印帮助信息");
                    return;
                }
            }
        }

        private static void Execute(int observeSeconds, HttpRequest cp, string url)
    {
        try
        {
            int waitSeconds = 30;
            if (observeSeconds > 0)
            {
                waitSeconds = observeSeconds;
            }

            SendRequest(url,cp);
            semaphore.Wait(waitSeconds * 1000);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
        }
    }

    public static void SendRequest(string url,HttpRequest cp)
    {
        sendrequest = true;
        HttpClient hc = new HttpClient();
        //本地端口
        if (_filePathUpload != "")
        {
            if (cp.Method == RequestMethod.PUT || cp.Method == RequestMethod.POST)
            {

                FileCollection fc = new FileCollection();
                fc.Add(new HttpEmbedded.File() { FileTempSavePath = _filePathUpload });
                hc.PostFile(_url, fc, null);

            }
            else
            {

            }
        }
        else
        {
            int loop = 1;
            int responseCount = 0;
            if (_round > 0)
            {
                loop = _round;
            }

            for (int i = 0; i < loop; i++)
            {
                hc.Send(url, cp,(x, ctx) => {
                    responseCount++;
                    responsed = true;
                    //Console.ForegroundColor = ConsoleColor.DarkGreen;
                    //Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(StringEncoder.Decode(ctx.Request.GetBuffer()));
                    Console.WriteLine("");
                    Console.WriteLine(StringEncoder.Decode(ctx.Response.GetBuffer()));
                    if (!observeMode || (responseCount >= loop))
                    {
                        semaphore.Release();
                    }
                });
                Thread.Sleep(100);
                //TODO 此处设置时间间隔
            }
        }
    }

        private static void Close()
    {
        if (sendrequest && !responsed && !observeMode)
        {
            Console.WriteLine("超时时间已到，尚未收到服务端响应\r\n");
        }
        //使用信号量代替后，此句无用
        //Environment.Exit(0);
    }

        public static void PrintHelp()
        {
            string helpText = "\r\n" +
                            "用法：http command url [headers] [body]" +
                            "\r\n   " +
                            "\r\ncommand 可选值：" +
                            "\r\n  get" +
                            "\r\n  post" +
                            "\r\n  put" +
                            "\r\n  delete" +
                            "\r\n\r\nurl 格式" +
                            "\r\n  http://{host}[:{port}]/{path}[?{query}]" +
                            "\r\n\r headers 请求选项参数如下：" +
                            "\r\n" +
                            "\r\n 注：" +
                            "\r\n    1.字符串变量值用\"\"包裹" +
                            "\r\n    2.整型变量值用，直接输入整数即可，如 -size 1024" +
                            "\r\n" +
                            "\r\nbody 说明：" +
                            "\r\n   1.0x开始的字符串被识别为HEX字符串并被转为字节流" +
                            "\r\n   2.其它识别为普通字符串同时被编码成字节流，编码方式为UTF-8" +
                            "\r\n   3.带空格的字符串请用\"\"进行包裹" +
                            "\r\n示例：" +
                            "\r\n   httpclient get http://127.0.0.1/runtime/gettime?type=1 " +
                            "\r\n";
            Console.Write(helpText);
        }
    }
}
