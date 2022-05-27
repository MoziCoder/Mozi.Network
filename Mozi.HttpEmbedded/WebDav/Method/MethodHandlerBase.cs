using System;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.WebDav.Storage;
using Mozi.HttpEmbedded.WebDav.Exceptions;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    /// </summary>
    internal abstract class MethodHandlerBase
    {
        private const int DepthInfinity = -1;

        public static IWebDavStoreCollection GetParentCollection(IWebDavStore store, string path)
        {
            IWebDavStoreCollection collection;
            try
            {
                collection = WebDavExtensions.GetStoreItem(path, store) as IWebDavStoreCollection;
            }
            catch (UnauthorizedAccessException)
            {
                throw new WebDavUnauthorizedException();
            }
            catch (WebDavNotFoundException)
            {
                throw new WebDavConflictException();
            }
            if (collection == null)
            {
                throw new WebDavConflictException();
            }

            return collection;
        }

        public static IWebDavStoreItem GetItemFromCollection(IWebDavStoreCollection collection, string childUri)
        {
            IWebDavStoreItem item;
            try
            {
                UrlTree ut = new UrlTree(childUri);
                item = collection.GetItemByName(UrlEncoder.Decode(ut.Last().TrimEnd('/', '\\')));
            }
            catch (UnauthorizedAccessException)
            {
                throw new WebDavUnauthorizedException();
            }
            catch (WebDavNotFoundException)
            {
                throw new WebDavNotFoundException();
            }
            if (item != null)
            {
                return item;
            }
            throw new WebDavNotFoundException();
        }

        public static int GetDepthHeader(HttpRequest request)
        {
            string depth = request.Headers["Depth"];

            if (!string.IsNullOrEmpty(depth) && !depth.Equals("infinity"))
            {
                int value;
                if (int.TryParse(depth, out value))
                {
                    return value == 0 || value == 1 ? value : DepthInfinity;
                }
                return DepthInfinity;
            }
            return DepthInfinity;
        }

        public static bool GetOverwriteHeader(HttpRequest request)
        {
            string overwrite = request.Headers["Overwrite"];

            return overwrite != null && overwrite.Equals("T");
        }

        public static string GetTimeoutHeader(HttpRequest request)
        {

            string timeout = request.Headers["Timeout"];

            if (string.IsNullOrEmpty(timeout) || timeout.Equals("infinity") || timeout.Equals("Infinite, Second-4100000000"))
            {
                return "Second-345600";
            }
            return timeout;
        }
        public static Uri GetdestItemHeader(HttpRequest request)
        {
            string destPath = request.Headers["destItem"];


            if (string.IsNullOrEmpty(destPath))
                throw new WebDavConflictException();
            return new Uri(destPath);
        }
    }
}
