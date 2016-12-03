using System;
using ARM_Simulator.Enumerations;
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
            AssertFail<ArgumentException>(_armCore, "sub");
            AssertFail<ArgumentException>(_armCore, "sub ,");
            AssertFail<ArgumentException>(_armCore, "sub r1, #0");
            AssertFail<ArgumentException>(_armCore, "sub r1, r2");
            AssertFail<ArgumentException>(_armCore, "sub r1, r2, ");
            AssertFail<FormatException>(_armCore, "sub r1, r2, #0x");
            AssertFail<FormatException>(_armCore, "sub r1, r2, #");
            AssertFail<FormatException>(_armCore, "sub r1, r2, #0f");
        }

        [TestMethod]
        public void TestCalculation()
        {
            _armCore.DirectExecute("mov r0, #3");
            _armCore.DirectExecute("mov r1, #5");
            _armCore.DirectExecute("mov r2, #0x4");
            _armCore.DirectExecute("mov r3, r2, lsl#28");

            _armCore.DirectExecute("sub r4, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), -2);

            _armCore.DirectExecute("subs r4, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), -2);
            Assert.AreEqual(GetConditionFlags(), 10);

            _armCore.DirectExecute("sub r4, r0, #3");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), 0);

            _armCore.DirectExecute("subs r4, r0, #3");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), 0);
            Assert.AreEqual(GetConditionFlags(), 4);

            _armCore.DirectExecute("rsb r4, r1, r2, lsl#2");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), 11);

            _armCore.DirectExecute("rsbs r4, r2, r0");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), -1);
            Assert.AreEqual(GetConditionFlags(), 10);
        }
    }
}
