global using word = System.UInt16;
global using static AGC_Sharp.Utils;

using PowerArgs;
using AGC_Sharp.Hardware;

namespace AGC_Sharp
{
    internal class StartupArgs
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [ArgRequired, ArgShortcut("m"), ArgDescription("The AGC hardware type to emulate.")]
        public string MachineType { get; set; }

        [ArgShortcut("r"), ArgDescription("The core rope program file to load."), ArgExistingFile]
        public string RopeFile { get; set; }

        [ArgShortcut("l"), ArgDescription("The file to write log data into, rather than the terminal.")]
        public string? LogFile { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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

            if (parsedArgs.LogFile is not null)
            {
                SetupLogWriter(parsedArgs.LogFile);
            }

            // Determine the computer type to emulate from the input arguments.
            Computer? computer = null;
            if (Hardware.Block1.AGC.Names.Any(s => s.Equals(parsedArgs.MachineType)))
            {
                computer = new Hardware.Block1.AGC();
            }
            else
            {
                Console.WriteLine("Invalid machine type specified.");
                return;
            }

            while (true)
            {
                computer.Initialize(parsedArgs.RopeFile);
                computer.Execute();
            }
        }
    }
}