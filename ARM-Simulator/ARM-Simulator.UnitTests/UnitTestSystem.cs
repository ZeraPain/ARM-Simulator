using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für UnitTestSystem
    /// </summary>
    [TestClass]
    public class UnitTestSystem
    {
        private readonly ArmCore _armCore;
        public UnitTestSystem()
        {
            _armCore = new ArmCore();
        }

        [TestMethod]
        public void TestConditionFlags()
        {
            _armCore.DirectExecute("mov r0, #3");
            _armCore.DirectExecute("mov r1, #5");
            _armCore.DirectExecute("mvn r3, #3");

            _armCore.DirectExecute("subs r2, r0, #3");
            Assert.AreEqual(GetConditionFlags(), 4);

            _armCore.DirectExecute("subs r2, r0, #4");
            Assert.AreEqual(GetConditionFlags(), 10);

            _armCore.DirectExecute("adds r2, r2, #1");
            Assert.AreEqual(GetConditionFlags(), 4);

            _armCore.DirectExecute("teq r0, r0");
            Assert.AreEqual(GetConditionFlags(), 4);

            _armCore.DirectExecute("tst r0, r0");
            Assert.AreEqual(GetConditionFlags(), 0);

            _armCore.DirectExecute("tst r0, r3");
            Assert.AreEqual(GetConditionFlags(), 4);

            _armCore.DirectExecute("cmp r0, #3");
            Assert.AreEqual(GetConditionFlags(), 4);

            _armCore.DirectExecute("mov r4, #0");
            _armCore.DirectExecute("sub r4, r4, #3");
            _armCore.DirectExecute("cmn r0, r4");
            Assert.AreEqual(GetConditionFlags(), 4);
        }

        [TestMethod]
        public void TestPipeline()
        {
            _armCore.DirectExecute("mov r0, #0");

            _armCore.Tick("mov r0, #12");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 0);

            _armCore.Tick("mov r0, #13");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 0);

            _armCore.Tick("mov r0, #14");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 12);

            _armCore.Tick("");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 13);

            _armCore.Tick("");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 14);
        }

        private byte GetConditionFlags()
        {
            return (byte)((_armCore.GetRegValue(ArmRegister.Cpsr) & 0xf0000000) >> 28);
        }
    }
}
