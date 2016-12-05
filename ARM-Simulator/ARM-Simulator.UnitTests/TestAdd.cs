using System;
using ARM_Simulator.Utilitiy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für TestAdd
    /// </summary>
    [TestClass]
    public class TestAdd : TestBase
    {
        [TestMethod]
        public void TestSyntax()
        {
            AssertFail<ArgumentException>("add");
            AssertFail<ArgumentException>("add ,");
            AssertFail<ArgumentException>("add r1, #0");
            AssertFail<ArgumentException>("add r1, r2");
            AssertFail<ArgumentException>("add r1, r2, ");
            AssertFail<FormatException>("add r1, r2, #0x");
            AssertFail<FormatException>("add r1, r2, #");
            AssertFail<FormatException>("add r1, r2, #0f");
        }

        [TestMethod]
        public void TestCalculation()
        {
            TestSimulator.TestCommand("mov r0, #3");
            TestSimulator.TestCommand("mov r1, #5");
            TestSimulator.TestCommand("mov r2, #0x4");
            TestSimulator.TestCommand("mov r3, r2, lsl#28");

            TestSimulator.TestCommand("add r4, r0, #7");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), 10);

            TestSimulator.TestCommand("adds r4, r0, #7");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), 10);
            Assert.AreEqual(GetConditionFlags(), 0);

            TestSimulator.TestCommand("add r4, r3, r3");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), int.MinValue);

            TestSimulator.TestCommand("adds r4, r3, r3");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), int.MinValue);
            Assert.AreEqual(GetConditionFlags(), 9);

            TestSimulator.TestCommand("adds r4, r1, r2, lsl#2");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), 21);
            Assert.AreEqual(GetConditionFlags(), 0);

            TestSimulator.TestCommand("add r4, r1, r1, lsl#2");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), 25);
        }
    }
}
