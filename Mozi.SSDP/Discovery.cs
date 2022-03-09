using System;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    /// <summary>
    /// 在线数据包
    /// </summary>
    public class AlivePackage : ByebyePackage
    {
        public int CacheTimeout { get; set; }
        public string Location { get; set; }
        public string Server { get; set; }
        public int SearchPort { get; set; }
        public int SecureLocation { get; set; }

        public override TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add("HOST", HOST);
            headers.Add("SERVER", Server);
            headers.Add("NT", NT.ToString());
            headers.Add("NTS", SSDPType.Alive.ToString());
            headers.Add("USN", USN.ToString());
            headers.Add("LOCATION", Location);
            headers.Add("CACHE-CONTROL", $"max-age = {CacheTimeout}");
            return headers;
        }

        public new static AlivePackage Parse(HttpRequest req)
        {
            AlivePackage pack = new AlivePackage();
            var sHost = req.Headers.GetValue("HOST");
            pack.HOST = sHost;
            pack.Server = req.Headers.GetValue("SERVER");
            var sNt = req.Headers.GetValue("NT");
            pack.NT = TargetDesc.Parse(sNt);
            var sNTS = req.Headers.GetValue("NTS");
            var sUSN = req.Headers.GetValue("USN");
            pack.USN = USNDesc.Parse(sUSN);
            pack.Location = req.Headers.GetValue("LOCATION");
            var sCacheControl = req.Headers.GetValue("CACHE-CONTROL");
            if (!string.IsNullOrEmpty(sCacheControl))
            {
                string[] cacheItems = sCacheControl.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (cacheItems.Length == 2)
                {
                    pack.CacheTimeout = int.Parse(cacheItems[1].Trim());
                }
            }
            return pack;
        }

    }
    /// <summary>
    /// 搜索数据包
    /// </summary>
    public class SearchResponsePackage : SearchPackage
    {
        public int CacheTimeout { get; set; }
        public DateTime Date { get; set; }
        public string Ext { get; set; }
        public string Server { get; set; }
        public USNDesc USN { get; set; }
        public string Location { get; set; }

        public override TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add("HOST", HOST);
            headers.Add("MAN", "\"" + SSDPType.Discover.ToString() + "\"");
            headers.Add("ST", ST.ToString());
            headers.Add("CACHE-CONTROL", $"max-age = {CacheTimeout}");
            headers.Add("DATE", DateTime.UtcNow.ToString("r"));
            headers.Add("EXT", "");
            headers.Add("LOCATION", Location);
            headers.Add("SERVER", Server);
            headers.Add("USN", USN.ToString());
            return headers;
        }

        public new static SearchResponsePackage Parse(HttpRequest req)
        {
            SearchResponsePackage pack = new SearchResponsePackage();
            var sHost = req.Headers.GetValue("HOST");
            pack.HOST = sHost;
            pack.MAN = req.Headers.GetValue("MAN");
            pack.MX = int.Parse(req.Headers.GetValue("MX"));
            var st = req.Headers.GetValue("ST");
            pack.ST = TargetDesc.Parse(st);
            var sNTS = req.Headers.GetValue("NTS");
            var sUSN = req.Headers.GetValue("USN");
            return pack;
        }

    }

    /// <summary>
    /// 查询头信息
    /// <para>
    ///     发送包设置<see cref="SearchPackage.MAN"/>参数无效，默认为 "ssdp:discover"
    /// </para>
    /// </summary>
    public class SearchPackage : AbsAdvertisePackage
    {
        public string MAN { get; set; }
        //-ssdp:all 搜索所有设备和服务
        //-upnp:rootdevice 仅搜索网络中的根设备
        //-uuid:device-UUID 查询UUID标识的设备
        //-urn:schemas-upnp-org:device:device-Type:version 查询device-Type字段指定的设备类型，设备类型和版本由UPNP组织定义。
        //-urn:schemas-upnp-org:service:service-Type:version 查询service-Type字段指定的服务类型，服务类型和版本由UPNP组织定义。
        public TargetDesc ST { get; set; }
        /// <summary>
        /// 查询等待时间 取值范围0-5
        /// </summary>
        public int MX { get; set; }
        /// <summary>
        /// 用户代理
        /// </summary>
        public string UserAgent { get; set; }
        public int TcpPort { get; set; }
        public string CPFN { get; set; }
        public string CPUUID { get; set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add("HOST", HOST);
            headers.Add("MAN", "\"" + SSDPType.Discover.ToString() + "\"");
            headers.Add("ST", ST.ToString());
            headers.Add("MX", $"{MX}");
            return headers;
        }

        public static SearchPackage Parse(HttpRequest req)
        {
            SearchPackage pack = new SearchPackage();
            var sHost = req.Headers.GetValue("HOST")??req.Headers.GetValue("Host");
            pack.HOST = sHost;
            //IPV4
            pack.MAN = req.Headers.GetValue("MAN");
            pack.MX = int.Parse(req.Headers.GetValue("MX"));
            var st = req.Headers.GetValue("ST");
            pack.ST = TargetDesc.Parse(st);
            return pack;
        }

    }
    /// <summary>
    /// 离线头信息
    /// </summary>
    public class ByebyePackage : AbsAdvertisePackage
    {
        public TargetDesc NT { get; set; }
        //public string NTS { get; set; }
        public USNDesc USN { get; set; }
        //
        public int BOOTID { get; set; }
        public int CONFIGID { get; set; }

        public override TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add("HOST", HOST);
            headers.Add("NT", NT.ToString());
            headers.Add("NTS", "\"" + SSDPType.Byebye.ToString() + "\"");
            headers.Add("USN", USN.ToString());
            return headers;
        }

        public static ByebyePackage Parse(HttpRequest req)
        {
            ByebyePackage pack = new ByebyePackage();
            var sHost = req.Headers.GetValue("HOST") ?? req.Headers.GetValue("Host");
            pack.HOST = sHost;
            var sNt = req.Headers.GetValue("NT");
            pack.NT = TargetDesc.Parse(sNt);
            var sNTS = req.Headers.GetValue("NTS");
            var sUSN = req.Headers.GetValue("USN");
            pack.USN = USNDesc.Parse(sUSN);
            return pack;
        }
    }
    /// <summary>
    /// 更新数据包
    /// </summary>
    public class UpdatePackage : AlivePackage
    {
        public int NEXTBOOTID { get; set; }

        public override TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add("HOST", HOST);
            headers.Add("SERVER", Server);
            headers.Add("NT", NT.ToString());
            headers.Add("NTS", SSDPType.Update.ToString());
            headers.Add("USN", USN.ToString());
            headers.Add("LOCATION", Location);
            headers.Add("CACHE-CONTROL", $"max-age = {CacheTimeout}");
            return headers;
        }

        public new static UpdatePackage Parse(HttpRequest req)
        {
            UpdatePackage pack = new UpdatePackage();
            var sHost = req.Headers.GetValue("HOST") ?? req.Headers.GetValue("Host");
            pack.HOST = sHost;
            pack.Server = req.Headers.GetValue("SERVER");
            var sNt = req.Headers.GetValue("NT");
            pack.NT = TargetDesc.Parse(sNt);
            var sNTS = req.Headers.GetValue("NTS");
            var sUSN = req.Headers.GetValue("USN");
            pack.USN = USNDesc.Parse(sUSN);
            pack.Location = req.Headers.GetValue("LOCATION");
            var sCacheControl = req.Headers.GetValue("CACHE-CONTROL");
            if (!string.IsNullOrEmpty(sCacheControl))
            {
                string[] cacheItems = sCacheControl.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (cacheItems.Length == 2)
                {
                    pack.CacheTimeout = int.Parse(cacheItems[1].Trim());
                }
            }
            return pack;
        }
    }
}
