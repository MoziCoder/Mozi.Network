using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>COPY</c> WebDAV扩展方法
    /// </summary>
    internal class Copy : MethodHandlerBase, IMethodHandler
    {
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="WebDAVServer" /> </param>
        /// <param name="context"> 
        /// <see cref="HttpContext" /> 
        ///  </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="WebDAVServer" /></param>
        public StatusCode Invoke(WebDAVServer server, HttpContext context, IWebDavStore store)
        {
            IWebDavStoreItem source = WebDavExtensions.GetStoreItem(context.Request.Path, store);
            if (source is IWebDavStoreDocument || source is IWebDavStoreCollection)
            {
                string destPath = context.Request.Headers["destItem"];
                IWebDavStoreCollection destParentCollection = GetParentCollection(store, destPath);

                bool copyContent = GetDepthHeader(context.Request) != 0;
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

                    if (destItem is IWebDavStoreCollection)
                    {
                        destParentCollection.Delete(destItem);
                    }

                    isNew = false;
                }

                destParentCollection.CopyItemTo(source, destName, copyContent);
                return isNew ? StatusCode.Created : StatusCode.NoContent;
            }
            else
            {
                return StatusCode.Forbidden;
            }
        }
    }
}