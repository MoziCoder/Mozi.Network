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

    //TODO 2020/09/19 增加WebService功能
    //TODO 2020/09/28 增加信号量机制
    //TODO 2021/05/05 实现HTTPS功能
    //TODO 2021/05/05 实现管道机制pipelining 即同一TCP链接允许发起多个HTTP请求 HTTP/1.1
    //TODO 2021/05/07 增加分块传输 chunked
    //TODO 2021/06/21 实现多端口监听
    //TODO 2021/06/21 是否考虑增加中间件功能
    //TODO 2021/11/22 增加禁用缓存的功能 禁止304
    //TODO 2021/11/22 增加流量统计/访问统计功能

    //TODO 2021/11/22 实现简易的API处理能力,OnRequest("{action}/{id}",Func<T,T>{});

    //TODO 2022/02/16 尝试使用ArraySegement来处理数据

    //Transfer-Encoding: chunked 主要是为解决服务端无法预测Content-Length的问题

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

        protected  SocketServer _sc = new SocketServer();

        private WebDav.WebDAVServer _davserver;

        private int _port = 80;
        private int _iporthttps = 443;

        //最大文件大小
        private long _maxFileSize = 10 * 1024 * 1024;
        //最大请求尺寸
        private long _maxRequestSize = 10 * 1024 * 1024;

        //禁止直接IP访问，但应排除本地地址127.0.0.1
        private bool _forbideIPAccess = false;

        /// <summary>
        /// 默认为程序集运行路径的TEMP目录
        /// </summary>
        private string _tempPath = AppDomain.CurrentDomain.BaseDirectory + @"Temp\";
        private string _serverRoot = AppDomain.CurrentDomain.BaseDirectory;

        private string _serverName = "HttpEmbedded";

        //默认首页为index.html,index.htm

        public string[] _indexPages = new string[] {  };

        /// <summary>
        /// 默认首页
        /// </summary>
        public string IndexPages { get { return string.Join(",",_indexPages); } }
        /// <summary>
        /// 允许的方法
        /// </summary>
        private readonly RequestMethod[] MethodAllow = new RequestMethod[] { RequestMethod.OPTIONS, RequestMethod.TRACE, RequestMethod.GET, RequestMethod.HEAD, RequestMethod.POST, RequestMethod.COPY, RequestMethod.PROPFIND, RequestMethod.LOCK, RequestMethod.UNLOCK };
        /// <summary>
        /// 公开的方法
        /// </summary>
        private readonly RequestMethod[] MethodPublic = new RequestMethod[] { RequestMethod.OPTIONS, RequestMethod.GET, RequestMethod.HEAD, RequestMethod.PROPFIND, RequestMethod.PROPPATCH, RequestMethod.MKCOL, RequestMethod.PUT, RequestMethod.DELETE, RequestMethod.COPY, RequestMethod.MOVE, RequestMethod.LOCK, RequestMethod.UNLOCK };

        //证书管理器
        private CertManager _certMg;
        //HTTPS开启标识
        private bool _httpsEnabled = false;

        private MemoryCache _cache = new MemoryCache();

        /// <summary>
        /// 服务器启动时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 服务器HTTP协议版本
        /// </summary>
        public HttpVersion ProtocolVersion { get; set; }
        /// <summary>
        /// 是否使用基本认证
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
        /// 最大请求长度
        /// </summary>
        public long MaxRequestSize { get { return _maxRequestSize; } private set { _maxRequestSize = value; } }
        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port
        {
            get { return _port; }
            private set { _port = value; }
        }
        /// <summary>
        /// HTTPS服务端口
        /// </summary>
        internal int PortHTTPS
        {
            get { return _iporthttps; }
            private set { _iporthttps = value; }
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
        public string ServerName
        {
            get { return _serverName; }
            private set { _serverName = value; }
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
        /// 服务器运行状态
        /// </summary>
        public bool Running
        {
            get; set;
        }

        internal MemoryCache Cache { get { return _cache; }  }

        /// <summary>
        /// 服务端收到完整请求包时触发
        /// </summary>
        public Request Request;

        public HttpServer()
        {
            StartTime = DateTime.MinValue;
            Timezone = string.Format("UTC{0:+00;-00;}:00", TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours);
            //配置默认服务器名
            _serverName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "/" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Auth = new Authenticator();
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
                    if (_httpsEnabled)
                    {
                        //SSL解析数据包
                        ///HelloPackage proto = TLSProtocol.ParseClientHello(args.Data);
                    }
                    context.Request = HttpRequest.Parse(args.Data);
                    context.Request.ClientAddress = args.IP;



                    //TODO HTTP/1.1 通过Connection控制连接 服务器同时对连接进行监测 保证服务器效率
                    //DONE 此处应判断Content-Length然后继续读流
                    //TODO 如何解决文件传输内存占用过大的问题
                    long contentLength = -1;
                    if (context.Request.Headers.Contains(HeaderProperty.ContentLength.PropertyName))
                    {

                        var propContentLength = context.Request.Headers.GetValue(HeaderProperty.ContentLength.PropertyName);
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
                    string doc = DocLoader.Load("Error.html");
                    TemplateEngine pc = new TemplateEngine();
                    pc.LoadFromText(doc);
                    pc.Set("Error", new
                    {
                        Code = StatusCode.InternalServerError.Code.ToString(),
                        Title = StatusCode.InternalServerError.Text,
                        Time = DateTime.Now.ToUniversalTime().ToString("r"),
                        Description = ex.Message,
                        Source = ex.StackTrace ?? ex.StackTrace.ToString(),
                    });
                    pc.Prepare();

                    context.Response.Write(pc.GetBuffer());
                    context.Response.SetContentType(Mime.GetContentType("html"));
                    sc = StatusCode.InternalServerError;
                    Log.Error(ex.Message + ":" + ex.StackTrace ?? "");
                }
                finally
                {

                }
            }
            //最后响应数据     
            if (args.Socket != null && args.Socket.Connected&&context.Request!=null)
            {
                context.Response.AddHeader(HeaderProperty.Server, ServerName);
                context.Response.SetStatus(sc);

                //处理压缩
                var body = context.Response.Body;
                //判断客户机支持的压缩类型
                var acceptEncoding = context.Request.Headers.GetValue(HeaderProperty.AcceptEncoding.PropertyName) ?? "";
                var acceptEncodings = acceptEncoding.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                //忽略对媒体类型的压缩 默认GZIP作为压缩类型
                if (EnableCompress && !Mime.IsMedia(context.Response.ContentType) && acceptEncodings.Contains("gzip"))
                {
                    if (body.Length > ZipOption.MinContentLength)
                    {
                        body = GZip.Compress(body);
                        context.Response.CompressBody(body);
                        context.Response.AddHeader(HeaderProperty.ContentEncoding, "gzip");
                    }
                }
                args.Socket.Send(context.Response.GetBuffer());
                //等待指定的秒数，以发送完剩余数据
                args.Socket.Close(12);
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
            var authorization = context.Request.Headers.GetValue(HeaderProperty.Authorization.PropertyName);
            if (!string.IsNullOrEmpty(authorization) && Auth.Check(authorization))
            {
                context.Request.IsAuthorized = true;
                return HandleRequest(ref context);
            }
            else
            {
                //发送验证要求
                context.Response.AddHeader(HeaderProperty.WWWAuthenticate, string.Format("{0} realm=\"{1}\"", Auth.AuthType.Name, AuthorizationType.REALM));
                return StatusCode.Unauthorized;
            }
        }
        //TODO 2020/09/18 考虑增加断点续传的功能
        //TODO 2020/09/18 增加缓存功能
        //DONE 2020/09/19 增加默认页面功能
        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="context"></param>
        private StatusCode HandleRequest(ref HttpContext context)
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
                context.Response.SetContentType(contenttype);

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
                            string ifmodifiedsince = context.Request.Headers.GetValue(HeaderProperty.IfModifiedSince.PropertyName);
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
                        pc.Set("Info", new{ VersionName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),Copyright="&copy;2020-2022"});
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
                    //if (pathReal.EndsWith("/"))
                    //{

                    //}
                    //响应静态文件
                    if (st.Exists(pathReal, ""))
                    {
                        string ifmodifiedsince = context.Request.Headers.GetValue(HeaderProperty.IfModifiedSince.PropertyName);
                        if (st.CheckIfModified(pathReal, ifmodifiedsince))
                        {
                            DateTime dtModified = st.GetLastModifiedTime(pathReal).ToUniversalTime();
                            context.Response.AddHeader(HeaderProperty.LastModified, dtModified.ToString("r"));
                            context.Response.Write(st.Load(pathReal, ""));                            
                            
                            //ETag 仅测试 不具备判断缓存的能力
                            context.Response.AddHeader(HeaderProperty.ETag, string.Format("{0:x2}:{1:x2}", dtModified.ToUniversalTime().Ticks, context.Response.ContentLength));
                        }
                        else
                        {
                            return StatusCode.NotModified;
                        }
                    }
                    else
                    {
                        //50X 返回错误信息页面
                        string doc = DocLoader.Load("Error.html");
                        TemplateEngine pc = new TemplateEngine();
                        pc.LoadFromText(doc);
                        pc.Set("Error", new
                        {
                            Code = StatusCode.NotFound.Code.ToString(),
                            Title = StatusCode.NotFound.Text,
                            Time = DateTime.Now.ToUniversalTime().ToString("r"),
                            Description = "未找到指定的资源",
                            Source = "路径信息："+path,
                        });
                        pc.Prepare();
                        context.Response.Write(pc.GetBuffer());
                        context.Response.SetContentType(Mime.GetContentType("html"));
                        return StatusCode.NotFound;
                    }
                }
                else
                {
                    //动态页面默认ContentType为txt/plain
                    context.Response.SetContentType(Mime.GetContentType("txt"));
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
            Router router = Router.Default;
            if (router.Match(context.Request.Path) != null)
            {
                //判断返回结果
                object result = null;
                result = router.Invoke(context);
                if (result != null)
                {
                    context.Response.Write(result.ToString());
                }
                return StatusCode.Success;
            }
            return StatusCode.NotFound;
        }

        //TODO 静态文件统一处理
        private StatusCode HandleRequestStaticFile(ref HttpContext context)
        {
            //全量发送

            //部分发送
            return StatusCode.Success;
        }
        /// <summary>
        /// 处理METHOD-OPTIONS请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleRequestOptions(ref HttpContext context)
        {
            foreach (RequestMethod verb in MethodAllow)
                context.Response.AddHeader("Allow", verb.Name);
            foreach (RequestMethod verb in MethodPublic)
                context.Response.AddHeader("Public", verb.Name);
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
                return _davserver.ProcessRequest(ref context);
            }
            return StatusCode.Forbidden;
            //RequestMethod.PROPFIND,RequestMethod.PROPPATCH RequestMethod.MKCOL RequestMethod.COPY RequestMethod.MOVE RequestMethod.LOCK RequestMethod.UNLOCK
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
            _port = port;
            return this;
        }
        /// <summary>
        /// 启用认证
        /// <para>此方法可连续配置用户</para>
        /// </summary>
        /// <param name="at">访问认证类型<see cref="E:Auth.AuthType"/></param>
        /// <returns></returns>
        public HttpServer UseAuth(AuthorizationType at)
        {
            EnableAuth = true;
            Auth.SetAuthType(at);
            return this;
        }
        /// <summary>
        /// 设置服务器认证用户
        /// <para>如果<see cref="F:EnableAuth"/>=<see cref="bool.False"/>,此设置就没有意义</para>
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
            _httpsEnabled = true;
            return _certMg;
        }
        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start()
        {
            StartTime = DateTime.Now;
            _sc.Start(_port);
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
        private bool CheckIfAccessBlocked(string ipAddress)
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
            _maxRequestSize = size;
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
        /// <param name="filePath"></param>
        public void SetIndexPage(string pattern)
        {
            _indexPages = pattern.Split(new char[] { ',' });
        }
    }

    public delegate void Request(string srcHost, int srcPort, HttpRequest request);
}
