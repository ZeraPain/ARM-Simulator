using System;
using System.Collections.Generic;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;

namespace ARM_Simulator.Model.Commands
{
    internal class MoveStatus : ICommand
    {
        public void Parse(string parameterString)
        {
            throw new NotImplementedException();
        }

        public void Execute(Core armCore)
        {
            throw new NotImplementedException();
        }

        public byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public void Link(Dictionary<string, int> commandTable, Dictionary<string, int> dataTable, int commandOffset)
        {

        }

        public int GetCommandSize() => 4;
    }
}
