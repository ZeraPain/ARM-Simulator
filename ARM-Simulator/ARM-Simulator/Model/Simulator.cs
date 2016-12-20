using System.Collections.Generic;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;

namespace ARM_Simulator.Model
{
    public class Simulator
    {
        public Core ArmCore { get; protected set; }
        public Memory Memory { get; protected set; }

        public Simulator()
        {
            Memory = new Memory(0x40000, 0x10000);
            ArmCore = new Core(Memory);
        }

        public List<Command> LoadFile(string path)
        {
            ArmCore.Reset();

            var parser = new Parser(path);
            var source = parser.Encode();
            ArmCore.SetEntryPoint(parser.GetEntryPoint());
            Memory.LoadSource(source);

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
