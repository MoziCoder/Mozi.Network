using System.Text.RegularExpressions;

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
        /// </summary>
        /// <param name="pack"></param>
        public virtual void SendMessage(string host,int port,CoAPPackage pack)
        {
            _socket.SendTo(pack.Pack(), host, port);
        }

        //public virtual void Send(string url,)
        ///<summary>
        /// <list type="table">
        /// <listheader>URI格式</listheader>
        /// <item><term>格式1：</term>coap://{host}[:{port}]/{path}</item>
        /// <item><term>格式2：</term>coap://{host}[:{port}]/{path}[?{query}]</item>
        /// <item><term>格式3：</term>coap://{host}[:{port}]/{path1}[/{path2}]...[/{pathN}][?{query}]</item>
        /// </list>
        /// </summary>
        public void Get(string url)
        {
            string host = "";
            int port = CoAPProtocol.Port;
            Regex reg = new Regex("^(coap|coaps)://\\w+(:\\d+)?(/[a-zA-Z0-9%]+){1,}(\\?[a-zA-Z0-9%])?");
            Match mt = reg.Match(url);
        }

        public void Post(string url)
        {

        }
    }
}
