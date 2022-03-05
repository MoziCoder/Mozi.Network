using System;
using System.Collections.Generic;

namespace Mozi.IoT.CoAP
{
    //请求示例
    // Get coap://127.0.0.1/core/time?timezone=+8 COAP/1
    // Message-Type: Confirmable
    // Message-ID: 123456
    // Token: 01 02 03 04 05 06 07 08
    // ContentFormat: text/plain
    // Block1: 1/0/128

    //响应示例

    // COAP/1 205 Content
    // Message-Type: Confirmable
    // Message-ID: 123456
    // Token: 01 02 03 04 05 06 07 08



    //public static CoAPOptionDefine IfMatch = new CoAPOptionDefine("If-Match", 1);
    //public static CoAPOptionDefine UriHost = new CoAPOptionDefine("Uri-Host", 3);
    //public static CoAPOptionDefine ETag = new CoAPOptionDefine("ETag", 4);
    //public static CoAPOptionDefine IfNoneMatch = new CoAPOptionDefine("If-None-Match", 5);
    //public static CoAPOptionDefine ExtendedTokenLength = new CoAPOptionDefine("Extended-Token-Length", 6);

    //public static CoAPOptionDefine UriPort = new CoAPOptionDefine("Uri-Port", 7);
    //public static CoAPOptionDefine LocationPath = new CoAPOptionDefine("Location-Path", 8);
    //public static CoAPOptionDefine UriPath = new CoAPOptionDefine("Uri-Path", 11);
    //public static CoAPOptionDefine ContentFormat = new CoAPOptionDefine("Content-Format", 12);
    //public static CoAPOptionDefine MaxAge = new CoAPOptionDefine("Max-Age", 14);
    //public static CoAPOptionDefine UriQuery = new CoAPOptionDefine("Uri-Query", 15);
    //public static CoAPOptionDefine Accept = new CoAPOptionDefine("Accept", 17);
    //public static CoAPOptionDefine LocationQuery = new CoAPOptionDefine("Location-Query", 20);

    //public static CoAPOptionDefine Block2 = new CoAPOptionDefine("Block2", 23);    //RFC 7959
    //public static CoAPOptionDefine Block1 = new CoAPOptionDefine("Block1", 27);    //RFC 7959

    //public static CoAPOptionDefine Size2 = new CoAPOptionDefine("Size2", 28); //RFC 7959

    //public static CoAPOptionDefine ProxyUri = new CoAPOptionDefine("Proxy-Uri", 35);
    //public static CoAPOptionDefine ProxyScheme = new CoAPOptionDefine("Proxy-Scheme", 39);

    //public static CoAPOptionDefine Size1 = new CoAPOptionDefine("Size1", 60);

    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ParseRequest(args);
        }

        /// <summary>
        /// 处理参数
        /// </summary>
        /// <param name="args"></param>
        static void ParseRequest(IList<string> args)
        {
            //Console.WriteLine(String.Join("|", args));
            var dicArgs = new Dictionary<string, object>();
            if (args != null && args.Count > 0)
            {
                if (args.Count > 0)
                {

                }
                else
                {

                }

                for (var i = 0; i < args.Count; i++)
                {
                    var argName = args[i];
                    var argValue = "";
                    if (argName.StartsWith("-"))
                    {
                        if (args.Count > i + 1)
                        {
                            argValue = args[i + 1];
                        }
                        if (argValue.StartsWith("-"))
                        {
                            argValue = "";
                        }
                        dicArgs.Add(argName.ToUpper(), argValue);
                    }
                }
            }
        }
        public void Execute()
        {
            CoAPClient cc = new CoAPClient();
            //本地端口
            cc.SetPort(12340);
            cc.Start();
            cc.Get("coap://100.100.0.105/sensor/getinfo", CoAPMessageType.Confirmable);
        }
        /// <summary>
        /// 执行并阻塞一定的时间
        /// </summary>
        /// <param name="action"></param>
        /// <param name="milliseconds">毫秒数</param>
        public static void ExecuteAndWait(Action action, int milliseconds)
        {
            IAsyncResult result = action.BeginInvoke(null, null);
            try
            {
                if (result.AsyncWaitHandle.WaitOne(milliseconds))
                {
                    action.EndInvoke(result);
                }
                else
                {
                    throw new TimeoutException("超时时间已到，但没有获取到程序的执行结果");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                result.AsyncWaitHandle.Close();
            }
        }
    }
}
