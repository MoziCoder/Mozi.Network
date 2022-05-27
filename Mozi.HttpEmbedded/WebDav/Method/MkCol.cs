using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>MKCOL</c> WebDAV扩展方法
    /// </summary>
    internal class MkCol : MethodHandlerBase, IMethodHandler
    {
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="WebDAVServer" /> </param>
        /// <param name="context"> 
        ///     <see cref="HttpContext" /> 
        /// </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="WebDAVServer" /></param>
        public StatusCode Invoke(WebDAVServer server, HttpContext context, IWebDavStore store)
        {
            if (context.Request.Body.Length == 0)
            {
                IWebDavStoreCollection collection = GetParentCollection(store, context.Request.Path.Replace("/", "\\"));
                UrlTree ut = new UrlTree(context.Request.Path);
                string collectionName = UrlEncoder.Decode(ut.Last().TrimEnd('/', '\\'));
                if (collection.GetItemByName(collectionName) != null)
                    return StatusCode.MethodNotAllowed;

                collection.CreateCollection(collectionName);

                return StatusCode.Success;
            }
            return StatusCode.UnsupportedMediaType;
        }
    }
}