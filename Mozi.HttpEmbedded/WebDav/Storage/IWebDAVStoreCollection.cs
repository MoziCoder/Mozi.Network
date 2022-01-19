using System.Collections.Generic;

namespace Mozi.HttpEmbedded.WebDav.Storage
{
    public interface IWebDavStoreCollection : IWebDavStoreItem
    {
        IEnumerable<IWebDavStoreItem> Items
        {
            get;
        }

        IWebDavStoreItem GetItemByName(string name);

        IWebDavStoreCollection CreateCollection(string name);

        void Delete(IWebDavStoreItem item);

        IWebDavStoreDocument CreateDocument(string name);

        IWebDavStoreItem CopyItemTo(IWebDavStoreItem source, string destName, bool includeContent);

        IWebDavStoreItem MoveItemTo(IWebDavStoreItem source, string destName);
    }
}