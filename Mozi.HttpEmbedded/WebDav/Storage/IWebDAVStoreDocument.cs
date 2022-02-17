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
        /// MIMEֵ <see cref="IWebDavStoreItem" />.
        /// </summary>
        string MimeType
        {
            get;
        }

        string Etag
        {
            get;
        }

        Stream OpenReadStream();

        Stream OpenWriteStream(bool append);
    }
}