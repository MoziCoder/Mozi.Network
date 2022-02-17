using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>DELETE</c> WebDAV扩展方法
    /// </summary>
    internal class Delete : MethodHandlerBase, IMethodHandler
    {
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="DavServer" /> </param>
        /// <param name="context">
        /// <see cref="HttpContext" /> 
        ///  </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="DavServer" /></param>
        public StatusCode HandleRequest(DavServer server, HttpContext context, IWebDavStore store)
        {
            //父目录资源清单
            IWebDavStoreCollection collection = GetParentCollection(store, context.Request.Path);

            //子目录资源清单
            IWebDavStoreItem item = GetItemFromCollection(collection, context.Request.Path);

            //移除项目
            collection.Delete(item);
            return StatusCode.Success;
        }
    }
}