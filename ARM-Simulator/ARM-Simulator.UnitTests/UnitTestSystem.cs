using ARM_Simulator.Enumerations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für UnitTestSystem
    /// </summary>
    [TestClass]
    public class UnitTestSystem : TestBase
    {
        [TestMethod]
        public void TestPipeline()
        {
            _armCore.DirectExecute("mov r0, #0");

            _armCore.Tick("mov r0, #12");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), 0);

            _armCore.Tick("mov r0, #13");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), 0);

            _armCore.Tick("mov r0, #14");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), 12);

            _armCore.Tick("");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), 13);

            _armCore.Tick("");
            Assert.AreEqual(_armCore.GetRegValue(Register.R0), 14);
        }
    }
}
