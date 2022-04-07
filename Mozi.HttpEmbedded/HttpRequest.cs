using System;
using System.Collections.Generic;
using System.Text;
using Mozi.HttpEmbedded.Cookie;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded
{
    //DONE 应将 GET/POST 查询字段进行区分 

    /// <summary>
    /// HTTP请求
    /// </summary>
    public class HttpRequest
    {

        private byte[] _body = new byte[] { };

        private TransformHeader _headers;
        private FileCollection _files;
        private RequestCookie _cookies;

        /// <summary>
        /// 协议类型,参看<see cref="ProtocolType"/>所列出的值
        /// </summary>
        public ProtocolType Protocol { get; protected set; }
        /// <summary>
        /// 协议版本
        /// </summary>
        public HttpVersion Version { get; protected set; }
        /// <summary>
        /// 请求路径
        /// </summary>
        public string Path { get; protected set; }
        /// <summary>
        /// 查询字符串
        /// </summary>
        public string QueryString { get; protected set; }
        /// <summary>
        /// 查询 索引可忽略大小写
        /// </summary>
        public Dictionary<string, string> Query = new Dictionary<string, string>(new StringCompareIgnoreCase());
        /// <summary>
        /// POST 索引可忽略大小写
        /// </summary>
        public Dictionary<string, string> Post = new Dictionary<string, string>(new StringCompareIgnoreCase());
        /// <summary>
        /// 可接受压缩算法
        /// </summary>
        public string AcceptEncoding { get; protected set; }
        /// <summary>
        /// 请求地址
        /// </summary>
        public string Host { get; protected set; }
        /// <summary>
        /// 客户端信息
        /// </summary>
        public string UserAgent { get; protected set; }
        /// <summary>
        /// 请求方法
        /// </summary>
        public RequestMethod Method { get; protected set; }
        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType { get; protected set; }
        /// <summary>
        /// 内容编码类型
        /// </summary>
        public string ContentCharset { get; protected set; }
        /// <summary>
        /// 内容大小
        /// </summary>
        public string ContentLength { get; protected set; }
        /// <summary>
        /// 请求头
        /// </summary>
        public TransformHeader Headers { get { return _headers; } protected set { _headers = value; } }
        ///// <summary>
        ///// 二进制通讯数据
        ///// </summary>
        //public byte[] PackData { get; protected set; }
        /// <summary>
        /// 原始请求首行数据
        /// </summary>
        public byte[] RequestLineData { get; protected set; }
        /// <summary>
        /// 原始请求首行字符串
        /// </summary>
        public string RequestLineString { get; protected set; }
        /// <summary>
        /// 请求头数据
        /// </summary>
        public byte[] HeaderData { get; protected set; }
        /// <summary>
        /// 请求数据体，有效荷载数据
        /// </summary>
        public byte[] Body { get { return _body; } protected set { _body = value; } }
        /// <summary>
        /// 文件
        /// </summary>
        public FileCollection Files { get { return _files; } protected set { _files = value; } }
        /// <summary>
        /// Cookie
        /// </summary>
        public RequestCookie Cookies { get { return _cookies; } protected set { _cookies = value; } }
        /// <summary>
        /// 客户机IP地址
        /// </summary>
        public string ClientAddress { get; internal set; }
        /// <summary>
        /// 客户机是否已通过认证
        /// </summary>
        public bool IsAuthorized { get; internal set; }
        /// <summary>
        /// 客户机接受的语言选项
        /// </summary>
        public LanguagePriority[] AcceptLanguage { get; private set; }
        /// <summary>
        /// 请求的来源地址
        /// </summary>
        public string Referer { get; private set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpRequest()
        {
            //默认HTTP/1.1
            Version = HttpVersion.Version11;
            _headers = new TransformHeader();
            _files = new FileCollection();
            _cookies = new RequestCookie();
            _body = new byte[] { };
        }
        /// <summary>
        /// 解析请求数据包
        /// <code>
        ///                     
        ///  GET / HTTP/1.1\r\nHost: 127.0.0.1:9000\r\n
        ///  User-Agent: Mozilla/5.0 (Windows NT 5.2; rv:22.0) Gecko/20100101 Firefox/22.0\r\n
        ///  Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n
        ///  Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3\r\n
        ///  Accept-Encoding: gzip, deflate\r\n
        ///  Authorization: Basic c2Rmc2RmOnNmc2Rm\r\n
        ///  Connection: keep-alive\r\n
        ///  Cache-Control: max-age=0
        ///  
        /// </code>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HttpRequest Parse(byte[] data)
        {
            HttpRequest req = new HttpRequest();
            //解析头
            int posCR = 0;
            int posCaret = 0;
            int count = 0;

            int indLine = 0;
            int dataLength = data.Length;

            while ((posCR < dataLength) && Array.IndexOf(data, ASCIICode.CR, posCR + 1) > 0)
            {
                posCR = Array.IndexOf(data, ASCIICode.CR, posCR + 1);

                //连续两个CR
                byte[] fragement = new byte[posCR - posCaret];
                Array.Copy(data, posCaret, fragement, 0, posCR - posCaret);
                if (indLine == 0)
                {
                    ParseRequestLine(ref req, fragement);
                }
                else
                {
                    ParseHeaders(ref req, fragement);
                }

                if ((Array.IndexOf(data, ASCIICode.CR, posCR + 1) == posCR + 2))
                {
                    break;
                }
                //跳过分割字节段
                posCaret = posCR + 2;
                indLine++;
                //TODO 置空对象
            }

            //头信息解析
            //HOST
            ParseHeaderHost(ref req);
            //User-Agent
            ParseHeaderUserAgent(ref req);
            //Accept-Language
            ParseHeaderAcceptLanguage(ref req);
            //Referer
            ParseHeaderReferer(ref req);
            //Content-Type
            ParseHeaderContentType(ref req);
            //Range
            ParseHeaderRange(ref req);
            //解析Cookie
            ParseCookie(ref req);
            //TODO 此处是否需要分辨GET/POST
            //解析数据 荷载部分
            if (data.Length > posCR + 4)
            {
                req.Body = new byte[data.Length - (posCR + 4)];
                //TODO 此处又重新生成一个数据对象，导致内存占用过大
                Array.Copy(data, posCR + 4, req.Body, 0, req.Body.Length);
                ParsePayload(ref req, req.Body);
            }
            return req;
        }
        //TODO HTTP/2.0 是基于二进制数据帧，此处需要重新适配
        //TODO 先判断包体是否经过压缩
        /// <summary>
        /// 解析请求体正文
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParsePayload(ref HttpRequest req, byte[] data)
        {
            string formType = req.Headers.GetValue(HeaderProperty.ContentType.PropertyName);
            if (formType != null)
            {
                if (formType.Contains("application/x-www-form-urlencoded"))
                {
                    ParsePayloadFormUrl(ref req,ref data);
                }
                else if (formType.Contains("multipart/form-data"))
                {
                    ParsePayloadFormData(ref req,ref data);
                }
                else
                {
                    ParsePayloadText(ref req,ref data);
                }
            }
        }
        /// <summary>
        /// 分析请求体 application/x-www-form-urlencoded
        /// 请求体数据类似于查询字符串
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParsePayloadFormUrl(ref HttpRequest req,ref byte[] data)
        {
            req.Post = UrlEncoder.ParseQuery(StringEncoder.Decode(data));
        }
        //TODO 文件流应写入缓冲区,否则会造成内存占用过大
        //DONE 此处仅能解析一个文件，继续修改代码
        //TODO multipart/form-data 也能提交文本内容，此处未能很好的处理
        /// <summary>
        /// 分析请求体 multipart/form-data
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        /// <example>
        /// <code>
        ///   --{boundary}
        ///   Content-Disposition: form-data; name="{file.fieldname}"; filename="{file.name}"\r\n
        ///   Content-Type: application/octet-stream\r\n
        ///   \r\n
        ///   {file.binary}\r\n
        ///   \r\n
        ///   --{boundary}\r\n
        ///   Content-Disposition: form-data; name="{file.fieldname}"; filename="{file.name}"\r\n
        ///   Content-Type: application/octet-stream\r\n
        ///   \r\n
        ///   {file.binary}\r\n
        ///   --{boundary}\r\n
        ///   Content-Disposition: form-data; name="{field.name}"\r\n
        ///   \r\n
        ///   --{boundary}--\r\n
        /// </code>
        /// </example>
        private static void ParsePayloadFormData(ref HttpRequest req,ref byte[] data)
        {
            string contentType = req.Headers.GetValue(HeaderProperty.ContentType.PropertyName);
            string boundary = "";
            //此处仅用；分割，提高通用性
            string[] values = contentType.Split(new[] { ((char)ASCIICode.SEMICOLON).ToString() }, StringSplitOptions.RemoveEmptyEntries);

            //取得分割符号boundary
            foreach (var s in values)
            {
                if (s.Trim().StartsWith("boundary"))
                {
                    boundary = s.Trim().Replace("boundary=", "");
                    break;
                }
            }

            byte[] bBound = StringEncoder.Encode(boundary);

            //分割 form-data
            int indBoundary = -1, indFragFirst/*分割起点*/ = 0, indFragNext/*下段分割起点*/ = 0;

            while ((indBoundary + 1) < data.Length && Array.IndexOf(data, ASCIICode.MINUS, indBoundary + 1) >= 0)
            {
                try
                {
                    indFragFirst = indFragNext;
                    indBoundary = Array.IndexOf(data, ASCIICode.MINUS, indBoundary + 1);

                    //循环检测分割段
                    bool isFragStart = true;
                    bool isFragEnd = false;

                    for (int i = 0; i < bBound.Length; i++)
                    {
                        if ((data.Length >= indBoundary + bBound.Length) && data.Length >= (indBoundary + i + 2 - 1) && data[indBoundary + i + 2] != bBound[i])
                        {
                            isFragStart = false;
                        }
                    }
                    if (isFragStart)
                    {
                        indFragNext = indBoundary;
                        //跳过分割
                        indBoundary += bBound.Length + 2 + 2;
                    }

                    //分割信息段
                    if (isFragStart && indFragNext > 0)
                    {
                        //Console.WriteLine("发现片段{0}-{1},长度{2} Byte", indFragFirst, indFragNext, indFragNext - indFragFirst);
                        int posCR = 0, posCaret/*分割起始位*/ = 0, index = 0;

                        byte[] fragment = new byte[indFragNext - indFragFirst];

                        Array.Copy(data, indFragFirst, fragment, 0, indFragNext - indFragFirst);

                        bool isFile = false;

                        string fieldName = string.Empty;
                        string fileName = string.Empty;

                        ///<example>
                        ///-----------------------------97671069125495\r\n
                        ///Content-Disposition: form-data; name=\"mailaddress\"\r\n
                        ///\r\n
                        ///abcdefg
                        ///\r\n
                        ///</example>

                        //提取字段头属性
                        while ((posCR = Array.IndexOf(fragment, ASCIICode.CR, posCR + 1)) > 0)
                        {
                            //TODO 对普通字段的处理有问题

                            byte[] fraghead = new byte[posCR - posCaret];
                            Array.Copy(fragment, posCaret, fraghead, 0, posCR - posCaret);

                            //Content-Disposition
                            if (index == 1)
                            {
                                //TODO 文件名会有乱码的问题，如何解决？文件名的编码格式不同
                                //内容描述信息
                                //Content-Disposition: form-data; name="{field.name}"; filename="{file.name}"
                                string disposition = StringEncoder.Decode(fraghead);

                                string[] headers = disposition.Split(new[] { ((char)ASCIICode.SEMICOLON).ToString() }, StringSplitOptions.RemoveEmptyEntries);
                                fieldName = headers[1].Trim().Replace("name=", "").Trim((char)ASCIICode.QUOTE);

                                if (headers.Length > 2)
                                {
                                    fileName = headers[2];
                                    if (fileName.Contains("filename="))
                                    {
                                        isFile = true;
                                        fileName = fileName.Trim().Replace("filename=", "").Trim((char)ASCIICode.QUOTE);
                                    }
                                }
                            }
                            //Content-Type
                            if (index == 2)
                            {

                            }

                            //跳过分割字节段
                            posCaret = posCR + 2;
                            index++;

                            //连续两个CR
                            if (Array.IndexOf(fragment, ASCIICode.CR, posCR + 1) != posCR + 2)
                            {

                            }
                            else
                            {
                                break;
                            }
                        }
                        //解析数据
                        if (data.Length > posCR + 4)
                        {
                            //内容末端还有两个字符(\r\n)，故此处再减两个字节
                            var postField = new byte[fragment.Length - (posCR + 4 + 2)];
                            Array.Copy(fragment, posCR + 4, postField, 0, postField.Length);

                            if (isFile)
                            {
                                File file = new File
                                {
                                    FileName = HtmlEncoder.EntityCodeToString(fileName),
                                    FileData = postField,
                                    FileIndex = req.Files.Count,
                                    FieldName = fieldName
                                };
                                //file.FileTempSavePath = AppDomain.CurrentDomain.BaseDirectory + @"Temp\" + file.FileName.ToString();
                                req.Files.Add(file);
                                ////写入临时目录
                                //FileStream fs = new FileStream(file.FileTempSavePath, FileMode.OpenOrCreate);
                                //int length = fragbody.Length - (posCR + 4);
                                //int blockSize = 1024;
                                //int count = length / blockSize;
                                //byte[] blockData = new byte[blockSize];
                                //int mode = length % blockSize;
                                //for (int i = 0; i < count; i++)
                                //{
                                //    Array.Copy(fragbody, posCR + 4 + i * blockData.Length, blockData, 0, blockData.Length);
                                //    fs.Write(blockData, 0, blockData.Length);
                                //}
                                //if (mode > 0)
                                //{
                                //    Array.Resize(ref blockData, mode);
                                //    Array.Copy(fragbody, posCR + 4 + blockSize * count, blockData, 0, blockData.Length);
                                //    fs.Write(blockData, 0, blockData.Length);
                                //}
                                //fs.Flush();
                                //fs.Dispose();
                            }
                            else
                            {
                                //var postField = new byte[fragbody.Length - (posCR + 4)];
                                req.Query.Add(fieldName, StringEncoder.Decode(postField));
                            }
                            //TODO 置空对象
                            fragment = null;
                            postField = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occurs while parsing multipart/form-data:{0}", ex.Message);
                }
            }
        }
        //TODO 后续扩展实现，暂时不实现
        /// <summary>
        /// 分析请求体 文本类型
        ///     application/json
        ///     text/plain
        ///     text/xml
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParsePayloadText(ref HttpRequest req,ref byte[] data)
        {

        }
        /// <summary>
        /// 解析头属性
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParseHeaders(ref HttpRequest req, byte[] data)
        {
            HeaderProperty hp = HeaderProperty.Parse(data);
            req.Headers.Add(hp.PropertyName, hp.PropertyValue);
            #if DEBUG
                Console.WriteLine("{0}:{1}",hp.PropertyTag,hp.PropertyValue);
            #endif
        }
        /// <summary>
        /// 解析UserAgent
        /// </summary>
        /// <param name="req"></param>
        private static void ParseHeaderUserAgent(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.UserAgent.PropertyName))
            {
                req.UserAgent = req.Headers.GetValue(HeaderProperty.UserAgent.PropertyName);
            }
        }
        /// <summary>
        /// 解析请求目标主机地址
        /// </summary>
        /// <param name="req"></param>
        private static void ParseHeaderHost(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.Host.PropertyName))
            {
                req.Host = req.Headers.GetValue(HeaderProperty.Host.PropertyName);
            }
        }
        /// <summary>
        /// 解析来源页面地址
        /// </summary>
        /// <param name="req"></param>
        private static void ParseHeaderReferer(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.Referer.PropertyName))
            {
                req.Referer = req.Headers.GetValue(HeaderProperty.Referer.PropertyName);
            }
        }
        /// <summary>
        /// 解析接受语言排序
        /// 	语法：<code>zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3</code>
        /// 	分割符号为,
        /// </summary>
        /// <param name="req"></param>
        private static void ParseHeaderAcceptLanguage(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.AcceptLanguage.PropertyName))
            {
                var language = req.Headers.GetValue(HeaderProperty.AcceptLanguage.PropertyName) ?? "";
                var languages = language.Split(new char[] { (char)ASCIICode.COMMA }, StringSplitOptions.RemoveEmptyEntries);
                req.AcceptLanguage = new LanguagePriority[languages.Length];
                try
                {
                    for (int i = 0; i < languages.Length; i++)
                    {
                        var lan = languages[i];
                        var lans = lan.Split(new char[] { (char)ASCIICode.COMMA }, StringSplitOptions.RemoveEmptyEntries);
                        req.AcceptLanguage[i] = new LanguagePriority()
                        {
                            LanguageName = lans[0],
                            Weight = lans.Length > 1 ? int.Parse(lans[1]) : 1
                        };
                    }
                }
                catch
                {

                }
            }
        }
        /// <summary>
        /// 解析文档类型
        /// </summary>
        /// <param name="req"></param>
        private static void ParseHeaderContentType(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.ContentType.PropertyName))
            {
                var contentType = req.Headers.GetValue(HeaderProperty.ContentType.PropertyName);
                string[] cts = contentType.Split(new[] { ((char)ASCIICode.SEMICOLON).ToString() + ((char)ASCIICode.SPACE).ToString() }, StringSplitOptions.RemoveEmptyEntries);
                if (cts.Length > 0)
                {
                    req.ContentType = cts[0];
                    if (cts.Length > 2)
                    {
                        req.ContentCharset = cts[2];
                    }
                }
            }
        }
        /// <summary>
        /// 解析文档请求范围 
        /// <para>
        ///     此功能主要应用于断点续传
        /// </para>
        /// </summary>
        /// <param name="req"></param>
        private static void ParseHeaderRange(ref HttpRequest req)
        {

        }
        /// <summary>
        /// 解析Cookie
        /// </summary>
        /// <param name="req"></param>
        private static void ParseCookie(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.Cookie.PropertyName))
            {
                req.Cookies = RequestCookie.Parse(req.Headers.GetValue(HeaderProperty.Cookie.PropertyName));
            }
        }

        /// <summary>
        /// 解析首行数据
        /// 方法 查询 协议
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParseRequestLine(ref HttpRequest req, byte[] data)
        {
            //解析起始行
            req.RequestLineData = data;
            req.RequestLineString = Encoding.UTF8.GetString(data);
            string[] sFirst = req.RequestLineString.Split(new[] { (char)ASCIICode.SPACE }, StringSplitOptions.None);
            //方法 查询 协议 
            string sMethod = sFirst[0];
            string sUrl = sFirst[1];
            string sProtocol = sFirst[2];

            RequestMethod rm = AbsClassEnum.Get<RequestMethod>(sMethod);
            req.Method = rm;

            //判断方法是否是已知方法
            if (Equals(req.Method,null))
            {
                req.Method = new RequestMethod(sMethod);
            }
            string[] urls = sUrl.Split(new[] { (char)ASCIICode.QUESTION }, StringSplitOptions.RemoveEmptyEntries);
            req.Path = urls[0];
            if (urls.Length > 1)
            {
                req.QueryString = urls[1];
                req.Query = UrlEncoder.ParseQuery(urls[1]);
            }

            string sProtoType = sProtocol.Substring(0, sProtocol.IndexOf((char)ASCIICode.DIVIDE));
            string sProtoVersion = sProtocol.Substring(sProtocol.IndexOf((char)ASCIICode.DIVIDE) + 1);

            req.Protocol = AbsClassEnum.Get<ProtocolType>(sProtoType);
            req.Version = AbsClassEnum.Get<HttpVersion>(sProtoVersion);

        }
        //TODO 此功能需要重试以进行验证
        /// <summary>
        /// 数据重播
        /// </summary>
        /// <param name="headerKeyNameUpperCase">头属性名传大写</param>
        /// <returns></returns>
        public byte[] GetBuffer(bool headerKeyNameUpperCase)
        {
            List<byte> data = new List<byte>();
            //注入状态信息
            data.AddRange(GetRequestLine());
            data.AddRange(TransformHeader.Carriage);
            //注入默认头部
            data.AddRange(_headers.GetBuffer(headerKeyNameUpperCase));
            //注入Cookie
            data.AddRange(_cookies.GetBuffer());
            //注入分割符
            data.AddRange(TransformHeader.Carriage);
            //注入响应包体
            data.AddRange(_body);
            return data.ToArray();
        }
        /// <summary>
        /// 数据重播
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            return GetBuffer(false);
        }
        /// <summary>
        /// 设置头信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpRequest SetHeader(string key,string value)
        {
            _headers.Add(key, value);
            return this;
        }
        /// <summary>
        /// 设置头信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpRequest SetHeader(HeaderProperty key,string value)
        {
            _headers.Add(key, value);
            return this;
        }
        /// <summary>
        /// 应用头信息集合 覆盖所有头属性
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public HttpRequest SetHeaders(TransformHeader headers)
        {
            if (headers != null)
            {
                _headers = headers;
            }
            return this;
        }
        /// <summary>
        /// 设置请求路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public HttpRequest SetPath(string path)
        {
            Path = path;
            return this;
        }
        /// <summary>
        /// 设置请求方法
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public HttpRequest SetMethod(RequestMethod method)
        {
            Method = method;
            return this;
        }
        /// <summary>
        /// 设置请求HTTP协议
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public HttpRequest SetProtocol(HttpVersion version)
        {
            Version = version;
            return this;
        }
        /// <summary>
        /// 设置Body数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public HttpRequest SetBody(byte[] data)
        {
            if (data != null)
            {
                _body = data;
                ContentLength = _body.Length.ToString();
                SetHeader(HeaderProperty.ContentLength, data.Length.ToString());
            }
            return this;
        }
        /// <summary>
        /// 生成请求行
        /// </summary>
        /// <returns></returns>
        public byte[] GetRequestLine()
        {
            return StringEncoder.Encode(string.Format("{0} {1} HTTP/{2}", Method.Name, Path, Version.Version));
        }
        /// <summary>
        /// 设置URI信息，分别注入到<see cref="HeaderProperty.Host"/>和<see cref="HeaderProperty.Referer"/>两个头属性中
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public HttpRequest SetUri(UriInfo uri)
        {
            //注入Host
            SetHeader(HeaderProperty.Host, string.IsNullOrEmpty(uri.Domain) ? uri.Host : uri.Domain);
            SetHeader(HeaderProperty.Referer, uri.Url);
            return this;
        }
        ~HttpRequest()
        {
            //PackData = null;
            RequestLineData = null;
            _body = null;
            _headers = null;
            _cookies = null;
            HeaderData = null;
            _files = null;
            AcceptLanguage = null;
        }
    }
    /// <summary>
    /// HttpRequest扩展
    /// </summary>
    public static class HttpRequestExtension
    {
        
    }
}