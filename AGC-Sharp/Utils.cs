using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp
{
    internal class Utils
    {
        public static word Octal(word input)
        {
            // Yes I know that converting to a string and back is abominable,
            // let's do some performance profiling to see whether an integer-based
            // solution is more performant. Either way we're gonna need to do a conversion.
            string str = Convert.ToString(input);
            word result = Convert.ToUInt16(str, 8);
            return result;
        }
    }
}
