namespace Mozi.HttpEmbedded.WebDav
{
    internal class LockProperty
    {
        public string Locktype { get; set; }
        public string Lockscope { get; set; }
        public string Depth { get; set; }
        public string Owner { get; set; }
        public string Timeout { get; set; }
        public string Locktoken { get; set; }

        /// <summary>
        /// The standard constructor
        /// </summary>
        public LockProperty()
        {
            Locktype = string.Empty;
            Lockscope = string.Empty;
            Depth = string.Empty;
            Owner = string.Empty;
            Timeout = string.Empty;
            Locktoken = string.Empty;
        }

        public LockProperty(string locktype, string lockscope, string depth, string owner, string timeout, string locktoken)
        {
            Locktype = locktype;
            Lockscope = lockscope;
            Depth = depth;
            Owner = owner;
            Timeout = timeout;
            Locktoken = locktoken;
        }
    }
}