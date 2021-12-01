using System.Net;
using Mozi.Network.Core;

namespace Mozi.NTP
{
    /// <summary>
    /// UDP通讯缓冲对象
    /// </summary>
    public class UDPStateObject : StateObject
    {
        public EndPoint RemoteEndPoint;
    }
}
