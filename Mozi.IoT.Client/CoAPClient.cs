using System;
using System.Text.RegularExpressions;
using System.Net;

namespace Mozi.IoT
{


    //CoAP拥塞机制由请求方进行自主控制，故请求方需要实现拥塞控制算法
    /// <summary>
    /// CoAP客户端
    /// </summary>
    public class CoAPClient : CoAPPeer
    {
        private bool _randomPort = true;
        //private ushort _remotePort = CoAPProtocol.Port;
        //private string _remotehost = "";

        ///// <summary>
        ///// 远端服务器地址
        ///// </summary>
        //public string RemoteAddress { get { return _remotehost; } protected set { _remotehost = value; } }

        ///// <summary>
        ///// 远端服务器端口
        ///// </summary>
        //public ushort RemotePort { get { return _remotePort; } protected set { _remotePort = value; } }

        public CoAPClient() 
        {
            //配置本地服务口地址
        }
        /// <summary>
        /// 设置本地端口，默认为<see cref=" CoAPProtocol.Port"/>,如果不设置则使用随机端口
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public CoAPClient SetPort(ushort port)
        {
            BindPort = port;
            _randomPort = false;
            return this;
        }
        ///// <summary>
        ///// 设置远端地址
        ///// 格式{host}:{port}
        ///// </summary>
        ///// <param name="remoteAddress"></param>
        ///// <returns></returns>
        //public CoAPClient SetRemote(string remoteAddress)
        //{
        //    string[] info = remoteAddress.Split(new char[] { ':' });
        //    SetRemoteHost(info[0]);
        //    SetRemotePort(ushort.Parse(info[1]));
        //    return this;
        //}

        //public CoAPClient SetRemotePort(ushort port)
        //{
        //    RemotePort = port;
        //    return this;
        //}
        //public CoAPClient SetRemoteHost(string host)
        //{
        //    RemoteAddress = host;
        //    return this;
        //}
        /// <summary>
        /// 发送请求消息,此方法为高级方法。如果对协议不够了解，请不要调用。
        /// DOMAIN地址请先转换为IP地址，然后填充到Uri-Host选项中
        /// </summary>
        /// <param name="pack"></param>
        public virtual void SendMessage(string host,int port,CoAPPackage pack)
        {
            _socket.SendTo(pack.Pack(), host, port);
        }

        ///<summary>
        /// 填入指定格式的URI，如果是域名，程序会调用DNS进行解析
        /// <list type="table">
        /// <listheader>URI格式:{host}-IPV4地址,IPV6地址,Domain域名;{path}-路径,请使用REST样式路径;{query}为查询参数字符串</listheader>
        ///     <item><term>格式1：</term>coap://{host}[:{port}]/{path}</item>
        ///     <item><term>格式2：</term>coap://{host}[:{port}]/{path}[?{query}]</item>
        ///     <item><term>格式3：</term>coap://{host}[:{port}]/{path1}[/{path2}]...[/{pathN}][?{query}]</item> 
        /// </list>
        /// </summary>
        public void Get(string url,CoAPMessageType msgType)
        {
            string proto = "", host = "", address = "",domain="",sPort="", path = "", query = "";
            int port = CoAPProtocol.Port;
            string[] paths, queries;
            bool isDomain=false;

            CoAPPackage cp = new CoAPPackage();
            cp.Code = CoAPRequestMethod.Get;
            cp.Token = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            cp.MesssageId = 12345;
            cp.MessageType = msgType??CoAPMessageType.Confirmable;

            Regex reg = new Regex("^(coap|coaps)://((([a-zA-Z0-9\\.-]+){2,})|(\\[?[a-zA-Z0-9\\.:]+){2,}\\]?)(:\\d+)?((/[a-zA-Z0-9-\\.%]+){0,}(\\?)?([%=a-zA-Z0-9]+(&)?){0,})$");
            Regex regProto = new Regex("(coap|coaps)(?=://)");
            Regex regHost = new Regex("(?<=\\://)(([a-zA-Z0-9-]+\\.?){2,}|(\\[?[a-zA-Z0-9-\\.:]+){2,}]?)(:\\d+)?");

            Regex regIPV4 = new Regex("^(\\d+\\.\\d+\\.\\d+\\.\\d+(?=:\\d+))|(\\d+\\.\\d+\\.\\d+\\.\\d+)$");
            Regex regIPV6 = new Regex("^((?<=\\[)(([a-zA-Z0-9]+(\\.|:)?){2,})(?=\\]))|([a-zA-Z0-9]+(\\.|:)?){2,}$");
            Regex regDomain = new Regex("^(([a-zA-Z0-9-]+(\\.)?){2,})|(([a-zA-Z0-9-]+(\\.)?){2,}(?=:\\d+))$");

            Regex regPath = new Regex("(?<=(://(([a-zA-Z0-9-]+\\.?){2,}|(\\[?[a-zA-Z0-9-\\.:]+){2,}]?)(:\\d+)?))(/[a-zA-Z0-9-\\.%]+){1,}((?=\\?))?");
            Regex regQuery = new Regex("(?<=\\?)([%=a-zA-Z0-9-]+(&)?){1,}");

            if (reg.IsMatch(url))
            {

                //分离协议类型
                proto=regProto.Match(url).Value;

                //分离域名和端口
                address = regHost.Match(url).Value;

                //IPV4
                if (regIPV4.IsMatch(address))
                {
                    host = regIPV4.Match(address).Value;
                    sPort = address.Replace(host, "").Replace(":","");

                //domain
                }else if (regDomain.IsMatch(address)){
                    host = regDomain.Match(address).Value;
                    domain = host;
                    sPort = address.Replace(host, "").Replace(":","");
                    isDomain = true;
                }
                //IPV6
                else
                {
                    host = regIPV6.Match(address).Value;
                    
                    sPort= address.Replace(host, "").Replace("[]:","");
                }
                if(!int.TryParse(sPort,out port))
                {
                    port = 0;
                }

                if (isDomain)
                {
                    host = GetDomainAddress(host);
                    cp.SetOption(CoAPOptionDefine.UriHost, domain);
                }

                if (port > 0 && (port != CoAPProtocol.Port || port != CoAPProtocol.SecurePort))
                {
                    cp.SetOption(CoAPOptionDefine.UriPort, (uint)port);
                }

                //分离路径地址
                path = regPath.Match(url).Value;
                paths = path.Split(new char[] { '/' });
                if (paths.Length > 0)
                {
                    //第一项为空分割，故抛弃
                    for (int i = 1; i < paths.Length; i++)
                    {
                        cp.SetOption(CoAPOptionDefine.UriPath, paths[i]);
                    }
                }

                //分离查询参数
                query = regQuery.Match(url).Value;

                if (query.Length > 0)
                {
                    queries = query.Split(new char[] { '&' });

                    for (int i = 0; i < queries.Length; i++)
                    {
                        cp.SetOption(CoAPOptionDefine.UriQuery, queries[i]);
                    }
                }
                if (!string.IsNullOrEmpty(host))
                {
                    SendMessage(host, port==0?CoAPProtocol.Port:port, cp);
                }
                else
                {
                    throw new Exception($"DNS无法解析指定的域名:{domain}");
                }
            }
            else
            {
                throw new Exception($"URL格式不正确{url}");
            }
        }

        public void Get(string url)
        {
            Get(url, CoAPMessageType.Confirmable);
        }
        public void Post(string url)
        {

        }
        /// <summary>
        /// DNS解析指定域名
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        protected virtual string GetDomainAddress(string domain)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(domain);
                IPAddress[] addresses = entry.AddressList;

                if (addresses.Length > 0)
                {
                    return addresses[0].ToString();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
