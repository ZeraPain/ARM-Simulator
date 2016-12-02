using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    [TestClass]
    public class UnitTestCommands
    {
        private readonly ArmCore _armCore;
        public UnitTestCommands()
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

            _armCore.DirectExecute("add r2, r0, #7");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 10);

            _armCore.DirectExecute("add r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 8);

            _armCore.DirectExecute("add r2, r1, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 10);

            _armCore.DirectExecute("add r2, r1, r1, lsl#2");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 25);

            AssertFail<ArgumentException>(_armCore, "add r1, r2 r2");
            AssertFail<FormatException>(_armCore, "add r1, r2, #0x");
            AssertFail<ArgumentException>(_armCore, "add r1, r1");
            AssertFail<ArgumentException>(_armCore, "add r1");
            AssertFail<ArgumentException>(_armCore, "add r1, #0");
            AssertFail<FormatException>(_armCore, "add r1, r1, #0ff");
        }

        [TestMethod]
        public void TestSubstract()
        {
            _armCore.DirectExecute("mov r0, #3");
            _armCore.DirectExecute("mov r1, #5");

            _armCore.DirectExecute("sub r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), -2);

            _armCore.DirectExecute("sub r2, r0, #7");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), -4);

            _armCore.DirectExecute("rsb r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 2);
        }

        [TestMethod]
        public void TestLogical()
        {
            _armCore.DirectExecute("mov r0, #3");
            _armCore.DirectExecute("mov r1, #5");

            _armCore.DirectExecute("and r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 1);

            _armCore.DirectExecute("eor r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 6);

            _armCore.DirectExecute("orr r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 7);

            _armCore.DirectExecute("orn r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), -5);

            _armCore.DirectExecute("bic r2, r0, r1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 2);

            _armCore.DirectExecute("bic r2, r1, r0");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 4);
        }

        [TestMethod]
        public void TestShift()
        {
            _armCore.DirectExecute("mov r0, #3");
            _armCore.DirectExecute("mov r1, #8");

            _armCore.DirectExecute("lsl r2, r0, #1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 6);
            _armCore.DirectExecute("lsr r2, r0, #1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 1);
            _armCore.DirectExecute("ror r2, r1, #1");
            Assert.AreEqual(_armCore.GetRegValue(ArmRegister.R2), 4);
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
