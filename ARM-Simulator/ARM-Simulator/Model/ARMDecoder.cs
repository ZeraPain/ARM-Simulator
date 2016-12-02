using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Commands;

namespace ARM_Simulator.Model
{
    public class ArmDecoder
    {
        public ICommand Decode(string commandLine)
        {
            var command = ParseCommand(commandLine);

            switch (command.Opcode)
            {
                case ArmOpCode.Nop:
                     throw new ArgumentException("Invalid Opcode");
                case ArmOpCode.Mov:
                case ArmOpCode.Movs:
                case ArmOpCode.Mvn:
                case ArmOpCode.Mvns:
                    var mov = new Move(command);
                    mov.Decode();
                    return mov;
                case ArmOpCode.Add:
                case ArmOpCode.Adds:
                    var add = new Add(command);
                    add.Decode();
                    return add;
                case ArmOpCode.Sub:
                case ArmOpCode.Subs:
                case ArmOpCode.Rsb:
                case ArmOpCode.Rsbs:
                    var sub = new Substract(command);
                    sub.Decode();
                    return sub;
                case ArmOpCode.And:
                case ArmOpCode.Ands:
                case ArmOpCode.Eor:
                case ArmOpCode.Eors:
                case ArmOpCode.Orr:
                case ArmOpCode.Orrs:
                case ArmOpCode.Orn:
                case ArmOpCode.Orns:
                case ArmOpCode.Bic:
                case ArmOpCode.Bics:
                    var log = new Logical(command);
                    log.Decode();
                    return log;
                case ArmOpCode.Lsl:
                case ArmOpCode.Lsls:
                case ArmOpCode.Lsr:
                case ArmOpCode.Lsrs:
                case ArmOpCode.Ror:
                case ArmOpCode.Rors:
                case ArmOpCode.Asr:
                case ArmOpCode.Asrs:
                    var shift = new Shift(command);
                    shift.Decode();
                    return shift;
                case ArmOpCode.Tst:
                case ArmOpCode.Teq:
                    var tst = new Test(command);
                    tst.Decode();
                    return tst;
                case ArmOpCode.Cmp:
                case ArmOpCode.Cmn:
                    var cmp = new Compare(command);
                    cmp.Decode();
                    return cmp;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Command ParseCommand(string command)
        {
            if (command == string.Empty)
                return new Command(ArmOpCode.Nop, null);

            var index = command.IndexOf(' ');
            if (index == -1)
                index = command.IndexOf('\t');

            if (index == -1 || command.Length < index + 1)
                throw new ArgumentException("Invalid Syntax");

            ArmOpCode opcode;
            Enum.TryParse(command.Substring(0, index), true, out opcode);
            if (opcode == ArmOpCode.Nop)
                throw new ArgumentException("Unable to parse an invalid Opcode");

            var parameters = command.Substring(index).Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parameters.Length; i++)
            {
                parameters[i] = parameters[i].Replace(" ", string.Empty);
            }

            return new Command(opcode, parameters);
        }

        public static ArmRegister ParseRegister(string regString)
        {
            ArmRegister register;
            Enum.TryParse(regString, true, out register);

            if (register == ArmRegister.None)
                throw new ArgumentException("Invalid register");

            return register;
        }

        public static void ParseOperand2(string operand2, ref ArmRegister srcReg, ref short immediate)
        {
            if (operand2.StartsWith("#"))
            {
                immediate = ParseImmediate(operand2, 8);
                return;
            }

            srcReg = ParseRegister(operand2);
        }

        public static short ParseImmediate(string parameter, byte bits)
        {
            var value = parameter.StartsWith("#0x") ? short.Parse(parameter.Substring(3), System.Globalization.NumberStyles.HexNumber) : short.Parse(parameter.Substring(1));
            if (value > (Math.Pow(2, bits) -1) || value < 0)
                throw new ArgumentOutOfRangeException();

            return value;
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
