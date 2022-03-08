using Mozi.IoT.Encode;
using System;

namespace Mozi.IoT
{
    /// <summary>
    /// 资源标记属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CoAPResourceAttribute : Attribute
    {

    }
    /// <summary>
    /// 资源描述属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ResourceDescriptionAttribute : Attribute
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
    }

    public class ResourceInfo
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public Type ResourceType { get; set; }
    }
    /// <summary>
    /// CoAP资源
    /// </summary>
    public abstract class CoAPResource
    {
        /// <summary>
        /// 服务端实例
        /// </summary>
        public CoAPServer Server { get; set; }
        /// <summary>
        /// 资源大小
        /// </summary>
        public abstract long ResourceSize { get; }
        /// <summary>
        /// 默认分块大小，单位Bytes 
        /// </summary>
        /// <remarks>
        /// 如果资源尺寸过大，则必须合理配置此大小。
        /// 取值范围为16-2048Bytes BlockOptionValue中Size的数据容量。参考<see cref="BlockOptionValue"/>
        /// </remarks>
        public virtual long BlockSize { get { return 128; } }
        /// <summary>
        /// GET方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual CoAPPackage OnGet(CoAPPackage request)
        {
            return new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = request.MesssageId, Token = request.Token, Code = CoAPResponseCode.Forbidden };
        }
        /// <summary>
        /// Post方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual CoAPPackage OnPost(CoAPPackage request)
        {
            return new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = request.MesssageId, Token = request.Token, Code = CoAPResponseCode.Forbidden };
        }
        /// <summary>
        /// PUT方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual CoAPPackage OnPut(CoAPPackage request)
        {
            return new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = request.MesssageId, Token = request.Token, Code = CoAPResponseCode.Forbidden };
        }
        /// <summary>
        /// Delete方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual CoAPPackage OnDelete(CoAPPackage request)
        {
            return new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = request.MesssageId, Token = request.Token, Code = CoAPResponseCode.Forbidden };
        }
        /// <summary>
        /// 分块查找
        /// </summary>
        /// <param name="indBlock"></param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        protected virtual byte[] Seek(int indBlock, int blockSize)
        {
            return new byte[] { };
        }
    }
    /// <summary>
    /// 时间服务 UTC时间
    /// </summary>
    /// <remarks>
    /// 用于客户机查询服务时间或客户机时间校准
    /// </remarks>
    [ResourceDescription(Namespace = "core", Name = "time")]
    public class TimeResource : CoAPResource
    {
        public override long ResourceSize { get => 0; }

        public override CoAPPackage OnGet(CoAPPackage request)
        {
            CoAPPackage pack = base.OnGet(request);
            DateTime dt = DateTime.Now;
            pack.Payload = StringEncoder.Encode(dt.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            pack.Code = CoAPResponseCode.Content;
            return pack;
        }
    }

    [ResourceDescription(Namespace = "core", Name = "runtime")]
    public class RuntimeResource : CoAPResource
    {
        public override long ResourceSize { get => 0; }

        public override CoAPPackage OnGet(CoAPPackage request)
        {
            return base.OnGet(request);
        }
    }
}
