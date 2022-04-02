using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 此命名空间的所有方法均来自<see cref="Mozi.HttpEmbedded.Encode"/>
/// </summary>
namespace Mozi.IoT.Encode
{
    //DONE 解决URL汉字转码的问题
    /// <summary>
    /// URL特殊字符转码
    /// </summary>
    public static class UrlEncoder
    {
        private static readonly char[] From = { ' ', '"', '#', '%', '&', '(', ')', '+', ',', '/', ':', ';', '<', '=', '>', '?', '@', '\\', '|' };

        private static readonly string[] To = { "%20", "%22", "%23", "%25", "%26", "%28", "%29", "%2B", "%2C", "%2F", "%3A", "%3B", "%3C", "%3D", "%3E", "%3F", "%40", "%5C", "%7C" };

        /// <summary>
        /// URL特殊字符解码
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Decode(string data)
        {
            //特殊字符
            for (int i = 0; i < To.Length; i++)
            {
                string s = To[i];
                if (data.Contains(s))
                {
                    data = data.Replace(s, To[i]);
                }
            }

            //其他宽字符
            var startIndex = -1;
            var endIndex = -1;
            for (int i = 0; i < data.Length; i++)
            {
                var item = data[i];
                if (startIndex == -1 && item.Equals('%'))
                {
                    startIndex = i;
                }
                if (item.Equals('%') && !data[i + 2].Equals('%'))
                {
                    endIndex = i + 2;
                }
            }
            if (startIndex != -1)
            {

                var groupMath = data.Substring(startIndex, endIndex - startIndex + 1);
                data = data.Replace(groupMath, StringEncoder.Decode(Hex.From(groupMath.Replace('%', ' '))));

            }
            return data;
        }
        /// <summary>
        /// URL特殊字符编码
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Encode(string data)
        {
            for (int i = 0; i < From.Length; i++)
            {
                char s = From[i];
                if (data.Contains(s.ToString()))
                {
                    data = data.Replace(s.ToString(), To[i]);
                }
            }
            return data;
        }
        /// <summary>
        /// 解析查询字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseQuery(string data)
        {
            data = Decode(data);
            Dictionary<string, string> res = new Dictionary<string, string>();
            string[] querys = data.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in querys)
            {
                string[] kp = item.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (kp.Length > 0)
                {
                    var key = kp[0];
                    var value = kp.Length > 1 ? kp[1] : "";
                    res.Add(key, value);
                }
            }
            return res;
        }
    }
    /// <summary>
    /// URI信息类 带DNS解析功能
    /// </summary>
    public class UriInfo
    {
        public string Url { get; set; }
        public string Protocol { get; set; }
        public string Host { get; set; }
        public string Domain { get; set; }
        public int Port { get; set; }
        public string Path { get; set; }
        public string[] Paths { get; set; }
        public string Query { get; set; }
        public string[] Queries { get; set; }

        public UriInfo()
        {
            Paths = new string[0];
            Queries = new string[0];
        }
        /// <summary>
        /// 分析请求链接，带DNS解析功能
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static UriInfo Parse(string url)
        {
            UriInfo uri = new UriInfo();

            string address = "", sPort = "", path = "";

            string[] paths;
            bool isDomain = false;

            Regex reg = new Regex("^[a-zA-Z]+://((([a-zA-Z0-9\\.-]+){2,})|(\\[?[a-zA-Z0-9\\.:]+){2,}\\]?)(:\\d+)?((/[a-zA-Z0-9-\\.%]{0,}){0,}(\\?)?([[%=a-zA-Z0-9-\\*\\./]+(&)?){0,})$");
            Regex regProto = new Regex("[a-zA-Z]+(?=://)");
            Regex regHost = new Regex("(?<=\\://)(([a-zA-Z0-9-]+\\.?){2,}|(\\[?[a-zA-Z0-9-\\.:]+){2,}]?)(:\\d+)?");

            Regex regIPV4 = new Regex("^(\\d+\\.\\d+\\.\\d+\\.\\d+(?=:\\d+))|(\\d+\\.\\d+\\.\\d+\\.\\d+)$");
            Regex regIPV6 = new Regex("^((?<=\\[)(([a-zA-Z0-9]+(\\.|:)?){2,})(?=\\]))|([a-zA-Z0-9]+(\\.|:)?){2,}$");
            Regex regDomain = new Regex("^(([a-zA-Z0-9-]+(\\.)?){2,})|(([a-zA-Z0-9-]+(\\.)?){2,}(?=:\\d+))$");

            Regex regPath = new Regex("(?<=(://(([a-zA-Z0-9-]+\\.?){2,}|(\\[?[a-zA-Z0-9-\\.:]+){2,}]?)(:\\d+)?))(/[a-zA-Z0-9-\\.%]{0,}){1,}((?=\\?))?");
            Regex regQuery = new Regex("(?<=\\?)([%=a-zA-Z0-9-\\*\\./]+(&)?){1,}");

            if (reg.IsMatch(url))
            {
                uri.Url = url;

                //分离协议类型
                uri.Protocol = regProto.Match(url).Value;

                //分离域名和端口
                address = regHost.Match(url).Value;

                //IPV4
                if (regIPV4.IsMatch(address))
                {
                    uri.Host = regIPV4.Match(address).Value;
                    sPort = address.Replace(uri.Host, "").Replace(":", "");
                    //domain
                }
                else if (regDomain.IsMatch(address))
                {
                    uri.Host = regDomain.Match(address).Value;
                    uri.Domain = uri.Host;
                    sPort = address.Replace(uri.Host, "").Replace(":", "");
                    isDomain = true;
                }
                //IPV6
                else
                {
                    uri.Host = regIPV6.Match(address).Value;

                    sPort = address.Replace(uri.Host, "").Replace("[]:", "");
                }

                int port;
                if (!int.TryParse(sPort, out port))
                {
                    uri.Port = 0;
                }
                else
                {
                    uri.Port = port;
                }

                if (isDomain)
                {
                    uri.Host = DNSResolver.GetDomainAddress(uri.Host);
                }

                //分离路径地址
                uri.Path = regPath.Match(url).Value;
                paths = uri.Path.Split(new char[] { '/' });
               
                if (paths.Length > 0)
                {
                    uri.Paths = new string[paths.Length - 1];
                    Array.Copy(paths, 1, uri.Paths, 0, uri.Paths.Length);
                }
                //分离查询参数
                uri.Query = regQuery.Match(url).Value;
                if (uri.Query.Length > 0)
                {
                    uri.Queries = uri.Query.Split(new char[] { '&' });
                }
            }
            else
            {
                throw new Exception($"URL格式不正确{url}");
            }
            return uri;
        }
    }
}