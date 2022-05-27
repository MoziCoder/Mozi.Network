using System;

namespace Mozi.SSDP
{
    /// <summary>
    /// 目标描述符号，ST|NT|URN|UDN
    /// <list type="table">
    ///     <listheader>格式如下</listheader>
    ///     <item>--upnp:rootdevice</item>
    ///      <item>--uuid:device-UUID</item>
    ///      <item>--urn:schemas-upnp-org:device:deviceType:v</item>
    ///      <item>--urn:schemas-upnp-org:service:serviceType:v</item>
    ///      <item>--urn:domain-name:device:deviceType:v</item>
    ///      <item>--urn:domain-name:service:serviceType:v</item>
    /// </list>
    /// </summary>
    public class TargetDesc : USNDesc
    {
        /// <summary>
        /// 是否ssdp:all
        /// </summary>
        public bool IsAll = false;
        /// <summary>
        /// 此值代表upnp:rootdevice
        /// </summary>
        public static TargetDesc RootDevice = new TargetDesc { IsRootDevice = true };
        /// <summary>
        /// 代表ssdp:all
        /// </summary>
        public static TargetDesc All = new TargetDesc { IsAll = true };

        public new string ToString()
        {
            if (IsAll)
            {
                return SSDPType.All.ToString();
            }
            string result;
            if (IsRootDevice)
            {
                result = SSDPType.RootDevice.ToString();
            }
            else
            {
                if (string.IsNullOrEmpty(DeviceId))
                {
                    result = string.Format("urn:{0}:{1}:{2}:{3}", Domain, ServiceType == ServiceCategory.Device ? "device" : "service", ServiceName, Version);
                }
                else
                {
                    result = "uuid:" + DeviceId;
                }
            }
            return result;
        }
        /// <summary>
        /// 解析NT|ST|"URN"字段值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static new TargetDesc Parse(string data)
        {
            //uuid:device-UUID::urn:domain-name:service:serviceType:v
            TargetDesc desc = new TargetDesc()
            {
                Domain = "",
                IsRootDevice = false,
                DeviceId = "",
                ServiceName = "",
                Version = 0,
            };
            string[] items = data.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                //ssdp:all
                if (data == SSDPType.All.ToString())
                {
                    desc.IsAll = true;
                }
                else
                {
                    //upnp:rootdevice
                    if (data == SSDPType.RootDevice.ToString())
                    {
                        desc.IsRootDevice = true;
                        desc.DeviceId = items[1];
                    }
                    //specific device
                    else if (items[0] == "uuid")
                    {
                        desc.DeviceId = items[1];
                    }
                    //URN
                    else
                    {
                        desc.Domain = items[1];
                        var serviceType = items[2];
                        desc.ServiceType = serviceType == "device" ? ServiceCategory.Device : ServiceCategory.Service;
                        desc.ServiceName = items[3];
                        desc.Version = int.Parse(items[4]);
                    }
                }
            }
            finally
            {

            }
            return desc;
        }
    }

    /// <summary>
    /// Unique Service Name
    /// <list type="table">
    /// <description>承载的描述符号如下</description>
    /// <item>--uuid:device-UUID</item>
    /// <item>--uuid:device-UUID::upnp:rootdevice</item>
    /// <item>--uuid:device-UUID::urn:schemas-upnp-org:device:deviceType:v</item>
    /// <item>--uuid:device-UUID::urn:schemas-upnp-org:service:serviceType:v</item>
    /// <item>--uuid:device-UUID::urn:domain-name:device:deviceType:v</item>
    /// <item>--uuid:device-UUID::urn:domain-name:service:serviceType:v</item>
    /// </list>
    /// </summary>
    public class USNDesc : URNDesc
    {
        /// <summary>
        /// 是否根设备
        /// </summary>
        public bool IsRootDevice = false;

        /// <summary>
        /// 设备UUID
        /// </summary>
        public string DeviceId = "";
        /// <summary>
        /// 转为URN字符串
        /// </summary>
        /// <returns></returns>
        internal string ToURN()
        {
            string result = "";
            if (!IsRootDevice)
            {
                result = string.Format("urn:{0}:{1}:{2}:{3}", Domain, ServiceType == ServiceCategory.Device ? "device" : "service", ServiceName, Version);
            }
            else
            {

            }
            return result;
        }
        /// <summary>
        /// 转为USN格式字符串
        /// <list type="number">
        /// <item>--uuid:device-UUID::upnp:rootdevice</item> 
        /// <item>--uuid:device-UUID::upnp:rootdevice </item>
        /// <item>--uuid:device-UUID::urn:schemas-upnp-org:device:deviceType:v</item>
        /// <item>--uuid:device-UUID::urn:schemas-upnp-org:service:serviceType:v</item>
        /// <item>--uuid:device-UUID::urn:domain-name:device:deviceType:v</item>
        /// <item>--uuid:device-UUID::urn:domain-name:service:serviceType:v</item>
        /// </list>
        /// </summary>
        /// <returns></returns>
        public new string ToString()
        {
            var result = "uuid:" + DeviceId;
            if (IsRootDevice)
            {
                result += "::" + SSDPType.RootDevice.ToString();
            }
            else
            {
                result += string.Format("::urn:{0}:{1}:{2}:{3}", Domain, ServiceType == ServiceCategory.Device ? "device" : "service", ServiceName, Version);
            }
            return result;
        }
        /// <summary>
        /// 包解析
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public new static USNDesc Parse(string data)
        {
            //uuid:device-UUID::urn:domain-name:service:serviceType:v
            USNDesc desc = new USNDesc()
            {
                Domain = "",
                IsRootDevice = false,
                DeviceId = "",
                ServiceName = "",
                Version = 0,
            };
            string[] items = data.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                if (data.Contains(SSDPType.RootDevice.ToString()))
                {
                    desc.IsRootDevice = true;
                    desc.DeviceId = items[1];
                }
                else
                {
                    desc.DeviceId = items[1];
                    desc.Domain = items[3];
                    var serviceType = items[4];
                    desc.ServiceType = serviceType == "device" ? ServiceCategory.Device : ServiceCategory.Service;
                    desc.ServiceName = items[5];
                    desc.Version = int.Parse(items[6]);
                }
            }
            finally
            {

            }
            return desc;
        }
    }
    /// <summary>
    /// URN描述，资源描述符
    /// <list type="table">
    ///     <listheader>格式如下</listheader>
    ///      <item>--urn:schemas-upnp-org:device:deviceType:v</item>
    ///      <item>--urn:schemas-upnp-org:service:serviceType:v</item>
    ///      <item>--urn:domain-name:device:deviceType:v</item>
    ///      <item>--urn:domain-name:service:serviceType:v</item>
    /// </list>
    /// </summary>
    public class URNDesc
    {
        /// <summary>
        /// 域名，关联URN字段
        /// </summary>
        public string Domain = "schemas-upnp-org";
        /// <summary>
        /// 指示是服务还是设备
        /// </summary>
        public ServiceCategory ServiceType = ServiceCategory.Service;
        /// <summary>
        /// 如果<see cref="ServiceType"/>是<see cref="ServiceCategory.Service"/>,指示的是服务类型；如果<see cref="ServiceType"/>是<see cref="ServiceCategory.Device"/>，指示的是设备类型。
        /// </summary>
        public string ServiceName = "simplehost";
        /// <summary>
        /// 服务或设备的版本
        /// </summary>
        public int Version = 1;
        /// <summary>
        /// 转为URN格式描述符号
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("urn:{0}:{1}:{2}:{3}", Domain, ServiceType == ServiceCategory.Device ? "device" : "service", ServiceName, Version);
        }
        /// <summary>
        /// 解析URN描述字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static URNDesc Parse(string data)
        {
            URNDesc desc = new URNDesc();
            string[] items = data.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            desc.Domain = items[1];
            var serviceType = items[2];
            desc.ServiceType = serviceType == "device" ? ServiceCategory.Device : ServiceCategory.Service;
            desc.ServiceName = items[4];
            desc.Version = int.Parse(items[5]);
            return desc;
        }
    }
    /// <summary>
    /// UDN描述符号 uuid:{device-UUID}
    /// </summary>
    public class UDNDesc
    {
        /// <summary>
        /// 设备UUID值
        /// </summary>
        public string DeviceId { get; set; }
        public override string ToString()
        {
            return $"uuid:{DeviceId}";
        }

        public static UDNDesc Parse(string data)
        {
            UDNDesc desc = new UDNDesc();
            desc.DeviceId = data.Replace("uuid:", "");
            return desc;
        } 
    }
    /// <summary>
    /// 设备还是服务
    /// </summary>
    public enum ServiceCategory
    {
        Device = 1,
        Service = 2
    }
}
