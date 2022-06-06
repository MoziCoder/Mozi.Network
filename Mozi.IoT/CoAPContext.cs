using System;

namespace Mozi.IoT
{
    
    /// <summary>
    /// 请求上下文对象
    /// </summary>
    /// <remarks>
    /// 注意Request和Response并不是紧密关联，响应不是必须的，服务端有可能在收到响应后不会做出任何回应
    /// </remarks>
    public class CoAPContext : IDisposable
    {

        private bool _disposedValue;

        /// <summary>
        /// 请求对象
        /// </summary>
        public CoAPPackage Request { get; set; }
        /// <summary>
        /// 响应对象
        /// </summary>
        public CoAPPackage Response { get; set; }
        /// <summary>
        /// 服务器对象
        /// </summary>
        public CoAPPeer Server { get; set; }
        /// <summary>
        /// 客户端地址
        /// </summary>
        public string ClientAddress { get; set; }
        /// <summary>
        /// 客户端端口
        /// </summary>
        public int ClientPort { get; set; }
        /// <summary>
        /// 析构
        /// </summary>
        ~CoAPContext()
        {
            Dispose(disposing: false);
        }
        /// <summary>
        /// 销毁
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {

                }
                Request = null;
                Response = null;
                _disposedValue = true;
            }
        }
        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
