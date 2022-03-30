using System.IO;
using System.Security.Principal;
using Mozi.HttpEmbedded.Source;
using Mozi.HttpEmbedded.WebDav.Exceptions;
using Mozi.HttpEmbedded.WebDav.Utilities;

namespace Mozi.HttpEmbedded.WebDav.Storage.Disk
{
    /// <summary>
    /// </summary>
    public sealed class DiskStoreDocument : DiskStoreItem, IWebDavStoreDocument
    {
        public DiskStoreDocument(DiskStoreCollection parentCollection, string path) : base(parentCollection, path)
        {

        }

        public long Size
        {
            get { return new FileInfo(ItemPath).Length; }
        }

        public string MimeType
        {
            get
            {
                return Mime.GetContentType(Ext);
            }
        }

        public string Etag
        {
            get
            {
                return Md5Util.Md5Hash4Utf8String(ItemPath + ModifyDate + Hidden + Size);
            }
        }



        public Stream OpenRead()
        {
            Stream stream = null;
            try
            {
                WindowsImpersonationContext wic = Identity.Impersonate();
                stream = new FileStream(ItemPath, FileMode.Open, FileAccess.Read, FileShare.None);
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }
            return stream;
        }

        public Stream OpenWrite(bool append)
        {
            if (append)
            {
                FileStream result = new FileStream(ItemPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                result.Seek(0, SeekOrigin.End);
                return result;
            }

            Stream stream = null;
            try
            {

                WindowsImpersonationContext wic = Identity.Impersonate();
                stream = new FileStream(ItemPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }
            return stream;
        }

    }
}