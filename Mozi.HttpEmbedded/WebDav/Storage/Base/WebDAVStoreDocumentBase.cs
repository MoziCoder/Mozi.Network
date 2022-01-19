using Mozi.HttpEmbedded.Source;

namespace Mozi.HttpEmbedded.WebDav.Storage.Base
{
    public class WebDavStoreDocumentBase : WebDavStoreItemBase
    {
        protected WebDavStoreDocumentBase(IWebDavStoreCollection parentCollection, string name) : base(parentCollection, name)
        {
        }

        public string MimeType
        {
            get
            {
                //TODO 此处需要跟踪
                return Mime.GetContentType(Name);
            }
        }
    }
}