using System;

namespace Mozi.HttpEmbedded.WebDav.Storage.Base
{
    public abstract class WebDavStoreBase : IWebDavStore
    {
        private readonly IWebDavStoreCollection _root;

        protected WebDavStoreBase(IWebDavStoreCollection root)
        {
            if (root != null)
                _root = root;
            else
                throw new ArgumentNullException("root");
        }

        public IWebDavStoreCollection Root
        {
            get
            {
                return _root;
            }
        }
    }
}