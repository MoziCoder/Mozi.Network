using System;
using System.IO;
using System.Security.Principal;
using System.Threading;
using Mozi.HttpEmbedded.WebDav.Storage.Base;

namespace Mozi.HttpEmbedded.WebDav.Storage.Disk
{

    public class DiskStoreItem : WebDavStoreItemBase
    {

        protected readonly WindowsIdentity Identity;

        private readonly DiskStoreCollection _parentCollection;
        private readonly string _path;

        protected DiskStoreItem(DiskStoreCollection parentCollection, string path) : base(parentCollection, path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            _parentCollection = parentCollection;
            _path = path;
            Identity = (WindowsIdentity)Thread.GetData(Thread.GetNamedDataSlot(WebDAVServer.HttpUser));
        }

        public override string ItemPath
        {
            get
            {
                return _path;
            }
        }

        public new string Name
        {
            get
            {
                return Path.GetFileName(_path);
            }

            set
            {
                throw new InvalidOperationException("Unable to rename item");
            }
        }
        public string Ext
        {
            get
            {
                return Path.GetExtension(_path);
            }
        }
        public new bool IsCollection
        {
            get
            {
                //取资源的属性清单
                FileAttributes attr = System.IO.File.GetAttributes(_path);

                //区分文档和目录
                return (attr & FileAttributes.Directory) == FileAttributes.Directory;
            }
        }

        public override DateTime CreateDate
        {
            get
            {

                return System.IO.File.GetCreationTime(_path);
            }
        }

        public override DateTime ModifyDate
        {
            get
            {

                return System.IO.File.GetLastWriteTime(_path);
            }
        }

        public new int Hidden
        {
            get
            {
                DirectoryInfo dir = new DirectoryInfo(_path);
                return (dir.Attributes & FileAttributes.Hidden) != 0 ? 1 : 0;
            }
        }
    }
}