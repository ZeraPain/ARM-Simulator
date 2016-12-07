using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für TestAdd
    /// </summary>
    [TestClass]
    public class TestCompare : TestBase
    {
        [TestMethod]
        public void TestSyntax()
        {
            AssertFail<ArgumentException>("cmp r1,");
            AssertFail<ArgumentException>("cmp ,");
            AssertFail<FormatException>("cmp r1, #0x");
            AssertFail<FormatException>("cmp r1, #");
            AssertFail<FormatException>("cmp r1, #0f");
        }

        [TestMethod]
        public void TestCalculation()
        {
            TestSimulator.TestCommand("mov r0, #3");
            TestSimulator.TestCommand("mov r1, #2");
            TestSimulator.TestCommand("mvn r2, #2");

            TestSimulator.TestCommand("cmp r0, #3");
            Assert.AreEqual(GetConditionFlags(), 4);

            TestSimulator.TestCommand("cmp r0, r0");
            Assert.AreEqual(GetConditionFlags(), 4);

            TestSimulator.TestCommand("cmn r0, r2");
            Assert.AreEqual(GetConditionFlags(), 4);

            TestSimulator.TestCommand("cmn r0, r1");
            Assert.AreEqual(GetConditionFlags(), 0);

            TestSimulator.TestCommand("tst r1, r2");
            Assert.AreEqual(GetConditionFlags(), 4);

            TestSimulator.TestCommand("teq r1, r1");
            Assert.AreEqual(GetConditionFlags(), 4);
        }
    }
}
