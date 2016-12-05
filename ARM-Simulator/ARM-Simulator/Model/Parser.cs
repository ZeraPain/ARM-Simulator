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

            throw new ArgumentException("Unable to parse an invalid Command");
        }

        private static ICommand ParseDataAccess(string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            var args = cmdString.Substring(3, cmdString.Length - 3).ToLower();
            cmdString = cmdString.Substring(0, 3).ToLower();

            var conditionFlags = ParseCondition(ref args);
            var parameters = ParseParameters(parameterString, new[] {'[', ']'});

            switch (cmdString)
            {
                case "str":
                    return new DataAccess(conditionFlags, EMemOpcode.Str, parameters);
                case "ldr":
                    return new DataAccess(conditionFlags, EMemOpcode.Ldr, parameters);
            }

            return null;
        }

        private static ICommand ParseArithmetic(string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            var args = cmdString.Substring(3, cmdString.Length - 3).ToLower();
            cmdString = cmdString.Substring(0, 3).ToLower();

            var setConditionFlags = ParseSetConditionFlags(ref args);
            var conditionFlags = ParseCondition(ref args);
            var parameters = ParseParameters(parameterString, new[] {','});

            switch (cmdString)
            {
                case "mov":
                    return new Move(conditionFlags, EOpcode.Mov, setConditionFlags, parameters);
                case "mvn":
                    return new Move(conditionFlags, EOpcode.Mvn, setConditionFlags, parameters);
                case "add":
                    return new Add(conditionFlags, EOpcode.Add, setConditionFlags, parameters);
                case "sub":
                    return new Substract(conditionFlags, EOpcode.Sub, setConditionFlags, parameters);
                case "rsb":
                    return new Substract(conditionFlags, EOpcode.Rsb, setConditionFlags, parameters);
                case "and":
                    return new Logical(conditionFlags, EOpcode.And, setConditionFlags, parameters);
                case "eor":
                    return new Logical(conditionFlags, EOpcode.Eor, setConditionFlags, parameters);
                case "orr":
                    return new Logical(conditionFlags, EOpcode.Orr, setConditionFlags, parameters);
                case "bic":
                    return new Logical(conditionFlags, EOpcode.Bic, setConditionFlags, parameters);
                case "tst":
                    return new Test(conditionFlags, EOpcode.Tst, parameters);
                case "teq":
                    return new Test(conditionFlags, EOpcode.Teq, parameters);
                case "cmp":
                    return new Compare(conditionFlags, EOpcode.Cmp, parameters);
                case "cmn":
                    return new Compare(conditionFlags, EOpcode.Cmn, parameters);
            }

            return null;
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

        private static bool ParseSetConditionFlags(ref string args)
        {
            if (string.IsNullOrEmpty(args))
                return false;

            if (!args.StartsWith("s"))
                return false;

            args = args.Substring(1, args.Length - 1);
            return true;
        }

        private static ECondition ParseCondition(ref string args)
        {
            if (string.IsNullOrEmpty(args))
                return ECondition.Always;

            if (args.Length <= 1)
                return ECondition.Always;

            switch (args.Substring(0, 2))
            {
                case "eq":
                    return ECondition.Equal;
                case "ne":
                    return ECondition.NotEqual;
                case "cs":
                case "hs":
                    return ECondition.CarrySet;
                case "cc":
                case "lo":
                    return ECondition.CarryClear;
                case "mi":
                    return ECondition.Minus;
                case "pl":
                    return ECondition.Plus;
                case "vs":
                    return ECondition.OverflowSet;
                case "vc":
                    return ECondition.OverflowClear;
                case "hi":
                    return ECondition.Higher;
                case "ls":
                    return ECondition.LowerOrSame;
                case "ge":
                    return ECondition.GreaterEqual;
                case "lt":
                    return ECondition.LessThan;
                case "gt":
                    return ECondition.GreaterThan;
                case "le":
                    return ECondition.LessEqual;
                case "al":
                    return ECondition.Always;
                default:
                    throw new ArgumentException("Unknown condition");
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

        public static void ParseOperand2(string operand2, string shiftValue, ref ERegister? srcReg, ref short immediate, ref EShiftInstruction? shiftInst, ref byte shiftCount)
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
