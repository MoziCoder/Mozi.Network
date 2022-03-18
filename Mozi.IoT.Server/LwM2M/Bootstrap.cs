using Mozi.IoT.Generic;

namespace Mozi.IoT.Server.LwM2M
{
    //发现
    //写入
    //删除

    /// <summary>
    /// 引导
    /// </summary>
    /// <remarks>
    ///     Factory Bootstrap
    ///     Bootstrap from Smartcard
    ///     Client Initiated Bootstrap
    ///     Server Initiated Bootstrap
    ///     Smartcard>Factory>Client Initiated>Server Initiated
    /// </remarks>
    class Bootstrap
    {

        public void Discover()
        {

        }

        public void Write()
        {

        }
        public void Delete()
        {

        }

    }
    /// <summary>
    /// 设备注册
    /// </summary>
    class Registeration
    {
        public void Register()
        {

        }

        public void Update()
        {

        }

        public void DeRegister()
        {

        }
    }
    /// <summary>
    /// 设备管理与服务配置
    /// </summary>
    class DeviceConfig
    {
        public void Read()
        {

        }
        public void Create()
        {

        }
        public void Delete()
        {

        }
        public void Write()
        {

        }
        public void Execute()
        {

        }
        public void WriteAttribute()
        {

        }
        public void Discover()
        {

        }
    }
    class InformationReport
    {
        public void Observe()
        {

        }
        public void CancelObserve()
        {

        }
    }
    //LwM2M 1.2
    class LwM2MObjects:AbsClassEnum
    {
        private string _name = "";
        private int _id = -1;

        //1.2
        public static LwM2MObjects LwM2MCOSE=new LwM2MObjects("LwM2M COSE",23);
        public static LwM2MObjects MQTTServer=new LwM2MObjects("MQTT Server",24);
        public static LwM2MObjects LwM2MGateway=new LwM2MObjects("LwM2M Gateway",25);
        public static LwM2MObjects LwM2MGatewayRouting = new LwM2MObjects("LwM2M Gateway Routing", 26);
        public static LwM2MObjects Connectivity5GNR = new LwM2MObjects("5G-NR Connectivity",27);

        //1.1 TS
        public static LwM2MObjects OSCORE=new LwM2MObjects("OSCORE", 21);

        //1.0 TS
        public static LwM2MObjects Security =new LwM2MObjects("Security", 0);
        public static LwM2MObjects Server =new LwM2MObjects("Server", 1);
        public static LwM2MObjects AccessControl  = new LwM2MObjects("Access Control",2);
        public static LwM2MObjects Device=new LwM2MObjects("Device", 3);
        public static LwM2MObjects ConnectivityMonitoring  = new LwM2MObjects("Connectivity Monitoring",4);
        public static LwM2MObjects FirmwareUpdate  = new LwM2MObjects("Firmware Update",5);
        public static LwM2MObjects Location =new LwM2MObjects("Location", 6);
        public static LwM2MObjects ConnectivityStatistics = new LwM2MObjects("Connectivity Statistics",7);

        public string Name
        {
            get { return _name; }
        }
        public int Id
        {
            get
            {
                { return _id; }
            }
        }
        protected override string Tag => _name;

        public LwM2MObjects(string name, int id)
        {
            _name = name;
            _id = id;
        }

    }
}
