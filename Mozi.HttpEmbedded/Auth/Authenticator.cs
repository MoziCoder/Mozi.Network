using System;
using System.Collections.Generic;
using System.Linq;
using Mozi.HttpEmbedded.Encode;
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
        protected string _realm = "";

        public virtual string Realm { get => _realm; set => _realm = value; }
        //public readonly Dictionary<string, object> Challenge = new Dictionary<string, object>();

        //public Dictionary<string, object> Response = new Dictionary<string, object>();

        public abstract AuthorizationType AuthType { get; }

        public abstract bool Check(string username,string pwd,string statement,string reqMethod);

        public abstract string ParseUsername(string statement);

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

    /// <summary>
    /// Basic只对用户和密码进行简单的认证
    /// <para>
    /// 客户端的回应密码是Base64串
    /// </para>
    /// <code>
    /// 报文范例
    /// 质询 
    ///     WWW-Authenticate: Basic realm="{明文}"    
    /// 响应
    ///     Authorization: Basic YWRtaW46YWRtaW4=
    /// </code>
    /// </summary>
    public class BasicAuth : AuthScheme
    {
        public override AuthorizationType AuthType => AuthorizationType.Basic;

        public override string ParseUsername(string statement)
        {
            string userinfo = Base64.From(statement);
            var indBnd = userinfo.IndexOf((char)ASCIICode.COLON);
            string username = userinfo.Substring(0, indBnd);
            return username;
        }
        /// <summary>
        ///  验证认证信息
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="reqMethod"></param>
        /// <returns></returns>
        public override bool Check(string username,string pwd,string statement,string reqMethod)
        {
            string userinfo = Base64.From(statement);
            var indBnd = userinfo.IndexOf((char)ASCIICode.COLON);
            string uname = userinfo.Substring(0, indBnd);
            string cipher = userinfo.Substring(indBnd + 1);
            return uname.Equals(username) && cipher.Equals(pwd);
        }
        /// <summary>
        /// 生成质询信息
        /// </summary>
        /// <returns></returns>
        public override string GetChallenge()
        {
            return $"realm={Realm}";
        }

        public override string GenerateAuthorization(string username, string pwd)
        {
            return AuthType.Name+" "+Base64.To($"{username}:{pwd}");
        }

    }
    /// <summary>
    /// Digest认证,仅MD5认证
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
    public class DigestAuth : AuthScheme
    {
        public override AuthorizationType AuthType =>AuthorizationType.Digest;

        /// <summary>
        /// 取得返回字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string sReturn = "";
            return sReturn;
        }

        public override string ParseUsername(string statement)
        {
            Dictionary<string, string> arrData = Parse(statement);
            string username = arrData["username"];
            return username;
        }

        /// <summary>
        /// 验证认证信息
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="reqMethod"></param>
        /// <returns></returns>
        public override bool Check(string username,string pwd,string statement,string reqMethod)
        {
            try
            {
                Dictionary<string, string> arrData = Parse(statement);     
                string realm = arrData["realm"];
                string nonce = arrData["nonce"];
                string nc = arrData["nc"];
                string cnonce = arrData["cnonce"];
                string qop = arrData["qop"];
                string opaque = arrData["opaque"];
                string url = arrData.ContainsKey("uri")?arrData["uri"]:"";
                string response = arrData["response"];

                //response=MD5(MD5(username: realm:password):nonce:nc:cnonce:qop:MD5(< request - method >:url))
                //string urlenc= Encrypt.MD5Encrypt($"{requestmethod}:{url}");
                //HA1: HD: HA2
                var ha1 = Encrypt.MD5Encrypt($"{username}:{realm}:{pwd}").ToLower();
                var hd = $"{nonce}:{nc}:{cnonce}:{qop}";
                var ha2 = Encrypt.MD5Encrypt($"{reqMethod}:{url}").ToLower();
                var encrypt = Encrypt.MD5Encrypt($"{ha1}:{hd}:{ha2}").ToLower();
                return response.Equals(encrypt, StringComparison.CurrentCultureIgnoreCase);
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 生成质询信息
        /// </summary>
        /// <returns></returns>
        public override string GetChallenge()
        {
            List<string> clgs = new List<string>();
            clgs.Add($"realm=\"{Realm}\"");
            //clgs.Add($"domain=\"\"");
            clgs.Add($"nonce=\"{CacheControl.GenerateRandom(32)}\"");
            clgs.Add($"opaque=\"{CacheControl.GenerateRandom(32)}\"");
            //nonce过期标识
            //clgs.Add($"stale=\"\"");
            clgs.Add($"algorithm=\"MD5\"");
            clgs.Add($"qop=\"auth\"");
            //OPTIONAL
            //charset
            //userhash

            var clg = string.Join(",", clgs);
            return clg;
        }
        /// <summary>
        /// 生成认证字符串,用于客户端请求
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public override string GenerateAuthorization(string username, string pwd)
        {
            return GenerateAuthorization(username, pwd, Realm, null, 1, null, "auth", null, "GET","/");
        }
        /// <summary>
        /// 生成认证字符串,用于客户端请求
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <param name="realm"></param>
        /// <param name="nonce"></param>
        /// <param name="nc"></param>
        /// <param name="cnonce"></param>
        /// <param name="qop"></param>
        /// <param name="opaque"></param>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GenerateAuthorization(string username,string pwd,string realm,string nonce,uint nc,string cnonce,string qop,string opaque,string method,string url)
        {
            //response=MD5(MD5(username:realm:password):nonce:nc:cnonce:qop:MD5(< request - method >:url))
            string ha1 = "",hd="",ha2="";
            string response = "";

            if (string.IsNullOrEmpty(nonce))
            {
                nonce = CacheControl.GenerateRandom(32);
            }
            if (string.IsNullOrEmpty(cnonce))
            {
                cnonce = CacheControl.GenerateRandom(32);
            }
            if (string.IsNullOrEmpty(opaque))
            {
                opaque = CacheControl.GenerateRandom(32);
            }
            string snc = Hex.To(BitConverter.GetBytes(nc));

            if (string.IsNullOrEmpty(opaque))
            {
                opaque = "auth";
            }
            if (string.IsNullOrEmpty(method))
            {
                method = "GET";
            }
            ha1 = Encrypt.MD5Encrypt($"{username}:{realm}:{pwd}").ToLower();
            hd = $"{nonce}:{nc}:{cnonce}:{qop}";
            ha2 = Encrypt.MD5Encrypt($"{method}:{url}").ToLower();
            response = Encrypt.MD5Encrypt($"{ha1}:{hd}:{ha2}").ToLower();
            return AuthType.Name + " " + $"username=\"{username}\",realm=\"{realm}\",response=\"{response}\",nonce=\"{nonce}\",nc={snc},cnonce=\"{cnonce}\",opaque=\"{opaque}\",qop=\"{qop}\",url=\"{url}\"";
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
    internal class WSSE : AuthScheme
    {
        public override AuthorizationType AuthType => AuthorizationType.WSSE;

        public override string GetChallenge()
        {
            throw new NotImplementedException();
        }

        public override string GenerateAuthorization(string username, string pwd)
        {
            throw new NotImplementedException();
        }

        public override string ParseUsername(string statement)
        {
            throw new NotImplementedException();
        }

        public override bool Check(string username, string pwd, string statement, string reqMethod)
        {
            throw new NotImplementedException();
        }
    }
}
