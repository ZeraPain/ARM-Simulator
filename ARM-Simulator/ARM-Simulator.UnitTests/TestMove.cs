using System;
using ARM_Simulator.Utilitiy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für TestAdd
    /// </summary>
    [TestClass]
    public class TestMove : TestBase
    {
        [TestMethod]
        public void TestSyntax()
        {
            AssertFail<ArgumentException>("mov");
            AssertFail<ArgumentException>("mov ,");
            AssertFail<FormatException>("mov r1, #0x");
            AssertFail<FormatException>("mov r1, #");
            AssertFail<FormatException>("mov r1, #0f");
        }

        [TestMethod]
        public void TestCalculation()
        {
            TestSimulator.TestCommand("mov r0, #77");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 77);

            TestSimulator.TestCommand("mov r0, #0x3f");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 0x3f);

            TestSimulator.TestCommand("mov r1, #2");
            TestSimulator.TestCommand("mov r0, r1");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 2);
            TestSimulator.TestCommand("mov r1, r1, lsl r1");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R1), 8);

            TestSimulator.TestCommand("mov r0, r0, lsl #2");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 8);

            TestSimulator.TestCommand("mvn r0, #0");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), -1);

            TestSimulator.TestCommand("movs r0, #0");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), 0);
            Assert.AreEqual(GetConditionFlags(), 4);

            TestSimulator.TestCommand("mvns r0, #0");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R0), -1);
            Assert.AreEqual(GetConditionFlags(), 8);

            TestSimulator.TestCommand("mov r1, #8");
            TestSimulator.TestCommand("movs r0, r1, lsl#29");
            Assert.AreEqual(GetConditionFlags(), 6);
        }
    }
}
