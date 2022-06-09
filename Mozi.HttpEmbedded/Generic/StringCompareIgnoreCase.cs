using System.Collections.Generic;

namespace Mozi.HttpEmbedded.Generic
{
    /// <summary>
    /// ���Դ�Сд�Ƚ���
    /// </summary>
    public class StringCompareIgnoreCase : IEqualityComparer<string>
    {
        /// <summary>
        /// ���Դ�Сд���бȽ�
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
        /// ����Hashֵ
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}