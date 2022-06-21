using System.Collections.Generic;

namespace Mozi.Live
{
    /// <summary>
    /// 会话管理器
    /// </summary>
    public class SessionManager
    {
        private List<Session> _sessions = new List<Session>();
        
        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="se"></param>
        public void Add(Session se)
        {
            _sessions.Add(se);
        }
        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="se"></param>
        public void Remove(Session se)
        {
            _sessions.Remove(se);
        }
        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="id"></param>
        public void Remove(string id)
        {
            _sessions.RemoveAll(x => x.Id == id);
        }
    }
    /// <summary>
    /// 会话管理
    /// </summary>
    public class Session
    {
        public string Id { get; set; }

        public int Timeout { get; set; }
    }
}
