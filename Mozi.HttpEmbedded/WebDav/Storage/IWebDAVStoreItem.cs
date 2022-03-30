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

        DateTime CreateDate
        {
            get;
        }

        DateTime ModifyDate
        {
            get;
        }

        int Hidden
        {
            get;
        }
    }
}