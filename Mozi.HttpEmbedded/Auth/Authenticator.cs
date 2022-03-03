using System;
using System.Collections.Generic;
using System.Linq;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded.Auth
{
    //TODO 认证应区分目录
    //TODO Basic认证过于简单，不能起到很好的安全效果
    /// <summary>
    /// 认证器
    /// </summary>
    public class Authenticator
    {
        private readonly List<User> _users = new List<User>();

        public AuthorizationType AuthType { get; private set; }

        public virtual bool Check(string data)
        {
            string authHead = data.Substring(0, data.IndexOf((char)ASCIICode.SPACE));
            string authBody = data.Substring(data.IndexOf((char)ASCIICode.SPACE) + 1);

            AuthorizationType authType = AbsClassEnum.Get<AuthorizationType>(authHead);

            if (authType != null)
            {
                if (authType.Equals(AuthorizationType.Basic))
                {
                    string userinfo = Base64.From(authBody);
                    var indBnd = userinfo.IndexOf((char)ASCIICode.COLON);
                    string username = userinfo.Substring(0, indBnd);
                    string password = userinfo.Substring(indBnd + 1);
                    return IsValidUser(username, password);
                }
                else
                {

                }
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


    public abstract class AuthDatagram
    {
        /// <summary>
        /// 质询要素
        /// </summary>
        public string[] ElementsChallenge;
        /// <summary>
        /// 响应要素
        /// </summary>
        public string[] ElementsResponse;

        public readonly Dictionary<string, object> Challenge = new Dictionary<string, object>();

        public Dictionary<string, object> Response = new Dictionary<string, object>();

        public abstract AuthDatagram Parse(string data);
        /// <summary>
        /// 设置质询要素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public AuthDatagram SetClgElement(string key, object value)
        {
            if (Challenge.ContainsKey(key))
            {
                Challenge[key] = value;
            }
            else
            {
                Challenge.Add(key, value);
            }
            return this;
        }

        public string GetChallenge()
        {
            List<string> clgs = new List<string>();
            foreach (var o in Challenge)
            {
                clgs.Add(string.Format("{0}=\"{1}\"", o.Key, o.Value));
            }
            var clg = string.Format("{0} {1}", this.GetType().Name, string.Join(",", clgs));
            return clg;
        }

        public abstract string GetResponse();
    }

    ///// <summary>
    ///// Basic只对用户和密码进行简单的认证
    ///// <para>
    ///// 客户端的回应密码是Base64串
    ///// </para>
    ///// <code>
    ///// 报文范例
    ///// 质询 
    /////     WWW-Authenticate: Digest realm="testrealm@host.com"    
    ///// 响应
    /////     Authorization: Basic YWRtaW46YWRtaW4=
    ///// </code>
    ///// </summary>
    //public class Basic : AuthDatagraph
    //{

    //    /// <summary>
    //    /// 取得返回字符串
    //    /// </summary>
    //    /// <returns></returns>
    //    public override string ToString()
    //    {
    //        string sReturn = "";
    //        return sReturn;
    //    }
    //}
    /// <summary>
    /// Digest认证
    /// <code>
    /// 报文范例
    /// 质询
    ///     WWW-Authenticate: Digest realm="testrealm@host.com",
    ///                              qop="auth,auth-int",
    ///                              nonce="dcd98b7102dd2f0e8b11d0f600bfb0c093",
    ///                              opaque="5ccc069c403ebaf9f0171e9517f40e41"
    /// 响应
    /// Authorization: Digest realm="testrealm@host.com",
    ///                       username="Mufasa",　                           //客户端已知信息
    ///                       nonce="dcd98b7102dd2f0e8b11d0f600bfb0c093", 　 //服务器端质询响应信息
    ///                       uri="/dir/index.html",                         //客户端已知信息
    ///                       qop=auth, 　                                   //服务器端质询响应信息
    ///                       nc=00000001,                                   //客户端计算出的信息
    ///                       cnonce="0a4f113b",                             //客户端计算出的客户端nonce
    ///                       response="6629fae49393a05397450978507c4ef1",   //最终的摘要信息 ha3
    ///                       opaque="5ccc069c403ebaf9f0171e9517f40e41"　    //服务器端质询响应信息
    /// </code>
    /// </summary>
    public class Digest : AuthDatagram
    {
        /// <summary>
        /// 取得返回字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string sReturn = "";
            return sReturn;
        }

        public override AuthDatagram Parse(string data)
        {
            throw new NotImplementedException();
        }

        public override string GetResponse()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// WSSE认证
    /// <code>
    /// 质询
    /// WWW-Authenticate: WSSE realm="testrealm@host.com",
    ///                        profile="UsernameToken"    //服务器期望你用UsernameToken规则生成回应  UsernameToken规则：客户端生成一个nonce，然后根据该nonce，密码和当前日时来算出哈希值。
    /// 响应
    /// Authorization: WSSE profile="UsernameToken"
    ///                    X-WSSE:UsernameToken
    ///                    username="Mufasa",
    ///                    PasswordDigest="Z2Y......",
    ///                    Nonce="dcd98b7102dd2f0e8b11d0f600bfb0c093",//客户端将生成一个nonce值，并以该nonce值，密码，当前日时为基础，算出哈希值返回给服务器。
    ///                    Created="2010-01-01T09:00:00Z"
    /// </code>
    /// </summary>
    public class WSSE : AuthDatagram
    {
        public override AuthDatagram Parse(string data)
        {
            throw new NotImplementedException();
        }

        public override string GetResponse()
        {
            throw new NotImplementedException();
        }
    }
}
