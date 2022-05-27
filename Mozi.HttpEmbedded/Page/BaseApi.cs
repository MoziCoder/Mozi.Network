namespace Mozi.HttpEmbedded.Page
{
    /// <summary>
    /// Api抽象类
    /// </summary>
    public abstract class BaseApi
    {
        /// <summary>
        /// 绑定的上下文对象
        /// </summary>
        public HttpContext Context { get; internal set; }
    }
    /// <summary>
    /// Restfull Api抽象类
    /// </summary>
    internal abstract class BaseRestApi
    {
        public abstract ResponseMessage Get();
        public abstract ResponseMessage Post();
        public abstract ResponseMessage Put();
        public abstract ResponseMessage Delete();
    }
}
