using Microsoft.VisualStudio.TestTools.UnitTesting;
using AGC_Sharp;
using AGC_Sharp.ISA;

namespace AGC_Sharp_Tests
{
    [TestClass]
    public class ControlPulseTests
    {
        [TestMethod]
        public void TestA2X()
        {
            Cpu cpu = new();

            for (ushort i = ushort.MinValue; i < ushort.MaxValue; ++i)
            {
                cpu.RegisterA = i;
                ControlPulses.A2X(cpu);
                Assert.AreEqual(cpu.RegisterA, cpu.AdderX);
            }
        }

        [TestMethod]
        public void TestCI()
        {
            Cpu cpu = new();

            cpu.AdderCarry = false;
            ControlPulses.CI(cpu);
            Assert.IsTrue(cpu.AdderCarry);
        }
    }
}