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

            switch (cmdString.Substring(0, 3).ToLower())
            {
                case "str":
                    return new DataAccess(conditionFlags, EMemOpcode.Str, ParseParameters(parameterString, new[] { '[', ']' }));
                case "ldr":
                    return new DataAccess(conditionFlags, EMemOpcode.Ldr, ParseParameters(parameterString, new[] { '[', ']' }));
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

            switch (cmdString)
            {
                case "mov":
                    return new Move(conditionFlags, EOpcode.Mov, setConditionFlags, ParseParameters(parameterString, new[] { ',' }));
                case "mvn":
                    return new Move(conditionFlags, EOpcode.Mvn, setConditionFlags, ParseParameters(parameterString, new[] { ',' }));
                case "add":
                    return new Add(conditionFlags, EOpcode.Add, setConditionFlags, ParseParameters(parameterString, new[] { ',' }));
                case "sub":
                    return new Substract(conditionFlags, EOpcode.Sub, setConditionFlags, ParseParameters(parameterString, new[] { ',' }));
                case "rsb":
                    return new Substract(conditionFlags, EOpcode.Rsb, setConditionFlags, ParseParameters(parameterString, new[] { ',' }));
                case "and":
                    return new Logical(conditionFlags, EOpcode.And, setConditionFlags, ParseParameters(parameterString, new[] { ',' }));
                case "eor":
                    return new Logical(conditionFlags, EOpcode.Eor, setConditionFlags, ParseParameters(parameterString, new[] { ',' }));
                case "orr":
                    return new Logical(conditionFlags, EOpcode.Orr, setConditionFlags, ParseParameters(parameterString, new[] { ',' }));
                case "bic":
                    return new Logical(conditionFlags, EOpcode.Bic, setConditionFlags, ParseParameters(parameterString, new[] { ',' }));
                case "tst":
                    return new Test(conditionFlags, EOpcode.Tst, ParseParameters(parameterString, new[] { ',' }));
                case "teq":
                    return new Test(conditionFlags, EOpcode.Teq, ParseParameters(parameterString, new[] { ',' }));
                case "cmp":
                    return new Compare(conditionFlags, EOpcode.Cmp, ParseParameters(parameterString, new[] { ',' }));
                case "cmn":
                    return new Compare(conditionFlags, EOpcode.Cmn, ParseParameters(parameterString, new[] { ',' }));
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
                case "mi":
                    return ECondition.Minus;
                case "pl":
                    return ECondition.Plus;
                case "vs":
                    return ECondition.OverflowSet;
                case "vc":
                    return ECondition.OverflowClear;
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

        public static void ParseOperand2(string operand2, ref ERegister? srcReg, ref short immediate)
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

        public static void ParseShiftInstruction(string parameter, ref EShiftInstruction? shiftInst, ref byte shiftCount)
        {
            var shiftParameters = parameter.Split('#');
            if (shiftParameters.Length != 2)
                throw new ArgumentException("Invalid Shiftinstruction");

            EShiftInstruction test;
            shiftInst = Enum.TryParse(shiftParameters[0], true, out test) ? (EShiftInstruction?)test : null;
            if (shiftInst == null)
                throw new ArgumentException("Invalid Shiftinstruction");

            shiftCount = byte.Parse(shiftParameters[1]);
        }
    }
}
