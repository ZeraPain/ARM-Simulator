using System.Collections.Generic;
using ARM_Simulator.Annotations;
using ARM_Simulator.Model.Components;
using ARM_Simulator.ViewModel.Observables;

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
        public List<ObservableCommand> LoadFile(string path)
        {
            ArmCore.Reset();
            Memory.Initialise();

            var parser = new Parser(path);
            var linker = new Linker(Memory, parser.CommandList, parser.CommandTable, parser.DataList, parser.DataTable);

            var commandList = linker.CompileAndLink();
            ArmCore.SetEntryPoint(linker.EntryPoint);

            return commandList;
        }

        public void TestCommand([NotNull] string commandLine)
        {
            var command = Parser.ParseLine(commandLine);
            ArmCore.TestCommand(command);
        }
    }
}
