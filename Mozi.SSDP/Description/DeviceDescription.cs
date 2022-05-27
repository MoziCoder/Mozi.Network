using System;
using System.Collections.Generic;
using System.Xml;

namespace Mozi.SSDP.Description
{
    //TODO 进一步实现Description
    internal class DeviceDescription
    {
        public SpecVersion Version { get; set; }
        public string UrlBase { get; set; }
        public Device Device { get; set; }

        public string CreateDocument()
        {
            XmlDocument doc = new XmlDocument();
            //declare
            var declare= doc.CreateXmlDeclaration("1.0", "", "");
            doc.AppendChild(declare);

            //root
            var root = doc.CreateElement("root", "urn:schemas-upnp-org:device-1-0");

            //verion
            var version = doc.CreateElement("specVersion");
            var major = doc.CreateElement("major");
            major.Value = Version.Major.ToString();
            
            var minor = doc.CreateElement("minor");
            minor.Value = Version.Major.ToString();
            version.AppendChild(major);
            version.AppendChild(minor);
            root.AppendChild(version);

            //urlbase

            var urlbase = doc.CreateElement("URLBase");
            urlbase.Value = UrlBase;
            root.AppendChild(urlbase);

            //device
            var device = doc.CreateElement("device");
            root.AppendChild(device);

            var deviceType = doc.CreateElement("deviceType");
            deviceType.Value = Device.DeviceType.ToString();

            doc.AppendChild(root);
            return doc.OuterXml;
        }
    }
    internal class Device
    {
        public URNDesc DeviceType { get; set; }
        public string FriendlyName { get; set; }
        public string Manufacturer { get; set; }
        public string ManufacturerURL { get; set; }
        public string ModelDescription { get; set; }
        public string ModelName { get; set; }
        public string ModelNumber { get; set; }
        public string ModelURL { get; set; }
        public string SerialNumber { get; set; }
        public UDNDesc UDN { get; set; }
        public string UPC { get; set; }

        public List<IconInfo> IconList { get; set; }
        public List<ServiceInfo> ServiceList { get; set; }
        public List<DeviceInfo> DeviceList { get; set; }
    }

    internal class IconInfo
    {
        public string Mimetype { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Depth { get; set; }
        public string Url { get; set; }
    }

    internal class ServiceInfo
    {
        public URNDesc ServiceType { get; set; }
         //<serviceId>urn:upnp-org:serviceId:serviceID</serviceId>
        public string ServiceId { get; set; }
       
        public string SCPDURL { get; set; }
        public string ControlURL{ get; set; }
        public string eventSubURL{ get; set; }
    }
    internal class DeviceInfo
    {

    }
    internal class SpecVersion
    {
        public int Major { get; set; }
        public int Minor { get; set; }
    }

    internal class ServiceDescription
    {
        public SpecVersion Version { get; set; }
        public List<ActionInfo> ActionList { get; set; }

        public List<StateVariable> ServiceStateTable { get; set; }
    }

    internal class StateVariable
    {
        public string Name { get; set; }
        public bool SendEvents { get; set; }
        public string DataType { get; set; }
        public string DefaultValue { get; set; }
        public List<string> AllowedValueList { get; set; }

        public List<AllowedValueRange>  AllowedValueRange{get;set;}
    }

    internal class AllowedValueRange
    {
        public string Max { get; set; }

        public string Min { get; set; }
        public string Step { get; set; }

    }
    internal class ActionInfo
    {
        public string Name { get; set; }
        public List<ActionArgument> ArgumentList { get; set; }
    }

    internal class ActionArgument
    {
        public string Name { get; set; }
        public string Direction { get; set; }

        public string Retval { get; set; }

        public string RelatedStateVariable { get; set; }
    }
}
