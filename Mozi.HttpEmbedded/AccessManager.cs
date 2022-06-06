using System.Collections.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 客户端访问控制 黑名单控制|白名单控制
    /// </summary>
    public class AccessManager
    {
        private static AccessManager _access;

        private readonly List<string> _blacklist = new List<string>();

        /// <summary>
        /// 单实例
        /// </summary>
        public static AccessManager Instance
        {
            get { return _access ?? (_access = new AccessManager()); }
        }

        private AccessManager()
        {

        }
        /// <summary>
        /// 增加黑名单成员
        /// </summary>
        /// <param name="ipAddress"></param>
        private void AddMember(string ipAddress)
        {
            _blacklist.Add(ipAddress);
        }
        /// <summary>
        /// 将成员从黑名单中移除
        /// </summary>
        /// <param name="ipAddress"></param>
        private void RemoveMember(string ipAddress)
        {
            _blacklist.Remove(ipAddress);
        }
        /// <summary>
        /// 检查是否在黑名单中
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public bool CheckBlackList(string ipAddress)
        {
            return _blacklist.Exists(x => x.Equals(ipAddress));
        }
    }
}
