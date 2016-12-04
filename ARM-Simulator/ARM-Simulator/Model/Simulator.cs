using System.Collections.Generic;
using System.IO;
using System.Linq;
using ARM_Simulator.Model.Components;

namespace ARM_Simulator.Model
{
    public class Simulator
    {
        public Core ArmCore { get; }
        private readonly Memory _memory;

        public Simulator()
        {
            _memory = new Memory(0x40000, 0x10000);
            ArmCore = new Core(_memory);
        }

        public List<int> Compile(string path)
        {
            var lines = File.ReadAllLines(path);
            var parser = new Parser();

            return lines.Select(line => parser.ParseLine(line)).Select(command => command.Encode()).ToList();
        }

        public void LoadSource(List<int> source)
        {
            _memory.WriteSource(source);
        }

        public void Run()
        {
            while (true)
            {
                ArmCore.Tick();
            }
        }

        public void TestCommand(string commandLine)
        {
            var parser = new Parser();
            var command = parser.ParseLine(commandLine);
            ArmCore.TestCommand(command);
        }
    }
}
