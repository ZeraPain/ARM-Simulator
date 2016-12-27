using System;
using System.Collections.Generic;
using ARM_Simulator.Annotations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;

namespace ARM_Simulator.Model.Commands
{
    internal class Datadefinition : ICommand
    {
        protected EDataSize DataSize;
        protected string Label;
        protected int Value;
        protected bool Linked;

        public Datadefinition(EDataSize dataSize, string parameterString)
        {
            Linked = false;
            DataSize = dataSize;
            Label = parameterString;
        }

        public void Parse([NotNull] string parameterString)
        {
            throw new NotImplementedException();
        }

        public void Execute(Core armCore)
        {
            throw new NotImplementedException();
        }

        public int Encode()
        {
            if (!Linked)
                throw new Exception("Cannot encode an unlinked command");

            return Value;
        }

        public void Link(Dictionary<string, int> commandTable, [NotNull] Dictionary<string, int> dataTable, int commandOffset)
        {
            if (!dataTable.ContainsKey(Label))
                throw new Exception("Unknown Label: " + Label);

            Value = dataTable[Label];
            Linked = true;
        }
    }
}
