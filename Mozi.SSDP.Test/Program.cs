using System;
using System.Net.NetworkInformation;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SSDPHost ssdp = SSDPHost.Instance;
            ssdp.Activate();
            Console.ReadLine();
        }
    }
}
