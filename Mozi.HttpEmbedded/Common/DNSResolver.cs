using System.Net;

namespace Mozi.HttpEmbedded.Common
{
    /// <summary>
    /// DNS客户端
    /// </summary>
    public class DNSResolver
    {
        /// <summary>
        /// DNS解析指定域名
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static string GetDomainAddress(string domain)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(domain);
                IPAddress[] addresses = entry.AddressList;

                if (addresses.Length > 0)
                {
                    return addresses[0].ToString();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
