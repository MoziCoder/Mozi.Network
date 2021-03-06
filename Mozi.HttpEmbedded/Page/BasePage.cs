namespace Mozi.HttpEmbedded.Page
{
    //TODO 待模版引擎实现完成后再实现此模块
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

    internal class Info : BasePage
    {
        public override void Get()
        {
            throw new System.NotImplementedException();
        }

        public override void Post()
        {
            throw new System.NotImplementedException();
        }

        public override void RedirectTo(string url)
        {
            throw new System.NotImplementedException();
        }

        public override void Render()
        {
            throw new System.NotImplementedException();
        }
    }
}
