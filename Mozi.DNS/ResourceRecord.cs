namespace Mozi.DNS
{
    public class DNSProtocol
    {

    }

    public class DNSRequest 
    {
        
    }

    public class DNSResponse
    {

    }
    /// <summary>
    /// 资源记录
    /// </summary>
    class ResourceRecord
    {
        //NAME 名字
        //TYPE 类型
        //CLASS 类
        //TTL 生存时间
        //RDLENGTH RDATA所占的字节数
        //RDATA 数据
    }

    class ResourceType
    {
        //A 	1 	a host address
        //NS 	2 	an authoritative name server
        //MD 	3 	a mail destination(Obsolete - use MX)
        //MF 	4 	a mail forwarder(Obsolete - use MX)
        //CNAME 	5 	the canonical name for an alias
        //SOA 	6 	marks the start of a zone of authority
        //MB 	7 	a mailbox domain name(EXPERIMENTAL)
        //MG 	8 	a mail group member(EXPERIMENTAL)
        //MR 	9 	a mail rename domain name(EXPERIMENTAL)
        //NULL 	10 	a null RR(EXPERIMENTAL)
        //WKS 	11 	a well known service description
        //PTR 	12 	a domain name pointer
        //HINFO 	13 	host information
        //MINFO 	14 	mailbox or mail list information
        //MX 	15 	mail exchange
        //TXT 	16 	text strings
    }

    class DNSPackage
    {

    }

    class DNSPackageHeader
    {

    }

    class DNSPackageQuestion
    {

    }

    class DNSPackageAuthority
    {

    
    }
    
    class DNSPackageAdditional
    {
        
    }

    class DNSServer
    {

    }
}
