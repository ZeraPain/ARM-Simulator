using ARM_Simulator.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für TestAdd
    /// </summary>
    [TestClass]
    public class TestMultiply : TestBase
    {
        [TestMethod]
        public void TestCalculation()
        {
            TestSimulator.TestCommand("mov r0, #0");
            TestSimulator.TestCommand("mov r1, #1");
            TestSimulator.TestCommand("mov r1, r1, lsl#16");
            TestSimulator.TestCommand("mov r2, #2");

            TestSimulator.TestCommand("mul r0, r1, r1");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 0);

            TestSimulator.TestCommand("mul r0, r2, r2");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 4);

            TestSimulator.TestCommand("mul r0, r1, r2");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 0x20000);

            TestSimulator.TestCommand("mla r0, r1, r2, r2");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 0x20002);
        }
    }
}
