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
            string str = Convert.ToString(input);
            word result = Convert.ToUInt16(str, 8);
            return result;
        }
    }
}
