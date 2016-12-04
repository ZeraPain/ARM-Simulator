using System;
using ARM_Simulator.Utilitiy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für TestAdd
    /// </summary>
    [TestClass]
    public class TestLogical : TestBase
    {
        [TestMethod]
        public void TestSyntax()
        {
            AssertFail<ArgumentException>("tst r1,");
            AssertFail<ArgumentException>("tst ,");
            AssertFail<ArgumentException>("tst r1, r2, ");
            AssertFail<FormatException>("tst r1, #0x");
            AssertFail<FormatException>("tst r1, #");
            AssertFail<FormatException>("tst r1, #0f");
        }

        [TestMethod]
        public void TestCalculation()
        {
            TestSimulator.TestCommand("mov r0, #3");
            TestSimulator.TestCommand("mov r1, #5");

            TestSimulator.TestCommand("and r2, r0, r1");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(Register.R2), 1);

            TestSimulator.TestCommand("eor r2, r0, r1");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(Register.R2), 6);

            TestSimulator.TestCommand("orr r2, r0, r1");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(Register.R2), 7);

            TestSimulator.TestCommand("bic r2, r0, r1");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(Register.R2), 2);

            TestSimulator.TestCommand("bic r2, r1, r0");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(Register.R2), 4);
        }
    }
}
