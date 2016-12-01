using ARM_Simulator.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var decoder = new ARMDecoder();
            decoder.Decode("mov r1, r2, r2");
        }
    }
}
