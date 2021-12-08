using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
