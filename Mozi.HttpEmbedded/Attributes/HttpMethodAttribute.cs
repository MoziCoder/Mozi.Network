using System;

namespace Mozi.HttpEmbedded.Attributes
{
    /// <summary>
    /// 禁止访问的方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal class ForbiddenMethodAttribute : Attribute
    {

    }
    /// <summary>
    /// 允许访问的动作
    /// </summary>
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false)]
    internal class HttpGetAttribute : Attribute
    {

    }
    /// <summary>
    /// 允许访问的动作
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class HttpPostAttribute : Attribute
    {

    }
    /// <summary>
    /// 允许访问的动作
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class HttpPutAttribute : Attribute
    {

    }
    /// <summary>
    /// 允许访问的动作
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class HttpDeleteAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method)]
    internal class UrlRewriteAttribute : Attribute
    {

    }
}
