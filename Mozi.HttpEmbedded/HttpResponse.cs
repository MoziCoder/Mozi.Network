using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mozi.HttpEmbedded.Cookie;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Generic;
using Mozi.HttpEmbedded.Source;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// HTTP响应
    /// </summary>
    public class HttpResponse
    {
        private byte[] _body = new byte[0];

        private string _contentType = "text/plain";

        private string _charset = "";

        private TransformHeader _headers;
        private ResponseCookie _cookies;

        /// <summary>
        /// HTTP协议版本
        /// </summary>
        public HttpVersion Version { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public StatusCode Status { get; protected set; }
        /// <summary>
        /// 内容长度
        /// </summary>
        public int ContentLength { get { return _body.Length; } }
        /// <summary>
        /// Mime类型，默认为 text/plain
        /// </summary>
        public string ContentType { get { return _contentType; } protected set { _contentType = value; } }
        /// <summary>
        /// 请求头
        /// </summary>
        public TransformHeader Headers { get { return _headers; } protected set { _headers = value; } }
        /// <summary>
        /// 压缩类型
        /// </summary>
        public string ContentEncoding { get; set; }
        /// <summary>
        /// 内容编码集
        /// </summary>
        public string Charset { get { return _charset; } set { _charset = value; } }
        /// <summary>
        /// 文档是否被压缩过
        /// </summary>
        public bool ContentEncoded { get; protected set; }
        /// <summary>
        /// 响应数据体
        /// </summary>
        public byte[] Body { get { return _body; } protected set { _body = value; } }
        /// <summary>
        /// Cookie
        /// </summary>
        public ResponseCookie Cookies { get { return _cookies; } protected set { _cookies = value; } }
        /// <summary>
        /// 是否自动增加头部信息
        /// 自动追加的头属性为<see cref="HeaderProperty.Date"/>,<see cref="HeaderProperty.ContentType"/>,<see cref="HeaderProperty.ContentLength"/>
        /// </summary>
        public bool DontAddAutoHeader = false;
        ///// <summary>
        ///// 是否自动生成头 Content-Length
        ///// </summary>
        //public bool AutoAddHeaderContentLength = true;
        ///// <summary>
        ///// 是否自动生成头 Date
        ///// </summary>
        //public bool AutoAddHeaderDate = true;
        ///// <summary>
        ///// 是否自动生成头 Content-Type
        ///// </summary>
        //public bool AutoAddHeaderContentType = true;

        public HttpResponse()
        {
            _headers = new TransformHeader();
            Version = HttpVersion.Version11;
            _cookies = new ResponseCookie();
        }
        /// <summary>
        /// 设置协议版本
        /// </summary>
        /// <param name="version"></param>
        public void SetVersion(HttpVersion version)
        {
            Version = version;
        }
        /// <summary>
        /// 设置Cookie
        /// <see cref="ResponseCookie.Set(string, string)"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetCookie(string name,string value)
        {
            _cookies.Set(name, value);
        }
        /// <summary>
        /// 设置Cookie
        /// <see cref="ResponseCookie.Set(string, string, string, string)"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="value"></param>
        public void SetCookie(string name,string domain,string path,string value)
        {
            _cookies.Set(name, domain, path, value);
        }
        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="status"></param>
        public HttpResponse SetStatus(StatusCode status)
        {
            Status = status;
            return this;
        }
        /// <summary>
        /// 设置文档类型
        /// <para>
        /// 默认为text/plain
        /// </para>
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public HttpResponse SetContentType(string contentType)
        {
            _contentType = contentType;
            return this;
        }
        public HttpResponse SetHeaders(TransformHeader headers)
        {
            _headers = headers;
            return this;
        }
        /// <summary>
        /// 增加头部信息
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        public HttpResponse AddHeader(HeaderProperty head, string value)
        {
            _headers.Add(head, value);
            return this;
        }
        /// <summary>
        /// 增加头部信息
        /// </summary>
        /// <param name="item"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponse AddHeader(string item, string value)
        {
            _headers.Add(item, value);
            return this;
        }
        /// <summary>
        /// 写入字节数据
        /// </summary>
        /// <returns></returns>
        public HttpResponse Write(byte[] data)
        {
            if (_body == null)
            {
                _body = data;
            }
            else
            {
                Array.Resize(ref _body, _body.Length + data.Length);
                Array.Copy(data, _body, data.Length);
            }
            return this;
        }
        /// <summary>
        /// 写入文本
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public HttpResponse Write(string data)
        {
            Write(StringEncoder.Encode(data));
            return this;
        }
        /// <summary>
        /// 写入压缩的数据
        /// </summary>
        /// <param name="body"></param>
        internal void CompressBody(byte[] body)
        {
            _body = body;
            ContentEncoded = true;
        }
        //TODO 此处需要调试
        //Transfer-Encoding
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public HttpResponse SendFile(string filepath)
        {
            FileInfo fi = new FileInfo(filepath);
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            SetContentType(Mime.Default);
            //RFC6266
            AddHeader(HeaderProperty.ContentDisposition, string.Format("attachment; filename=\"{0}\"; filename*=utf-8''{1}",fi.Name,UrlEncoder.Encode(fi.Name)));
            MemoryStream ms = new MemoryStream();
            try
            {
                byte[] buffer = new byte[1024];
                int readCount = 0;
                while ((readCount = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, readCount);
                }
                Write(ms.GetBuffer());
            }
            finally
            {
                ms.Close();
                fs.Close();
            }
            return this;
        }
        //TODO 2021/05/10 如果压缩介入，就要对包体进行压缩
        /// <summary>
        /// 从缓冲区中取出数据
        /// </summary>
        /// <param name="headerKeyUppercase">头属性 键名是否大写</param>
        /// <returns></returns>
        public byte[] GetBuffer(bool headerKeyUppercase)
        {
            List<byte> data = new List<byte>();
            //注入状态信息
            data.AddRange(GetStatusLine());
            data.AddRange(TransformHeader.Carriage);
            if (!DontAddAutoHeader)
            {
                //注入包体大小 字节长度
                AddHeader(HeaderProperty.ContentLength, _body.Length.ToString());
                //注入文档类型
                AddHeader(HeaderProperty.ContentType, _contentType + (!string.IsNullOrEmpty(Charset) ? "; " + Charset : ""));
                //注入响应时间
                AddHeader(HeaderProperty.Date, DateTime.Now.ToUniversalTime().ToString("r"));
            }
            //注入头部
            data.AddRange(_headers.GetBuffer(headerKeyUppercase));
            //注入Cookie
            data.AddRange(_cookies.GetBuffer());
            //注入分割符
            data.AddRange(TransformHeader.Carriage);
            //注入响应包体
            data.AddRange(_body);
            return data.ToArray();
        }
        /// <summary>
        /// 取出缓存的数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
           return GetBuffer(false);
        }
        /// <summary>
        /// 响应状态
        /// </summary>
        /// <returns></returns>
        public byte[] GetStatusLine()
        {
            return StringEncoder.Encode(string.Format("HTTP/{0} {1} {2}", Version.Version, Status.Code, Status.Text));
        }
        /// <summary>
        /// 重定向302
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public StatusCode Redirect(string path)
        {
            _headers.Add(HeaderProperty.Location.PropertyName, path);
            return StatusCode.Found;
        }
        /// <summary>
        /// HttpResponse反向解析
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HttpResponse Parse(byte[] data)
        {
            HttpResponse resp = new HttpResponse();
            //响应行
            //头
            //包体
            //解析头
            int posCR = 0;
            int posCaret = 0;
            int index = 0;
            int dataLength = data.Length;

            while ((posCR < dataLength) && Array.IndexOf(data, ASCIICode.CR, posCR + 1) > 0)
            {
                posCR = Array.IndexOf(data, ASCIICode.CR, posCR + 1);

                //连续两个CR
                byte[] fragement = new byte[posCR - posCaret];
                Array.Copy(data, posCaret, fragement, 0, posCR - posCaret);
                if (index == 0)
                {
                    ParseRequestLine(ref resp, fragement);
                }
                else
                {
                    ParseHeaders(ref resp, fragement);
                }

                if ((Array.IndexOf(data, ASCIICode.CR, posCR + 1) == posCR + 2))
                {
                    break;
                }
                //跳过分割字节段
                posCaret = posCR + 2;
                index++;
                //TODO 置空对象
            }

            //解析Cookie
            //解析数据 荷载部分
            if (data.Length > posCR + 4)
            {
                resp.Body = new byte[data.Length - (posCR + 4)];
                //TODO 此处又重新生成一个数据对象，导致内存占用过大
                Array.Copy(data, posCR + 4, resp.Body, 0, resp.Body.Length);
            }
            return resp;
        }

        /// <summary>
        /// 解析首行数据
        /// 协议 状态 描述 
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="data"></param>
        private static void ParseRequestLine(ref HttpResponse resp, byte[] data)
        {
            //解析起始行
            var RequestLineString = Encoding.UTF8.GetString(data);
            string[] sFirst = RequestLineString.Split(new[] { (char)ASCIICode.SPACE }, StringSplitOptions.None);
            //协议 状态 描述 
            string sProtocol = sFirst[0];
            string sStatusCode = sFirst[1];
            string sStatusDescription = sFirst[2];

            string sProtoType = sProtocol.Substring(0, sProtocol.IndexOf((char)ASCIICode.DIVIDE));
            string sProtoVersion = sProtocol.Substring(sProtocol.IndexOf((char)ASCIICode.DIVIDE) + 1);

            resp.Status = AbsClassEnum.Get<StatusCode>(sStatusCode);
            resp.Version = AbsClassEnum.Get<HttpVersion>(sProtoVersion);

        }
        /// <summary>
        /// 解析头属性
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="data"></param>
        private static void ParseHeaders(ref HttpResponse resp, byte[] data)
        {
            HeaderProperty hp = HeaderProperty.Parse(data);            
            
            #if DEBUG
                Console.WriteLine("{0}:{1}",hp.PropertyTag,hp.PropertyValue);
            #endif
            if (!hp.PropertyName.Equals(HeaderProperty.SetCookie))
            {
                resp.Headers.Add(hp.PropertyName, hp.PropertyValue);
            }
            else
            {
                ParseCookie(ref resp, hp.PropertyValue);
            }
        }
        /// <summary>
        /// 解析Cookie
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="setCookieValue"></param>
        private static void ParseCookie(ref HttpResponse resp,string setCookieValue)
        {
            HttpCookie cookie = HttpCookie.Parse(setCookieValue);
            resp.Cookies.Set(cookie.Name, cookie.Domain, cookie.Path, cookie.Value);
        }
        ~HttpResponse()
        {
            _body = null;
        }
    }
}