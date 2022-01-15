using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Mozi.IoT
{

    //TODO 还有很多问题没有解决，暂时不能使用这种模式

    public class UDPSocketIOCP
    {
        private int _maxConnectionCount;    //最大连接数  
        private int _receiveBufferSize;    //最大接收字节数  
        private BufferManager _bufferManager;
        private const int opsToAlloc = 2;
        private Socket _sc;            //监听Socket  
        private SocketEventPool _socketArgsStack;
        private Semaphore _maxAcceptedClientsSignal;

        protected int _iport = 80;

        /// <summary>
        /// 服务器启动事件
        /// </summary>
        public  ServerStart OnServerStart;

        /// <summary>
        /// 数据接收开始事件
        /// </summary>
        public  ReceiveStart OnReceiveStart;
        /// <summary>
        /// 数据接收完成事件
        /// </summary>
        public  ReceiveEnd AfterReceiveEnd;
        /// <summary>
        /// 服务器停用事件
        /// </summary>
        public  AfterServerStop AfterServerStop;

        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get { return _iport; }
        }
        public Socket SocketMain
        {
            get { return _sc; }
        }

        /// <summary>  
        /// 构造函数  
        /// </summary>  
        /// <param name="maxConnections">最大连接数</param>  
        /// <param name="receiveBufferSize">缓存区大小</param>  
        public UDPSocketIOCP(int maxConnections, int receiveBufferSize)
        {
            _maxConnectionCount = maxConnections;
            _receiveBufferSize = receiveBufferSize;
            _bufferManager = new BufferManager(receiveBufferSize * maxConnections * opsToAlloc, receiveBufferSize);
            _socketArgsStack = new SocketEventPool(maxConnections);
            _maxAcceptedClientsSignal = new Semaphore(maxConnections, maxConnections);
            Init();
        }

        public UDPSocketIOCP() : this(1000, 1024 * 4)
        {

        }

        /// <summary>  
        /// 初始化  
        /// </summary>  
        public void Init()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds   
            // against memory fragmentation  
            _bufferManager.InitBuffer();
            // preallocate pool of SocketAsyncEventArgs objects  
            SocketAsyncEventArgs readWriteEventArg;

            for (int i = 0; i < _maxConnectionCount; i++)
            {
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readWriteEventArg.UserToken = new StateObject();

                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object  
                _bufferManager.SetBuffer(readWriteEventArg);
                // add SocketAsyncEventArg to the pool  
                _socketArgsStack.Push(readWriteEventArg);
            }
        }


        /// <summary>  
        /// 启动服务器 
        /// </summary>  
        /// <param name="port"></param>  
        public bool Start(int port)
        {
            _iport = port;
            try
            {

                _sc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, _iport);
                //允许端口复用
                _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.IpTimeToLive, 32);
                _sc.Bind(endpoint);
                StartReceive();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>  
        /// 关闭服务器  
        /// </summary>  
        public void Shutdown()
        {
            try
            {
                _sc.Shutdown(SocketShutdown.Both);
                _sc.Close();
            }
            catch (Exception)
            {

            }

            _sc.Close();
        }

        private void StartReceive()
        {
            SocketAsyncEventArgs receiveSocketArgs = _socketArgsStack.Pop();
            StateObject so = new StateObject();
            receiveSocketArgs.UserToken = so;
            receiveSocketArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            receiveSocketArgs.SetBuffer(so.Buffer, 0, so.Buffer.Length);
            _sc.ReceiveFromAsync(receiveSocketArgs);
        }



        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                case SocketAsyncOperation.ReceiveFrom:
                    ProcessReceive(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
            StartReceive();
        }
        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                    // check if the remote host closed the connection  
                    StateObject token = (StateObject)e.UserToken;
                    token.IP = ((IPEndPoint)e.RemoteEndPoint).Address.ToString();
                    token.RemotePort = ((IPEndPoint)e.RemoteEndPoint).Port;
                    byte[] data = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);
                    lock (token.Buffer)
                    {
                        token.Data.AddRange(data);
                    }

                    //e.SetBuffer(e.Offset, e.BytesTransferred);
                    //继续接收. 为什么要这么写,请看Socket.ReceiveAsync方法的说明  

                    InvokeAfterReceiveEnd(token, token.WorkSocket);
                //if (!token.WorkSocket.ReceiveAsync(e))
                //{
                //    ProcessReceive(e);
                //}
                CloseClientSocket(e);
            }
            catch (Exception xe)
            {
                CloseClientSocket(e);
            }
        }

        //关闭客户端  
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            StateObject token = e.UserToken as StateObject;
            e.UserToken = new StateObject();
            _socketArgsStack.Push(e);
        }



        /// <summary>  
        /// 对数据进行打包,然后再发送  
        /// </summary>  
        /// <param name="token"></param>  
        /// <param name="message"></param>  
        /// <returns></returns>  
        public void SendMessage(StateObject token, byte[] message)
        {
            try
            {
                //发送消息  
                SocketAsyncEventArgs sendArg = new SocketAsyncEventArgs();
                sendArg.UserToken = token;
                sendArg.SetBuffer(message, 0, message.Length);
                token.WorkSocket.SendAsync(sendArg);
            }
            catch (Exception e)
            {

            }
        }

        private void InvokeAfterReceiveEnd(StateObject so, Socket client)
        {
            //RemoveClientSocket(so);
            if (AfterReceiveEnd != null)
            {
                AfterReceiveEnd(this,
                    new DataTransferArgs()
                    {
                        Data = so.Data.ToArray(),
                        IP = so.IP,
                        Port = so.RemotePort,
                        Socket = so.WorkSocket,
                        Client = client,
                        State = so
                    });
            }
        }

        /// <summary>
        /// 向指定地址发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void SendTo(byte[] buffer, string host, int port)
        {
            
            _sc.SendTo(buffer, new IPEndPoint(IPAddress.Parse(host), port));
        }
    }
    public class SocketEventPool
    {
        ConcurrentStack<SocketAsyncEventArgs> _stack;

        public SocketEventPool(int capacity)
        {
            _stack = new ConcurrentStack<SocketAsyncEventArgs>();
        }

        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            }
            lock (_stack)
            {
                _stack.Push(item);
            }
        }
        public SocketAsyncEventArgs Pop()
        {
            SocketAsyncEventArgs result;
            lock (_stack)
            {
                _stack.TryPop(out result);
            }
            return result;
        }

        // The number of SocketAsyncEventArgs instances in the pool  
        public int Count
        {
            get { return _stack.Count; }
        }

        public void Clear()
        {
            _stack.Clear();
        }
    }
    internal class BufferManager
    {
        private int m_numBytes;                 // the total number of bytes controlled by the buffer pool  
        private byte[] m_buffer;                // the underlying byte array maintained by the Buffer Manager  
        private Stack<int> m_freeIndexPool;     //   
        private int m_currentIndex;
        private int m_bufferSize;

        public BufferManager(int totalBytes, int bufferSize)
        {
            m_numBytes = totalBytes;
            m_currentIndex = 0;
            m_bufferSize = bufferSize;
            m_freeIndexPool = new Stack<int>();
        }

        // Allocates buffer space used by the buffer pool  
        public void InitBuffer()
        {
            // create one big large buffer and divide that   
            // out to each SocketAsyncEventArg object  
            m_buffer = new byte[m_numBytes];
        }

        // Assigns a buffer from the buffer pool to the   
        // specified SocketAsyncEventArgs object  
        //  
        // <returns>true if the buffer was successfully set, else false</returns>  
        public bool SetBuffer(SocketAsyncEventArgs args)
        {

            if (m_freeIndexPool.Count > 0)
            {
                args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_bufferSize);
            }
            else
            {
                if (m_numBytes - m_bufferSize < m_currentIndex)
                {
                    return false;
                }
                args.SetBuffer(m_buffer, m_currentIndex, m_bufferSize);
                m_currentIndex += m_bufferSize;
            }
            return true;
        }

        // Removes the buffer from a SocketAsyncEventArg object.    
        // This frees the buffer back to the buffer pool  
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            m_freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
