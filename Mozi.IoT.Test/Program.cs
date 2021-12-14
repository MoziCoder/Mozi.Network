using System;

namespace Mozi.IoT.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            CoAPServer cs = new CoAPServer();
            cs.Start();
            Console.ReadLine();
        }
    }
}
