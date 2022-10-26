global using word = System.UInt16;
global using static AGC_Sharp.Utils;

using PowerArgs;
using AGC_Sharp.Hardware;

namespace AGC_Sharp
{
    internal class StartupArgs
    {
        [ArgRequired, ArgShortcut("m"), ArgDescription("The AGC hardware type to emulate")]
        public string MachineType { get; set; }
        [ArgRequired, ArgShortcut("r"), ArgDescription("The core rope program file to load."), ArgExistingFile]
        public string RopeFile { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            StartupArgs parsedArgs;
            try
            {
                parsedArgs = Args.Parse<StartupArgs>(args);
            }
            catch (ArgException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<StartupArgs>());
                return;
            }

            Computer? computer = null;
            // Determine the computer type to emulate from the input arguments.
            if (Hardware.Block1.AGC.Names.Any(s => s.Equals(parsedArgs.MachineType)))
            {
                computer = new Hardware.Block1.AGC(parsedArgs.RopeFile);
            }
            else
            {
                Console.WriteLine("Invalid machine type specified.");
                return;
            }

            while (true)
            {
                computer.Execute();
            }
        }
    }
}