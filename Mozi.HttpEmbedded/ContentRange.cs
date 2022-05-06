namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 请求内容的范围
    /// </summary>
    public class ContentRange
    {
        //数据为uint 这里为了表示更大范围，用Int64表示
        public long Start { get; set; }

        public long End { get; set; }
    }
}
