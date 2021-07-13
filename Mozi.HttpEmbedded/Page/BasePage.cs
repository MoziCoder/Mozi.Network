namespace Mozi.HttpEmbedded.Page
{
    /// <summary>
    /// 页面抽象类
    /// </summary>
    public abstract class BasePage
    {
        /// <summary>
        /// 上下文对象
        /// </summary>
        protected HttpContext Context { get; set; }
        /// <summary>
        /// 重定向
        /// </summary>
        /// <param name="url"></param>
        public abstract void RedirectTo(string url);
        /// <summary>
        /// GET方法
        /// </summary>
        public abstract void Get();
        /// <summary>
        /// POST方法
        /// </summary>
        public abstract void Post();

    }
}
