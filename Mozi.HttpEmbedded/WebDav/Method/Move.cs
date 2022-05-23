using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>MOVE</c> WebDAV扩展方法
    /// </summary>
    internal class Move : MethodHandlerBase, IMethodHandler
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
            var source = WebDavExtensions.GetStoreItem(context.Request.Path, store);

            string destPath = context.Request.Headers["destItem"];
            IWebDavStoreCollection destParentCollection = GetParentCollection(store, destPath);

            bool isNew = true;
            UrlTree ut = new UrlTree(destPath);
            string destName = UrlEncoder.Decode(ut.Last().TrimEnd('/', '\\'));
            IWebDavStoreItem destItem = destParentCollection.GetItemByName(destName);
            if (destItem != null)
            {
                if (source.ItemPath == destItem.ItemPath)
                {
                    return StatusCode.Forbidden;
                }

                if (!GetOverwriteHeader(context.Request))
                {
                    return StatusCode.PreconditionFailed;
                }

                destParentCollection.Delete(destItem);
                isNew = false;
            }

            destParentCollection.MoveItemTo(source, destName);

            return StatusCode.Success;
        }
    }
}