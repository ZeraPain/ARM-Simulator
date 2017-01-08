using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ARM_Simulator.Annotations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.Model
{
    internal class Linker
    {
        protected Memory Ram;
        protected Dictionary<string, int> CommandTable;
        protected List<string> CommandList;
        protected Dictionary<string, int> DataTable;
        protected List<byte[]> DataList;
        public int EntryPoint { get; protected set; }
        private readonly int _align;

        public Linker(Memory ram, [NotNull] Parser parser)
        {
            EntryPoint = -1;
            Ram = ram;
            _align = parser.Align;
            CommandList = parser.CommandList;
            CommandTable = parser.CommandTable;
            DataTable = parser.DataTable;
            DataList = parser.DataList;
            UpdateCommandTable(parser.EntryFunction);
            UpdateDataTable();
        }

        private void UpdateDataTable()
        {
            var offset = (int)Ram.DataSectionStart;

            var keys = DataTable.Keys.ToList();
            foreach (var key in keys)
                DataTable[key] += offset;
        }

        private void UpdateCommandTable(string entryFunction)
        {
            var offset = 0;

            for (var i = 0; i < CommandList.Count; i++)
            {
                for (var k = 0; k < CommandTable.Count; k++)
                {
                    var label = CommandTable.ElementAt(k);
                    if (label.Value == i)
                    {
                        CommandTable[label.Key] = offset;
                        if (label.Key == entryFunction) EntryPoint = offset;
                    }
                }

                offset += Parser.ParseLine(CommandList[i]).GetCommandSize(_align);
            }
        }

        [NotNull]
        public List<ObservableCommand> CompileAndLink()
        {
            var commandList = CompileAndLinkTextSection();
            CompileAndLinkDataSection();

            return commandList;
        }

        [NotNull]
        private List<ObservableCommand> CompileAndLinkTextSection()
        {
            var commandList = new List<ObservableCommand>();

            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    var offset = 0;
                    foreach (var t in CommandList)
                    {
                        var observableCommand = new ObservableCommand
                        {
                            Status = EntryPoint == offset ? EPipeline.Fetch : EPipeline.None,
                            Address = offset,
                            Breakpoint = false,
                            Commandline = t
                        };

                        foreach (var label in CommandTable.Where(label => label.Value == offset))
                            observableCommand.Label = label.Key;

                        commandList.Add(observableCommand);

                        var command = Parser.ParseLine(t);
                        command.Link(CommandTable, DataTable, offset);
                        offset += command.GetCommandSize(_align);
                        binaryWriter.Write(GenerateBytes(command, _align));
                    }

                    Ram.WriteTextSection(memoryStream.ToArray());
                }
            }

            return commandList;
        }

        [NotNull]
        private static byte[] GenerateBytes([NotNull] ICommand command, int align)
        {
            var size = command.GetCommandSize(align);
            var encCommand = command.Encode();
            var retEncCommand = new byte[size];
            Array.Copy(encCommand, 0, retEncCommand, 0, encCommand.Length);
            return retEncCommand;
        }

        private void CompileAndLinkDataSection()
        {
            var data = DataList.SelectMany(a => a).ToArray();
            Ram.WriteDataSection(data);
        }
    }
}
