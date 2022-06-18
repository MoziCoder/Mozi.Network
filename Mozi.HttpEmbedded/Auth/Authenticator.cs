using System;
using System.Collections.Generic;
using System.Linq;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded.Auth
{
    //TODO 认证应区分目录
    //DONE Basic认证过于简单，不能起到很好的安全效果
    /// <summary>
    /// 认证器
    /// </summary>
    public class Authenticator
    {
        private readonly List<User> _users = new List<User>();

        /// <summary>
        /// 认证类型
        /// </summary>
        public AuthorizationType AuthType { get; private set; }

        //认证方案
        private AuthScheme _scheme;

        private string _realm = "http-auth@mozicoder.org";

        /// <summary>
        /// 域
        /// </summary>
        public string Realm
        {
            get
            {
                return _realm;
            }
        }
        /// <summary>
        /// 最大尝试次数
        /// </summary>
        public int MaxTryCount = 5;
        /// <summary>
        /// 
        /// </summary>
        public Authenticator()
        {
            AuthType = AuthorizationType.None;
        }
        /// <summary>
        /// 检验
        /// </summary>
        /// <param name="data"></param>
        /// <param name="reqMethod"></param>
        /// <returns></returns>
        public virtual bool Check(string data,string reqMethod)
        {
            string authHead = data.Substring(0, data.IndexOf((char)ASCIICode.SPACE));
            string authBody = data.Substring(data.IndexOf((char)ASCIICode.SPACE) + 1);

            AuthorizationType authType = AbsClassEnum.Get<AuthorizationType>(authHead);

            //比对请求认证类型是否正确
            if (authType != null&&authHead.Equals(authType.Name,StringComparison.CurrentCultureIgnoreCase))
            {
                User user = FindUser(_scheme.ParseUsername(authBody));
                if (user != null)
                {
                    //验证
                    return _scheme.Check(user.UserName,user.Password,authBody, reqMethod);
                }
                return false;
            }
            return false;
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
            if(tp==AuthorizationType.Basic)
            {
                 _scheme = new BasicAuth();
            }
            else if (tp == AuthorizationType.Digest)
            {
                  _scheme = new DigestAuth();
            }
            else if (tp == AuthorizationType.None)
            {
                 _scheme = null;
            }
            if (_scheme != null)
            {
                _scheme.Realm = _realm;
            }
            return this;
        }
        /// <summary>
        /// 生成质询字符串
        /// </summary>
        /// <returns></returns>
        public string GetChallenge()
        {
            if (_scheme != null)
            {
                return $"{AuthType.Name} {_scheme.GetChallenge()}";
            }
            else
            {
                return "";
            }
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
        /// <summary>
        /// 查找用户名
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public User FindUser(string userName)
        {
            return _users.Find(x => x.UserName.Equals(userName));
        }
        /// <summary>
        /// 设置域信息
        /// </summary>
        /// <param name="realm"></param>
        public void SetRealm(string realm)
        {
            _realm = realm;
            if (AuthType != AuthorizationType.None)
            {
                _scheme.Realm = _realm;
            }
        }
        /// <summary>
        /// 生成认证信息，客户端用，服务端无意义
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public string GenerateAuthorization(string username,string pwd)
        {
            if (AuthType != AuthorizationType.None)
            {
                return _scheme.GenerateAuthorization(username, pwd);
            }
            else
            {
                return null;
            }
        }
    }
    /// <summary>
    /// 认证方案
    /// </summary>
    public abstract class AuthScheme
    {
        //认证域，在浏览器场景中会以明文的形式提供给前端
        private string _realm = "";
        /// <summary>
        /// 认证域
        /// </summary>
        public virtual string Realm { get => _realm; set => _realm = value; }
        //public readonly Dictionary<string, object> Challenge = new Dictionary<string, object>();

        //public Dictionary<string, object> Response = new Dictionary<string, object>();
        /// <summary>
        /// 认证类型
        /// </summary>
        public abstract AuthorizationType AuthType { get; }
        /// <summary>
        /// 检查客户端提交的认证信息是否合法
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <param name="statement"></param>
        /// <param name="reqMethod"></param>
        /// <returns></returns>
        public abstract bool Check(string username,string pwd,string statement,string reqMethod);
        /// <summary>
        /// 获取认证信息中的用户名
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        public abstract string ParseUsername(string statement);
        /// <summary>
        /// 解析认证键值对
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual Dictionary<string, string> Parse(string data)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] arrData = data.Split(new char[] { (char)ASCIICode.COMMA },StringSplitOptions.RemoveEmptyEntries);
            foreach(var r in arrData)
            {
                string[] kp= r.Trim().Split(new char[] { (char)ASCIICode.EQUAL }, StringSplitOptions.RemoveEmptyEntries);
                if (kp.Length == 2)
                {
                    dic.Add(kp[0].Trim(),kp[1].Trim().Trim(new char[] { (char)ASCIICode.QUOTE }));
                }
            }
            return dic;
        }
        /// <summary>
        /// 生成质询文本
        /// </summary>
        /// <returns></returns>
        public abstract string GetChallenge();

        /// <summary>
        /// 生成认证信息
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public abstract string GenerateAuthorization(string username,string pwd);
    }
}
