using Mozi.HttpEmbedded.WebDav.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Mozi.HttpEmbedded.WebDav.Storage.Disk
{
    public sealed class DiskStoreCollection : DiskStoreItem, IWebDavStoreCollection
    {
        private readonly Dictionary<string, WeakReference> _items = new Dictionary<string, WeakReference>();


        public DiskStoreCollection(DiskStoreCollection parentCollection, string path) : base(parentCollection, path)
        {

        }

        public IEnumerable<IWebDavStoreItem> Items
        {
            get
            {
                HashSet<WeakReference> toDelete = new HashSet<WeakReference>(_items.Values);
                List<IWebDavStoreItem> items = new List<IWebDavStoreItem>();

                //查询目录
                List<string> directories = new List<string>();
                try
                {
                    string[] dirNames = Directory.GetDirectories(ItemPath);
                    foreach (string dirName in dirNames)
                    {
                        try
                        {
                            bool canread = CanReadDirectory(Path.Combine(ItemPath, dirName));
                            if (canread)
                            {
                                directories.Add(dirName);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }
                //查询子目录
                foreach (string subDirectoryPath in directories)
                {
                    string name = Path.GetFileName(subDirectoryPath);
                    DiskStoreCollection collection = null;

                    WeakReference wr;
                    if (_items.TryGetValue(name, out wr))
                    {
                        collection = wr.Target as DiskStoreCollection;
                        if (collection == null)
                            toDelete.Remove(wr);
                    }

                    if (collection == null)
                    {
                        collection = new DiskStoreCollection(this, subDirectoryPath);
                        _items[name] = new WeakReference(collection);
                    }

                    items.Add(collection);
                }
                //查询文件
                List<string> files = new List<string>();
                try
                {
                    files.AddRange(Directory.GetFiles(ItemPath).Where(fileName => CanReadFile(Path.Combine(ItemPath, fileName))));
                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }
                foreach (string filePath in files)
                {
                    string name = Path.GetFileName(filePath);
                    DiskStoreDocument document = null;

                    WeakReference wr;
                    if (_items.TryGetValue(name, out wr))
                    {
                        document = wr.Target as DiskStoreDocument;
                        if (document == null)
                            toDelete.Remove(wr);
                    }

                    if (document == null)
                    {
                        document = new DiskStoreDocument(this, filePath);
                        _items[name] = new WeakReference(document);
                    }
                    items.Add(document);
                }
                return items.ToArray();
            }
        }

        /// <summary>
        /// 检查文件读权限
        /// </summary>
        /// <param name="path"></param>
        /// <returns>
        /// </returns>
        private bool CanRead(string path)
        {
            if (System.IO.File.Exists(path))
                return CanReadFile(path);
            return Directory.Exists(path) && CanReadDirectory(path);
        }

        /// <summary>
        /// 检查文件读权限
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool CanReadFile(string path)
        {
            try
            {
                System.IO.File.Open(path, FileMode.Open, FileAccess.Read).Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 检查目录读权限
        /// </summary>
        /// <param name="path"></param>
        /// <returns>
        /// </returns>
        private static bool CanReadDirectory(string path)
        {
            bool readAllow = false;
            bool readDeny = false;
            DirectorySecurity accessControlList = Directory.GetAccessControl(path);
            if (accessControlList == null)
                return false;
            AuthorizationRuleCollection accessRules = accessControlList.GetAccessRules(true, true, typeof(SecurityIdentifier));

            foreach (FileSystemAccessRule rule in accessRules.Cast<FileSystemAccessRule>().Where(rule => (FileSystemRights.Read & rule.FileSystemRights) == FileSystemRights.Read))
            {
                switch (rule.AccessControlType)
                {
                    case AccessControlType.Allow:
                        readAllow = true;
                        break;
                    case AccessControlType.Deny:
                        readDeny = true;
                        break;
                }
            }

            return readAllow && !readDeny;
        }

        public IWebDavStoreItem GetItemByName(string name)
        {
            string path = Path.Combine(ItemPath, name);

            //using (Identity.Impersonate())
            {
                if (System.IO.File.Exists(path) && CanReadFile(path))
                    return new DiskStoreDocument(this, path);
                if (Directory.Exists(path) && CanReadDirectory(path))
                    return new DiskStoreCollection(this, path);
            }

            return null;
        }

        public IWebDavStoreCollection CreateCollection(string name)
        {
            string path = Path.Combine(ItemPath, name);

            try
            {
                WindowsImpersonationContext wic = Identity.Impersonate();
                Directory.CreateDirectory(path);
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }

            return GetItemByName(name) as IWebDavStoreCollection;
        }

        public void Delete(IWebDavStoreItem item)
        {
            DiskStoreItem diskItem = (DiskStoreItem)item;
            string itemPath = diskItem.ItemPath;
            if (item is DiskStoreDocument)
            {
                if (!System.IO.File.Exists(itemPath))
                    throw new WebDavNotFoundException();
                try
                {
                    WindowsImpersonationContext wic = Identity.Impersonate();
                    System.IO.File.Delete(itemPath);
                    wic.Undo();

                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }
            }
            else
            {
                if (!Directory.Exists(itemPath))
                    throw new WebDavNotFoundException();
                try
                {

                    WindowsImpersonationContext wic = Identity.Impersonate();
                    Directory.Delete(diskItem.ItemPath);
                    wic.Undo();
                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns>
        /// </returns>
        /// <exception cref="WebDavConflictException">项目存在</exception>
        /// <exception cref="WebDavUnauthorizedException">用户未认证或不可访问</exception>
        public IWebDavStoreDocument CreateDocument(string name)
        {
            string itemPath = Path.Combine(ItemPath, name);
            if (System.IO.File.Exists(itemPath) || Directory.Exists(itemPath))
                throw new WebDavConflictException();

            try
            {

                WindowsImpersonationContext wic = Identity.Impersonate();
                System.IO.File.Create(itemPath).Dispose();
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }

            DiskStoreDocument document = new DiskStoreDocument(this, itemPath);
            _items.Add(name, new WeakReference(document));
            return document;

        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destName"> <paramref name="source" /></param>
        /// <param name="includeContent"></param>
        /// <exception cref="WebDavUnauthorizedException">用户未认证或不可访问</exception>
        public IWebDavStoreItem CopyItemTo(IWebDavStoreItem source, string destName, bool includeContent)
        {
            string destItemItemPath = Path.Combine(ItemPath, destName);

            if (source.IsCollection)
            {
                try
                {
                    WindowsImpersonationContext wic = Identity.Impersonate();
                    DirectoryCopy(source.ItemPath, destItemItemPath, true);
                    wic.Undo();
                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }

                DiskStoreCollection collection = new DiskStoreCollection(this, destItemItemPath);
                _items.Add(destName, new WeakReference(collection));
                return collection;
            }


            try
            {
                WindowsImpersonationContext wic = Identity.Impersonate();
                System.IO.File.Copy(source.ItemPath, destItemItemPath, true);
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }

            DiskStoreDocument document = new DiskStoreDocument(this, destItemItemPath);
            _items.Add(destName, new WeakReference(document));
            return document;

        }

        /// <summary>
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {

            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }


            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            if (!copySubDirs) return;

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
                file.CopyTo(Path.Combine(destDirName, file.Name), false);

            foreach (DirectoryInfo subdir in dirs)
                DirectoryCopy(subdir.FullName, Path.Combine(destDirName, subdir.Name), copySubDirs);
        }


        public IWebDavStoreItem MoveItemTo(IWebDavStoreItem source, string destName)
        {

            string sourceItemPath = "";
            DiskStoreItem sourceItem = (DiskStoreItem)source;
            sourceItemPath = sourceItem.ItemPath;

            if (sourceItemPath.Equals(""))
            {
                throw new Exception("Path to the source item not defined.");
            }


            string destItemItemPath = Path.Combine(ItemPath, destName);


            if (source.IsCollection)
            {
                try
                {

                    WindowsImpersonationContext wic = Identity.Impersonate();
                    Directory.Move(sourceItemPath, destItemItemPath);
                    wic.Undo();
                }
                catch
                {
                    throw new WebDavUnauthorizedException();
                }


                var collection = new DiskStoreCollection(this, destItemItemPath);
                _items.Add(destName, new WeakReference(collection));
                return collection;
            }

            try
            {

                WindowsImpersonationContext wic = Identity.Impersonate();
                System.IO.File.Move(sourceItemPath, destItemItemPath);
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }


            DiskStoreDocument document = new DiskStoreDocument(this, destItemItemPath);
            _items.Add(destName, new WeakReference(document));
            return document;

        }

    }
}