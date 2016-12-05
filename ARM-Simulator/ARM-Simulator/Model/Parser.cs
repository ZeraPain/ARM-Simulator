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

            switch (cmdString.Substring(0, 3).ToLower())
            {
                case "str":
                    return new DataAccess(MemOpcode.Str, ParseParameters(parameterString, new[] { '[', ']' }));
                case "ldr":
                    return new DataAccess(MemOpcode.Ldr, ParseParameters(parameterString, new[] { '[', ']' }));
            }

            return null;
        }

        private static ICommand ParseArithmetic(string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            if (cmdString.Length == 3)
            {
                switch (cmdString)
                {
                    case "tst":
                        return new Test(Opcode.Tst, ParseParameters(parameterString, new[] { ',' }));
                    case "teq":
                        return new Test(Opcode.Teq, ParseParameters(parameterString, new[] { ',' }));
                    case "cmp":
                        return new Compare(Opcode.Cmp, ParseParameters(parameterString, new[] { ',' }));
                    case "cmn":
                        return new Compare(Opcode.Cmn, ParseParameters(parameterString, new[] { ',' }));
                }
            }

            switch (cmdString.Substring(0, 3).ToLower())
            {
                case "mov":
                    return new Move(Opcode.Mov, ParseSetConditionFlags(cmdString, 3), ParseParameters(parameterString, new[] { ',' }));
                case "mvn":
                    return new Move(Opcode.Mvn, ParseSetConditionFlags(cmdString, 3), ParseParameters(parameterString, new[] { ',' }));
                case "add":
                    return new Add(Opcode.Add, ParseSetConditionFlags(cmdString, 3), ParseParameters(parameterString, new[] { ',' }));
                case "sub":
                    return new Substract(Opcode.Sub, ParseSetConditionFlags(cmdString, 3), ParseParameters(parameterString, new[] { ',' }));
                case "rsb":
                    return new Substract(Opcode.Rsb, ParseSetConditionFlags(cmdString, 3), ParseParameters(parameterString, new[] { ',' }));
                case "and":
                    return new Logical(Opcode.And, ParseSetConditionFlags(cmdString, 3), ParseParameters(parameterString, new[] { ',' }));
                case "eor":
                    return new Logical(Opcode.Eor, ParseSetConditionFlags(cmdString, 3), ParseParameters(parameterString, new[] { ',' }));
                case "orr":
                    return new Logical(Opcode.Orr, ParseSetConditionFlags(cmdString, 3), ParseParameters(parameterString, new[] { ',' }));
                case "bic":
                    return new Logical(Opcode.Bic, ParseSetConditionFlags(cmdString, 3), ParseParameters(parameterString, new[] { ',' }));
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
