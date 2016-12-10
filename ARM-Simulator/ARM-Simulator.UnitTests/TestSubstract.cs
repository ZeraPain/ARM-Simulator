using System;
using ARM_Simulator.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für TestAdd
    /// </summary>
    [TestClass]
    public class TestSubstract : TestBase
    {
        [TestMethod]
        public void TestSyntax()
        {
            AssertFail<ArgumentException>("sub");
            AssertFail<ArgumentException>("sub ,");
            AssertFail<ArgumentException>("sub r1, #0");
            AssertFail<ArgumentException>("sub r1, r2");
            AssertFail<ArgumentException>("sub r1, r2, ");
            AssertFail<FormatException>("sub r1, r2, #0x");
            AssertFail<FormatException>("sub r1, r2, #");
            AssertFail<FormatException>("sub r1, r2, #0f");
        }

        [TestMethod]
        public void TestCalculation()
        {
            TestSimulator.TestCommand("mov r0, #3");
            TestSimulator.TestCommand("mov r1, #5");
            TestSimulator.TestCommand("mov r2, #0x4");
            TestSimulator.TestCommand("mov r3, r2, lsl#28");

            TestSimulator.TestCommand("sub r4, r0, r1");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), -2);

            TestSimulator.TestCommand("subs r4, r0, r1");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), -2);
            Assert.AreEqual(GetConditionFlags(), 10);

            TestSimulator.TestCommand("sub r4, r0, #3");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), 0);

            TestSimulator.TestCommand("subs r4, r0, #3");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), 0);
            Assert.AreEqual(GetConditionFlags(), 4);

            TestSimulator.TestCommand("rsb r4, r1, r2, lsl#2");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), 11);

            TestSimulator.TestCommand("rsbs r4, r2, r0");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R4), -1);
            Assert.AreEqual(GetConditionFlags(), 10);
        }
    }
}
