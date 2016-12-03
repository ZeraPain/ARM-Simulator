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
            AssertFail<ArgumentException>(_armCore, "cmp r1,");
            AssertFail<ArgumentException>(_armCore, "cmp ,");
            AssertFail<ArgumentException>(_armCore, "cmp r1, r2, ");
            AssertFail<FormatException>(_armCore, "cmp r1, #0x");
            AssertFail<FormatException>(_armCore, "cmp r1, #");
            AssertFail<FormatException>(_armCore, "cmp r1, #0f");
        }

        [TestMethod]
        public void TestCalculation()
        {
            _armCore.DirectExecute("mov r0, #3");
            _armCore.DirectExecute("mov r1, #2");
            _armCore.DirectExecute("mvn r2, #2");

            _armCore.DirectExecute("cmp r0, #3");
            Assert.AreEqual(GetConditionFlags(), 4);

            _armCore.DirectExecute("cmp r0, r0");
            Assert.AreEqual(GetConditionFlags(), 4);

            _armCore.DirectExecute("cmn r0, r2");
            Assert.AreEqual(GetConditionFlags(), 4);

            _armCore.DirectExecute("cmn r0, r1");
            Assert.AreEqual(GetConditionFlags(), 0);

            _armCore.DirectExecute("tst r1, r2");
            Assert.AreEqual(GetConditionFlags(), 4);

            _armCore.DirectExecute("teq r1, r1");
            Assert.AreEqual(GetConditionFlags(), 4);
        }
    }
}
