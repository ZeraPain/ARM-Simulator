using System.Collections.Generic;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;

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

        public List<Command> LoadFile(string path)
        {
            var parser = new Parser(path);
            var source = parser.Encode();
            ArmCore.SetEntryPoint(parser.GetEntryPoint());
            _memory.LoadSource(source);

            return parser.GetCommandList();
        }

        public void TestCommand(string commandLine)
        {
            var parser = new Parser();
            var command = parser.ParseLine(commandLine);
            ArmCore.TestCommand(command);
        }
    }
}
