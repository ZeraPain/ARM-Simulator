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
        public List<ObservableCommand> LoadFile(string[] hLines)
        {
            // Reset out components
            ArmCore.Reset();
            Memory.Initialise();

            // Parse the program code
            var parser = new Parser();
            parser.ParseFile(hLines);

            // Compile and Link 
            var linker = new Linker(Memory, parser);
            var commandList = linker.CompileAndLink();

            ArmCore.SetEntryPoint(linker.EntryPoint);

            // Return commandlist to our ViewModel
            return commandList;
        }

        // Only necessary for unit tests
        public void TestCommand([NotNull] string commandLine)
        {
            var command = Parser.ParseLine(commandLine);
            ArmCore.TestCommand(command);
        }
    }
}
