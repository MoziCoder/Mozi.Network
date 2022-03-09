using Mozi.StateService.Generic;
using System;
using System.Collections.Generic;

namespace Mozi.StateService
{
    /// <summary>
    /// 终端状态类型
    /// </summary>
    public enum ClientLifeState
    {
        Unknown = 0,
        Alive = 0x31,
        Byebye = 0x32,
        Busy = 0x33,
        Idle = 0x34
    }
    //statename:alive|byebye|busy|idle|offline

    /// <summary>
    /// 版本号
    /// </summary>
    public enum HeartBeatProtocolVersion
    {
        Version1 = 0x31,
        Version2 = 0x32,
    }
    ///// <summary>
    ///// 包类型
    ///// </summary>
    //public enum PackageType
    //{
    //    HeartBeat=0x31,
    //    Subscribe=0x32,
    //    UnSubscribe=0x33,
    //    Publish=0x34,
    //}
    /// <summary>
    /// {version}|{packagetype}
    /// </summary>
    public class BasicHeartBeatPackage
    {
        /// <summary>
        /// 协议版本<see cref="HeartBeatProtocolVersion"/>
        /// </summary>
        public byte Version { get; set; }
        /// <summary>
        /// 包荷载长度
        /// </summary>
        public ushort PackageBodyLength { get; set; }

        public static BasicHeartBeatPackage Parse(byte[] data)
        {
            BasicHeartBeatPackage pack = new BasicHeartBeatPackage
            {
                Version = data[0]
            };
            return pack;
        }
    }

    /// <summary>
    /// 状态数据协议包
    /// <para>
    ///     <see cref="DeviceName"/>和<see cref="DeviceId"/>为主键值，区分终端类型和终端标识
    /// </para>
    /// <para>
    /// 所有字符串均按ASCII编码，字符集不能超过ASCII，超过ASCII的部分请自行转码
    /// </para>
    /// </summary>
    public class HeartBeatPackage:BasicHeartBeatPackage
    {
        /// <summary>
        /// 状态名
        /// </summary>
        public byte StateName { get; set; }
        public ushort DeviceNameLength { get; set; }
        /// <summary>
        /// 设备名 区分业务群
        /// </summary>
        public string DeviceName { get; set; }
        public ushort DeviceIdLength { get; set; }
        /// <summary>
        /// 设备唯一标识
        /// </summary>
        public string DeviceId { get; set; }
        public ushort AppVersionLength { get; set; }
        /// <summary>
        /// 终端应用程序版本
        /// </summary>
        public string AppVersion { get; set; }
        public ushort UserNameLength { get; set; }
        /// <summary>
        /// 登录用户名 可自定义或承载一定的业务功能
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// UTC时间戳 
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// 数据封包
        /// </summary>
        /// <returns></returns>
        public byte[] Pack()
        {
            List<byte> arr = new List<byte>();

            byte[] deviceName = DeviceName.ToBytes();
            byte[] deviceId = DeviceId.ToBytes();
            byte[] appVersion = AppVersion.ToBytes();
            byte[] userName = UserName.ToBytes();

            Timestamp = DateTime.Now.ToTimestamp();

            arr.AddRange(Timestamp.ToBytes());
            arr.InsertRange(0, userName);
            arr.InsertRange(0, ((ushort)userName.Length).ToBytes());

            arr.InsertRange(0, appVersion);
            arr.InsertRange(0, ((ushort)appVersion.Length).ToBytes());

            arr.InsertRange(0, deviceId);
            arr.InsertRange(0, ((ushort)deviceId.Length).ToBytes());

            arr.InsertRange(0, deviceName);
            arr.InsertRange(0, ((ushort)deviceName.Length).ToBytes());

            arr.Insert(0, StateName);

            arr.InsertRange(0, ((ushort)arr.Count).ToBytes());
            arr.Insert(0, Version);
            return arr.ToArray();
        }

        /// <summary>
        /// 协议包解析
        /// </summary>
        /// <param name="pg"></param>
        /// <returns></returns>
        public static new HeartBeatPackage Parse(byte[] pg)
        {
            HeartBeatPackage state = new HeartBeatPackage
            {
                Version = pg[0],
                PackageBodyLength = pg.ToUInt16(1)
            };
            byte[] body = new byte[state.PackageBodyLength];
            Array.Copy(pg, 1 + 2, body, 0, body.Length);

            state.StateName = body[0];
            state.DeviceNameLength = body.ToUInt16(1);
            state.DeviceIdLength = body.ToUInt16(2 * 1 + 1 + state.DeviceNameLength);
            state.AppVersionLength = body.ToUInt16(2 * 2 + 1 + state.DeviceNameLength + state.DeviceIdLength);
            state.UserNameLength = body.ToUInt16(2 * 3 + 1 + state.DeviceNameLength + state.DeviceIdLength + state.AppVersionLength);
            state.Timestamp = body.ToInt64(2 * 4 + 1 + state.DeviceNameLength + state.DeviceIdLength + state.AppVersionLength + state.UserNameLength);

            byte[] deviceName = new byte[state.DeviceNameLength];
            byte[] deviceId = new byte[state.DeviceIdLength];
            byte[] appVersion = new byte[state.AppVersionLength];
            byte[] userName = new byte[state.UserNameLength];

            Array.Copy(body, 2 * 1 + 1, deviceName, 0, state.DeviceNameLength);
            Array.Copy(body, 2 * 2 + 1 + state.DeviceNameLength, deviceId, 0, state.DeviceIdLength);
            Array.Copy(body, 2 * 3 + 1 + state.DeviceNameLength + state.DeviceIdLength, appVersion, 0, state.AppVersionLength);
            Array.Copy(body, 2 * 4 + 1 + state.DeviceNameLength + state.DeviceIdLength + state.AppVersionLength, userName, 0, state.UserNameLength);

            state.DeviceName = deviceName.AsString();
            state.DeviceId = deviceId.AsString();
            state.AppVersion = appVersion.AsString();
            state.UserName = userName.AsString();

            return state;
        }
    }

    /// <summary>
    /// 心跳转发包
    /// </summary>
    public class HeartBeatPublishPackage : BasicHeartBeatPackage
    {
        public ushort HostLength { get; set; }
        public string SrcHost { get; set; }
        public ushort SrcPort { get; set; }

        public HeartBeatPackage HeartBeat { get; set; }

        public byte[] Pack()
        {
            List<byte> data = new List<byte>();
            byte[] byteBeat = HeartBeat.Pack();
            data.AddRange(byteBeat);
            data.InsertRange(0, ((ushort)byteBeat.Length).ToBytes());

            data.InsertRange(0, SrcPort.ToBytes());
            byte[] byteHost = SrcHost.ToBytes();

            data.InsertRange(0, byteHost);
            data.InsertRange(0, ((ushort)byteHost.Length).ToBytes());
            data.Insert(0, Version);

            return data.ToArray();
        }
        /// <summary>
        /// 解析转发包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static new HeartBeatPublishPackage Parse(byte[] data)
        {
            HeartBeatPublishPackage pack = new HeartBeatPublishPackage
            {
                Version = data[0]
            };
            ushort hostLength = BitConverter.ToUInt16(data, 1);
            byte[] byteHost = new byte[hostLength];
            Array.Copy(data, 1 +2, byteHost, 0, hostLength);
            pack.SrcHost = byteHost.AsString();
            pack.SrcPort = BitConverter.ToUInt16(data, 1 + 2 + hostLength);
            pack.PackageBodyLength = BitConverter.ToUInt16(data, 1 + 2 + hostLength+2);
            //TODO 此处未完全解析
            byte[] body = new byte[pack.PackageBodyLength];
            Array.Copy(data, 1 + 2 + hostLength + 2+2, body, 0, body.Length);
            pack.HeartBeat = HeartBeatPackage.Parse(body);
            return pack;
        }
    }
    /// <summary>
    /// 扩展方法
    /// </summary>
    internal static class Extension
    {
        public static byte[] ToBytes(this string data)
        {
            return System.Text.Encoding.ASCII.GetBytes(data);
        }
        public static string AsString(this byte[] data)
        {
            return System.Text.Encoding.ASCII.GetString(data);
        }
    }
}
