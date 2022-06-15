namespace Mozi.HttpEmbedded.Compress
{
    /// <summary>
    /// 传输压缩类型
    /// </summary>
    public enum ContentEncoding
    {
        /// <summary>
        /// none
        /// </summary>
        None,
        /// <summary>
        /// gzip
        /// </summary>
        Gzip,
        /// <summary>
        /// deflate
        /// </summary>
        Deflate,
        /// <summary>
        /// bzip2
        /// </summary>
        Bzip2,
    }
}