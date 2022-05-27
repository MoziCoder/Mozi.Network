using Mozi.HttpEmbedded;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebService;

namespace Mozi.SSDP
{
    /// <summary>
    /// 控制请求包
    /// </summary>
    public class ControlActionPackage:AbsAdvertisePackage
    {
        /// <summary>
        /// 文档格式
        /// </summary>
        public string ContentType { get; set; }
        public int ContentLength {get;set;}
        public string UserAgent { get; set; }
        //SOAPACTION:"urn:schema-upnp-org:service:serviceType:v#actionName"
        public SOAPActionDesc SOAPAction { get; set; }
        /// <summary>
        /// 请求体解析，使用<see cref="HttpEmbedded.WebService.SoapEnvelope.Parse(string, SoapVersion)"/>
        /// </summary>
        public SoapEnvelope Body { get; set; }
        public override TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add("HOST", $"{HOST}");
            headers.Add("CONTENT-TYPE", "text/xml; charset=\"utf-8\"");
            headers.Add("USER-AGENT", UserAgent);
            headers.Add("SOAPACTION", "\""+SOAPAction.ToString()+"\"");
            return headers;
        }

        public static ControlActionPackage Parse(HttpRequest req)
        {
            ControlActionPackage pack = new ControlActionPackage();
            pack.Path = req.Path;
            pack.HOST = req.Headers.GetValue("HOST");
            pack.ContentType = req.Headers.GetValue("CONTENT-TYPE");
            pack.UserAgent = req.Headers.GetValue("USER-AGENT");
            pack.SOAPAction = SOAPActionDesc.Parse(req.Headers.GetValue("SOAPACTION"));
            pack.Body = SoapEnvelope.Parse(StringEncoder.Decode(req.Body), SoapVersion.Ver11);
            return pack;
        }
    }


    ///// <summary>
    ///// 控制响应包
    ///// </summary>
    //public class ControlActionResponsePackage : AbsAdvertisePackage
    //{
    //    public string TransferEncoding { get; set; }
    //    public DateTime Date { get; set; }
    //    public int ContentLength { get; set; }
    //    public string Server { get; set; }

    //    public override TransformHeader GetHeaders()
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}
    public class ControlQueryPackage
    {

    }
    /// <summary>
    /// SOAPAction描述字符串
    /// </summary>
    public class SOAPActionDesc 
    {
        public string ActionName { get; set; }
        public URNDesc USN { get; set; }

        public override string ToString()
        {
            return USN + "#" + ActionName;
        }

        public static SOAPActionDesc Parse(string data)
        {
            SOAPActionDesc desc = new SOAPActionDesc();
            string[] sd = data.Split(new char[] { '#' });
            if (sd.Length >= 2)
            {
                desc.USN = URNDesc.Parse(sd[0]);
                desc.ActionName = sd[1];
                return desc;
            }
            else
            {
                return null;
            }
        }
    }
}
