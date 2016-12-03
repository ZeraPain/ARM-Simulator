using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Commands;

namespace ARM_Simulator.Model
{
    public class Parser
    {
        public ICommand ParseLine(string commandLine)
        {
            var command = ParseCommand(commandLine);

            switch (command.Opcode)
            {
                case null:
                     throw new ArgumentException("Invalid Opcode");
                case Opcode.Mov:
                case Opcode.Mvn:
                    var mov = new Move();
                    mov.Decode(command);
                    return mov;
                case Opcode.Add:
                    var add = new Add();
                    add.Decode(command);
                    var test = add.GetBitCommand();
                    var dc = new Decoder();
                    dc.Decode(test);
                    return add;
                case Opcode.Sub:
                case Opcode.Rsb:
                    var sub = new Substract();
                    sub.Decode(command);
                    return sub;
                case Opcode.And:
                case Opcode.Eor:
                case Opcode.Orr:
                case Opcode.Bic:
                    var log = new Logical();
                    log.Decode(command);
                    return log;
                case Opcode.Tst:
                case Opcode.Teq:
                    var tst = new Test();
                    tst.Decode(command);
                    return tst;
                case Opcode.Cmp:
                case Opcode.Cmn:
                    var cmp = new Compare();
                    cmp.Decode(command);
                    return cmp;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Command ParseCommand(string command)
        {
            var setConditionFlags = false;

            if (command == string.Empty)
                return new Command(null, false, null);

            var index = command.IndexOf(' ');
            if (index == -1)
                index = command.IndexOf('\t');

            if (index == -1 || command.Length < index + 1)
                throw new ArgumentException("Invalid Syntax");

            var opCodeString = command.Substring(0, index);

            Opcode opcode;
            var opCode = Enum.TryParse(opCodeString, true, out opcode) ? (Opcode?)opcode : null;

            if (opCode == null)
            {
                if (opCodeString.Length >= 3)
                {
                    switch (opCodeString.Substring(0, 3).ToLower())
                    {
                        case "mov":
                            opCode = Opcode.Mov;
                            setConditionFlags = ParseSetConditionFlags(opCodeString, 3);
                            break;
                        case "mvn":
                            opCode = Opcode.Mvn;
                            setConditionFlags = ParseSetConditionFlags(opCodeString, 3);
                            break;
                        case "add":
                            opCode = Opcode.Add;
                            setConditionFlags = ParseSetConditionFlags(opCodeString, 3);
                            break;
                        case "sub":
                            opCode = Opcode.Sub;
                            setConditionFlags = ParseSetConditionFlags(opCodeString, 3);
                            break;
                        case "rsb":
                            opCode = Opcode.Rsb;
                            setConditionFlags = ParseSetConditionFlags(opCodeString, 3);
                            break;
                        case "and":
                            opCode = Opcode.And;
                            setConditionFlags = ParseSetConditionFlags(opCodeString, 3);
                            break;
                        case "eor":
                            opCode = Opcode.Eor;
                            setConditionFlags = ParseSetConditionFlags(opCodeString, 3);
                            break;
                        case "orr":
                            opCode = Opcode.Orr;
                            setConditionFlags = ParseSetConditionFlags(opCodeString, 3);
                            break;
                        case "bic":
                            opCode = Opcode.Bic;
                            setConditionFlags = ParseSetConditionFlags(opCodeString, 3);
                            break;
                    }
                }
            }

            if (opCode == null)
                throw new ArgumentException("Unable to parse an invalid Opcode");

            var parameters = command.Substring(index).Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parameters.Length; i++)
            {
                parameters[i] = parameters[i].Replace(" ", string.Empty);
            }

            return new Command(opCode, setConditionFlags, parameters);
        }

        private static bool ParseSetConditionFlags(string opCodeString, int index)
        {
            if (opCodeString.Length <= index)
                return false;

            return opCodeString.Substring(index, 1).ToUpper() == "S";
        }

        public static Register? ParseRegister(string regString)
        {
            Register reg;
            var register = Enum.TryParse(regString, true, out reg) ? (Register?)reg : null;

            if (register == null)
                throw new ArgumentException("Invalid register");

            return register;
        }

        public static void ParseOperand2(string operand2, ref Register? srcReg, ref short immediate)
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

        public static void ParseShiftInstruction(string parameter, ref ShiftInstruction? shiftInst, ref byte shiftCount)
        {
            var shiftParameters = parameter.Split('#');
            if (shiftParameters.Length != 2)
                throw new ArgumentException("Invalid Shiftinstruction");

            ShiftInstruction test;
            shiftInst = Enum.TryParse(shiftParameters[0], true, out test) ? (ShiftInstruction?)test : null;
            if (shiftInst == null)
                throw new ArgumentException("Invalid Shiftinstruction");

            shiftCount = byte.Parse(shiftParameters[1]);
        }
    }
}
