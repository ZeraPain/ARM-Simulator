using System.Collections.Generic;
using ARM_Simulator.Annotations;
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

        [NotNull]
        public List<Command> LoadFile(string path)
        {
            ArmCore.Reset();
            Memory.Initialise();

            var parser = new Parser(path);
            ArmCore.SetEntryPoint(parser.GetEntryPoint());
            Memory.WriteTextSection(parser.EncodeTextSection());
            Memory.WriteDataSection(parser.EncodeDataSection());
            return parser.GetCommandList();
        }

        public void TestCommand(string commandLine)
        {
            var parser = new Parser();
            var command = parser.ParseLine(commandLine, 0);
            ArmCore.TestCommand(command);
        }
    }
}
