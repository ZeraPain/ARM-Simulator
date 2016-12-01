using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private void AssertFail<T>(ArmCore armCore, string command)
        {
            try
            {
                armCore.Pipeline.DirectExecute(command);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is T);
            }
        }

        [TestMethod]
        public void TestMov()
        {
            var armCore = new ArmCore();
            armCore.Pipeline.DirectExecute("mov r0, #77");
            Assert.AreEqual(armCore.Registers[ArmRegister.R0], 77);

            armCore.Pipeline.DirectExecute("mov r0, #0x3f");
            Assert.AreEqual(armCore.Registers[ArmRegister.R0], 0x3f);

            armCore.Pipeline.DirectExecute("mov r1, #2");
            armCore.Pipeline.DirectExecute("mov r0, r1");
            Assert.AreEqual(armCore.Registers[ArmRegister.R0], 2);
            armCore.Pipeline.DirectExecute("mov r0, r1, lsl #2");
            Assert.AreEqual(armCore.Registers[ArmRegister.R0], 8);

            AssertFail<ArgumentException>(armCore, "mov r1, r2, r2");
            AssertFail<ArgumentException>(armCore, "mov r1, r2, #0x");
            AssertFail<ArgumentException>(armCore, "mov r1");
            AssertFail<FormatException>(armCore, "mov r1, #0ff");

        }
    }
}
