using System;

namespace Mozi.HttpEmbedded.Attributes
{
    /// <summary>
    /// 响应内容 文档类型
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue)]
    internal class ContentTypeAttribute : Attribute
    {
        public string ContentType { get; set; }
        public string Encoding { get; set; }
        public ContentTypeAttribute(string contentType,string encoding)
        {
            ContentType = contentType;
            Encoding = encoding;
        }
        public ContentTypeAttribute(string contentType):this(contentType,"")
        {
            
        }
    }
}
