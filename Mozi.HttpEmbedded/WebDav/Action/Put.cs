using System.IO;
using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>PUT</c> WebDAV扩展方法
    /// </summary>
    internal class Put : MethodHandlerBase, IMethodHandler
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
            IWebDavStoreCollection parentCollection = GetParentCollection(store, context.Request.Path);

            UrlTree ut = new UrlTree(context.Request.Path);
            string itemName = UrlEncoder.Decode(ut.Last().TrimEnd('/', '\\'));

            IWebDavStoreItem item = parentCollection.GetItemByName(itemName);
            IWebDavStoreDocument doc;
            if (item != null)
            {
                doc = item as IWebDavStoreDocument;
                if (doc == null)
                    return StatusCode.MethodNotAllowed;
            }
            else
            {
                doc = parentCollection.CreateDocument(itemName);
            }

            if (context.Request.Body.Length < 0)
                return StatusCode.LengthRequired;

            using (Stream stream = doc.OpenWrite(false))
            {
                stream.Write(context.Request.Body, 0, context.Request.Body.Length);
            }

            return StatusCode.Success;
        }
    }
}