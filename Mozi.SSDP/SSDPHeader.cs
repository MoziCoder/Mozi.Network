using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    /// <summary>
    /// SSDP头信息
    /// </summary>
    public class SSDPHeader
    {
        public static HeaderProperty St = new HeaderProperty("ST");
        public static HeaderProperty NT = new HeaderProperty("NT");
        public static HeaderProperty Host= new HeaderProperty("HOST");
        public static HeaderProperty CacheControl= new HeaderProperty("CACHE-CONTROL");
        public static HeaderProperty Location= new HeaderProperty("LOCATION");
        public static HeaderProperty NTS= new HeaderProperty("NTS");
        public static HeaderProperty Server= new HeaderProperty("SERVER");
        public static HeaderProperty Usn= new HeaderProperty("USN");
        public static HeaderProperty BootId= new HeaderProperty("BOOTID.UPNP.ORG");
        public static HeaderProperty ConfigId= new HeaderProperty("CONFIGID.UPNP.ORG");
        public static HeaderProperty NextBootId = new HeaderProperty("NEXTBOOTID.UPNP.ORG");
        public static HeaderProperty SearchPort= new HeaderProperty("SEARCHPORT.UPNP.ORG");
        public static HeaderProperty SecureLocation= new HeaderProperty("SECURELOCATION.UPNP.ORG");
        public static HeaderProperty Man= new HeaderProperty("MAN");
        public static HeaderProperty Mx= new HeaderProperty("MX");
        public static HeaderProperty UseAgent= new HeaderProperty("USER-AGENT");
        public static HeaderProperty TcpPort= new HeaderProperty("TCPPORT.UPNP.ORG");
        public static HeaderProperty CPFN= new HeaderProperty("CPFN.UPNP.ORG");
        public static HeaderProperty CPUUID= new HeaderProperty("CPUUID.UPNP.ORG");
        public static HeaderProperty ContentLength= new HeaderProperty("CONTENT-LENGTH");
        public static HeaderProperty ContentType= new HeaderProperty("CONTENT-TYPE");
        public static HeaderProperty SoapAction= new HeaderProperty("SOAPACTION");
        public static HeaderProperty TransferEncoding= new HeaderProperty("TRANSFER-ENCODING");
        public static HeaderProperty Date= new HeaderProperty("DATE");
        public static HeaderProperty Callback= new HeaderProperty("CALLBACK");
        public static HeaderProperty StateVar= new HeaderProperty("STATEVAR");
        public static HeaderProperty Timeout= new HeaderProperty("TIMEOUT");
        public static HeaderProperty Sid= new HeaderProperty("SID");
        public static HeaderProperty AcceptedStateVar= new HeaderProperty("ACCEPTED-STATEVAR");
        public static HeaderProperty Seq= new HeaderProperty("SEQ");
    }
}
