using System;

namespace Mozi.HttpEmbedded.Attributes
{
    /// <summary>
    /// 响应内容 文档类型
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue)]
    internal class ContentTypeAttribute : Attribute
    {
        private string contentType;
        private string encoding;

        public string ContentType { get => contentType; set => contentType = value; }
        public string Encoding { get => encoding; set => encoding = value; }
    }
}
