using System;
using ARM_Simulator.Enumerations;
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
            AssertFail<ArgumentException>(_armCore, "tst r1,");
            AssertFail<ArgumentException>(_armCore, "tst ,");
            AssertFail<ArgumentException>(_armCore, "tst r1, r2, ");
            AssertFail<FormatException>(_armCore, "tst r1, #0x");
            AssertFail<FormatException>(_armCore, "tst r1, #");
            AssertFail<FormatException>(_armCore, "tst r1, #0f");
        }

        [TestMethod]
        public void TestCalculation()
        {
            _armCore.DirectExecute("mov r0, #3");
            _armCore.DirectExecute("mov r1, #5");

            _armCore.DirectExecute("and r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(Register.R2), 1);

            _armCore.DirectExecute("eor r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(Register.R2), 6);

            _armCore.DirectExecute("orr r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(Register.R2), 7);

            _armCore.DirectExecute("bic r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(Register.R2), 2);

            _armCore.DirectExecute("bic r2, r1, r0");
            Assert.AreEqual(_armCore.GetRegValue(Register.R2), 4);
        }
    }
}
