using System.IO;

namespace Mozi.HttpEmbedded.WebDav.Storage
{
    public interface IWebDavStoreDocument : IWebDavStoreItem
    {
        long Size
        {
            get;
        }

        /// <summary>
        /// MIME <see cref="IWebDavStoreItem" />.
        /// </summary>
        string MimeType
        {
            get;
        }

        string Etag
        {
            get;
        }

        Stream OpenRead();

        Stream OpenWrite(bool append);
    }
}