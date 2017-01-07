using System;
using System.Collections.Generic;
using System.Globalization;
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
            Parse(parameterString);

            Label = parameterString;
        }

        public void Parse([NotNull] string parameterString)
        {
            if (parameterString.StartsWith("0x", StringComparison.Ordinal))
            {
                Linked = int.TryParse(parameterString.Substring(2, parameterString.Length - 2), NumberStyles.HexNumber,
                    null, out Value);
                return;
            }

            Linked = int.TryParse(parameterString, out Value);
        }

        public void Execute(Core armCore)
        {
            throw new NotSupportedException();
        }

        public int Encode()
        {
            if (!Linked) throw new InvalidOperationException();

            return Value;
        }

        public void Link(Dictionary<string, int> commandTable, [NotNull] Dictionary<string, int> dataTable, int commandOffset)
        {
            if (Linked) return;
            if (!dataTable.ContainsKey(Label)) throw new Exception("Unknown Label: " + Label);

            Value = dataTable[Label];
            Linked = true;
        }
    }
}
