namespace Mozi.HttpEmbedded.Page
{
    //TODO PUT DELETE 方法一般被列为不安全的方法，故暂时不实现
    /// <summary>
    /// 页面抽象类
    /// </summary>
    internal abstract class BasePage
    {
        /// <summary>
        /// 上下文对象
        /// </summary>
        public HttpContext Context { get; set; }
        /// <summary>
        /// 重定向
        /// </summary>
        /// <param name="url"></param>
        public abstract void RedirectTo(string url);
        /// <summary>
        /// GET方法回调
        /// </summary>
        public abstract void Get();
        /// <summary>
        /// POST方法回调
        /// </summary>
        public abstract void Post();
        /// <summary>
        /// 页面首次加载时渲染
        /// </summary>
        public abstract void Render();
    }
}
