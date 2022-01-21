using System.Collections.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 客户端访问控制 黑名单控制|白名单控制
    /// </summary>
    public class AccessManager
    {
        public static AccessManager _access;

        private readonly List<string> _blacklist = new List<string>();

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
        public bool CheckBlackList(string ipAddress)
        {
            return _blacklist.Exists(x => x.Equals(ipAddress));
        }
    }
}
