using Mozi.HttpEmbedded.Encode;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 请求完成时回调
    /// </summary>
    /// <param name="context"></param>
    public delegate void RequestComplete(HttpContext context);

    //DONE http客户端，因http客户端实现比较多，暂时不实现，待后期规划
    //TODO 应同步实现Https HttpQUIC(http3.0)
    //TODO 应加入TLS安全传输

    /// <summary>
    /// Http客户端
    /// </summary>
    public class HttpClient
    {
        private const string Charset = "UTF-8";
        private string _userAgent = "Mozilla/5.0 (Linux;Android 4.4.2;OEM Device) AppleWebKit/537.36 (KHTML,like Gecko) Chrome/39.0.2171.71  Mozi/1.3.8";
        private const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        private const string AcceptEncoding = "gzip, deflate";

        /// <summary>
        /// 接收到响应时触发回调
        /// </summary>
        public RequestComplete ResponseReceived;
        /// <summary>
        /// 用户代理
        /// </summary>
        public string UserAgent { get => _userAgent; set => _userAgent = value; }

        //TODO 此处应考虑Gzip解码
        /// <summary>
        /// 发送HTTP请求
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
        private void Send(string url, RequestMethod method,Dictionary<HeaderProperty,string> headers,byte[] body,RequestComplete callback)
        {
            SocketClient sc = new SocketClient();
            HttpRequest req = new HttpRequest();
            //分析URL路径
            UriInfo uri = UriInfo.Parse(url);

            if (!string.IsNullOrEmpty(uri.Url))
            {
                req.SetUri(uri);
                req.SetMethod(method);
                req.SetPath(uri.Path + (String.IsNullOrEmpty(uri.Query) ? "" : "&" + uri.Query));
                req.SetHeader(HeaderProperty.UserAgent, UserAgent);
                req.SetHeader(HeaderProperty.Accept, Accept);
                req.SetHeader(HeaderProperty.AcceptEncoding, AcceptEncoding);
                req.SetBody(body);
                if (headers != null)
                {
                    foreach (KeyValuePair<HeaderProperty, string> h in headers)
                    {
                        req.SetHeader(h.Key, h.Value);
                    }
                }
                HttpContext ctx = new HttpContext
                {
                    Request = req
                };

                sc.AfterReceiveEnd = new ReceiveEnd((x, y) =>
                {
                    //TODO 选择适当的时机关闭链接
                    HttpResponse resp = HttpResponse.Parse(y.Data);
                    ctx.Response = resp;
                    if (callback != null)
                    {
                        callback(ctx);
                    }

                    if (ResponseReceived != null)
                    {
                        ResponseReceived(ctx);
                    }
                    sc.Shutdown();
                });

                sc.Connect(uri.Host, uri.Port == 0 ? 80 : uri.Port);
                if (sc.Connected)
                {
                    sc.SendTo(req.GetBuffer());
                }
                else
                {
                    throw new Exception("无法访问指定的服务器:"+(string.IsNullOrEmpty(uri.Domain)?uri.Host:uri.Domain)+(uri.Port==0?":80":(":"+uri.Port)));
                }
            }
            else
            {
                throw new Exception($"分析指定的地址:{url}时出错，请检查地址是否合法");
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
        public void Get(string url, Dictionary<HeaderProperty, string> headers, RequestComplete callback)
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
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="headers">附加的头信息,参见Get方法</param>
        /// <param name="body"></param>
        /// <param name="callback">回调方法</param>
        public void Post(string url, Dictionary<HeaderProperty, string> headers,byte[] body, RequestComplete callback)
        {
            Send(url, RequestMethod.POST, headers, body, callback);
        }
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="body">请求包体</param>
        /// <param name="callback">回调方法</param>
        public void Post(string url,byte[] body, RequestComplete callback)
        {
            Post(url, null, body, callback);
        }
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        public void Post(string url,byte[] body)
        {
            Post(url, body);
        }
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url">url地址，格式http://{host|domain}[:{port}]/[{path}[?query]]</param>
        /// <param name="headers">附加的头信息,参见Get方法</param>
        /// <param name="body">请求文本，文本会被编码成UTF-8格式。请求时会附加<see cref="HeaderProperty.ContentType"/>头属性</param>
        /// <param name="callback">回调方法</param>
        public void Post(string url, Dictionary<HeaderProperty, string> headers, string body,RequestComplete callback)
        {
            byte[] bytesBody = Encode.StringEncoder.Encode(body);
            if (headers == null)
            {
                headers = new Dictionary<HeaderProperty, string>();
            }
            if (!headers.ContainsKey(HeaderProperty.ContentType))
            {
                headers.Add(HeaderProperty.ContentType, $"text/plain; charset={Charset}");
            }
            Post(url, headers, bytesBody, callback);
        }
        /// <summary>
        /// HttpPost方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        public void Post(string url, Dictionary<HeaderProperty, string> headers, string body)
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
        public void PostFile(string url, Dictionary<HeaderProperty, string> headers, FileCollection files,RequestComplete callback)
        {
            byte[] byteNewLine = new byte[] { ASCIICode.CR, ASCIICode.LF };
            string sNewLine=System.Text.Encoding.ASCII.GetString(byteNewLine);
            string boundary = "--"+CacheControl.GenerateRandom(8);
            if (headers == null)
            {
                headers = new Dictionary<HeaderProperty, string>();
            }
            headers.Add(HeaderProperty.ContentType, $"multipart/form-data; boundary={boundary}");
            if (files.Count > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    foreach (File f in files)
                    {
                        FileInfo fi = new FileInfo(f.FileTempSavePath);
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
    }

    class HttClientS:HttpClient
    {

    }
    class HttpClientQUIC : HttpClient
    {

    }
}
