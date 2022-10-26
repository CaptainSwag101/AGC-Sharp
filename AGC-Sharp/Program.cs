global using word = System.UInt16;
global using static AGC_Sharp.Utils;

using PowerArgs;
using AGC_Sharp.Hardware;
using AGC_Sharp.Hardware.Block1;

namespace AGC_Sharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Computer comp = new Block1Computer();
        }
    }
}