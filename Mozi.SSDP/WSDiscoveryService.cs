namespace Mozi.SSDP
{

    internal class WSDiscoveryNamespace 
    {
        //s11 http://schemas.xmlsoap.org/soap/envelope/ [SOAP 1.1]
        //s12 http://www.w3.org/2003/05/soap-envelope [SOAP 1.2]
        //a http://www.w3.org/2005/08/addressing [WS-Addressing]
        //d http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01 This specification
        //ds http://www.w3.org/2000/09/xmldsig#   // [XML Sig]
        //wsse http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd   [WS - Security]
        //xs http://www.w3.org/2001/XMLSchema //[XML Schema Part 1, 2]
        //ec http://www.w3.org/2001/10/xml-exc-c14n#  //[EXC-C14N]
    }


    /// <summary>
    /// WS-Discovery服务
    /// </summary>
    class WSDiscoveryService : SSDPService
    {

    }


    internal class WSDiscoveryProtocol:SSDPProtocol
    {
        /// <summary>
        /// SSDP组播地址IPV6
        /// </summary>
        public new const string MulticastAddressIPv6 = "FF02::C";
        /// <summary>
        /// SSDP组播端口
        /// </summary>
        public new const int ProtocolPort = 3702;
    }
}
