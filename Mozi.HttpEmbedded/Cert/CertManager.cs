using System;
using System.Security.Cryptography.X509Certificates;

namespace Mozi.HttpEmbedded.Cert
{
    /// <summary>
    /// SSL证书管理
    /// </summary>
    public sealed class CertManager
    {
        private X509Certificate _cert;

        public CertManager()
        {

        }

        /// <summary>
        /// 配置安全证书
        /// <para>
        ///     证书类型为x509
        /// </para>
        /// </summary>
        /// <param name="filePath">
        ///     证书必须为X509 *.pfx
        /// </param>
        /// <param name="password">证书密码</param>
        /// <returns></returns>
        public void LoadCert(string filePath,string password)
        {
            _cert = new X509Certificate(filePath, password);
            Valid();
        }
        /// <summary>
        /// 验证
        /// </summary>
        /// <returns></returns>
        private void Valid()
        {
            //验证有效期
            var periodStart= _cert.GetEffectiveDateString();
            var periodEnd = _cert.GetExpirationDateString();

            if(DateTime.Today.ToUniversalTime().CompareTo(DateTime.ParseExact(periodStart,"yyyy/M/d H:mm:ss",null)) < 0)
            {
                throw new Exception("证书有效期还没有开始");
            }

            if (DateTime.Today.ToUniversalTime().CompareTo(DateTime.ParseExact(periodEnd,"yyyy/M/d H:mm:ss", null)) > 0)
            {
                throw new Exception("证书已过有效期");
            }

            //TODO 验证证书的域名绑定
        }
    }
    /// <summary>
    /// TLS会话层
    /// </summary>
    public interface ITLSSession
    {

    }
}
