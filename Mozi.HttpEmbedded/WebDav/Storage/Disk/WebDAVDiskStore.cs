using System;
using System.IO;

namespace Mozi.HttpEmbedded.WebDav.Storage.Disk
{
    public sealed class WebDavDiskStore : IWebDavStore
    {
        private readonly string _rootPath;

        public WebDavDiskStore(string rootPath)
        {
            if (rootPath == null)
            {
                throw new ArgumentNullException(rootPath);
            }
            else
            {
                if (!Path.IsPathRooted(rootPath))
                {
                    rootPath = AppDomain.CurrentDomain.BaseDirectory + rootPath + "\\";
                }
                if (!Directory.Exists(rootPath))
                {
                    throw new DirectoryNotFoundException(rootPath);
                }
            }
            _rootPath = rootPath;
        }

        public string RootPath
        {
            get
            {
                return _rootPath;
            }
        }

        public IWebDavStoreCollection Root
        {
            get
            {
                return new DiskStoreCollection(null, _rootPath);
            }
        }
    }
}