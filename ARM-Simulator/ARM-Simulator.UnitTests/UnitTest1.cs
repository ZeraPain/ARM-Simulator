using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private readonly ArmCore _armCore;
        public UnitTest1()
        {
            _armCore = new ArmCore();
        }

        [TestMethod]
        public void TestMov()
        {
            _armCore.DirectExecute("mov r0, #77");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 77);

            _armCore.DirectExecute("mov r0, #0x3f");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 0x3f);

            _armCore.DirectExecute("mov r1, #2");
            _armCore.DirectExecute("mov r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 2);
            _armCore.DirectExecute("mov r0, r1, lsl #2");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 8);

            AssertFail<ArgumentException>(_armCore, "mov r1, r2, r2");
            AssertFail<FormatException>(_armCore, "mov r1, #0x");
            AssertFail<ArgumentException>(_armCore, "mov r1");
            AssertFail<FormatException>(_armCore, "mov r1, #0ff");
        }

        [TestMethod]
        public void TestAdd()
        {
            _armCore.DirectExecute("mov r0, #3");
            _armCore.DirectExecute("mov r1, #5");

            _armCore.DirectExecute("add r0, r0, #7");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 10);
            _armCore.DirectExecute("add r0, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 15);
            _armCore.DirectExecute("add r0, r1, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 10);
            _armCore.DirectExecute("add r0, r1, r1, lsl#2");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R0), 25);

            AssertFail<ArgumentException>(_armCore, "add r1, r2 r2");
            AssertFail<FormatException>(_armCore, "add r1, r2, #0x");
            AssertFail<ArgumentException>(_armCore, "add r1, r1");
            AssertFail<ArgumentException>(_armCore, "add r1");
            AssertFail<ArgumentException>(_armCore, "add r1, #0");
            AssertFail<FormatException>(_armCore, "add r1, r1, #0ff");
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

        private void AssertFail<T>(ArmCore armCore, string command)
        {
            try
            {
                armCore.DirectExecute(command);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is T);
            }
        }
    }
}
