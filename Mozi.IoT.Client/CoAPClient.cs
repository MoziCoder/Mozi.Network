namespace Mozi.IoT
{
    /// <summary>
    /// CoAP客户端
    /// </summary>
    public class CoAPClient : CoAPPeer
    {
        private ushort _remotePort = CoAPProtocol.Port;
        private string _remotehost = "";

        /// <summary>
        /// 远端服务器地址
        /// </summary>
        public string RemoteAddress { get { return _remotehost; } protected set { _remotehost = value; } }

        /// <summary>
        /// 远端服务器端口
        /// </summary>
        public ushort RemotePort { get { return _remotePort; } protected set { _remotePort = value; } }

        public CoAPClient()
        {
            //配置本地服务口地址
        }
        /// <summary>
        /// 设置本地端口，默认为<see cref=" CoAPProtocol.Port"/>
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public CoAPClient SetPort(ushort port)
        {
            BindPort = port;
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
        /// 发送请求消息 
        /// <list type="table">
        /// <item><term>格式：</term>coap://{host}:[{port}]/{path}</item>
        /// <item><term>格式：</term>coap://{host}:[{port}]/{path}[?{query}]</item>
        /// <item><term>格式：</term>coap://{host}:[{port}]/{path1}[/{path2}]..[/{pathN}][?{query}]</item>
        /// </list>
        /// </summary>
        /// <param name="pack"></param>
        public void SendMessage(string url,CoAPPackage pack)
        {

        }
    }
}
