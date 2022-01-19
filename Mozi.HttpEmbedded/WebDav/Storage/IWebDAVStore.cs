namespace Mozi.HttpEmbedded.WebDav.Storage
{
    public interface IWebDavStore
    {
        /// <summary>
        /// Gets the root collection of this <see cref="IWebDavStore" />.
        /// </summary>
        /// <value>
        /// The root.
        /// </value>
        IWebDavStoreCollection Root
        {
            get;
        }
    }
}