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
            new Subinstruction(2, Octal(0), Octal(0), "STD2", STD2),
            new Subinstruction(0, Octal(0), Octal(7), "TC0", TC0),
            new Subinstruction(0, Octal(1), Octal(7), "CCS0", CCS0),
            new Subinstruction(1, Octal(1), Octal(7), "CCS1", CCS1),
        };

        private static void CCS0(CPU cpu)
        {
            switch (cpu.Timepulse)
            {
                case 1:
                    RB(cpu);
                    WS(cpu);
                    break;
                case 2:
                    RZ(cpu);
                    WY(cpu);
                    break;
                case 3:
                    WG(cpu);
                    break;
                case 6:
                    RG(cpu);
                    RSC(cpu);
                    WB(cpu);
                    TSGN(cpu);
                    //WP(cpu);
                    break;
                case 7:
                    switch (cpu.BR)
                    {
                        case 0b00:
                        case 0b01:
                            RC(cpu);
                            TMZ(cpu);
                            break;
                        case 0b10:
                        case 0b11:
                            RB(cpu);
                            TMZ(cpu);
                            break;
                    }
                    break;
                case 8:
                    switch (cpu.BR)
                    {
                        case 0b00:
                            //GP(cpu);
                            //TP(cpu);
                            break;
                        case 0b01:
                            R1(cpu);
                            WX(cpu);
                            //GP(cpu);
                            //TP(cpu);
                            break;
                        case 0b10:
                            R2(cpu);
                            WX(cpu);
                            //GP(cpu);
                            //TP(cpu);
                            break;
                        case 0b11:
                            R1(cpu);
                            R2(cpu);
                            WX(cpu);
                            //GP(cpu);
                            //TP(cpu);
                            break;
                    }
                    break;
                case 9:
                    RB(cpu);
                    WSC(cpu);
                    WG(cpu);
                    break;
                case 10:
                    switch (cpu.BR)
                    {
                        case 0b00:
                            RC(cpu);
                            WA(cpu);
                            break;
                        case 0b01:
                            R1C(cpu);
                            WA(cpu);
                            break;
                        case 0b10:
                            RB(cpu);
                            WA(cpu);
                            break;
                        case 0b11:
                            R1C(cpu);
                            WA(cpu);
                            break;
                    }
                    break;
                case 11:
                    RU(cpu);
                    ST1(cpu);
                    WZ(cpu);
                    break;
            }
        }

        private static void CCS1(CPU cpu)
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
                case 5:
                    RA(cpu);
                    WY(cpu);
                    CI(cpu);
                    break;
                case 7:
                    RG(cpu);
                    RSC(cpu);
                    WB(cpu);
                    //WP(cpu);
                    break;
                case 8:
                    RU(cpu);
                    WB(cpu);
                    //GP(cpu);
                    //TP(cpu);
                    break;
                case 10:
                    RC(cpu);
                    WA(cpu);
                    //WOVI(cpu);
                    break;
                case 11:
                    RG(cpu);
                    WSC(cpu);
                    WB(cpu);
                    NISQ(cpu);
                    break;
            }
        }

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

        private static void TC0(CPU cpu)
        {
            switch (cpu.Timepulse)
            {
                case 1:
                    RB(cpu);
                    WY(cpu);
                    WS(cpu);
                    CI(cpu);
                    break;
                case 3:
                    WG(cpu);
                    break;
                case 4:
                    WA(cpu);
                    //WOVI(cpu);
                    break;
                case 7:
                    RG(cpu);
                    RSC(cpu);
                    WB(cpu);
                    //WP(cpu);
                    break;
                case 8:
                    RZ(cpu);
                    WQ(cpu);
                    //GP(cpu);
                    //TP(cpu);
                    break;
                case 9:
                    RB(cpu);
                    WSC(cpu);
                    WG(cpu);
                    break;
                case 10:
                    RU(cpu);
                    WZ(cpu);
                    break;
                case 11:
                    NISQ(cpu);
                    break;
            }
        }
    }
}
