using Mozi.HttpEmbedded.WebDav.Exceptions;
using System;

namespace Mozi.HttpEmbedded.WebDav.Storage.Base
{
    public class WebDavStoreItemBase : IWebDavStoreItem
    {
        private readonly IWebDavStoreCollection _parentCollection;
        private string _name;

        protected WebDavStoreItemBase(IWebDavStoreCollection parentCollection, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            _parentCollection = parentCollection;
            _name = name;
        }
        public IWebDavStoreCollection ParentCollection
        {
            get
            {
                return _parentCollection;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                string fixedName = (value ?? string.Empty).Trim();
                if (fixedName == _name) return;
                if (!OnNameChanging(_name, fixedName))
                    throw new WebDavForbiddenException();
                string oldName = _name;
                _name = fixedName;
                OnNameChanged(oldName, _name);
            }
        }

        public virtual DateTime CreateDate
        {
            get
            {
                return DateTime.Now;
            }
        }

        public virtual DateTime ModifyDate
        {
            get
            {
                return DateTime.Now;
            }
        }

        public virtual string ItemPath
        {
            get { return string.Empty; }
        }

        public bool IsCollection
        {
            get
            {
                return true;
            }
        }

        public int Hidden
        {
            get
            {
                return 0;
            }
        }

        protected virtual bool OnNameChanging(string oldName, string newName)
        {
            return true;
        }

        protected virtual void OnNameChanged(string oldName, string newName)
        {
        }
    }
}