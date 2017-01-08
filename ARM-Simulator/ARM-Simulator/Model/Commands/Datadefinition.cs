using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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
        protected byte[] Value;
        protected bool Linked;

        public Datadefinition(EDataSize dataSize, [NotNull] string parameterString)
        {
            Linked = false;
            DataSize = dataSize;
            Parse(parameterString);

            Label = parameterString;
        }

        public void Parse([NotNull] string parameterString)
        {
            switch (DataSize)
            {
                case EDataSize.Word: // Could be and integer or a label, so we have to check if it is an integer or not
                    int intValue;
                    Linked = parameterString.StartsWith("0x", StringComparison.Ordinal) ? int.TryParse(parameterString.Substring(2, parameterString.Length - 2), NumberStyles.HexNumber, null, out intValue) : int.TryParse(parameterString, out intValue);
                    Value = BitConverter.GetBytes(intValue);
                    break;
                case EDataSize.Byte:
                    byte byteValue;
                    Linked = parameterString.StartsWith("0x", StringComparison.Ordinal) ? byte.TryParse(parameterString.Substring(2, parameterString.Length - 2), NumberStyles.HexNumber, null, out byteValue) : byte.TryParse(parameterString, out byteValue);
                    Value = BitConverter.GetBytes(byteValue);
                    break;
                case EDataSize.Ascii:
                    if (!parameterString.StartsWith("\"") || !parameterString.EndsWith("\"")) throw new ArgumentException();

                    Value = Encoding.UTF8.GetBytes(Regex.Unescape(parameterString.Substring(1, parameterString.Length - 2)));
                    Linked = true;
                    break;
                case EDataSize.Asciiz:
                    if (!parameterString.StartsWith("\"") || !parameterString.EndsWith("\"")) throw new ArgumentException();

                    var value = Encoding.ASCII.GetBytes(parameterString.Substring(1, parameterString.Length - 2));
                    Value = new byte[value.Length + 1];
                    Array.Copy(value, 0, Value, 0, value.Length);
                    Value[Value.Length - 1] = 0x0; // string termination
                    Linked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Execute(Core armCore)
        {
            throw new NotSupportedException();
        }

        public byte[] Encode()
        {
            if (!Linked) throw new InvalidOperationException();

            return Value;
        }

        public void Link(Dictionary<string, int> commandTable, [NotNull] Dictionary<string, int> dataTable, int commandOffset)
        {
            if (Linked) return;
            if (!dataTable.ContainsKey(Label)) throw new Exception("Unknown Label: " + Label);

            Value = BitConverter.GetBytes(dataTable[Label]);
            Linked = true;
        }

        public int GetCommandSize()
        {
            if (Value.Length % 4 == 0)
                return Value.Length;

            return Value.Length + (4 - Value.Length % 4);
        }
    }
}
