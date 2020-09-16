using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.Stores;

namespace Mozi.HttpEmbedded.WebDav.MethodHandlers
{
    /// <summary>
    ///  <c>MKCOL</c> WebDAV��չ����
    /// </summary>
    internal class MkCol : WebDavMethodHandlerBase, IMethodHandler
    {
        /// <summary>
        /// ��Ӧ����
        /// </summary>
        /// <param name="server"><see cref="DavServer" /> </param>
        /// <param name="context"> 
        ///     <see cref="HttpContext" /> 
        /// </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="DavServer" /></param>
        public StatusCode ProcessRequest(DavServer server, HttpContext context, IWebDavStore store)
        {
            if (context.Request.Body.Length > 0)
                return StatusCode.UnsupportedMediaType;
            IWebDavStoreCollection collection = GetParentCollection(store, context.Request.Path.Replace("/","\\"));
            UrlTree ut = new UrlTree(context.Request.Path);
            string collectionName = UrlEncoder.Decode(ut.Last().TrimEnd('/', '\\'));
            if (collection.GetItemByName(collectionName) != null)
                return StatusCode.MethodNotAllowed;

            collection.CreateCollection(collectionName);

            return StatusCode.Success;
        }
    }
}