namespace Mozi.HttpEmbedded
{

    public class Languages
    {

    }
    /// <summary>
    /// 语言权重
    /// </summary>
    public class LanguagePriority
    {
        /// <summary>
        /// 语言名称
        /// </summary>
        public string LanguageName { get; set; }
        /// <summary>
        /// 语言级别
        /// </summary>
        public decimal Weight { get; set; }
        /// <summary>
        /// 转为字符串格式 cn;q=0.5
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Weight > 0)
            {
                return LanguageName;
            }
            else
            {
                return $"{LanguageName};q={Weight}";
            }
        }

    }
}
