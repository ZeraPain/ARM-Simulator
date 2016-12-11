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
            TestSimulator.TestCommand("mov r5, r1, lsl#16");
            TestSimulator.TestCommand("mov r6, r1, lsl#20");
            TestSimulator.TestCommand("mov r2, #2");
            TestSimulator.TestCommand("mvn r7, #0");

            TestSimulator.TestCommand("mul r0, r5, r5");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 0);

            TestSimulator.TestCommand("mul r0, r2, r2");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 4);

            TestSimulator.TestCommand("mul r0, r5, r2");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 0x20000);

            TestSimulator.TestCommand("mla r0, r5, r2, r2");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 0x20002);

            TestSimulator.TestCommand("smull r3, r4, r6, r6");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R3), 0);
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), 0x100);

            TestSimulator.TestCommand("mov r3, #2");
            TestSimulator.TestCommand("mov r4, #2");
            TestSimulator.TestCommand("smlal r3, r4, r6, r6");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R3), 2);
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), 0x102);

            TestSimulator.TestCommand("mov r3, #0");
            TestSimulator.TestCommand("mov r4, #0");
            TestSimulator.TestCommand("smlal r3, r4, r6, r7");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R3), -1048576); //0xFFFF0000
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), -1); //0xFFFFFFFF
        }
    }
}
