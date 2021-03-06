using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mozi.HttpEmbedded.Common;

namespace Mozi.HttpEmbedded.Encode
{
    //DONE ????URL????ת????????
    /// <summary>
    /// URL?????ַ?ת??
    /// </summary>
    public static class UrlEncoder
    {
        private static readonly char[] From = { ' ', '"', '#', '%', '&', '(', ')', '+', ',', '/', ':', ';', '<', '=', '>', '?', '@', '\\', '|' };

        private static readonly string[] To = { "%20", "%22", "%23", "%25", "%26", "%28", "%29", "%2B", "%2C", "%2F", "%3A", "%3B", "%3C", "%3D", "%3E", "%3F", "%40", "%5C", "%7C" };

        /// <summary>
        /// URL?????ַ?????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Decode(string data)
        {
            //?????ַ?
            for (int i = 0; i < To.Length; i++)
            {
                string s = To[i];
                if (data.Contains(s))
                {
                    data = data.Replace(s, To[i]);
                }
            }

            //???????ַ?
            var startIndex = -1;
            var endIndex = -1;
            for(int i = 0; i < data.Length; i++)
            {
                var item = data[i];
                if (startIndex == -1 && item.Equals((char)ASCIICode.PERCENT))
                {
                    startIndex = i;
                }
                if (item.Equals((char)ASCIICode.PERCENT) &&!data[i+2].Equals((char)ASCIICode.PERCENT))
                {
                    endIndex = i+2;
                }
            }
            if (startIndex != -1)
            {

                var groupMath = data.Substring(startIndex, endIndex - startIndex + 1);
                data = data.Replace(groupMath, StringEncoder.Decode(Hex.From(groupMath.Replace((char)ASCIICode.PERCENT, (char)ASCIICode.SPACE))));

            }
            return data;
        }
        /// <summary>
        /// URL?????ַ?????
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
        /// ??????ѯ?ַ???
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseQuery(string data)
        {
            data = Decode(data);
            Dictionary<string, string> res = new Dictionary<string, string>();
            string[] querys = data.Split(new[] { (char)ASCIICode.AND }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in querys)
            {
                string[] kp = item.Split(new[] { (char)ASCIICode.EQUAL }, StringSplitOptions.RemoveEmptyEntries);
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
    /// URI??Ϣ?? ??DNS????????
    /// </summary>
    public class UriInfo
    {
        /// <summary>
        /// ԭʼ??ַ??Ϣ
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// ??ַ??Э????Ϣ
        /// </summary>
        public string Protocol { get; set; }
        /// <summary>
        /// ??????Ϣ
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// ??????Ϣ
        /// </summary>
        public string Domain { get; set; }
        /// <summary>
        /// ?˿???Ϣ
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// ·????Ϣ
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        ///  ?ָ???·??
        /// </summary>
        public string[] Paths { get; set; }
        /// <summary>
        /// ??ѯ?ַ???
        /// </summary>
        public string Query { get; set; }
        /// <summary>
        /// ??ѯ?ַ?????ֵ??
        /// </summary>
        public Dictionary<string,string> Queries { get; set; }

        public UriInfo()
        {
            Paths = new string[0];
            Queries = new Dictionary<string, string>();
        }

        /// <summary>
        /// ???????????ӣ???DNS????????
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <remarks>???????????ַ?????ת?壬?????ַ????д???#??????????ȡ#֮ǰ??????</remarks>
        public static UriInfo Parse(string url)
        {
            if (url.IndexOf("#") > 0)
            {
                url = url.Substring(0, url.IndexOf("#"));
            }
            UriInfo uri = new UriInfo();

            string address = "", sPort = "", path = "";

            string[] paths;
            bool isDomain = false;
            
            Regex reg = new Regex("^[a-zA-Z]+://((([a-zA-Z0-9\\.-]+){2,})|(\\[?[a-zA-Z0-9\\.:]+){2,}\\]?)(:\\d+)?((/[a-zA-Z0-9-\\.%]{0,}){0,}(\\?)?([%=a-zA-Z0-9\\.-_#]+(&)?){0,})$");
            Regex regProto = new Regex("[a-zA-Z]+(?=://)");
            Regex regHost = new Regex("(?<=\\://)(([a-zA-Z0-9-]+\\.?){2,}|(\\[?[a-zA-Z0-9-\\.:]+){2,}]?)(:\\d+)?");

            Regex regIPV4 = new Regex("^(\\d+\\.\\d+\\.\\d+\\.\\d+(?=:\\d+))|(\\d+\\.\\d+\\.\\d+\\.\\d+)$");
            Regex regIPV6 = new Regex("^((?<=\\[)(([a-zA-Z0-9]+(\\.|:)?){2,})(?=\\]))|([a-zA-Z0-9]+(\\.|:)?){2,}$");
            Regex regDomain = new Regex("^(([a-zA-Z0-9-]+(\\.)?){2,})|(([a-zA-Z0-9-]+(\\.)?){2,}(?=:\\d+))$");

            Regex regPath = new Regex("(?<=(://(([a-zA-Z0-9-]+\\.?){2,}|(\\[?[a-zA-Z0-9-\\.:]+){2,}]?)(:\\d+)?))(/[a-zA-Z0-9-\\.%]{0,}){1,}((?=\\?))?");
            Regex regQuery = new Regex("(?<=\\?)([%=a-zA-Z0-9\\.-_#]+(&)?){1,}");

            if (reg.IsMatch(url))
            {
                uri.Url = url;

                //????Э??????
                uri.Protocol = regProto.Match(url).Value;

                //?????????Ͷ˿?
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

                //????·????ַ
                uri.Path = regPath.Match(url).Value;
                paths = uri.Path.Split(new char[] { '/' });

                if (paths.Length > 0)
                {
                    uri.Paths = new string[paths.Length - 1];
                    Array.Copy(paths, 1, uri.Paths, 0, uri.Paths.Length);
                }
                //??????ѯ????
                uri.Query = regQuery.Match(url).Value;
                if (uri.Query.Length > 0)
                {
                    string[] query = uri.Query.Split(new char[] { '&' });
                    foreach(var q in query)
                    {
                        string[] kv = q.Split(new char[] { '=' });
                        uri.Queries.Add(kv[0], kv.Length >= 2 ? kv[1] : null);
                    }
                }
            }
            else
            {
                throw new Exception($"URL??ʽ????ȷ{url}");
            }
            return uri;
        }
    }
}