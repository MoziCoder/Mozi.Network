using System;

namespace Mozi.HttpEmbedded.WebDav.Storage
{
    public interface IWebDavStoreItem
    {
        IWebDavStoreCollection ParentCollection
        {
            get;
        }
        string Name
        {
            get;
            set;
        }

        string ItemPath
        {
            get;
        }

        bool IsCollection
        {
            get;
        }

        DateTime CreationDate
        {
            get;
        }

        DateTime ModificationDate
        {
            get;
        }

        int Hidden
        {
            get;
        }
    }
}