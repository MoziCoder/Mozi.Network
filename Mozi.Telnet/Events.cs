using System.Net.Sockets;

namespace Mozi.Telnet
{
    public delegate void NegotiateEvent(Socket so, NegotiatePack np);
    public delegate void AuthEvent(Socket so, Session se);
    public delegate void SessionStart(Session se);
    public delegate void SessionStop(Session se);
    public delegate void CommandReceived(Socket so, string command);
    public delegate void DataEvent(Socket so, Session se, byte[] data);
}
