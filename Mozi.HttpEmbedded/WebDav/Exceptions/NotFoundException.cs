using System;

namespace Mozi.HttpEmbedded.WebDav.Exceptions
{
    /// <summary>
    /// 404
    /// </summary>
    [Serializable]
    public class WebDavNotFoundException : WebDavException
    {
        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException">
        /// </param>
        public WebDavNotFoundException(string message = null, Exception innerException = null) : base(StatusCode.NotFound, message, innerException)
        {

        }
    }
}