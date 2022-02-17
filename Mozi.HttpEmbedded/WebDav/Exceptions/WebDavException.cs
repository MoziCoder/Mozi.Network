using System;

namespace Mozi.HttpEmbedded.WebDav.Exceptions
{
    /// <summary>
    /// </summary>
    [Serializable]
    public class WebDavException : Exception
    {
        private StatusCode _status;
        private string _message;

        public override string Message { get { return _message; } }

        public WebDavException(StatusCode status, string message, Exception innerException)
        {
            _status = status;
            _message = message;
        }
        public StatusCode Status
        {
            get { return _status; }
        }
    }
}