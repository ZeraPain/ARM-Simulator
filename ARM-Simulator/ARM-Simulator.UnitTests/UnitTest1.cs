using System;
using ARM_Simulator.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMov()
        {
            var decoder = new ARMDecoder();
            decoder.Decode("mov r1, #0");
            decoder.Decode("mov r1, #0x0");

            decoder.Decode("mov r1, r2");
            decoder.Decode("mov r1, r2, LSL#2");

            try
            {
                decoder.Decode("mov r1, r2, r2");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentException);
            }

            try
            {
                decoder.Decode("mov r1, r2, #0x");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentException);
            }

            try
            {
                decoder.Decode("mov r1");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentException);
            }

            try
            {
                decoder.Decode("mov r1, #0ff");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is FormatException);
            }
        }
    }
}
