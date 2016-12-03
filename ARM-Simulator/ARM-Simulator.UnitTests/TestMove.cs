using System;
using ARM_Simulator.Enumerations;
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
            AssertFail<ArgumentException>(_armCore, "mov");
            AssertFail<ArgumentException>(_armCore, "mov ,");
            AssertFail<ArgumentException>(_armCore, "mov r1, r2, r3");
            AssertFail<FormatException>(_armCore, "mov r1, #0x");
            AssertFail<FormatException>(_armCore, "mov r1, #");
            AssertFail<FormatException>(_armCore, "mov r1, #0f");
        }

        [TestMethod]
        public void TestCalculation()
        {
            _armCore.DirectExecute("mov r0, #77");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), 77);

            _armCore.DirectExecute("mov r0, #0x3f");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), 0x3f);

            _armCore.DirectExecute("mov r1, #2");
            _armCore.DirectExecute("mov r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), 2);

            _armCore.DirectExecute("mov r0, r1, lsl #2");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), 8);

            _armCore.DirectExecute("mvn r0, #0");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), -1);

            _armCore.DirectExecute("movs r0, #0");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), 0);
            Assert.AreEqual(GetConditionFlags(), 4);

            _armCore.DirectExecute("mvns r0, #0");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), -1);
            Assert.AreEqual(GetConditionFlags(), 8);

            _armCore.DirectExecute("mov r1, #8");
            _armCore.DirectExecute("movs r0, r1, lsl#29");
            Assert.AreEqual(GetConditionFlags(), 6);
        }
    }
}
