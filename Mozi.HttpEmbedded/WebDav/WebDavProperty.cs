using System.Xml;

namespace Mozi.HttpEmbedded.WebDav
{
    /// <summary>
    ///  core WebDAV server.
    /// </summary>
    internal class WebDavProperty
    {
        public string Name;

        public string Namespace;

        public string Value;

        public WebDavProperty()
        {
            Namespace = string.Empty;
            Name = string.Empty;
            Value = string.Empty;
        }

        public WebDavProperty(string name)
        {
            Name = name;
            Value = string.Empty;
            Namespace = "DAV:";
        }

        public WebDavProperty(string name, string value)
        {
            Name = name;
            Value = value;
            Namespace = "DAV:";
        }

        public WebDavProperty(string name, string value, string ns)
        {
            Name = name;
            Value = value;
            Namespace = ns;
        }

        public override string ToString()
        {
            return StartString() + Value + EndString();
        }

        public string StartString()
        {
            if (Namespace == "DAV:")
                return "<D:" + Name + ">";
            return "<" + Name + " xmlns=\"" + Namespace + "\">";
        }

        public string EmptyString()
        {
            if (Namespace == "DAV:")
                return "<D:" + Name + "/>";
            return "<" + Name + " xmlns=\"" + Namespace + "\"/>";
        }

        public string EndString()
        {
            if (Namespace == "DAV:")
                return "</D:" + Name + ">";
            return "</" + Name + ">";
        }

        public XmlDocumentFragment ToXmlDocumentFragment(XmlDocument doc)
        {
            XmlDocumentFragment fragment = doc.CreateDocumentFragment();
            fragment.InnerXml = ToString();
            return fragment;
        }

        public XmlElement ToXmlElement(XmlDocument doc)
        {
            if (doc.DocumentElement == null) return doc.CreateElement(Name);
            // 命名空间前缀
            string prefix = doc.DocumentElement.GetPrefixOfNamespace(Namespace);

            // 新增元素
            XmlElement element = doc.CreateElement(prefix, Name, Namespace);
            element.InnerText = Value;
            return element;
        }
    }
}