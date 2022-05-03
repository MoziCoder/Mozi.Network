using System;

namespace Mozi.HttpEmbedded.Attributes
{
    /// <summary>
    /// 禁止访问的方法
    /// </summary>
    /// <remarks>
    /// 适用于公开的，又不允许HTTP访问的方法
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    internal class HttpForbiddenAttribute : Attribute
    {

    }

    /// <summary>
    /// 允许使用GET方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class HttpGetAttribute : Attribute
    {

    }

    /// <summary>
    /// 允许使用POST方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class HttpPostAttribute : Attribute
    {

    }

    /// <summary>
    /// 允许使用PUT方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class HttpPutAttribute : Attribute
    {

    }

    /// <summary>
    /// 允许使用DELETE方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class HttpDeleteAttribute : Attribute
    {

    }
}
