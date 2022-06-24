using System;
using System.Collections.Generic;
using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded.Auth
{
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
    /// Authorization: Digest realm="testrealm@host.com",                    //认证域
    ///                       username="Mufasa",　                           //客户端已知信息
    ///                       nonce="dcd98b7102dd2f0e8b11d0f600bfb0c093", 　 //服务端发送的随机数
    ///                       uri="/dir/index.html",                         //请求地址
    ///                       qop=auth, 　                                   //认证方式 auth仅头部信息
    ///                       nc=00000001,                                   //客户端认证次数
    ///                       cnonce="0a4f113b",                             //客户端计算出的客户端nonce
    ///                       response="6629fae49393a05397450978507c4ef1",   //最终的摘要信息 也就是需要对比的关键信息
    ///                       opaque="5ccc069c403ebaf9f0171e9517f40e41"　    //透传字符串 B64|HEX
    /// </code>
    /// </summary>
    public class DigestAuth : AuthScheme
    {
        /// <summary>
        /// 认证类型
        /// </summary>
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
        /// <summary>
        /// 解析用户名
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        public override string ParseUsername(string statement)
        {
            Dictionary<string, string> arrData = Parse(statement);
            string username = arrData["username"];
            return username;
        }

        /// <summary>
        /// 验证认证信息
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
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
}
