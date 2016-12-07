using System;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Commands;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model
{
    public class Parser
    {
        public ICommand ParseLine(string commandLine)
        {
            var index = commandLine.IndexOf(' ');
            if (index == -1)
                index = commandLine.IndexOf('\t');

            if (index == -1 || commandLine.Length < index + 1)
                throw new ArgumentException("Invalid Syntax");

            var arithmetic = ParseArithmetic(commandLine.Substring(0, index), commandLine.Substring(index));
            if (arithmetic != null)
                return arithmetic;

            var dataaccess = ParseDataAccess(commandLine.Substring(0, index), commandLine.Substring(index));
            if (dataaccess != null)
                return dataaccess;

            var blocktransfer = ParseBlockTransfer(commandLine.Substring(0, index), commandLine.Substring(index));
            if (blocktransfer != null)
                return blocktransfer;

            throw new ArgumentException("Unable to parse an invalid Command");
        }

        private static ICommand ParseBlockTransfer(string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            var args = cmdString.Substring(3, cmdString.Length - 3).ToLower();
            cmdString = cmdString.Substring(0, 3).ToLower();

            var condition = ParseCondition(ref args);
            var parameters = ParseParameters(parameterString, new[] { '{', '}' });

            switch (cmdString)
            {
                case "stm":
                    return new Blocktransfer(condition, ERequestType.Store, parameters);
                case "ldm":
                    return new Blocktransfer(condition, ERequestType.Load, parameters);
            }

            return null;
        }

        private static ICommand ParseDataAccess(string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            var args = cmdString.Substring(3, cmdString.Length - 3).ToLower();
            cmdString = cmdString.Substring(0, 3).ToLower();

            var conditionFlags = ParseCondition(ref args);
            var size = ParseSize(args);
            var parameters = ParseParameters(parameterString, new[] {'[', ']'});

            switch (cmdString)
            {
                case "str":
                    return new DataAccess(conditionFlags, ERequestType.Store, size, parameters);
                case "ldr":
                    return new DataAccess(conditionFlags, ERequestType.Load, size, parameters);
            }

            return null;
        }

        private static ICommand ParseArithmetic(string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            var args = cmdString.Substring(3, cmdString.Length - 3).ToLower();
            var condition = ParseCondition(ref args);
            bool setConditionFlags;
            var parameters = ParseParameters(parameterString, new[] { ',' });

            EOpcode opCode;
            if (!Enum.TryParse(cmdString.Substring(0, 3).ToLower(), true, out opCode))
                return null;

            switch (opCode)
            {
                case EOpcode.And:
                case EOpcode.Eor:
                case EOpcode.Sub:
                case EOpcode.Rsb:
                case EOpcode.Add:
                case EOpcode.Orr:
                case EOpcode.Mov:
                case EOpcode.Bic:
                case EOpcode.Mvn:
                    setConditionFlags = ParseSetConditionFlags(args);
                    break;
                case EOpcode.Tst:
                case EOpcode.Teq:
                case EOpcode.Cmp:
                case EOpcode.Cmn:
                    setConditionFlags = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Arithmetic(condition, opCode, setConditionFlags, parameters);
        }

        private static string[] ParseParameters(string parameterString, char[] seperator)
        {
            var parameters = parameterString.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parameters.Length; i++)
            {
                parameters[i] = parameters[i].Replace(" ", string.Empty);
            }

            return parameters;
        }

        private static bool ParseSetConditionFlags(string args)
        {
            if (string.IsNullOrEmpty(args))
                return false;

            if (args.Length < 1)
                return false;

            return args == "s";
        }

        private static ESize ParseSize(string args)
        {
            if (string.IsNullOrEmpty(args))
                return ESize.Word;

            if (args.Length < 1)
                return ESize.Word;

            switch (args)
            {
                case "b":
                case "sb":
                    return ESize.Byte;
                default:
                    throw new ArgumentException("Unknown Size");
            }
        }

        private static ECondition ParseCondition(ref string args)
        {
            if (string.IsNullOrEmpty(args))
                return ECondition.Always;

            if (args.Length < 2)
                return ECondition.Always;

            switch (args.Substring(0, 2))
            {
                case "eq":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.Equal;
                case "ne":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.NotEqual;
                case "cs":
                case "hs":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.CarrySet;
                case "cc":
                case "lo":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.CarryClear;
                case "mi":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.Minus;
                case "pl":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.Plus;
                case "vs":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.OverflowSet;
                case "vc":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.OverflowClear;
                case "hi":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.Higher;
                case "ls":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.LowerOrSame;
                case "ge":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.GreaterEqual;
                case "lt":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.LessThan;
                case "gt":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.GreaterThan;
                case "le":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.LessEqual;
                case "al":
                    args = args.Substring(0, args.Length - 2);
                    return ECondition.Always;
                default:
                    return ECondition.Always;
            }
        }

        public static ERegister? ParseRegister(string regString)
        {
            ERegister reg;
            var register = Enum.TryParse(regString, true, out reg) ? (ERegister?)reg : null;

            if (register == null)
                throw new ArgumentException("Invalid register");

            return register;
        }

        public static void ParseOperand2(string operand2, string shiftValue, ref ERegister? srcReg, ref int immediate, ref EShiftInstruction? shiftInst, ref byte shiftCount)
        {
            if (operand2.StartsWith("#"))
            {
                immediate = ParseImmediate<byte>(operand2);
                if (shiftValue != null)
                {
                    shiftCount = ParseImmediate<byte>(shiftValue);
                    if (shiftCount > 16) throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                srcReg = ParseRegister(operand2);
                if (shiftValue != null)
                {
                    ParseShiftInstruction(shiftValue, ref shiftInst, ref shiftCount);
                } 
            }
        }

        public static T ParseImmediate<T>(string parameter)
        {
            if (parameter.StartsWith("#0x"))
            {
                var value = long.Parse(parameter.Substring(3), System.Globalization.NumberStyles.HexNumber);
                return (T)Convert.ChangeType(value, typeof(T));
            }
                
            if (parameter.StartsWith("#"))
                parameter = parameter.Substring(1);

            return (T)Convert.ChangeType(long.Parse(parameter), typeof(T));
        }

        public static void ParseShiftInstruction(string parameter, ref EShiftInstruction? shiftInst, ref byte shiftCount)
        {
            var shiftParameters = parameter.Split('#');
            if (shiftParameters.Length != 2)
                throw new ArgumentException("Invalid Shiftinstruction");

            EShiftInstruction test;
            shiftInst = Enum.TryParse(shiftParameters[0], true, out test) ? (EShiftInstruction?)test : null;
            if (shiftInst == null)
                throw new ArgumentException("Invalid Shiftinstruction");

            shiftCount = ParseImmediate<byte>(shiftParameters[1]);
            if (shiftCount > 64) throw new ArgumentOutOfRangeException();
        }
    }
}
