using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                        binaryWriter.Write(command.Encode());
                    }

                    Ram.WriteTextSection(memoryStream.ToArray());
                }
            }
        }

        private void CompileAndLinkDataSection()
        {
            var data = DataList.SelectMany(a => a).ToArray();
            Ram.WriteDataSection(data);
        }
    }
}
