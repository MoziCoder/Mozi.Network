using System;

namespace Mozi.HttpEmbedded
{
    //HTTP通讯Request与Response是成对存在的，故需要构建上下文对象来联系请求与响应

    /// <summary>
    /// 请求上下文对象
    /// </summary>
    public class HttpContext : IDisposable
    {

        private bool _disposedValue;

        /// <summary>
        /// 请求对象
        /// </summary>
        public HttpRequest Request { get; set; }
        /// <summary>
        /// 响应对象
        /// </summary>
        public HttpResponse Response { get; set; }
        /// <summary>
        /// 服务器对象
        /// </summary>
        public HttpServer Server { get; set; }
        /// <summary>
        /// 客户端地址
        /// </summary>
        public string ClientAddress { get; set; }
        /// <summary>
        /// 是否已响应请求
        /// </summary>
        public bool Responsed { get { return Response != null; } }
        /// <summary>
        /// 客户端端口
        /// </summary>
        public int ClientPort { get; set; }

        //public DateTime ReceiveTime { get; set; }

        //public DateTime ReceiveCompleteTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        ~HttpContext()
        {
            Dispose(disposing: false);
        }
        /// <summary>
        /// 释放并回收
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
        /// 释放并回收
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
