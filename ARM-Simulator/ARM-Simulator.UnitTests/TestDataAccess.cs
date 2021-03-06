﻿using System;
using System.Reflection;
using ARM_Simulator.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ARM_Simulator.UnitTests
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für TestDataAccess
    /// </summary>
    [TestClass]
    public class TestDataAccess : TestBase
    {
        [TestMethod]
        public void TestSyntax()
        {
            AssertFail<TargetParameterCountException>("ldr");
            AssertFail<Exception>("ldr r0, r1");
            AssertFail<TargetParameterCountException>("ldr [r0]");
            AssertFail<TargetParameterCountException>("ldr r0, []");
            AssertFail<TargetParameterCountException>("ldr r0, r1]]");
            AssertFail<ArgumentException>("ldr r0, [r1]!, #4");
        }

        [TestMethod]
        public void TestCalculation()
        {
            TestSimulator.TestCommand("mov r0, #7");
            TestSimulator.TestCommand("mov r2, #0");
            TestSimulator.TestCommand("mov r1, #0x1");
            TestSimulator.TestCommand("mov r1, r1, lsl#16");
            TestSimulator.TestCommand("str r0, [r1, #0x100]!");
            TestSimulator.TestCommand("ldr r2, [r1]");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R1), 0x10100);
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R2), 7);

            TestSimulator.TestCommand("str r0, [r1], #0x100");
            TestSimulator.TestCommand("ldr r2, [r1]");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R1), 0x10200);
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R2), 0);

            TestSimulator.TestCommand("str r0, [r1, r0]");
            TestSimulator.TestCommand("ldr r2, [r1, r0]");
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R1), 0x10200);
            Assert.AreEqual(TestSimulator.ArmCore.GetRegValue(ERegister.R2), 7);
        }
    }
}
