using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGC_Sharp.Hardware.Block1
{
    internal record Subinstruction(word ST, word SQ, word SQMask, string Name, Action<CPU> Function);

    internal partial class CPU
    {
        private Subinstruction[] ImplementedSubinstructions =
        {
            new Subinstruction(2, Octal(000), Octal(000), "STD2", STD2),
        };

        private static void STD2(CPU cpu)
        {
            switch (cpu.Timepulse)
            {
                case 1:
                    RZ(cpu);
                    WY(cpu);
                    WS(cpu);
                    CI(cpu);
                    break;
                case 3:
                    WG(cpu);
                    break;
                case 4:
                    RU(cpu);
                    WZ(cpu);
                    break;
                case 7:
                    RG(cpu);
                    RSC(cpu);
                    WB(cpu);
                    //WP(cpu);
                    break;
                case 8:
                    //GP(cpu);
                    //TP(cpu);
                    break;
                case 9:
                    RB(cpu);
                    WSC(cpu);
                    WG(cpu);
                    break;
                case 11:
                    NISQ(cpu);
                    break;
            }
        }
    }
}
