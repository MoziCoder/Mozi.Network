using System.Net;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    /// <summary>
    /// UDP通讯缓冲对象
    /// </summary>
    public class UDPStateObject : StateObject
    {
        /// <summary>
        /// 终结点信息
        /// </summary>
        public EndPoint RemoteEndPoint;
    }
}
