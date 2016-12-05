using System;
using ARM_Simulator.Model;
using ARM_Simulator.Utilitiy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für TestBase
    /// </summary>
    [TestClass]
    public class TestBase
    {
        protected Simulator TestSimulator;

        public TestBase()
        {
            TestSimulator = new Simulator();
        }

        protected void AssertFail<T>(string command)
        {
            try
            {
                TestSimulator.TestCommand(command);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is T);
            }
        }

        protected byte GetConditionFlags()
        {
            return (byte)((TestSimulator.ArmCore.GetRegValue(ERegister.Cpsr) & 0xf0000000) >> 28);
        }
    }
}
