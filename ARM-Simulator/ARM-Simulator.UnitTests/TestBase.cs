using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für TestBase
    /// </summary>
    [TestClass]
    public class TestBase
    {
        protected readonly Core _armCore;

        public TestBase()
        {
            _armCore = new Core();
        }

        protected void AssertFail<T>(Core armCore, string command)
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

        protected byte GetConditionFlags()
        {
            return (byte)((_armCore.GetRegValue(Register.Cpsr) & 0xf0000000) >> 28);
        }
    }
}
