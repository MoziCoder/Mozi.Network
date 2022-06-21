using System;
using System.Linq;
using System.Net.Sockets;
using Mozi.HttpEmbedded.Auth;
using Mozi.HttpEmbedded.Cache;
using Mozi.HttpEmbedded.Cert;
using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Compress;
using Mozi.HttpEmbedded.Document;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Page;
using Mozi.HttpEmbedded.Source;
using Mozi.HttpEmbedded.Template;

namespace Mozi.HttpEmbedded
{

    //DONE 2020/09/18 考虑增加断点续传的功能
    //TODO 2020/09/18 增加缓存功能
    //DONE 2020/09/19 增加默认页面功能
    //TODO 2020/09/19 增加WebService功能
    //TODO 2020/09/28 增加信号量机制
    //TODO 2021/05/05 实现HTTPS功能
    //TODO 2021/05/05 实现管道机制pipelining 即同一TCP链接允许发起多个HTTP请求 HTTP/1.1
    //DONE 2021/05/07 增加分块传输 chunked
    //TODO 2021/06/21 实现多端口监听
    //TODO 2021/06/21 是否考虑增加中间件功能
    //TODO 2021/11/22 增加禁用缓存的功能 禁止304
    //TODO 2021/11/22 增加流量统计/访问统计功能

    //TODO 2021/11/22 实现简易的API处理能力,OnRequest("{action}/{id}",Func<T,T>{});

    //TODO 2022/02/16 尝试使用ArraySegement来处理数据
    //TODO 2022/05/31 进一步丰富服务器事件

    //Transfer-Encoding: chunked 主要是为解决服务端无法预测Content-Length的问题

    //TODO 已实现断点续传
    /*断点续传*/
    //client->  
    //    HTTP GET /document.ext
    //    Range: bytes 0-1024, 1025-2048, 2049- 
    //server->
    //    HTTP/1.1 206 206 Partial Content| HTTP/1.1 Range Not Satisfiable
    //    Content-Range:bytes 0-1024/4048
    /**/

    /// <summary>
    /// Http服务器
    /// </summary>
    public class HttpServer
    {
        /// <summary>
        /// Socket对象
        /// </summary>
        protected  SocketServer _sc = new SocketServer();

        private WebDav.WebDAVServer _davserver;

        private  int _port = 80;

        private int _portTLS = 443;

        //最大文件大小
        private long _maxFileSize = 10 * 1024 * 1024;
        //最大请求尺寸
        private long _maxRequestBodySize = 10 * 1024 * 1024;

        //禁止直接IP访问，但应排除本地地址127.0.0.1
        private bool _forbideIPAccess = false;

        /// <summary>
        /// 默认为程序集运行路径的TEMP目录
        /// </summary>
        private string _tempPath = AppDomain.CurrentDomain.BaseDirectory + @"Temp\";
        private string _serverRoot = AppDomain.CurrentDomain.BaseDirectory;

        private string _serverName = "Mozi.HttpEmbedded";

        //默认首页为index.html,index.htm
        private string[] _indexPages = new string[] {  };

        /// <summary>
        /// 默认首页
        /// </summary>
        public string IndexPages { get { return string.Join(",",_indexPages); } }
        /// <summary>
        /// 允许的方法
        /// </summary>
        protected RequestMethod[] MethodAllow = new RequestMethod[] { RequestMethod.OPTIONS, RequestMethod.TRACE, RequestMethod.GET, RequestMethod.HEAD, RequestMethod.POST, RequestMethod.COPY, RequestMethod.PROPFIND, RequestMethod.LOCK, RequestMethod.UNLOCK };
        /// <summary>
        /// 公开的方法
        /// </summary>
        protected  RequestMethod[] MethodPublic = new RequestMethod[] { RequestMethod.OPTIONS, RequestMethod.GET, RequestMethod.HEAD, RequestMethod.PROPFIND, RequestMethod.PROPPATCH, RequestMethod.MKCOL, RequestMethod.PUT, RequestMethod.DELETE, RequestMethod.COPY, RequestMethod.MOVE, RequestMethod.LOCK, RequestMethod.UNLOCK };

        //证书管理器
        private CertManager _certMg;
        /// <summary>
        /// TLS开启标识
        /// </summary>
        protected bool _tlsEnabled = false;

        private MemoryCache _cache = new MemoryCache();

        /// <summary>
        /// 服务器启动时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 服务器协议版本
        /// </summary>
        public ProtocolVersion Version { get; set; }
        /// <summary>
        /// 是否使用访问认证
        /// </summary>
        public bool EnableAuth { get; private set; }
        /// <summary>
        /// 认证器
        /// </summary>
        public Authenticator Auth { get; private set; }
        /// <summary>
        /// 是否启用访问控制 IP策略
        /// </summary>
        public bool EnableAccessControl { get; private set; }
        /// <summary>
        /// 是否开启压缩 默认为GZip
        /// </summary>
        public bool EnableCompress { get; private set; }
        /// <summary>
        /// 压缩选项
        /// </summary>
        public CompressOption ZipOption { get; private set; }
        /// <summary>
        /// 最大接收文件大小 默认10Mb
        /// </summary>
        public long MaxFileSize { get { return _maxFileSize; } private set { _maxFileSize = value; } }
        /// <summary>
        /// 最大请求体长度
        /// </summary>
        public long MaxRequestBodySize { get { return _maxRequestBodySize; } private set { _maxRequestBodySize = value; } }
        /// <summary>
        /// 服务端口
        /// </summary>
        public virtual int Port
        {
            get { return _port; }
            protected set { _port = value; }
        }
        /// <summary>
        /// HTTPS服务端口
        /// </summary>
        internal int PortHTTPS
        {
            get { return _portTLS; }
            private set { _portTLS = value; }
        }
        /// <summary>
        /// 时区
        /// </summary>
        public string Timezone { get; set; }
        /// <summary>
        /// 编码格式
        /// </summary>
        public string Encoding { get; set; }
        /// <summary>
        /// 是否启用WebDav
        /// </summary>
        public bool EnableWebDav { get; private set; }
        /// <summary>
        /// 服务器名称
        /// </summary>
        public virtual string ServerName
        {
            get { return _serverName; }
            protected set { _serverName = value; }
        }
        /// <summary>
        /// 临时文件目录
        /// </summary>
        public string TempPath
        {
            get { return _tempPath; }
            private set { _tempPath = value; }
        }
        /// <summary>
        /// 服务程序集运行根目录
        /// </summary>
        public string ServerRoot
        {
            get { return _serverRoot; }
            private set { _serverRoot = value; }
        }
        /// <summary>
        /// 累计接收的请求次数
        /// </summary>
        public ulong TotalReceiveCount { get; set; }
        /// <summary>
        /// 累计发送的响应次数
        /// </summary>
        public ulong TotalSendCount { get; set; }
        /// <summary>
        /// 累计计收的字节数
        /// </summary>
        public ulong TotalReceivedBytes { get; set; }
        //TODO 此处还没有实现
        /// <summary>
        /// 累计发送的字节数
        /// </summary>
        internal ulong TotalSendBytes { get; set; }
        /// <summary>
        /// 服务器运行状态
        /// </summary>
        public bool Running
        {
            get; set;
        }
        internal MemoryCache Cache { get { return _cache; }  }
        /// <summary>
        /// 服务端收到完整请求包时触发,此处不应作任何数据包的修改处理
        /// </summary>
        public Request Request;
        /// <summary>
        /// 服务端响应时触发
        /// </summary>
        public Response Response;
        /// <summary>
        /// 完成响应后触发
        /// </summary>
        public RequestHandled RequestHandled;
        /// <summary>
        /// 默认路由管理器
        /// </summary>
        public Router Router = Router.Default;
        /// <summary>
        /// 
        /// </summary>
        public HttpServer()
        {
            StartTime = DateTime.MinValue;
            Version = ProtocolVersion.Version11;
            Timezone = string.Format("UTC{0:+00;-00;}:00", TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours);
            //配置默认服务器名
            _serverName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "/" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            _sc.OnServerStart += Socket_OnServerStart;
            _sc.OnClientConnect += Socket_OnClientConnect;
            _sc.OnReceiveStart += Socket_OnReceiveStart;
            _sc.AfterReceiveEnd += Socket_AfterReceiveEnd;
            _sc.AfterServerStop += Socket_AfterServerStop;
        }

        /// <summary>
        /// 服务器关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void Socket_AfterServerStop(object sender, ServerArgs args)
        {

        }
        protected virtual void Socket_OnServerStart(object sender, ServerArgs args)
        {
            //throw new NotImplementedException();
        }
        protected virtual void Socket_OnClientConnect(object sender, ClientConnectArgs args)
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 服务器启动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void Socket_OnReceiveStart(object sender, DataTransferArgs args)
        {

        }
        //TODO 响应码处理有问题

        //该方法为受保护类型，如果想实现更自由的实现，可以覆写该方法，但不建议这么做。此处处理比较复杂

        /// <summary>
        /// 解析请求包，响应请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void Socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            if (args.Data.Length==0)
            {
                return;
            }
            
            HttpContext context = new HttpContext();
            context.ClientAddress = args.IP;
            context.ClientPort = args.Port;
            context.Response = new HttpResponse();
            context.Server = this;
            StatusCode sc = StatusCode.Success;
            //如果启用了访问IP黑名单控制
            if (EnableAccessControl && CheckIfAccessBlocked(args.IP))
            {
                sc = StatusCode.Forbidden;
            }
            else
            {
                try
                {
                    //HTTPS 协议处理
                    if (_tlsEnabled)
                    {
                        // SSL解析数据包
                        // HelloPackage proto = TLSProtocol.ParseClientHello(args.Data);
                    }
                    context.Request = HttpRequest.Parse(args.Data);
                    context.Request.ClientAddress = args.IP;


                    //TODO HTTP/1.1 通过Connection控制连接 服务器同时对连接进行监测 保证服务器效率
                    //DONE 此处应判断Content-Length然后继续读流
                    //TODO 如何解决文件传输内存占用过大的问题
                    long contentLength = -1;
                    if (context.Request.Headers.Contains(HeaderProperty.ContentLength))
                    {

                        var propContentLength = context.Request.Headers.GetValue(HeaderProperty.ContentLength);
                        contentLength = int.Parse(propContentLength);

                    }
                    if (contentLength == -1 || contentLength <= context.Request.Body.Length)
                    {
                       
                    }
                    else
                    {
                        //TODO 此处是否会形成死循环
                        //继续读流
                        //TODO 网络数据包在传输时往往会受MTU/MSS值影响而分成多片断传输，故此处要继续读流,直到读取到指定的流长度
                        args.Socket.BeginReceive(args.State.Buffer, 0, args.State.Buffer.Length, SocketFlags.None, _sc.CallbackReceived, args.State);

                        //_sc.ProcessReceive(args.State);
                        return;
                    }

                    //当服务端接收到请求时触发
                    if (Request != null)
                    {
                        Request(args.IP, args.Port, context.Request);
                    }

                    if (!EnableAuth)
                    {
                        sc = HandleRequest(ref context);
                    }
                    else
                    {
                        sc = HandleAuth(ref context);
                    }
                }
                catch (Exception ex)
                {
                    //50X 返回错误信息页面
                    sc = HandleServerError(context, ex);
                }
                finally
                {

                }
            }
            //最后发送响应数据     
            if (args.Socket != null && args.Socket.Connected&&context.Request!=null)
            {
                
                //注入服务器名称
                context.Response.AddHeader(HeaderProperty.Server, ServerName);
                context.Response.SetStatus(sc);

                //处理压缩
                var body = context.Response.Body;
                //判断客户机支持的压缩类型
                var acceptEncoding = context.Request.Headers.GetValue(HeaderProperty.AcceptEncoding) ?? "";
                var acceptEncodings = acceptEncoding.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

                //忽略对媒体类型的压缩 默认GZIP作为压缩类型
                if (EnableCompress && !Mime.IsMedia(context.Response.ContentType) && acceptEncodings.Contains("gzip"))
                {
                    if (body.Length > ZipOption.MinContentLength)
                    {
                        body = GZip.Compress(body);
                        context.Response.WriteEncodeBody(body);
                        context.Response.AddHeader(HeaderProperty.ContentEncoding, "gzip");
                    }
                }

                var chunked = false;
                //Transfer-Encoding:chunked
                if (context.Response.Headers.Contains(HeaderProperty.TransferEncoding))
                {
                    var tranferEncoding=context.Response.Headers[HeaderProperty.TransferEncoding.PropertyName];
                    if (!string.IsNullOrEmpty(tranferEncoding) && tranferEncoding == "chunked")
                    {
                        context.Response.DontAddAutoHeader = true;
                        context.Response.AddHeader(HeaderProperty.Date, DateTime.Now.ToUniversalTime().ToString("r"));
                        context.Response.AddHeader(HeaderProperty.ContentType, context.Response.ContentType + (!string.IsNullOrEmpty(context.Response.Charset) ? "; " + context.Response.Charset : ""));
                    }
                    chunked = true;
                }
                else
                { 
                    
                }

                //统计数据
                TotalReceiveCount++;
                TotalSendCount++;

                //发送数据
                Send(args.Socket, context.Response.GetBuffer());

                //事件回调
                if (Response != null)
                {
                    Response(args.IP, args.Port, context.Response);
                }
                if (RequestHandled != null)
                {
                    RequestHandled(args.IP,args.Port,context);
                }
                if (!chunked)
                {
                    //等待指定的秒数，以发送完剩余数据
                    args.Socket.Close(12);
                }

            }
            GC.Collect();
        }

        /// <summary>
        /// 处理认证
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleAuth(ref HttpContext context)
        {
            var authorization = context.Request.Headers.GetValue(HeaderProperty.Authorization);
            if (!string.IsNullOrEmpty(authorization) && Auth.Check(authorization,context.Request.Method.Name))
            {
                context.Request.IsAuthorized = true;
                return HandleRequest(ref context);
            }
            else
            {
                //发送验证要求
                context.Response.AddHeader(HeaderProperty.WWWAuthenticate,Auth.GetChallenge());
                return StatusCode.Unauthorized;
            }
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="context"></param>
        protected virtual StatusCode HandleRequest(ref HttpContext context)
        {
            RequestMethod method = context.Request.Method;
            if (method == RequestMethod.OPTIONS)
            {
                return HandleRequestOptions(ref context);
            }
            if (method == RequestMethod.GET || method == RequestMethod.POST || method == RequestMethod.HEAD || method == RequestMethod.PUT || method == RequestMethod.DELETE || method == RequestMethod.TRACE || method == RequestMethod.CONNECT)
            {
                StaticFiles st = StaticFiles.Default;
                var path = context.Request.Path;

                //URL解码
                path = UrlEncoder.Decode(path);

                string fileext = GetPathResourceExt(path);
                string contenttype = Mime.GetContentType(fileext);
                //判断资源类型
                //TODO 此处应特殊处理某些类型的文件，比如.asp|.aspx|.jsp
                bool isStatic = st.IsStatic(fileext);
                //TODO 仅静态文件才会设置内容格式

                if (isStatic)
                {
                    context.Response.SetContentType(contenttype);
                }

                var pathReal = path;
                if (pathReal == "/")
                {
                    var existsIndex = false;
                    foreach (var r in _indexPages)
                    {
                        pathReal = path + r;
                        if (st.Exists(pathReal, ""))
                        {
                            existsIndex = true;
                            string ifmodifiedsince = context.Request.Headers.GetValue(HeaderProperty.IfModifiedSince);
                            if (st.CheckIfModified(pathReal, ifmodifiedsince))
                            {
                                DateTime dtModified = st.GetLastModifiedTime(pathReal).ToUniversalTime();
                                context.Response.AddHeader(HeaderProperty.LastModified, dtModified.ToString("r"));
                                context.Response.Write(st.Load(pathReal, ""));

                                //ETag 仅测试 不具备判断缓存的能力
                                context.Response.AddHeader(HeaderProperty.ETag, CacheControl.GenerateETag(dtModified.ToUniversalTime(), context.Response.ContentLength));
                                context.Response.SetContentType(Mime.GetContentType("html"));

                                return StatusCode.Success;
                            }
                            else
                            {
                                return StatusCode.NotModified;
                            }
                        }
                    }
                    if (!existsIndex)
                    {
                        //TODO 加载指定首页
                        //优先加载
                        var doc = DocLoader.Load("Home.html");
                        TemplateEngine pc = new TemplateEngine();
                        pc.LoadFromText(doc);
                        pc.Set("Info", new{ Author="Jason",VersionName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),Copyright= "&copy;2020-2022 MoziCoder" });
                        pc.Prepare();

                        context.Response.Write(pc.GetBuffer());
                        context.Response.SetContentType(Mime.GetContentType("html"));
                        return StatusCode.Success;
                    }
                }
                //静态文件处理
                else if (st.Enabled && isStatic)
                {
                    //目录加载

                    //响应静态文件
                    if (st.Exists(pathReal, ""))
                    {
                        return HandleRequestStaticFile(ref context, st, pathReal,contenttype);
                    }
                    else
                    {
                        //50X 返回错误信息页面
                        return HandleClientFound(context, path);
                    }
                }
                else
                {
                    //响应动态页面
                    return HandleRequestRoutePages(ref context);
                }
            }
            //WEBDAV部分
            else
            {
                return HandleRequestWebDAV(ref context);
            }
            return StatusCode.Success;
        }

        /// <summary>
        /// 路由API和页面
        /// </summary>
        /// <param name="context"></param>
        private StatusCode HandleRequestRoutePages(ref HttpContext context)
        { 
            if (Router.Match(context.Request.Path) != null)
            {
                //判断返回结果
                object result = null;
                result = Router.Invoke(context);
                if (result != null)
                {
                    context.Response.Write(result.ToString());
                }
                //动态页面默认ContentType为txt/plain
                if (string.IsNullOrEmpty(context.Response.ContentType))
                {
                    context.Response.SetContentType(Mime.GetContentType("txt"));
                }
                return StatusCode.Success;
            }
            return HandleClientFound(context, context.Request.Path);
        }

        //TODO 静态文件统一处理
        private StatusCode HandleRequestStaticFile(ref HttpContext context, StaticFiles st, string pathReal,string contentType)
        {
            string ifmodifiedsince = context.Request.Headers.GetValue(HeaderProperty.IfModifiedSince);
            string range = context.Request.Headers.GetValue(HeaderProperty.Range);
            context.Response.AddHeader(HeaderProperty.AcceptRanges, "bytes");

            if (st.CheckIfModified(pathReal, ifmodifiedsince))
            {
                //是否范围请求
                if (string.IsNullOrEmpty(range))
                {
                    DateTime dtModified = st.GetLastModifiedTime(pathReal).ToUniversalTime();
                    context.Response.AddHeader(HeaderProperty.LastModified, dtModified.ToString("r"));
                    context.Response.Write(st.Load(pathReal, ""));
                    //ETag 仅测试 不具备判断缓存的能力
                    context.Response.AddHeader(HeaderProperty.ETag, string.Format("{0:x2}:{1:x2}", dtModified.ToUniversalTime().Ticks, context.Response.ContentLength));
                    return StatusCode.Success;
                }
                else
                {
                    //TODO 断点续传功能还需要进一步测试

                    //Range: bytes=0-1024,2048-4096,4097-
                    string[] info = range.Split(new char[] { (char)ASCIICode.EQUAL }, StringSplitOptions.RemoveEmptyEntries);
                    if (info.Length > 1)
                    {
                        //bytes
                        string unit = info[0];
                        string[] ranges = info[1].Split(new char[] { (char)ASCIICode.COMMA },StringSplitOptions.RemoveEmptyEntries);

                        ContentRange[] arrRanges = new ContentRange[ranges.Length];

                        for(int i = 0; i < arrRanges.Length; i++)
                        {
                            //这里用-1表示范围未赋值
                            arrRanges[i]= new ContentRange() { Start = -1, End = -1 };
                            var r = arrRanges[i];
                            string[] rinfo = ranges[0].Trim().Split(new char[] { (char)ASCIICode.MINUS }, StringSplitOptions.RemoveEmptyEntries);
                            string sStart = "", sEnd = "";
                            if (ranges[0].StartsWith(((char)ASCIICode.MINUS).ToString()))
                            {
                                sEnd = rinfo[0];
                            }
                            else
                            {
                                sStart = rinfo[0];
                                sEnd = rinfo.Length > 1 ? rinfo[1] : "";
                            }

                            if (!string.IsNullOrEmpty(sStart))
                            {
                                r.Start=uint.Parse(sStart);
                            }

                            if (!string.IsNullOrEmpty(sEnd))
                            {
                                r.End = uint.Parse(sEnd);
                            }
                        }
                        long fileSize = st.GetFileSize(pathReal);
                        if (arrRanges.Length == 0)
                        {
                            return StatusCode.RangeNotSatisfiable;
                        }
                        //单个范围值 
                        else if (arrRanges.Length == 1)
                        {
                            ContentRange r = arrRanges[0];
                            if ((r.Start == -1 && r.End == -1) || (r.Start > r.End))
                            {
                                return StatusCode.RangeNotSatisfiable;
                            }
                            else
                            {
                                long start, end;
                                if (r.Start == -1)
                                {
                                    start = fileSize - r.End;
                                    end = fileSize - 1;
                                }else if (r.End == -1){
                                    start = r.Start;
                                    end = fileSize - 1;
                                }
                                else
                                {
                                    start = r.Start;
                                    end = r.End;
                                }
                                long totalSize = 0;
                                context.Response.Write(st.Load(pathReal, "", start, end, ref totalSize));
                                context.Response.AddHeader(HeaderProperty.ContentRange, string.Format("bytes {0}-{1}/{2}",start,end,totalSize));
                            }
                        }
                        //多个范围值
                        else
                        {
                            string boundary = CacheControl.GenerateRandom(8);
                            context.Response.SetContentType("multipart/byteranges; boundary="+boundary);
                            foreach(var r in arrRanges)
                            {
                                if ((r.Start == -1 && r.End == -1)||(r.Start>r.End))
                                {
                                    return StatusCode.RangeNotSatisfiable;
                                }
                                else
                                {
                                    long start, end;
                                    if (r.Start == -1)
                                    {
                                        start = fileSize - r.End;
                                        end = fileSize - 1;
                                    }
                                    else if (r.End == -1)
                                    {
                                        start = r.Start;
                                        end = fileSize - 1;
                                    }
                                    else
                                    {
                                        start = r.Start;
                                        end = r.End;
                                    }
                                    long totalSize = 0;
                                    context.Response.Write("--" + boundary);
                                    TransformHeader header = new TransformHeader();
                                    header.Add(HeaderProperty.ContentType,contentType);
                                    header.Add(HeaderProperty.ContentRange, string.Format("bytes {0}-{1}/{2}", start, end, totalSize));
                                    context.Response.Write(st.Load(pathReal, "", start, end, ref totalSize));
                                    context.Response.AddHeader(HeaderProperty.ContentRange, string.Format("bytes {0}-{1}/{2}", start, end, totalSize));
                                }
                            }
                            context.Response.Write("--" + boundary + "--");
                        }
                        return StatusCode.PartialContent;
                    }
                    else
                    {
                        return StatusCode.RangeNotSatisfiable;
                    }
                }
            }
            else
            {
                return StatusCode.NotModified;
            }
        }
        /// <summary>
        /// 处理OPTIONS请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestOptions(ref HttpContext context)
        {
            context.Response.AddHeader(HeaderProperty.Allow, MethodAllow.Select(x=>x.Name).ToArray());
            // Sends 200 OK
            return StatusCode.Success;
        }
        /// <summary>
        /// 处理WebDAV请求
        /// </summary>
        private StatusCode HandleRequestWebDAV(ref HttpContext context)
        {
            RequestMethod method = context.Request.Method;
            if (EnableWebDav)
            {
                return _davserver.HandleRequest(ref context);
            }
            return StatusCode.Forbidden;
            //RequestMethod.PROPFIND,RequestMethod.PROPPATCH RequestMethod.MKCOL RequestMethod.COPY RequestMethod.MOVE RequestMethod.LOCK RequestMethod.UNLOCK
        }
        /// <summary>
        /// 处理服务器异常50x请求
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private StatusCode HandleServerError(HttpContext context, Exception ex)
        {
            StatusCode sc;
            string doc = DocLoader.Load("Error.html");
            TemplateEngine pc = new TemplateEngine();
            pc.LoadFromText(doc);
            pc.Set("Error", new HandleError
            {
                Code = StatusCode.InternalServerError.Code.ToString(),
                Title = StatusCode.InternalServerError.Text,
                Time = DateTime.Now.ToUniversalTime().ToString("r"),
                Description = ex.Message,
                Source = ex.StackTrace ?? ex.StackTrace.ToString(),
                ServerName = _serverName
            }); ;
            pc.Prepare();

            context.Response.Write(pc.GetBuffer());
            context.Response.SetContentType(Mime.GetContentType("html"));
            sc = StatusCode.InternalServerError;
            Log.Error(ex.Message + ":" + ex.StackTrace ?? "");
            return sc;
        }
        /// <summary>
        /// 处理40x请求
        /// </summary>
        /// <param name="context"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private StatusCode HandleClientFound(HttpContext context, string path)
        {
            string doc = DocLoader.Load("Error.html");
            TemplateEngine pc = new TemplateEngine();
            pc.LoadFromText(doc);
            pc.Set("Error", new HandleError
            {
                Code = StatusCode.NotFound.Code.ToString(),
                Title = StatusCode.NotFound.Text,
                Time = DateTime.Now.ToUniversalTime().ToString("r"),
                Description = "未找到指定的资源",
                Source = "路径信息：" + path,
                ServerName = _serverName
            });
            pc.Prepare();
            context.Response.Write(pc.GetBuffer());
            context.Response.SetContentType(Mime.GetContentType("html"));
            return StatusCode.NotFound;
        }
        /// <summary>
        /// 取URL资源扩展名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetPathResourceExt(string path)
        {
            string[] file = path.Split(new[] { (char)ASCIICode.QUESTION }, StringSplitOptions.RemoveEmptyEntries);
            string ext = "";
            string purepath = file[0];
            if (purepath.LastIndexOf((char)ASCIICode.DOT) >= 0)
            {
                ext = purepath.Substring(purepath.LastIndexOf((char)ASCIICode.DOT) + 1);
            }
            return ext;
        }
        /// <summary>
        /// 配置服务端口 
        /// <para>
        /// 在调用<see cref="Start"/>之前设置参数
        /// </para>
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public HttpServer SetPort(int port)
        {
            Port = port;
            return this;
        }
        /// <summary>
        /// 启用认证
        /// <para>此方法可连续配置用户</para>
        /// </summary>
        /// <param name="at">访问认证类型</param>
        /// <returns></returns>
        public HttpServer UseAuth(AuthorizationType at)
        {
            if (at != AuthorizationType.None)
            {
                EnableAuth = true;
                if (Auth == null)
                {
                    Auth = new Authenticator();
                }
                Auth.SetAuthType(at);
            }
            else
            {
                EnableAuth = false;
                Auth = null;
            }
            return this;
        }
        /// <summary>
        /// 设置服务器认证用户
        /// <para>如果<see cref="F:EnableAuth"/>=<see cref="Boolean.False"/>,此设置就没有意义</para>
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userPassword"></param>
        /// <returns></returns>
        public HttpServer SetUser(string userName, string userPassword)
        {
            Auth.SetUser(userName, userPassword);
            return this;
        }
        //DONE 进一步实现GZIP的控制逻辑
        /// <summary>
        /// 启用Gzip
        /// </summary>
        /// <param name="option">此处设置CompressType无效，默认会被设置为<see cref="E:ContentEncoding.Gzip"/>。CompressLevel设置也无效</param>
        /// <returns></returns>
        public HttpServer UseGzip(CompressOption option)
        {
            EnableCompress = true;
            ZipOption = option;
            ZipOption.CompressType = ContentEncoding.Gzip;
            return this;
        }
        /// <summary>
        /// 设置静态文件根目录
        /// <para>设置后静态文件目录会变为HTTP的根目录，可以直接访问其下的子路径</para>
        /// </summary>
        /// <param name="root">静态文件根目录</param>
        /// <returns></returns>
        public HttpServer UseStaticFiles(string root)
        {
            StaticFiles.Default.Enabled = true;
            StaticFiles.Default.SetRoot(root);
            return this;
        }
        /// <summary>
        /// 配置虚拟目录
        /// </summary>
        /// <param name="name">访问名</param>
        /// <param name="path">真实相对路径</param>
        /// <returns></returns>
        public HttpServer SetVirtualDirectory(string name, string path)
        {
            if (StaticFiles.Default.Enabled)
            {
                StaticFiles.Default.SetVirtualDirectory(name, path);
            }
            return this;
        }
        /// <summary>
        /// 启用WebDav
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public HttpServer UseWebDav(string root)
        {
            EnableWebDav = true;
            //DONE WEBDAV服务初始化
            if (_davserver == null)
            {
                _davserver = new WebDav.WebDAVServer();
                _davserver.SetStore(root);
            }
            return this;
        }
        //TODO 实现一个反向代理服务
        /// <summary>
        /// 实现代理
        /// </summary>
        /// <returns></returns>
        internal HttpServer UseProxy()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        internal HttpServer UseErrorPage(string page)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置临时文件目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal HttpServer UseTempPath(string path)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置服务器名称
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        public HttpServer SetServerName(string serverName)
        {
            _serverName = serverName;
            return this;
        }
        //TODO HTTPS 功能尚未完全实现
        /// <summary>
        /// HTTPS功能未实现，请查看后期版本规划
        /// </summary>
        /// <returns></returns>
        internal CertManager UseHttps()
        {
            _certMg = new CertManager();
            _tlsEnabled = true;
            return _certMg;
        }
        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start()
        {
            StartTime = DateTime.Now;
            _sc.Start(Port);
            Running = true;
        }
        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Shutdown()
        {
            Running = false;
            _sc.Shutdown();
        }
        /// <summary>
        /// 是否启用访问控制 IP策略
        /// </summary>
        /// <param name="enabled"></param>
        public void UseAccessControl(bool enabled)
        {
            EnableAccessControl = enabled;
        }
        //DONE 实现访问黑名单 基于IP控制策略
        /// <summary>
        /// 检查访问黑名单
        /// </summary>
        /// <param name="ipAddress"></param>
        protected bool CheckIfAccessBlocked(string ipAddress)
        {
            return AccessManager.Instance.CheckBlackList(ipAddress);
        }
        //TODO 此处未实现控制
        /// <summary>
        /// 设置最大接收文件大小
        /// </summary>
        /// <param name="fileSize"></param>
        public void SetMaxFileSize(long fileSize)
        {
            _maxFileSize = fileSize;
        }
        //TODO 此处未实现控制
        /// <summary>
        /// 设置最大请求大小
        /// </summary>
        /// <param name="size"></param>
        public void SetMaxRequestSize(long size)
        {
            _maxRequestBodySize = size;
        }
        /// <summary>
        /// 设置临时文件目录
        /// </summary>
        /// <param name="path"></param>
        public void SetTempPath(string path)
        {
            _tempPath = path;
        }
        /// <summary>
        /// 设置首页 
        /// <para>设置默认首页后会关闭默认页面的返回,多个页面用","分割，优先访问前面的地址</para>
        /// </summary>
        /// <param name="pattern"></param>
        public void SetIndexPage(string pattern)
        {
            _indexPages = pattern.Split(new char[] { ',' });
        }
        /// <summary>
        /// 向会话接收方发送数据
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="data"></param>
        internal void Send(Socket peer,byte[] data)
        {
            peer.Send(data);
            TotalSendBytes++;
        }
        /// <summary>
        /// 发送Chunked数据
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="data"></param>
        internal void SendChunkedData(Socket peer,byte[] data)
        {
            if (peer.Connected)
            {
                UnsignedIntegerOptionValue ui = new UnsignedIntegerOptionValue
                {
                    Value = data.Length
                };
                string len=Hex.To(ui.Pack);
                peer.Send(StringEncoder.Encode(len));
                peer.Send(new byte[] { ASCIICode.CR, ASCIICode.LF });
                peer.Send(data);
                peer.Send(new byte[] { ASCIICode.CR, ASCIICode.LF });
            }
        }
        /// <summary>
        /// Chunked结束符号
        /// </summary>
        /// <param name="peer"></param>
        internal void SendChunkedEndData(ref Socket peer)
        {
            if (peer.Connected)
            {
                string end = "0\r\n\r\n";
                peer.Send(StringEncoder.Encode(end));
            }
        }
        /// <summary>
        /// 注册简易处理方法，默认为仅响应GET请求。方法原型为<see cref="RegisterHandler(string, RequestMethod, ApiHandler)"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        public void RegisterHandler(string name,ApiHandler handler)
        {
            Router.Get(name, handler);
        }
        /// <summary>
        /// 注册API,请注意跨线程问题
        /// </summary>
        /// <param name="name">方法名</param>
        /// <param name="method">HTTP请求类型</param>
        /// <param name="handler">委托</param>
        /// <remarks>这是一个快速注册API的方法，无需实现<see cref="BaseApi"/></remarks>
        public void RegisterHandler(string name,RequestMethod method,ApiHandler handler)
        {
            Router.RegisterGlobalMethod(name, method, handler);
        }
        /// <summary>
        /// 移除API
        /// </summary>
        /// <param name="name"></param>
        public void RemoveHandler(string name)
        {
            Router.UnRegisterGlobalMethod(name);
        }
    }
    /// <summary>
    /// 请求接收委托
    /// </summary>
    /// <param name="srcHost"></param>
    /// <param name="srcPort"></param>
    /// <param name="request"></param>
    public delegate void Request(string srcHost, int srcPort, HttpRequest request);
    /// <summary>
    /// 响应发送委托
    /// </summary>
    /// <param name="dstHost"></param>
    /// <param name="dstPort"></param>
    /// <param name="response"></param>
    public delegate void Response(string dstHost, int dstPort, HttpResponse response);
    /// <summary>
    /// 请求响应完成时触发
    /// </summary>
    /// <param name="dstHost"></param>
    /// <param name="dstPort"></param>
    /// <param name="context"></param>
    public delegate void RequestHandled(string dstHost, int dstPort, HttpContext context);
}
