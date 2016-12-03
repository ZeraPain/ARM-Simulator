using System;
using ARM_Simulator.Enumerations;
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
            AssertFail<ArgumentException>(_armCore, "add");
            AssertFail<ArgumentException>(_armCore, "add ,");
            AssertFail<ArgumentException>(_armCore, "add r1, #0");
            AssertFail<ArgumentException>(_armCore, "add r1, r2");
            AssertFail<ArgumentException>(_armCore, "add r1, r2, ");
            AssertFail<FormatException>(_armCore, "add r1, r2, #0x");
            AssertFail<FormatException>(_armCore, "add r1, r2, #");
            AssertFail<FormatException>(_armCore, "add r1, r2, #0f");
        }

        [TestMethod]
        public void TestCalculation()
        {
            _armCore.DirectExecute("mov r0, #3");
            _armCore.DirectExecute("mov r1, #5");
            _armCore.DirectExecute("mov r2, #0x4");
            _armCore.DirectExecute("mov r3, r2, lsl#28");

            _armCore.DirectExecute("add r4, r0, #7");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), 10);

            _armCore.DirectExecute("adds r4, r0, #7");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), 10);
            Assert.AreEqual(GetConditionFlags(), 0);

            _armCore.DirectExecute("add r4, r3, r3");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), int.MinValue);

            _armCore.DirectExecute("adds r4, r3, r3");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), int.MinValue);
            Assert.AreEqual(GetConditionFlags(), 9);

            _armCore.DirectExecute("adds r4, r1, r2, lsl#2");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), 21);
            Assert.AreEqual(GetConditionFlags(), 0);

            _armCore.DirectExecute("add r4, r1, r1, lsl#2");
            Assert.AreEqual(_armCore.GetRegValue(Register.R4), 25);
        }
    }
}
