using System;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav
{
    internal static class WebDavExtensions
    {
        public static Uri GetParentUri(this Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.Segments.Length == 1)
            {
                throw new InvalidOperationException("Cannot get parent of root");
            }

            string url = uri.ToString();
            int index = url.Length - 1;
            if (url[index] == '/')
            {
                index--;
            }

            while (url[index] != '/')
            {
                index--;
            }

            return new Uri(url.Substring(0, index + 1));
        }
        public static IWebDavStoreItem GetStoreItem(string path, IWebDavStore store)
        {
            IWebDavStoreCollection collection = store.Root;
            //ÅÐ¶Ï¸ùÂ·¾¶
            IWebDavStoreItem item = collection.GetItemByName(store.Root.ItemPath + "\\" + path);
            return item;
        }
    }
}