using System.Collections.Generic;

namespace Mozi.IoT.Encode
{
    class LinkFormator
    {
        public static void Parse(string text)
        {

        }

        public static void ToString(List<LinkInfo> links)
        {

        }
    }

    public class LinkInfo
    {
        public string Href { get; set; }
        public string ResourceType { get; set; }
        public ContentFormat ContentType { get; set; }
        public List<LinkAttribute> Attributes = new List<LinkAttribute>();
    }

    public class LinkAttribute
    {
        public string AttributeName { get; set; }
        public object AttributeValue { get; set; }
    }


}
