using System;
using System.Collections.Generic;
using Mozi.IoT.Encode;

namespace Mozi.IoT
{
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
        /// <summary>
        /// 文字描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 资源类型
        /// </summary>
        public string ResourceType { get; set; }
    }
    /// <summary>
    /// 资源信息
    /// </summary>
    public class ResourceInfo:LinkInfo
    {
        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 资源的定义声明类型
        /// </summary>
        public Type ClsType { get; set; }
        /// <summary>
        /// 文字描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 资源是否在线
        /// </summary>
        public bool Online { get; set; }

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
        /// <returns></returns>
        internal virtual void HandleBlock2Query(CoAPContext ctx)
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
        /// <returns></returns>
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
    [ResourceDescription(Namespace = "core", Name = "time",Description ="isotime")]
    public class TimeResource : CoAPResource
    {
        public override uint ResourceSize { get => 0; }

        public override CoAPPackage OnGet(CoAPContext ctx)
        {
            CoAPPackage pack = base.OnGet(ctx);
            DateTime dt = DateTime.Now;
            pack.Payload = StringEncoder.Encode(dt.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            pack.Code = CoAPResponseCode.Content;
            return pack;
        }
    }
    /// <summary>
    /// 资源发现入口，遵循/.well-known/coap接口范式
    /// </summary>
    /// <remarks>地址/.well-known/coap为RFC定义的地址，如果标准化实现此路径会引发安全问题</remarks>
    [ResourceDescription(Namespace = "core", Name = "link",Description ="discovery")]
    public class LinkResource : CoAPResource
    {
        public override uint ResourceSize => 0;

        public override CoAPPackage OnGet(CoAPContext ctx)
        {
            
            //TODO 此处需要查询缓冲
            CoAPPackage pack = base.OnGet(ctx);

            //返回Link-Format格式的资源入口信息
            ResourceManager rm = ResourceManager.Default;
            List<ResourceInfo> res=rm.GetAll();
            LinkInfoCollection infos = new LinkInfoCollection();
            infos.AddRange(res);

            //如果没有查询过滤
            if (string.IsNullOrEmpty(ctx.Request.Query))
            {
                pack.Payload = StringEncoder.Encode(LinkFormator.ToString(infos));
            }
            else
            {
                List<LinkInfo> results = infos.Find(ctx.Request.Query);
                pack.Payload = StringEncoder.Encode(LinkFormator.ToString(results));
            }
            pack.SetContentType(ContentFormat.LinkFormat);
            pack.Code = CoAPResponseCode.Content;
            return pack;
        }
    }

    //TODO 还未实现
    [ResourceDescription(Namespace = "core", Name = "cache", Description = "cache")]
    internal class CacheResource : CoAPResource
    {
        private static byte[] _buffer = new byte[] { };
        private static byte[] Buffer
        {
            get { return _buffer; }
            set { _buffer = value; }
        }
        public override uint ResourceSize => (uint)Buffer.Length;

        public override CoAPPackage OnGet(CoAPContext ctx)
        {
            return base.OnGet(ctx);
        }

        public override CoAPPackage OnPost(CoAPContext ctx)
        {
            return base.OnPost(ctx);
        }

        public override CoAPPackage OnPut(CoAPContext ctx)
        {
            return base.OnPut(ctx);
        }

        public override CoAPPackage OnDelete(CoAPContext ctx)
        {
            return base.OnDelete(ctx);
        }
    }
    /// <summary>
    /// 运行时
    /// </summary>
    [ResourceDescription(Namespace = "core", Name = "runtime")]
    internal class RuntimeResource : CoAPResource
    {
        public override uint ResourceSize { get => 0; }

        public override CoAPPackage OnGet(CoAPContext ctx)
        {
            return base.OnGet(ctx);
        }
    }
}
