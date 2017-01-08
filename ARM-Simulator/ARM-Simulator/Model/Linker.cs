using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ARM_Simulator.Annotations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;

namespace ARM_Simulator.Model
{
    internal class Linker
    {
        protected Memory Ram;
        protected Dictionary<string, int> CommandTable;
        protected List<string> CommandList;
        protected Dictionary<string, int> DataTable;
        protected List<byte[]> DataList;

        public Linker(Memory ram, List<string> commandList, Dictionary<string, int> commandTable, List<byte[]> dataList, Dictionary<string, int> dataTable)
        {
            Ram = ram;
            CommandList = commandList;
            CommandTable = commandTable;
            DataTable = dataTable;
            DataList = dataList;
            UpdateDataTable();
        }

        public Linker(List<string> commandList)
        {
            CommandList = commandList;
        }

        private void UpdateDataTable()
        {
            var offset = (int)Ram.DataSectionStart;

            var keys = DataTable.Keys.ToList();
            foreach (var key in keys)
                DataTable[key] += offset;
        }

        public void CompileAndLink()
        {
            CompileAndLinkTextSection();
            CompileAndLinkDataSection();
        }

        private void CompileAndLinkTextSection()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    for (var i = 0; i < CommandList.Count; i++)
                    {
                        var commandLine = CommandList[i];
                        var command = Parser.ParseLine(commandLine);
                        command.Link(CommandTable, DataTable, i);
                        binaryWriter.Write(GenerateBytes(command));
                    }

                    Ram.WriteTextSection(memoryStream.ToArray());
                }
            }
        }

        [NotNull]
        private static byte[] GenerateBytes([NotNull] ICommand command)
        {
            var encCommand = command.Encode();
            if (encCommand.Length % 4 == 0)
                return encCommand;

            var retEncCommand = new byte[encCommand.Length + (4 - encCommand.Length % 4)]; // make sure that we write words (align 2)
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
