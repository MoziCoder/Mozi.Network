using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Mozi.HttpEmbedded.Compress;
using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 请求完成时回调
    /// </summary>
    /// <param name="url">请求的URL地址</param>
    /// <param name="context"></param>
    public delegate void RequestComplete(string url,HttpContext context);
    /// <summary>
    /// 发起请求时回调
    /// </summary>
    /// <param name="url"></param>
    /// <param name="host"></param>
    /// <param name="req"></param>
    public delegate void RequestSend(string url, string host, HttpRequest req);
    /// <summary>
    /// 接收到响应时回调
    /// </summary>
    /// <param name="url"></param>
    /// <param name="host"></param>
    /// <param name="resp"></param>
    public delegate void ResponseReceive(string url, string host, HttpResponse resp);
    /// <summary>
    /// 请求异常时触发
    /// </summary>
    /// <param name="url"></param>
    /// <param name="host"></param>
    /// <param name="req"></param>
    /// <param name="ex"></param>
    public delegate void RequestException(string url, string host, HttpRequest req,Exception ex);
    //DONE http客户端，因http客户端实现比较多，暂时不实现，待后期规划
    //TODO 应同步实现Https HttpQUIC(http3.0)
    //TODO 应加入TLS安全传输
    //TODO 对chunked包进行响应

    /// <summary>
    /// Http客户端
    /// </summary>
    public class HttpClient
    {
        /// <summary>
        /// 编码类型
        /// </summary>
        public string Charset = "UTF-8";
        private string _userAgent = "Mozilla/5.0 (Linux;Android 4.4.2;OEM Device) AppleWebKit/537.36 (KHTML,like Gecko) Chrome/39.0.2171.71  Mozi/1.4.7";
        private const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

        private Auth.User _user;

        private Auth.AuthScheme _authScheme;

        //public Dictionary<HeaderProperty, string> DefaultHeader = new Dictionary<HeaderProperty, string>() 
        //{
        //    { HeaderProperty.UserAgent,"Mozilla/5.0 (Linux;Android 4.4.2;OEM Device) AppleWebKit/537.36 (KHTML,like Gecko) Chrome/39.0.2171.71  Mozi/1.3.8" },
        //    { HeaderProperty.Accept,"text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
        //    { HeaderProperty.AcceptEncoding,"gzip, deflate" }
        //};

        /// <summary>
        /// 请求发生异常时触发
        /// </summary>
        public RequestException RequestException;
        /// <summary>
        /// 接收到响应时触发回调
        /// </summary>
        public RequestComplete ResponseCompleted;
        /// <summary>
        /// 请求发起时触发
        /// </summary>
        public RequestSend RequestSend;
        /// <summary>
        /// 接收到响应时触发
        /// </summary>
        public ResponseReceive ResponseReceive;
        /// <summary>
        /// 数据压缩选项
        /// </summary>
        public ContentEncoding ContentEncoding = ContentEncoding.None;
        /// <summary>
        /// 是否自动解码被gzip|deflate压缩过的响应内容
        /// </summary>
        public bool AutoDecodeBody = false;
        /// <summary>
        /// 用户代理
        /// </summary>
        public string UserAgent { get => _userAgent; set => _userAgent = value; }

        /// <summary>
        /// 连接超时时间
        /// </summary>
        public int ConnectTimeout = 30;
        /// <summary>
        /// 响应超时时间
        /// </summary>
        internal int ReceiveTimeout = 30;

        //TODO 此处应考虑Gzip解码
        /// <summary>
        /// 发送HTTP请求,原型为<see cref="Send(string, HttpRequest, RequestComplete)"/>
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="method">请求方法</param>
        /// <param name="headers">
        /// 附加的头信息，内部会封装一部分头信息，其它头信息请自行附加。
        /// <list type="table">
        ///     <listheader>内部封装的头信息：</listheader>
        ///         <item><see cref="HeaderProperty.UserAgent"/></item>
        ///         <item><see cref="HeaderProperty.AcceptEncoding"/></item>
        ///         <item><see cref="HeaderProperty.Accept"/></item>
        ///         <item><see cref="HeaderProperty.Host"/></item>
        ///         <item><see cref="HeaderProperty.Referer"/></item>
        ///         <item><see cref="HeaderProperty.ContentLength"/></item>
        ///     </list>
        /// </param>
        /// <param name="body">请求包体</param>
        /// <param name="callback">回调方法</param>
        public void Send(string url, RequestMethod method,Dictionary<string,string> headers,byte[] body,RequestComplete callback)
        {
            HttpRequest req = new HttpRequest();
            req.SetMethod(method);
            req.SetBody(body);

            //设置头信息
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> h in headers)
                {
                    req.AddHeader(h.Key, h.Value);
                }
            }

            //设置认证用户
            if (_user != null && _authScheme != null)
            {
                req.AddHeader(HeaderProperty.Authorization,_authScheme.GenerateAuthorization(_user.UserName, _user.Password));
            }
            try
            {
                Send(url, req, callback);
            }catch(Exception ex) {
                if (RequestException != null)
                {
                    RequestException(url, req.Host, req, ex);
                }
                else
                {
                    throw ex;
                }
            }
        }
        /// <summary>
        /// 发送HTTP请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="req"></param>
        /// <param name="callback"></param>
        public void Send(string url,HttpRequest req,RequestComplete callback)
        {
            req.AddHeader(HeaderProperty.UserAgent, UserAgent);
            req.AddHeader(HeaderProperty.Accept, Accept);

            SocketClient sc = new SocketClient();
            sc.ConnectTimeout = ConnectTimeout;
           
            //分析URL路径，DNS解析
            UriInfo uri = UriInfo.Parse(url);

            if (!string.IsNullOrEmpty(uri.Url))
            {
                //注入URI信息
                req.SetUri(uri);

                HttpContext ctx = new HttpContext
                {
                    Request = req
                };

                sc.AfterReceiveEnd = new ReceiveEnd((x, args) =>
                {
                    //TODO 选择适当的时机关闭链接
                    HttpResponse resp = HttpResponse.Parse(args.Data);
                    ctx.Response = resp;
                    var cl = resp.Headers.GetValue(HeaderProperty.ContentLength);
                    var contentEncoding = resp.Headers.GetValue(HeaderProperty.ContentEncoding);
                    var chunked = string.IsNullOrEmpty(contentEncoding) && "chunked".Equals(contentEncoding);

                    if ((resp.Headers.Contains(HeaderProperty.ContentLength) &&long.Parse(cl)< resp.ContentLength)||args.Socket.Available>0)
                    {
                        args.Socket.BeginReceive(args.State.Buffer, 0, args.State.Buffer.Length, SocketFlags.None, sc.CallbackReceived, args.State);
                    }
                    else
                    {
                        //TODO 判断chunked 

                        //判断Content-Encoding
                        if (AutoDecodeBody) { 
                            if (resp.ContentEncoding == "gzip")
                            {
                                resp.WriteDecodeBody(GZip.Decompress(req.Body));
                            }else if (resp.ContentEncoding == "deflate"){
                                resp.WriteDecodeBody(Deflate.Decompress(req.Body));
                            }
                        }
                        if (callback != null)
                        {
                            callback(url, ctx);
                        }
                        if (ResponseCompleted != null)
                        {
                            ResponseCompleted(url, ctx);
                        }
                        if (ResponseReceive != null)
                        {
                            ResponseReceive(url, uri.Host, resp);
                        }
                        sc.Shutdown();
                    }
                });

                int defaultPort = 80;

                if (uri.Protocol.Equals("https",StringComparison.OrdinalIgnoreCase))
                {
                    defaultPort = 443;
                }
                defaultPort = uri.Port == 0 ? defaultPort : uri.Port;
                sc.Connect(uri.Host, defaultPort);

                if (sc.Connected)
                {
                    if (ContentEncoding == ContentEncoding.Gzip)
                    {
                        req.SetBody(GZip.Compress(req.Body));
                        req.AddHeader(HeaderProperty.AcceptEncoding, "gzip");
                    }else if (ContentEncoding == ContentEncoding.Deflate){
                        req.AddHeader(HeaderProperty.AcceptEncoding, "deflate");
                        req.SetBody(Deflate.Compress(req.Body));
                    }
                    sc.SendTo(req.GetBuffer());
                    if (RequestSend != null)
                    {
                        RequestSend(url, req.Host, req);
                    }
                }
                else
                {
                    throw new Exception("超时时间已到，无法访问指定的服务器:" + (string.IsNullOrEmpty(uri.Domain) ? uri.Host : uri.Domain) + $":{defaultPort}");
                }
            }
            else
            {
                throw new Exception($"分析指定的地址:{url}时出错，请检查地址是否合法");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseurl"></param>
        /// <param name="relativeAddress"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="callback"></param>
        public void Send(string baseurl,string[] relativeAddress, RequestMethod method, Dictionary<string, string> headers, byte[] body, RequestComplete callback)
        {
             foreach(var add in relativeAddress)
             {
                string path = baseurl;
                UriInfo uri = UriInfo.Parse(baseurl);

                var revAdd = add;
                if (revAdd.StartsWith("./"))
                {
                    revAdd = add.Substring(2);
                } else if (revAdd.StartsWith("../")){

                    while (revAdd.StartsWith("../"))
                    {
                        if (uri.Paths.Length > 0)
                        {
                            string[] paths = new string[uri.Paths.Length - 1];
                            Array.Copy(uri.Paths, paths, uri.Paths.Length - 1);
                            uri.Paths = paths;
                        }
                        revAdd = revAdd.Substring(3);
                    }
                }
                else if(revAdd.StartsWith("/"))
                {
                    uri.Paths = new string[0];
                }
                path = $"{uri.Protocol}://{uri.Domain ?? uri.Host}"+(uri.Port>0?$":{uri.Port}":"");

                if (uri.Paths.Length > 0)
                {
                    path += string.Join("", uri.Paths);
                }
                else
                {
                    if (!path.EndsWith("/")&&!revAdd.StartsWith("/"))
                    {
                        path += "/";
                    }
                }

                path += revAdd;

                Send(path,method,headers,body,callback);
             }
        }
        /// <summary>
        /// HttpGet方法
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="headers">
        /// 附加的头信息，内部会封装一部分头信息，其它头信息请自行附加。
        /// <list type="table">
        ///     <listheader>内部封装的头信息：</listheader>
        ///     <item><see cref="HeaderProperty.UserAgent"/></item>
        ///     <item><see cref="HeaderProperty.AcceptEncoding"/></item>
        ///     <item><see cref="HeaderProperty.Accept"/></item>
        ///     <item><see cref="HeaderProperty.Host"/></item>
        ///     <item><see cref="HeaderProperty.Referer"/></item>
        ///     <item><see cref="HeaderProperty.ContentLength"/></item>
        ///     </list>
        /// </param>
        /// <param name="callback">回调方法</param>
        public void Get(string url, Dictionary<string, string> headers, RequestComplete callback)
        {
            Send(url, RequestMethod.GET, headers,null,callback);
        }
        /// <summary>
        /// HttpGet方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        public void Get(string url,RequestComplete callback)
        {
            Get(url, null, callback);
        }
        internal void GetFile(string url,RequestComplete callback)
        {

        }
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="headers">附加的头信息,参见Get方法</param>
        /// <param name="body">请求文本，文本会被编码成UTF-8格式。请求时会附加<see cref="HeaderProperty.ContentType"/>头属性</param>
        /// <param name="callback">回调方法</param>
        public void Post(string url, Dictionary<string, string> headers, string body,RequestComplete callback)
        {
            byte[] payload = Encode.StringEncoder.Encode(body);
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            if (!headers.ContainsKey(HeaderProperty.ContentType.PropertyName))
            {
                headers.Add(HeaderProperty.ContentType.PropertyName, $"text/plain; charset={Charset}");
            }
            Post(url, headers, payload, callback);
        }
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="headers">附加的头信息,参见Get方法</param>
        /// <param name="body"></param>
        /// <param name="callback">回调方法</param>
        public void Post(string url, Dictionary<string, string> headers, byte[] body, RequestComplete callback)
        {
            Send(url, RequestMethod.POST, headers, body, callback);
        }
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="body">请求包体</param>
        /// <param name="callback">回调方法</param>
        public void Post(string url, byte[] body, RequestComplete callback)
        {
            Post(url, null, body, callback);
        }
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        public void Post(string url, byte[] body)
        {
            Post(url, body);
        }
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        public void Post(string url, Dictionary<string, string> headers, string body)
        {
            Post(url, headers, body, null);
        }
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="body">请求文本，文本会被编码成UTF-8格式。请求时会附加<see cref="HeaderProperty.ContentType"/>头属性</param>
        /// <param name="callback">回调方法</param>
        public void Post(string url,string body,RequestComplete callback)
        {
            Post(url, null, body, callback);
        }
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        public void Post(string url,string body)
        {
            Post(url, body, null);
        }
        //TODO 文件传输应加入进度显示
        /// <summary>
        /// 多文件提交 multipart/form-data;
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="headers">附加的头信息,参见Get方法</param>
        /// <param name="files">文件集合</param>
        /// <param name="callback">回调方法</param>
        public void PostFile(string url, Dictionary<string, string> headers, FileCollection files,RequestComplete callback)
        {
            byte[] byteNewLine = new byte[] { ASCIICode.CR, ASCIICode.LF };
            string sNewLine=System.Text.Encoding.ASCII.GetString(byteNewLine);
            string boundary = "--"+CacheControl.GenerateRandom(8);
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            headers.Add(HeaderProperty.ContentType.PropertyName, $"multipart/form-data; boundary={boundary}");
            if (files.Count > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    foreach (File f in files)
                    {
                        FileInfo fi = new FileInfo(f.Path);
                        FileStream fs = fi.OpenRead();
                        string header = $"--{boundary}" + sNewLine;
                        header += HeaderProperty.ContentDisposition.PropertyName + $": form-data; name=\"{f.FieldName}\"; filename=\"{HtmlEncoder.StringToEntityCode(fs.Name)}\"" + sNewLine;
                        header += HeaderProperty.ContentType.PropertyName + ": application/octet-stream" + sNewLine + sNewLine;
                        byte[] headerdata = System.Text.Encoding.ASCII.GetBytes(header);
                        ms.Write(headerdata, 0, headerdata.Length);
                        byte[] buffer = new byte[1024];
                        int readcount;
                        while ((readcount = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, readcount);
                        }
                        ms.Write(byteNewLine ,0,byteNewLine.Length);
                        fs.Close();
                    }
                    
                    string footer =  $"--{boundary}--"+ sNewLine;
                    byte[] footerdata = System.Text.Encoding.ASCII.GetBytes(footer);
                    ms.Write(footerdata, 0, footerdata.Length);
                    ms.Flush();
                    byte[] data = ms.GetBuffer();
                    ms.Close();
                    Post(url, headers, data, callback);
                }
            }
        }
        /// <summary>
        /// 多文件上传
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="files">文件集合</param>
        /// <param name="callback">回调方法</param>
        public void PostFile(string url,FileCollection files,RequestComplete callback)
        {
            PostFile(url, null, files, callback);
        }
        /// <summary>
        /// HttpPut方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="callback"></param>
        public void Put(string url, Dictionary<string, string> headers, string body, RequestComplete callback)
        {
            byte[] payload = StringEncoder.Encode(body);
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            if (!headers.ContainsKey(HeaderProperty.ContentType.PropertyName))
            {
                headers.Add(HeaderProperty.ContentType.PropertyName, $"text/plain; charset={Charset}");
            }
            Put(url, headers, payload, callback);
        }
        /// <summary>
        /// HttpPut方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="callback"></param>
        public void Put(string url, Dictionary<string, string> headers, byte[] body, RequestComplete callback)
        {
            Send(url, RequestMethod.PUT, headers, body, callback);
        }
        /// <summary>
        /// HttpPut方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        /// <param name="callback"></param>
        public void Put(string url, byte[] body, RequestComplete callback)
        {
            Put(url, null, body, callback);
        }
        /// <summary>
        /// HttpPut方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        public void Put(string url, byte[] body)
        {
            Put(url, body);
        }
        /// <summary>
        /// HttpPut方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        public void Put(string url, Dictionary<string, string> headers, string body)
        {
            Post(url, headers, body, null);
        }
        /// <summary>
        /// HttpPut方法
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="body">请求文本，文本会被编码成UTF-8格式。请求时会附加<see cref="HeaderProperty.ContentType"/>头属性</param>
        /// <param name="callback">回调方法</param>
        public void Put(string url, string body, RequestComplete callback)
        {
            Post(url, null, body, callback);
        }
        /// <summary>
        /// HttpPut方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        public void Put(string url, string body)
        {
            Post(url, body, null);
        }
        /// <summary>
        /// HttpDelete方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="callback"></param>
        public void Delete(string url, Dictionary<string, string> headers, RequestComplete callback)
        {
            Send(url, RequestMethod.GET, headers, null, callback);
        }
        /// <summary>
        /// HttpDelete方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        public void Delete(string url, RequestComplete callback)
        {
            Delete(url, null, callback);
        }

        //TODO 载入证书
        internal void LoadCert(string path)
        {

        }
        //TODO 载入证书
        internal void LoadCert(FileStream fs)
        {

        }
        /// <summary>
        /// 设置认证类型
        /// </summary>
        /// <param name="schema"></param>
        public void SetAuthorization(Auth.AuthScheme schema)
        {
            _authScheme = schema;
        }
        /// <summary>
        /// 设置用户名和密码
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        public void SetUser(string username,string pwd)
        {
            _user = new Auth.User() { UserName = username, Password = pwd, UserGroup = Auth.UserGroup.User };
        }
    }

    class HttClientS:HttpClient
    {

    }

    class HttpClientQUIC : HttpClient
    {

    }
}
