using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    /// </summary>
    public interface IMethodHandler
    {
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="WebDAVServer" /> </param>
        /// <param name="context"> 
        /// <see cref="HttpContext" /> 
        ///  </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="WebDAVServer" /></param>
        StatusCode Invoke(WebDAVServer server, HttpContext context, IWebDavStore store);
    }
}