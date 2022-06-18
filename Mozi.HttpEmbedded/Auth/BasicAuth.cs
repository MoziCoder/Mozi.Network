using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded.Auth
{
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
        /// <summary>
        /// 认证类型
        /// </summary>
        public override AuthorizationType AuthType => AuthorizationType.Basic;
        /// <summary>
        /// 解析用户名
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
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
        /// <param name="username"></param>
        /// <param name="pwd"></param>
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
        /// <summary>
        /// 生成认证信息，HttpClient调用时使用
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public override string GenerateAuthorization(string username, string pwd)
        {
            return AuthType.Name+" "+Base64.To($"{username}:{pwd}");
        }

    }
}
