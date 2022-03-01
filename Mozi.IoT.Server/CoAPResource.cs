using System;

namespace Mozi.IoT
{


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ResourceDescriptionAttribute:Attribute{
        public string Namespace { get; set; }
        public string Name { get; set; }
    }

    public class ResourceManager
    {

    }

    public class ResourceInfo
    {
        public string Namespace { get; set; }
        public string Name { get; set; }

    }
    /// <summary>
    /// CoAP资源
    /// </summary>
    public abstract class CoAPResource
    {
        public abstract long ResourceSize { get; }

        public virtual CoAPPackage OnGet(CoAPPackage request)
        {
            return new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = request.MesssageId, Token = request.Token, Code = CoAPResponseCode.Forbidden };
        }

        public virtual CoAPPackage OnPost(CoAPPackage request)
        {
            return new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = request.MesssageId, Token = request.Token, Code = CoAPResponseCode.Forbidden };
        }

        public virtual CoAPPackage OnPut(CoAPPackage request)
        {
            return new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = request.MesssageId, Token = request.Token, Code = CoAPResponseCode.Forbidden };
        }

        public virtual CoAPPackage OnDelete(CoAPPackage request)
        {
            return new CoAPPackage { MessageType = CoAPMessageType.Acknowledgement, MesssageId = request.MesssageId, Token = request.Token, Code = CoAPResponseCode.Forbidden };
        }
    }

    [ResourceDescription(Namespace ="core",Name ="runtime")]
    public class RuntimeResource : CoAPResource
    {
        public override long ResourceSize { get => 0; }

        public override CoAPPackage OnGet(CoAPPackage request)
        {
           return base.OnGet(request);
        }
    }
}
