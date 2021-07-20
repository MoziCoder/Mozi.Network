using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Mozi.HttpEmbedded
{

    //TODO 还有很多问题没有解决，暂时不能使用这种模式

    /// <summary>  
    /// 客户端连接数量变化时触发  
    /// </summary>  
    /// <param name="num">当前增加客户的个数(用户退出时为负数,增加时为正数,一般为1)</param>  
    /// <param name="token">增加用户的信息</param>  
    public delegate void OnClientNumberChange(int num, StateObject token);

    public class SocketServerICOP
    {
        private int _maxConnectionCount;    //最大连接数  
        private int _receiveBufferSize;    //最大接收字节数  
        private BufferManager _bufferManager;
        private const int opsToAlloc = 2;
        private Socket _sc;            //监听Socket  
        private SocketEventPool _socketArgsStack;
        private int _clientCount =0;              //连接的客户端数量  
        private Semaphore _maxAcceptedClientsSignal;

        protected int _iport = 80;

        List<StateObject> _clients; //客户端列表  

        /// <summary>
        /// 服务器启动事件
        /// </summary>
        public event ServerStart OnServerStart;
        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public event ClientConnect OnClientConnect;
        /// <summary>
        /// 客户端断开连接时间
        /// </summary>
        public event ClientDisConnect AfterClientDisConnect;
        /// <summary>
        /// 数据接收开始事件
        /// </summary>
        public event ReceiveStart OnReceiveStart;
        /// <summary>
        /// 数据接收完成事件
        /// </summary>
        public event ReceiveEnd AfterReceiveEnd;
        /// <summary>
        /// 服务器停用事件
        /// </summary>
        public event AfterServerStop AfterServerStop;

        /// <summary>  
        /// 获取客户端列表  
        /// </summary>  
        public List<StateObject> ClientList { get { return _clients; } }

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
        public SocketServerICOP(int maxConnections, int receiveBufferSize)
        {
            _clientCount = 0;
            _maxConnectionCount = maxConnections;
            _receiveBufferSize = receiveBufferSize; 
            _bufferManager = new BufferManager(receiveBufferSize * maxConnections * opsToAlloc, receiveBufferSize);
            _socketArgsStack = new SocketEventPool(maxConnections);
            _maxAcceptedClientsSignal = new Semaphore(maxConnections, maxConnections);
            Init();
        }

        public SocketServerICOP():this(65535,1024*4)
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
            _clients = new List<StateObject>();
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
        public bool StartServer(int port)
        {
            _iport = port;
            try
            {

                _clients.Clear();
                _sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endpoint = new IPEndPoint(System.Net.IPAddress.Any, _iport);
                //允许端口复用
                _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _sc.Bind(endpoint);
                _sc.Listen(_maxConnectionCount);
                StartAccept(null);
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
        public void StopServer()
        {
            foreach (StateObject token in _clients)
            {
                try
                {
                    token.WorkSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception) { }
            }
            try
            {
                _sc.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) 
            { 

            }

            _sc.Close();
            int c_count = _clients.Count;
            lock (_clients) 
            {
                _clients.Clear(); 
            }
        }


        public void CloseClient(StateObject token)
        {
            try
            {
                token.WorkSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) 
            { 

            }
        }

        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }

            _maxAcceptedClientsSignal.WaitOne();
            if (!_sc.AcceptAsync(acceptEventArg))
            {
                ProcessAccept(acceptEventArg);
            }
        }

        // This method is the callback method associated with Socket.AcceptAsync   
        // operations and is invoked when an accept operation is complete  
        //  
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                Interlocked.Increment(ref _clientCount);
                SocketAsyncEventArgs readEventArgs = _socketArgsStack.Pop();
                StateObject userToken = (StateObject)readEventArgs.UserToken;
                userToken.WorkSocket = e.AcceptSocket;
                userToken.ConnectTime = DateTime.Now;
                userToken.IP = ((IPEndPoint)(e.AcceptSocket.RemoteEndPoint)).Address.ToString();
                userToken.RemotePort = ((IPEndPoint)(e.AcceptSocket.RemoteEndPoint)).Port;

                lock (_clients) {
                    _clients.Add(userToken); 
                }
                if (!e.AcceptSocket.ReceiveAsync(readEventArgs))
                {
                    ProcessReceive(readEventArgs);
                }
            }
            catch (Exception ex)
            {
               
            }

            // Accept the next connection request  
            if (e.SocketError == SocketError.OperationAborted) return;
            StartAccept(e);
        }


        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
          
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }
        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                // check if the remote host closed the connection  
                StateObject token = (StateObject)e.UserToken;
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                { 
                    byte[] data = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);
                    lock (token.Buffer)
                    {
                        token.Data.AddRange(data);
                    }
                    //do
                    //{
                    //    //判断包的长度  
                    //    byte[] lenBytes = token.Buffer.GetRange(0, 4).ToArray();
                    //    int packageLen = BitConverter.ToInt32(lenBytes, 0);
                    //    if (packageLen > token.Buffer.Count - 4)
                    //    {   //长度不够时,退出循环,让程序继续接收  
                    //        break;
                    //    }

                    //    //包够长时,则提取出来,交给后面的程序去处理  
                    //    byte[] rev = token.Buffer.GetRange(4, packageLen).ToArray();
                    //    //从数据池中移除这组数据  
                    //    lock (token.Buffer)
                    //    {
                    //        token.Buffer.RemoveRange(0, packageLen + 4);
                    //    }
                    //    //将数据包交给后台处理,这里你也可以新开个线程来处理.加快速度.  

                    //    InvokeAfterReceiveEnd(token, token.WorkSocket);
                    //    //这里API处理完后,并没有返回结果,当然结果是要返回的,却不是在这里, 这里的代码只管接收.  
                    //    //若要返回结果,可在API处理中调用此类对象的SendMessage方法,统一打包发送.不要被微软的示例给迷惑了.  
                    //} while (token.Buffer.Count > 4);
                    e.SetBuffer(e.Offset, e.BytesTransferred);
                    //继续接收. 为什么要这么写,请看Socket.ReceiveAsync方法的说明  

                    InvokeAfterReceiveEnd(token, token.WorkSocket);
                    if (!token.WorkSocket.ReceiveAsync(e))
                    {
                        this.ProcessReceive(e);
                    }
                }
                else
                {
                    InvokeAfterReceiveEnd(token, token.WorkSocket);
                    //CloseClientSocket(e);
                }
            }
            catch (Exception xe)
            {
               
            }
        }
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client  
                StateObject token = (StateObject)e.UserToken;
                // read the next block of data send from the client  
                bool willRaiseEvent = token.WorkSocket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        //关闭客户端  
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            StateObject token = e.UserToken as StateObject;

            lock (_clients) {
                _clients.Remove(token); 
            }
            try
            {
                token.WorkSocket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception) 
            { 

            }
            token.WorkSocket.Close(); 
            Interlocked.Decrement(ref _clientCount);
            _maxAcceptedClientsSignal.Release();
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
            if (token == null || token.WorkSocket == null || !token.WorkSocket.Connected)
            {
                return;
            }
            try
            {
                //对要发送的消息,制定简单协议,头4字节指定包的大小,方便客户端接收(协议可以自己定)  
                byte[] buff = new byte[message.Length + 4];
                byte[] len = BitConverter.GetBytes(message.Length);
                Array.Copy(len, buff, 4);
                Array.Copy(message, 0, buff, 4, message.Length);
                //token.Socket.Send(buff);  //这句也可以发送, 可根据自己的需要来选择  
                //新建异步发送对象, 发送消息  
                SocketAsyncEventArgs sendArg = new SocketAsyncEventArgs();
                sendArg.UserToken = token;
                sendArg.SetBuffer(buff, 0, buff.Length);  //将数据放置进去.  
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
            if (item == null) { 
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
    class BufferManager
    {
        int m_numBytes;                 // the total number of bytes controlled by the buffer pool  
        byte[] m_buffer;                // the underlying byte array maintained by the Buffer Manager  
        Stack<int> m_freeIndexPool;     //   
        int m_currentIndex;
        int m_bufferSize;

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
                if ((m_numBytes - m_bufferSize) < m_currentIndex)
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
