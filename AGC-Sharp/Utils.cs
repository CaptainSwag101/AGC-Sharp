using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp
{
    internal class Utils
    {
        private static StreamWriter? debugPrintStream = null;

        public static void SetupLogWriter(string logPath)
        {
            debugPrintStream = new(new FileStream(logPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read));
        }

        public static async void Log(string? message = null)
        {
            // Should we be printing to a stream, or to the console?
            // If debugPrintStream is null, print to console.
            // Otherwise, asynchronously write to said stream for better performance.
            if (debugPrintStream is null)
            {
                Console.Write(message);
            }
            else
            {
                await Task.Run(() => debugPrintStream.Write(message));
                await debugPrintStream.FlushAsync();
            }
        }

        public static void LogLine(string? message = null) => Log($"{message}\n");

        public static word Octal(word input)
        {
            // Yes I know that converting to a string and back is abominable,
            // let's do some performance profiling to see whether an integer-based
            // solution is more performant. Either way we're gonna need to do a conversion.
            string str = Convert.ToString(input);
            word result = Convert.ToUInt16(str, 8);
            return result;
        }

        public static string ToOctal(word input, byte padding = 0)
        {
            string temp = Convert.ToString(input, 8);
            temp = temp.PadLeft(padding, '0');
            return temp;
        }
    }

    internal class Bitmasks
    {
        public const word BITMASK_1_12  = 0x0FFF;
        public const word BITMASK_10_14 = 0x3E00;
        public const word BITMASK_14    = 0x2000;
        public const word BITMASK_14_15 = 0x6000;
        public const word BITMASK_15_16 = 0xC000;
        public const word BITMASK_16    = 0x8000;
    }
}
