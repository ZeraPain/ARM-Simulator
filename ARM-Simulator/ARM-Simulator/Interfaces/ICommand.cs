using System.Collections.Generic;
using ARM_Simulator.Model.Components;

namespace ARM_Simulator.Interfaces
{
    public interface ICommand
    {
        void Parse(string parameterString);
        void Execute(Core armCore);
        byte[] Encode();
        int GetCommandSize();
        void Link(Dictionary<string, int> commandTable, Dictionary<string, int> dataTable, int commandOffset);
    }
}
