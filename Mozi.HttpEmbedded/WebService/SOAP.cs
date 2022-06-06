using System.Xml;
using System.Collections.Generic;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded.WebService
{

    /// <summary>
    /// SOAP envelope封装
    /// SOAP标准内容比较多，目前只实现WebService中需要封装的部分
    /// </summary>
    public class SoapEnvelope
    {

        public List<Namespace> Namespaces = new List<Namespace>()
        {
            new Namespace { Prefix="xsi",Uri="http://www.w3.org/2001/XMLSchema-instance" },
            new Namespace { Prefix="xsd",Uri="http://www.w3.org/2001/XMLSchema" }
        };

        public string NS_EncodingStyle = "http://www.w3.org/2001/12/soap-encoding";

        /// <summary>
        /// SOAP版本
        /// </summary>
        public SoapVersion Version = SoapVersion.Ver11;
        /// <summary>
        /// SOAP头
        /// </summary>
        public SoapHeader Header { get; set; }
        /// <summary>
        /// SOAP内容
        /// </summary>
        public SoapBody Body { get; set; }

        public string Prefix = "m";

        public string Namespace = "http://mozicoder.org/soap";

        public SoapEnvelope()
        {
            Body = new SoapBody();
        }
        /// <summary>
        /// 构造xml文档 父元素增加类命名空间定义<see href="XmlDocument.CreateElement(prefix,localName, namespaceUri)"/>后,创建子元素时命名空间地址会被隐去，此时可以随意添加前缀
        /// </summary>
        ///<returns></returns>
        public string CreateDocument()
        {
            SoapEnvelope envelope = this;

            XmlDocument doc = new XmlDocument();

            //declaration
            var declare = doc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            doc.AppendChild(declare);

            //envelope
            var nodeEnvelope = doc.CreateElement(envelope.Prefix,"Envelope", envelope.Version.Namespace);
            foreach (var ns in envelope.Namespaces)
            {
                nodeEnvelope.SetAttribute("xmlns:"+ns.Prefix, ns.Uri);
            }
            nodeEnvelope.SetAttribute("xmlns:" + envelope.Prefix, envelope.Version.Namespace);
            nodeEnvelope.SetAttribute("encodingStyle",envelope.Version.Namespace,envelope.NS_EncodingStyle);
            
            //header
            if (envelope.Header != null)
            {
                var nodeHeader = doc.CreateElement(envelope.Prefix,"Header", envelope.Version.Namespace);
                if (envelope.Header.Childs != null && envelope.Header.Childs.Length > 0)
                {

                }
                nodeEnvelope.AppendChild(nodeHeader);
            }

            //body
            var nodeBody = doc.CreateElement(envelope.Prefix,"Body", envelope.Version.Namespace);

            //fault
            if (envelope.Body.Fault != null)
            {
                var fault=doc.CreateElement(envelope.Prefix, "Fault", envelope.Version.Namespace);
                fault.SetAttribute("xmlns", envelope.Namespace);

                var faultCode = doc.CreateElement(envelope.Prefix, "faultcode", envelope.Namespace);
                var faultstring = doc.CreateElement(envelope.Prefix, "faultstring", envelope.Namespace);
                var faultactor = doc.CreateElement(envelope.Prefix, "faultactor", envelope.Namespace);
                var detail = doc.CreateElement(envelope.Prefix, "detail", envelope.Namespace); 

                faultCode.InnerText = envelope.Body.Fault.faultcode;
                faultstring.InnerText = envelope.Body.Fault.faultstring;
                faultactor.InnerText = envelope.Body.Fault.faultactor;
                detail.InnerText = envelope.Body.Fault.detail;

                fault.AppendChild(faultCode);
                fault.AppendChild(faultstring);
                fault.AppendChild(faultactor);
                fault.AppendChild(detail);
            }

            //bodyelements
            var nodeBodyMethod = doc.CreateElement(envelope.Body.Prefix,envelope.Body.Method, envelope.Body.Namespace);
            if (!string.IsNullOrEmpty(envelope.Namespace))
            {
                nodeBodyMethod.SetAttribute("xmlns", envelope.Namespace);
            }

            //methodparams
            foreach (var r in envelope.Body.Items)
            {
                var nodeItem = doc.CreateElement(envelope.Body.Prefix,r.Key, envelope.Body.Namespace);
                nodeItem.InnerText = r.Value;
                nodeBodyMethod.AppendChild(nodeItem);
            }

            nodeBody.AppendChild(nodeBodyMethod);
            nodeEnvelope.AppendChild(nodeBody);
            doc.AppendChild(nodeEnvelope);

            return doc.OuterXml;

        }

        /// <summary>
        /// 解析SOAP文件
        /// </summary>
        /// <param name="content"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static SoapEnvelope Parse(string content,SoapVersion version)
        {
            SoapEnvelope envelope = new SoapEnvelope();

            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager xm = new XmlNamespaceManager(doc.NameTable);

            xm.AddNamespace(version.Prefix, version.Namespace);
            doc.LoadXml(content);

            var nodeEnv = doc.SelectSingleNode(string.Format("/{0}:Envelope", version.Prefix), xm);

            envelope.Prefix = nodeEnv.Prefix;
            envelope.Namespace = nodeEnv.NamespaceURI;
            envelope.NS_EncodingStyle = nodeEnv.Attributes.GetNamedItem($"{envelope.Prefix}:encodingStyle").Value;

            //判断版本
            if (envelope.Namespace == SoapVersion.Ver11.Namespace)
            {
                envelope.Version = SoapVersion.Ver11;
            }
            else if(envelope.Namespace==SoapVersion.Ver12.Namespace)
            {
                envelope.Version = SoapVersion.Ver12;
            }else if (envelope.Namespace == SoapVersion.Ver12Dotnet.Namespace)
            {
                envelope.Version = SoapVersion.Ver12Dotnet;
            }

            var nodeBody = doc.SelectSingleNode(string.Format("/{0}:Envelope/{0}:Body",version.Prefix),xm);
            var nodeAction = nodeBody.FirstChild;
            if (nodeAction.LocalName == "Fault")
            {
                XmlNode fault = nodeAction;
                nodeAction = nodeAction.NextSibling;
            }
            envelope.Body.Method = nodeAction.LocalName;
            envelope.Body.Prefix = nodeBody.Prefix;
            envelope.Body.Namespace = nodeBody.NamespaceURI;

            var childs = nodeAction.ChildNodes;
            for(var i = 0; i < childs.Count; i++)
            {
                var child = childs[i];
                envelope.Body.Items.Add(child.LocalName, child.InnerText);
            }

            //version 
            //header
            //body
            //fault
            return envelope;
        }
    }
    /// <summary>
    /// SOAP头节点信息
    /// </summary>
    public class SoapHeader
    {
        public SoapHeaderChild[] Childs { get; set; }
    }
    public class SoapHeaderChild
    {
        public string Name { get; set; }
        public string actor {get;set;}
        public string mustUnderstand { get; set; }  //"0"|"1"
        public string encodingStyle { get; set; }
    }
    /// <summary>
    /// SOAP内容节点信息
    /// </summary>
    public class SoapBody
    {
        public SoapFault Fault { get; set; }
        public string Prefix { get; set; }
        public string Namespace { get; set; }
        public string Method = "";
        public Dictionary<string, string> Items = new Dictionary<string, string>();
    }
    /// <summary>
    /// SOAP错误信息
    /// </summary>
    public class SoapFault
    {
        //VersionMismatch SOAP Envelope 元素的无效命名空间被发现
        //MustUnderstand Header 元素的一个直接子元素（带有设置为 "1" 的 mustUnderstand 属性）无法被理解。
        //Client 消息被不正确地构成，或包含了不正确的信息。
        //Server 服务器有问题，因此无法处理进行下去。
        public string faultcode { get; set; }   
        public string faultstring { get; set; }
        public string faultactor { get; set; }
        public string detail { get; set; }
    }
    /// <summary>
    /// SOAP协议版本
    /// </summary>
    public class SoapVersion : AbsClassEnum
    {
        /// <summary>
        /// SOAPAction: "{ServiceName}/{ActionName}"
        /// text/xml
        /// </summary>
        public static SoapVersion Ver11 = new SoapVersion("1.1","soap", "http://schemas.xmlsoap.org/soap/envelope/");
        /// <summary>
        /// application/soap+xml
        /// </summary>
        public static SoapVersion Ver12 = new SoapVersion("1.2","soap12", "http://www.w3.org/2003/05/soap-envelope");
        public static SoapVersion Ver12Dotnet = new SoapVersion("dot1.2", "soap2", "http://www.w3.org/2003/05/soap-envelope");

        public string Version { get { return _vervalue; } }
        public string Namespace { get { return _namespace; } }
        public string Prefix { get { return _prefix; } }
        protected override string Tag { get { return _vervalue; } }

        private string _vervalue = "";
        private string _namespace = "";
        private string _prefix = "";

        public SoapVersion(string ver,string prefix,string nameSpace)
        {
            _vervalue = ver;
            _prefix = prefix;
            _namespace = nameSpace;
        }
    }
}
