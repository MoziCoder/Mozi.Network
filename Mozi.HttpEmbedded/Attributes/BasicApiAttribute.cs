using System;

/// <summary>
/// 属性标记还需要统筹设计
/// </summary>
namespace Mozi.HttpEmbedded.Attributes
{
    /// <summary>
    /// API标记属性
    /// </summary>
    /// <remarks>
    /// 使用此标记属性的实例不会包含HttpContext上下文对象，但可正常调用其实现的所有公开方法
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class BasicApiAttribute : Attribute
    {

    }
    /// <summary>
    /// API描述文档
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class ApiDescription : Attribute
    {
        public string Namespace { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class UrlRewriteAttribute : Attribute
    {

    }
}
