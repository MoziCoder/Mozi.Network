using System;
using System.Collections.Generic;
using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded.Cookie
{
    /// <summary>
    /// Cookie
    /// </summary>
    /// <remarks>
    /// name/domain/path 3要素为Cookie的约束条件
    /// </remarks>
    public class HttpCookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        [Obsolete("HTTP/1.1不再使用此值")]
        public string Expires { get; set; }
        public int MaxAge { get; set; }
        public string Path { get; set; }
        public string Domain { get; set; }
        public SameSiteMode SameSite { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
        //Secure和HttpOnly

        public HttpCookie()
        {
            Path = "/";
            Domain = "";
        }

        public HttpCookie(string name, string value) : this()
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// 转为Cookie格式字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> data = new List<string> { string.Format("{0}={1}", Name, Value) };
            if (MaxAge != 0)
            {
                data.Add(string.Format("max-age={0}", MaxAge));
            }
            data.Add(string.Format("path={0}", Path ?? "/"));
            data.Add(string.Format("domain={0}", Domain ?? ""));
            if (SameSite != SameSiteMode.None)
            {
                data.Add(string.Format("SameSite={0}", Enum.GetName(typeof(SameSiteMode), SameSite)));
            }
            if (Secure)
            {
                data.Add("Secure");
            }
            if (HttpOnly)
            {
                data.Add("HttpOnly");
            }
            //TODO 分割符号为“; ”
            return string.Join(new string(new[] {  (char)ASCIICode.SEMICOLON ,(char)ASCIICode.SPACE}), data);
        }

        /// <summary>
        /// 解析Cookie
        /// </summary>
        /// <param name="setCookieData"></param>
        /// <returns></returns>
        public static HttpCookie Parse(string setCookieData)
        {
            HttpCookie cookie = new HttpCookie();
            string[] options = setCookieData.Split(new[] { (char)ASCIICode.SEMICOLON }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < options.Length; i++)
            {
                string r = options[i];
                if (i == 0)
                {
                    string[] kp = r.Split(new[] { (char)ASCIICode.EQUAL }, StringSplitOptions.RemoveEmptyEntries);
                    if (kp.Length == 2)
                    {
                        cookie.Name = kp[0];
                        cookie.Value = kp[1];
                    }
                }
                else{
                    //secure
                    if (r.Trim().Equals("secure", StringComparison.OrdinalIgnoreCase))
                    {
                        cookie.Secure = true;
                    }
                    //httponly
                    if (r.Trim().Equals("httponly", StringComparison.OrdinalIgnoreCase))
                    {
                        cookie.HttpOnly = true;
                    }
                    //max-age
                    if (r.Trim().StartsWith("max-age", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] kp = r.Split(new[] { (char)ASCIICode.EQUAL }, StringSplitOptions.RemoveEmptyEntries);
                        if (kp.Length == 2)
                        {
                            cookie.MaxAge = int.Parse(kp[1]);
                        }
                    }
                    //path
                    if (r.Trim().StartsWith("path", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] kp = r.Split(new[] { (char)ASCIICode.EQUAL }, StringSplitOptions.RemoveEmptyEntries);
                        if (kp.Length == 2)
                        {
                            cookie.Path = kp[1];
                        }
                    }
                    //domain
                    if (r.Trim().StartsWith("domain", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] kp = r.Split(new[] { (char)ASCIICode.EQUAL }, StringSplitOptions.RemoveEmptyEntries);
                        if (kp.Length == 2)
                        {
                            cookie.Domain = kp[1];
                        }
                    }
                    //samesite
                    if (r.Trim().StartsWith("samesite", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] kp = r.Split(new[] { (char)ASCIICode.EQUAL }, StringSplitOptions.RemoveEmptyEntries);
                        if (kp.Length == 2)
                        {
                            cookie.SameSite = (SameSiteMode)Enum.Parse(typeof(SameSiteMode),kp[1]);
                        }
                    }
                }
            }
            return cookie;
        }
    }
    /// <summary>
    /// HttpCookie扩展类
    /// </summary>
    public static class HttpCookieExtension
    {
        /// <summary>
        /// 判断Cookie是否为同一属性 name/domain/path
        /// </summary>
        /// <param name="hc"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool Equals(this HttpCookie hc, HttpCookie target)
        {
            if (hc != null && target != null)
            {
                hc.Path = hc.Path ?? "/";
                target.Path = hc.Path ?? "/";
                hc.Domain = hc.Domain ?? "";
                target.Domain = target.Domain ?? "";
                return hc.Name.Equals(target.Name) && hc.Path.Equals(target.Name) && hc.Domain.Equals(target.Domain);
            }
            return false;
        }
    }
    public enum SameSiteMode
    {
        //
        // 摘要:
        //     No SameSite field will be set, the client should follow its default cookie policy.
        Unspecified = -1,
        //
        // 摘要:
        //     Indicates the client should disable same-site restrictions.
        None = 0,
        //
        // 摘要:
        //     Indicates the client should send the cookie with "same-site" requests, and with
        //     "cross-site" top-level navigations.
        Lax = 1,
        //
        // 摘要:
        //     Indicates the client should only send the cookie with "same-site" requests.
        Strict = 2
    }
    /// <summary>
    /// 请求Cookie
    /// </summary>
    public class RequestCookie
    {
        private Dictionary<string, string> _data = new Dictionary<string, string>();

        private void Append(string name, string value)
        {
            if (_data.ContainsKey(name))
            {
                _data[name] = value;
            }
            else
            {
                _data.Add(name, value);
            }
        }
        /// <summary>
        /// 解析请求Cookie
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static RequestCookie Parse(string data)
        {
            RequestCookie hc = new RequestCookie();
            string[] kps = data.Split(new char[] { (char)ASCIICode.SEMICOLON }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var kp in kps)
            {
                var startIndex = kp.IndexOf((char)ASCIICode.EQUAL);
                hc.Append(kp.Substring(0, startIndex).Trim(), kp.Substring(startIndex + 1));
            }
            return hc;
        }
        /// <summary>
        /// 取出缓冲区数据
        /// </summary>
        /// <returns></returns>
        public List<byte> GetBuffer()
        {
            List<byte> data = new List<byte>();
            foreach (var cookie in _data)
            {
                data.AddRange(StringEncoder.Encode(string.Format("{0}: {1}={2}", HeaderProperty.Cookie.PropertyName, cookie.Key, cookie.Value)));
                data.AddRange(TransformHeader.Carriage);
            }
            return data;
        }
        public string Get(string name)
        {
            return _data[name];
        }

        public string this[string ind]
        {
            get { return _data[ind]; }
        }
    }
    /// <summary>
    /// 响应Cookie
    /// </summary>
    public class ResponseCookie
    {
        private readonly List<HttpCookie> _cookies = new List<HttpCookie>();
        /// <summary>
        /// 设置Cookie
        /// <para>
        ///     Cookie三要素为name,domain,path。如果仍需要设置其他属性，请直接使用函数的返回值进行设置。
        /// </para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpCookie Set(string key, string value)
        {
            var cookie = _cookies.Find(x => x.Name.Equals(key));
            if (cookie==null)
            {
                cookie = new HttpCookie() { Name = key};
                _cookies.Add(cookie);
            }
            cookie.Value =  UrlEncoder.Encode(value);
            return cookie;
        }
        /// <summary>
        /// 设置Cookie
        /// <para>
        ///     Cookie三要素为name,domain,path。如果仍需要设置其他属性，请直接使用函数的返回值进行设置。
        /// </para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpCookie Set(string key,string domain,string path,string value)
        {
            var cookie = _cookies.Find(x => x.Name.Equals(key)&&x.Path.Equals(path)&&x.Domain.Equals(domain));
            if (cookie == null)
            {
                cookie = new HttpCookie() { Name = key, Domain=domain,Path=path };
                _cookies.Add(cookie);
            }
            cookie.Value = value;
            return cookie;
        }
        /// <summary>
        /// 删除Cookie
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HttpCookie Delete(string key)
        {
            HttpCookie cookie = Set(key, "");
            cookie.MaxAge = -1000;
            return cookie;
        }
        /// <summary>
        /// 取出缓冲区数据
        /// </summary>
        /// <returns></returns>
        public List<byte> GetBuffer()
        {
            List<byte> data = new List<byte>();
            foreach (var cookie in _cookies)
            {
                data.AddRange(StringEncoder.Encode(string.Format("{0}: {1}", HeaderProperty.SetCookie.PropertyName, cookie.ToString())));
                data.AddRange(TransformHeader.Carriage);
            }
            return data;
        }
    }
}