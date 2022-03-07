﻿using Mozi.IoT.Encode;
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
                    var arg0 = args[0];
                    if (arg0 == "-h" || arg0 == "-help")
                    {
                        //TODO 打印帮助信息
                    }
                    else if (arg0.Length > 1)
                    {

                        CoAPPackage cp = new CoAPPackage();
                        cp.MessageType = CoAPMessageType.Confirmable;
                        if (arg0.Equals("get", StringComparison.OrdinalIgnoreCase))
                        {
                            cp.Code = CoAPRequestMethod.Get;
                        }
                        else if (arg0.Equals("post", StringComparison.OrdinalIgnoreCase))
                        {
                            cp.Code = CoAPRequestMethod.Post;
                        }
                        else if (arg0.Equals("put", StringComparison.OrdinalIgnoreCase))
                        {
                            cp.Code = CoAPRequestMethod.Put;
                        }
                        else if (arg0.Equals("delete", StringComparison.OrdinalIgnoreCase))
                        {
                            cp.Code = CoAPRequestMethod.Delete;
                        }
                        else
                        {
                            Console.WriteLine("命令参数不正确，请键入 -h或-help 参数打印帮助信息");
                            return;
                        }

                        var url = args[1];

                        UriInfo uri = UriInfo.Parse(url);
                        cp.SetUri(uri);
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
                                    object argValueReal = null;
                                    //空字符串
                                    if (string.IsNullOrEmpty(argValue))
                                    {

                                    }
                                    //字符串
                                    else if (argValue.StartsWith("\""))
                                    {
                                        argValueReal = argValue.Trim(new char[] { '"' });
                                    //Hex字符串
                                    }else if (argValue.StartsWith("0x")){
                                        argValueReal= Hex.From(argValue.Substring(2));
                                    }
                                    //整数
                                    else
                                    {
                                        uint intValue;
                                        if(uint.TryParse(argValue,out intValue))
                                        {
                                            argValueReal = intValue;
                                        }
                                        else
                                        {
                                            argValueReal = argValue;
                                        }
                                    }
                                    dicArgs.Add(argName.ToLower(), argValueReal);
                                }
                                //包体
                                else
                                {
                                    payload=argName;
                                }
                            }
                            //参数映射
                            foreach (var r in dicArgs)
                            {
                                CoAPOptionDefine optName = CoAPOptionDefine.Unknown;
                                OptionValue optValue = null;
                                if (r.Value == null)
                                {
                                    optValue =new  EmptyOptionValue();
                                }
                                else if(r.Value is string)
                                {
                                    optValue = new StringOptionValue() { Value = r.Value };
                                }else if(r.Value is uint){
                                    optValue = new UnsignedIntegerOptionValue() { Value = r.Value };
                                }else if(r.Value is byte[]){
                                    optValue = new ArrayByteOptionValue() { Value = r.Value };
                                }
                                switch (r.Key)
                                {
                                    case "ifmatch":
                                        {
                                            optName = CoAPOptionDefine.IfMatch;
                                        }
                                        break;
                                    case  "etag":
                                        {
                                            optName = CoAPOptionDefine.ETag;
                                        }
                                        break;
                                    case  "ifnonematch":
                                        {
                                            optName = CoAPOptionDefine.IfNoneMatch;
                                        }
                                        break;
                                    case  "extendedtokenlength":
                                        {
                                            optName = CoAPOptionDefine.ExtendedTokenLength;
                                        }
                                        break;
                                    case  "locationpath":
                                        {
                                            optName = CoAPOptionDefine.LocationQuery;
                                        }
                                        break;
                                    case  "contentformat":
                                        {
                                            optName = CoAPOptionDefine.ContentFormat;
                                        }
                                        break;
                                    case  "maxage":
                                        {
                                            optName = CoAPOptionDefine.MaxAge;
                                        }
                                        break;
                                    case  "accept":
                                        {
                                            optName = CoAPOptionDefine.Accept;
                                        }
                                        break;
                                    case  "locationquery":
                                        {
                                            optName = CoAPOptionDefine.LocationQuery;
                                        }
                                        break;
                                    case  "block2":
                                        {
                                            optName = CoAPOptionDefine.Block2;
                                            optValue = BlockOptionValue.Parse((string)r.Value);
                                        }
                                        break;
                                    case  "block1":
                                        {
                                            optName = CoAPOptionDefine.Block1;
                                            optValue = BlockOptionValue.Parse((string)r.Value);
                                        }
                                        break;
                                    case  "size2":
                                        {
                                            optName = CoAPOptionDefine.Size2;
                                        }
                                        break;
                                    case  "proxyuri":
                                        {
                                            optName = CoAPOptionDefine.ProxyUri;
                                        }
                                        break;
                                    case  "proxyscheme":
                                        {
                                            optName = CoAPOptionDefine.ProxyScheme;
                                        }
                                        break;
                                    case "size1":
                                        {
                                            optName = CoAPOptionDefine.Size1;
                                        }
                                        break;
                                }
  
                                cp.SetOption(optName, optValue);

                                // "If-Match"
                                // "Uri-Host"
                                // "ETag"
                                // "If-None-Match"
                                // "Extended-Token-Length"
                                //"Uri-Port"
                                // "Location-Path"
                                // "Uri-Path"
                                // "Content-Format"
                                // "Max-Age"
                                // "Uri-Query"
                                // "Accept"
                                // "Location-Query"
                                // "Block2"
                                // "Block1"
                                // "Size2",
                                // "Proxy-Uri"
                                // "Proxy-Scheme"
                                // "Size1"

                                // "ifmatch"
                                // "etag"
                                // "ifnonematch"
                                // "extendedtokenlength"
                                // "locationpath"
                                // "contentformat"
                                // "maxage"
                                // "accept"
                                // "locationquery"
                                // "block2"
                                // "block1"
                                // "size2",
                                // "proxyuri"
                                // "proxyscheme"
                                // "size1"
                            }


                            if (!string.IsNullOrEmpty(payload)&& (cp.Code == CoAPRequestMethod.Post || cp.Code == CoAPRequestMethod.Put))
                            {
                                if (payload.StartsWith("\""))
                                {
                                    payload = payload.Trim(new char[] { '"' });
                                    cp.Payload = StringEncoder.Encode(payload);
                                }
                                else if(payload.StartsWith("0x"))
                                {
                                    var pd = Hex.From(payload.Substring(2));
                                    cp.Payload = pd;
                                }
                                else
                                {
                                    var pd = StringEncoder.Encode(payload);
                                    cp.Payload = pd;
                                }
                            }
                            Execute(uri.Host, uri.Port == 0 ? CoAPProtocol.Port : uri.Port, cp);

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

        public static void Execute(string host,int port,CoAPPackage cp)
        {
            CoAPClient cc = new CoAPClient();
            //本地端口
            cc.SetPort(12340);
            cc.Start();
            Cache.MessageCacheManager mc = new Cache.MessageCacheManager(cc);
            if (cp.MesssageId == 0)
            {
                cp.MesssageId = mc.GenerateMessageId();

            }
            //if (cp.Token == null)
            //{
            //    cp.Token = mc.GenerateToken(8);
            //}
            cc.SendMessage(host, port, cp);
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
