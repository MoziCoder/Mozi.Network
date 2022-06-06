namespace Mozi.HttpEmbedded.Compress
{
    /// <summary>
    /// 压缩选项
    /// </summary>
    public class CompressOption
    {
        /// <summary>
        /// 压缩算法类型
        /// </summary>
        public ContentEncoding CompressType { get; set; }
        /// <summary>
        /// 会被压缩的内容最小长度
        /// </summary>
        public int MinContentLength { get; set; }
        /// <summary>
        /// 压缩级别
        /// </summary>
        public int CompressLevel { get; set; }
        /// <summary>
        /// 启用压缩的MIME类型
        /// </summary>
        public string[] CompressTypes { get; set; }
        /// <summary>
        /// 是否对代理进行压缩
        /// </summary>
        public bool CompressProxied { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public CompressOption()
        {
            CompressTypes = new string[] { "text/html" };
        }
    }
}
