namespace Mozi.HttpEmbedded.Page
{
    /// <summary>
    /// Api抽象类
    /// </summary>
    public abstract class BaseApi
    {
        public HttpContext Context { get; internal set; }

    }
}
