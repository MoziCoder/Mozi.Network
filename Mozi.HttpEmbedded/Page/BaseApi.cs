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
}
