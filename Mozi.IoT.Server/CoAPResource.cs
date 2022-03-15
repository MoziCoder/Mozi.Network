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
        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name { get; set; }
    }
    /// <summary>
    /// 资源信息
    /// </summary>
    public class ResourceInfo
    {
        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name { get; set; }
        public Type ResourceType { get; set; }

        public override string ToString()
        {
            return (string.IsNullOrEmpty(Namespace) ? "" : ("/" + Namespace)) + "/" + Name;
        }
    }
    /// <summary>
    /// CoAP资源
    /// </summary>
    public abstract class CoAPResource
    {
        /// <summary>
        /// 资源总大小
        /// </summary>
        public abstract uint ResourceSize { get; }
        /// <summary>
        /// 默认分块大小128，单位Bytes 
        /// </summary>
        /// <remarks>
        /// 如果资源尺寸过大，则必须合理配置此大小。
        /// 取值范围为16-2048Bytes BlockOptionValue中Size的数据容量。参考<see cref="BlockOptionValue"/>
        /// </remarks>
        public virtual uint BlockSize { get { return 128; } }
        /// <summary>
        /// GET方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual CoAPPackage OnGet(CoAPContext ctx)
        {
            ctx.Response = new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = ctx.Request.MesssageId, Token = ctx.Request.Token, Code = CoAPResponseCode.Forbidden };
            return ctx.Response;
        }
        /// <summary>
        /// Post方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual CoAPPackage OnPost(CoAPContext ctx)
        {
            ctx.Response = new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = ctx.Request.MesssageId, Token = ctx.Request.Token, Code = CoAPResponseCode.Forbidden };
            return ctx.Response;
        }
        /// <summary>
        /// PUT方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual CoAPPackage OnPut(CoAPContext ctx)
        {
            ctx.Response = new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = ctx.Request.MesssageId, Token = ctx.Request.Token, Code = CoAPResponseCode.Forbidden };
            return ctx.Response;
        }
        /// <summary>
        /// Delete方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual CoAPPackage OnDelete(CoAPContext ctx)
        {
            ctx.Response = new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = ctx.Request.MesssageId, Token = ctx.Request.Token, Code = CoAPResponseCode.Forbidden };
            return ctx.Response;
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
        /// <summary>
        /// Block2分块协商
        /// </summary>
        /// <param name="ctx"></param>
        protected virtual void HandleBlock2Query(CoAPContext ctx)
        {
            CoAPOption opt = ctx.Request.Options.Find(x => x.Option == CoAPOptionDefine.Block2);
            if (opt != null)
            {
                OptionValue opt2 = new BlockOptionValue() { Pack = opt.Value.Pack };
                //if(opt2)
            }
        }
        /// <summary>
        /// 请求服务端资源大小，响应条件为 Get Size2=0
        /// </summary>
        /// <param name="ctx">响应上下文对象</param>
        internal virtual bool HandleSize2Query(CoAPContext ctx)
        {
            CoAPOption opt = ctx.Request.Options.Find(x => x.Option == CoAPOptionDefine.Size2);
            if (opt != null && int.Parse(opt.Value.ToString()) ==0 && ctx.Request.Code == CoAPRequestMethod.Get)
            {

                ctx.Response = new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = ctx.Request.MesssageId, Token = ctx.Request.Token, Code = CoAPResponseCode.Content };

                CoAPOption optResp = new CoAPOption() { Option = CoAPOptionDefine.Size2, Value = new UnsignedIntegerOptionValue() { Value = ResourceSize } };
                
                ctx.Response.SetOption(optResp);
                return true;
            }
            else
            {
                return false;
            }
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
        public override uint ResourceSize { get => 1024; }

        public override CoAPPackage OnGet(CoAPContext ctx)
        {
            CoAPPackage pack = base.OnGet(ctx);
            DateTime dt = DateTime.Now;
            pack.Payload = StringEncoder.Encode(dt.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            pack.Code = CoAPResponseCode.Content;
            return pack;
        }
    }

    [ResourceDescription(Namespace = "", Name = "Discovery")]
    internal class DiscoveryResource : CoAPResource
    {
        public override uint ResourceSize => 0;
    }
    [ResourceDescription(Namespace = "core", Name = "runtime")]
    public class RuntimeResource : CoAPResource
    {
        public override uint ResourceSize { get => 0; }

        public override CoAPPackage OnGet(CoAPContext ctx)
        {
            return base.OnGet(ctx);
        }
    }
}
