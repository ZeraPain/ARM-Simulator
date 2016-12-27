using System.Collections.Generic;
using System.IO;
using ARM_Simulator.Model.Components;

namespace ARM_Simulator.Model
{
    internal class Linker
    {
        protected Memory Ram;
        protected List<string> CommandList;
        protected Dictionary<string, int> CommandTable;
        protected Dictionary<string, byte[]> DataToLink;
        protected Dictionary<string, int> DataTable;

        public Linker(Memory ram, List<string> commandList, Dictionary<string, int> commandTable, Dictionary<string, byte[]> dataToLink)
        {
            Ram = ram;
            CommandList = commandList;
            CommandTable = commandTable;
            DataToLink = dataToLink;
            GenerateDataTable();
        }

        private void GenerateDataTable()
        {
            DataTable = new Dictionary<string, int>();

            var offset = (int)Ram.DataSectionStart;
            foreach (var data in DataToLink)
            {
                DataTable.Add(data.Key, offset);
                offset += data.Value.Length;
            }
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
                        binaryWriter.Write(command.Encode());
                    }

                    Ram.WriteTextSection(memoryStream.ToArray());
                }
            }
        }

        private void CompileAndLinkDataSection()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    foreach (var data in DataToLink)
                        binaryWriter.Write(data.Value);

                    Ram.WriteDataSection(memoryStream.ToArray());
                }
            }
        }
    }
}
