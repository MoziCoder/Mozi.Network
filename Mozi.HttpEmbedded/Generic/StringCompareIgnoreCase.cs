using System.Collections.Generic;

namespace Mozi.HttpEmbedded.Generic
{
    /// <summary>
    /// 忽略大小写比较器
    /// </summary>
    public class StringCompareIgnoreCase : IEqualityComparer<string>
    {
        /// <summary>
        /// 忽略大小写进行比较
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(string x, string y)
        {
            if (x != null && y != null)
            {
                return x.ToLowerInvariant() == y.ToLowerInvariant();
            }
            return false;
        }
        /// <summary>
        /// 返回Hash值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}