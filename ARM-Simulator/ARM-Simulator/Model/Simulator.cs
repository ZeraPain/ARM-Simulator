﻿using System.Threading;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;

namespace ARM_Simulator.Model
{
    public class Simulator
    {
        public Core ArmCore { get; }
        private bool _running;
        private readonly Memory _memory;
        private Thread _runThread;

        public Simulator()
        {
            _memory = new Memory(0x40000, 0x10000);
            ArmCore = new Core(_memory);
        }

        public void LoadFile(string path)
        {
            var parser = new Parser();
            var source = parser.ParseFile(path);
            var entry = parser.GetEntryPoint();
            ArmCore.SetRegValue(ERegister.Pc, entry);
            _memory.LoadSource(source);
        }

        private void Run()
        {
            while (_running)
            {
                ArmCore.Tick();
            }
        }

        public void Start()
        {
            _running = true;
            _runThread = new Thread(Run);
            _runThread.Start();
        }

        public void Stop()
        {
            _running = false;
            _runThread?.Join();
        }

        public void TestCommand(string commandLine)
        {
            var parser = new Parser();
            var command = parser.ParseLine(commandLine);
            ArmCore.TestCommand(command);
        }
    }
}
