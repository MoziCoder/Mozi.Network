using System;

namespace Mozi.HttpEmbedded.Auth
{
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
    internal class WSSEAuth : AuthScheme
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
