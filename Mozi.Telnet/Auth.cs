using System.Collections.Generic;
using System.Linq;

namespace Mozi.Telnet
{

    /// <summary>
    /// HTTP认证类型
    /// </summary>
    public class AuthorizationType : AbsClassEnum
    {
        /// <summary>
        /// 基本认证 明文传输 不安全
        /// </summary>
        public static readonly AuthorizationType Basic = new AuthorizationType("Basic");

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected override string Tag
        {
            get { return Name; }
        }

        private AuthorizationType(string name)
        {
            _name = name;
        }
    }
    //TODO 认证应区分目录
    //TODO Basic认证过于简单，不能起到很好的安全效果
    /// <summary>
    /// 认证器
    /// </summary>
    public class Authenticator
    {
        private readonly List<User> _users = new List<User>();

        public AuthorizationType AuthType { get; private set; }

        public virtual bool Check(string username,string password)
        {
            //解密部分
            return IsValidUser(username, password);
        }
        /// <summary>
        /// 是否有效用户
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userPassword"></param>
        /// <returns></returns>
        public bool IsValidUser(string userName, string userPassword)
        {
            return _users.Any(x => x.UserGroup == UserGroup.Admin && x.UserName.Equals(userName) && x.Password.Equals(userPassword));
        }
        /// <summary>
        /// 设置认证类型
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        public Authenticator SetAuthType(AuthorizationType tp)
        {
            AuthType = tp;
            return this;
        }
        /// <summary>
        /// 配置用户 
        /// 如果用户不存在会追加此用户，如果存在会刷新用户密码。
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userPassword"></param>
        /// <returns></returns>
        public Authenticator SetUser(string userName, string userPassword)
        {
            var user = _users.Find(x => x.UserName.Equals(userName));
            if (user == null)
            {
                _users.Add(new User() { UserName = userName, Password = userPassword, UserGroup = UserGroup.Admin });
            }
            else
            {
                user.Password = userPassword;
            }
            return this;
        }
    }
}
