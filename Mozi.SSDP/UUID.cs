using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Mozi.SSDP
{
    /// <summary>
    /// UUID = 4 * <hexOctet> “-” 2 * <hexOctet> “-” 2 * <hexOctet> “-” 2 * <hexOctet> “-” 6 * <hexOctet>
    ///     hexOctet = <hexDigit> <hexDigit>
    ///     hexDigit = “0”|“1”|“2”|“3”|“4”|“5”|“6”|“7”|“8”|“9”|“a”|“b”|“c”|“d”|“e”|“f”|“A”|“B”|“C”|“D”|“E”|“F”
    /// </summary>
    public class UUID
    {
        /// <summary>
        /// 生成设备唯一ID值
        /// </summary>
        /// <returns></returns>
        public static string Generate()
        {
            byte[] octet1 = System.Text.Encoding.ASCII.GetBytes("Mozi");
            byte[] octet2 = System.Text.Encoding.ASCII.GetBytes("06");
            byte[] octet3 = System.Text.Encoding.ASCII.GetBytes("30");
            byte[] octet4 = System.Text.Encoding.ASCII.GetBytes("id");

            byte[] octet5 = new byte[] { 0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD };

            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            List<NetworkInterfaceType> filterType = new List<NetworkInterfaceType>() { NetworkInterfaceType.Loopback, NetworkInterfaceType.Tunnel };
            foreach (var nt in interfaces)
            {
                if (!filterType.Contains(nt.NetworkInterfaceType))
                {
                    List<byte> listMacBit = nt.GetPhysicalAddress().GetAddressBytes().ToList();
                    for (int i = 0; i < octet5.Length; i++)
                    {
                        octet5[i] = (byte)((octet5[i] * listMacBit[i]) >> 8);
                    }
                }

            }
            Guid guid = new Guid(BitConverter.ToInt32(octet1,0), BitConverter.ToInt16(octet2,0), BitConverter.ToInt16(octet3, 0), octet4[0],octet4[1],octet5[0],octet5[1],octet5[2],octet5[3],octet5[4],octet5[5]);
            return guid.ToString();
        }
    }
}
