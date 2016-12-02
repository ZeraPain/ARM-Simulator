using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Commands;

namespace ARM_Simulator.Model
{
    public class ArmDecoder
    {
        public ICommand Decode(string command)
        {
            var cmd = ParseCommand(command);

            switch (cmd.Opcode)
            {
                case ArmOpCode.None:
                     throw new ArgumentException("Invalid Opcode");
                case ArmOpCode.Mov:
                    var mov = new Mov(cmd.Parameters);
                    mov.Decode();
                    return mov;
                case ArmOpCode.Add:
                    var add = new Add(cmd.Parameters);
                    add.Decode();
                    return add;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Command ParseCommand(string command)
        {
            if (command == string.Empty)
                return new Command(ArmOpCode.None, null);

            var index = command.IndexOf(' ');
            if (index == -1)
                index = command.IndexOf('\t');

            if (index == -1 || command.Length < index + 1)
                throw new ArgumentException("Invalid Syntax");

            ArmOpCode opcode;
            Enum.TryParse(command.Substring(0, index), true, out opcode);
            if (opcode == ArmOpCode.None)
                throw new ArgumentException("Unable to parse an invalid Opcode");

            var parameters = command.Substring(index).Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parameters.Length; i++)
            {
                parameters[i] = parameters[i].Replace(" ", string.Empty);
            }

            return new Command(opcode, parameters);
        }

        public static int ParseImmediate(string parameter)
        {
            return parameter.StartsWith("#0x") ? int.Parse(parameter.Substring(3), System.Globalization.NumberStyles.HexNumber) : int.Parse(parameter.Substring(1));
        }

        public static void ParseShiftInstruction(string parameter, ref ShiftInstruction shiftInst, ref byte shiftCount)
        {
            var shiftParameters = parameter.Split('#');
            if (shiftParameters.Length != 2)
                throw new ArgumentException("Invalid Shiftinstruction");

            Enum.TryParse(shiftParameters[0], true, out shiftInst);
            if (shiftInst == ShiftInstruction.None)
                throw new ArgumentException("Invalid Shiftinstruction");

            shiftCount = byte.Parse(shiftParameters[1]);
        }
    }
}
