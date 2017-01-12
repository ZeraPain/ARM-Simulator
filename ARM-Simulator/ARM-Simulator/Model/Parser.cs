using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ARM_Simulator.Annotations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Commands;
using ARM_Simulator.Resources;

namespace ARM_Simulator.Model
{
    public class Parser
    {
        public Dictionary<string, int> CommandTable { get; protected set; } // Labelname, Offset
        public List<string> CommandList { get; protected set; }

        public Dictionary<string, int> DataTable { get; protected set; } // Labelname, Offset
        public List<byte[]> DataList { get; protected set; }

        public string EntryFunction { get; protected set; }
        public int Align { get; protected set; }

        private enum EFileSection
        {
            Undefined,
            Text,
            Data,
            Comm
        }

        public Parser()
        {
            Align = 2;
            EntryFunction = "main";

            CommandTable = new Dictionary<string, int>();
            CommandList = new List<string>();

            DataTable = new Dictionary<string, int>();
            DataList = new List<byte[]>();
        }

        public void ParseFile([CanBeNull] string[] hLines)
        {
            if (hLines == null) return;

            CommandTable.Clear();
            CommandList.Clear();
            DataTable.Clear();
            DataList.Clear();

            var currentSection = EFileSection.Undefined;

            foreach (var hLine in hLines)
            {
                var line = hLine;

                // Check for comments
                var commentIndex = line.IndexOf("@", StringComparison.Ordinal);
                if (commentIndex > -1) line = line.Substring(0, commentIndex);

                commentIndex = line.IndexOf("//", StringComparison.Ordinal);
                if (commentIndex > -1) line = line.Substring(0, commentIndex);

                // Remove leading, ending spaces, tabs
                line = line.Trim(' ', '\t');
                if (string.IsNullOrEmpty(line))
                    continue;

                // Check for sections
                if (line.StartsWith(".", StringComparison.Ordinal))
                {
                    if (line.StartsWith(".text", StringComparison.Ordinal))
                    {
                        currentSection = EFileSection.Text;
                        continue;
                    }

                    if (line.StartsWith(".data", StringComparison.Ordinal))
                    {
                        currentSection = EFileSection.Data;
                        continue;
                    }

                    if (line.StartsWith(".global", StringComparison.Ordinal) || line.StartsWith(".globl", StringComparison.Ordinal))
                    {
                        var split = line.Split(new []{ ' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                        if (split.Length != 2) throw new ArgumentException();

                        EntryFunction = split[1];
                        continue;
                    }

                    if (line.StartsWith(".align", StringComparison.Ordinal))
                    {
                        var split = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (split.Length != 2) throw new ArgumentException();

                        Align = int.Parse(split[1]);
                        continue;
                    }

                    if (line.StartsWith(".type", StringComparison.Ordinal))
                    {
                        // TODO: Implement missing member
                        continue;
                    }

                    if (line.StartsWith(".Lfe", StringComparison.Ordinal))
                    {
                        // TODO: Implement missing member
                        continue;
                    }

                    if (line.StartsWith(".size", StringComparison.Ordinal))
                    {
                        // TODO: Implement missing member
                        continue;
                    }

                    if (line.Equals(".end", StringComparison.Ordinal))
                    {
                        // End of file
                        return;
                    }
                }

                switch (currentSection)
                {
                    case EFileSection.Undefined:
                        break;
                    case EFileSection.Text:
                        ParseTextSection(line);
                        break;
                    case EFileSection.Data:
                        ParseDataSection(line);
                        break;
                    case EFileSection.Comm:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ParseTextSection(string line)
        {
            // Check for label
            var labelIndex = line.IndexOf(":", StringComparison.Ordinal);
            if (labelIndex > -1)
            {
                var label = line.Substring(0, labelIndex);
                CommandTable.Add(label, CommandList.Count);

                if (line.Length > labelIndex + 1)
                    line = line.Substring(labelIndex + 1).Trim(' ', '\t');
                else
                    return;
            }

            // Add new Command
            CommandList.Add(line);
        }

        private void ParseDataSection([NotNull] string line)
        {
            // Check for label
            var labelIndex = line.IndexOf(":", StringComparison.Ordinal);
            if (labelIndex > -1)
            {
                var label = line.Substring(0, labelIndex);
                var offset = DataList.SelectMany(a => a).ToArray().Length;
                DataTable.Add(label, offset);

                if (line.Length > labelIndex + 1)
                    line = line.Substring(labelIndex + 1).Trim(' ', '\t');
                else
                    return;
            }

            var lineSplit = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (lineSplit.Length != 2)
                throw new ArgumentException("Invalid Syntax (parameter count): " + line);

            var data = ParseDataDefinition(lineSplit[0], lineSplit[1]);
            if (data != null)
                DataList.Add(data.Encode());
        }

        [CanBeNull]
        private static ICommand ParseDataDefinition([NotNull] string commandLine, [CanBeNull] string parameterString)
        {
            if (!commandLine.StartsWith(".", StringComparison.Ordinal) || (parameterString == null))
                return null;

            EDataSize dataSize;
            if (!Enum.TryParse(commandLine.Substring(1), true, out dataSize))
                return null;

            return new Datadefinition(dataSize, parameterString);
        }

        [NotNull]
        public static ICommand ParseLine([NotNull] string commandLine)
        {
            var indexS = commandLine.IndexOf(' ');
            var indexT = commandLine.IndexOf('\t');

            var index = -1;
            if ((indexS > -1) && (indexT > -1))
                index = Math.Min(indexS, indexT);
            else if ((indexS > -1) && (indexT == -1))
                index = indexS;
            else if ((indexT > -1) && (indexS == -1))
                index = indexT;

            try
            {
                if ((index == -1) || (commandLine.Length < index + 1) || commandLine.EndsWith(",")) throw new TargetParameterCountException();

                var commandString = commandLine.Substring(0, index);
                var parameterString = commandLine.Substring(index).Trim(' ', '\t');

                var datadefinition = ParseDataDefinition(commandString, parameterString);
                if (datadefinition != null)
                    return datadefinition;

                var arithmetic = ParseArithmetic(commandString, parameterString);
                if (arithmetic != null)
                    return arithmetic;

                var dataaccess = ParseDataAccess(commandString, parameterString);
                if (dataaccess != null)
                    return dataaccess;

                var blocktransfer = ParseBlockTransfer(commandString, parameterString);
                if (blocktransfer != null)
                    return blocktransfer;

                var jump = ParseJump(commandString, parameterString);
                if (jump != null)
                    return jump;

                var multiply = ParseMultiply(commandString, parameterString);
                if (multiply != null)
                    return multiply;

                throw new ArgumentException();
            }
            catch (TargetParameterCountException)
            {
                throw new TargetParameterCountException("Invalid Parameter Count: " + commandLine);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("Parameter Size is out of Range: " + commandLine);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("Invalid / Unknown Syntax: " + commandLine);
            }
            catch (FormatException)
            {
                throw new FormatException("Invalid Format: " + commandLine);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ": " + commandLine);
            }
        }

        [CanBeNull]
        private static ICommand ParseArithmetic([NotNull] string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            var args = cmdString.Substring(3, cmdString.Length - 3).ToLower();
            var condition = ParseCondition(ref args);
            bool setConditionFlags;

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

            return new Arithmetic(condition, opCode, setConditionFlags, parameterString);
        }

        [CanBeNull]
        private static ICommand ParseDataAccess(string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            var args = cmdString.Substring(3, cmdString.Length - 3).ToLower();
            cmdString = cmdString.Substring(0, 3).ToLower();

            var conditionFlags = ParseCondition(ref args);
            var size = ParseSize(args);

            switch (cmdString)
            {
                case "str":
                    return new DataAccess(conditionFlags, false, size, parameterString);
                case "ldr":
                    return new DataAccess(conditionFlags, true, size, parameterString);
                default:
                    return null;
            }
        }

        [CanBeNull]
        private static ICommand ParseBlockTransfer(string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            switch (cmdString.ToLower())
            {
                case "push":
                    cmdString = "stm";
                    parameterString = "sp!, " + parameterString;
                    break;
                case "pop":
                    cmdString = "ldm";
                    parameterString = "sp!, " + parameterString;
                    break;
            }

            var args = cmdString.Substring(3, cmdString.Length - 3).ToLower();
            cmdString = cmdString.Substring(0, 3).ToLower();
            var condition = ParseCondition(ref args);

            switch (cmdString)
            {
                case "stm":
                    return new Blocktransfer(condition, false, parameterString);
                case "ldm":
                    return new Blocktransfer(condition, true, parameterString);
                default:
                    return null;
            }
        }

        [CanBeNull]
        private static ICommand ParseJump(string cmdString, string parameterString)
        {
            cmdString = cmdString.ToLower();

            if (cmdString.StartsWith("bx", StringComparison.Ordinal))
            {
                cmdString = cmdString.Substring(2);
                var conditions = ParseCondition(ref cmdString);
                var rm = ParseRegister(parameterString);
                return new Jump(conditions, EJump.BranchExchange, rm);
            }

            if (cmdString.StartsWith("b", StringComparison.Ordinal))
            {
                var link = false;
                if (cmdString.StartsWith("bl", StringComparison.Ordinal))
                {
                    link = true;
                    cmdString = cmdString.Substring(2);
                }
                else
                {
                    cmdString = cmdString.Substring(1);
                }

                var conditions = ParseCondition(ref cmdString);

                return new Jump(conditions, link ? EJump.BranchLink : EJump.Branch, parameterString);
            }

            return null;
        }

        [CanBeNull]
        private static ICommand ParseMultiply([NotNull] string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            var multiplication = EMultiplication.None;
            if (cmdString.Length >= 3)
            {
                if (!Enum.TryParse(cmdString.Substring(0, 3).ToLower(), true, out multiplication))
                {
                    if (cmdString.Length < 5 || !Enum.TryParse(cmdString.Substring(0, 5).ToLower(), true, out multiplication))
                        return null;
                }
            }

            string args;
            switch (multiplication)
            {
                case EMultiplication.Mul:
                case EMultiplication.Mla:
                    args = cmdString.Substring(3, cmdString.Length - 3).ToLower();
                    break;
                case EMultiplication.Smlal:
                case EMultiplication.Smull:
                case EMultiplication.Umlal:
                case EMultiplication.UMull:
                    args = cmdString.Substring(5, cmdString.Length - 5).ToLower();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var conditionFlags = ParseCondition(ref args);
            var setConditionFlags = ParseSetConditionFlags(args);
            
            return new Multiply(conditionFlags, multiplication, setConditionFlags, parameterString);
        }

        [NotNull]
        public static string[] ParseParameters([NotNull] string parameterString, char[] seperator)
        {
            var parameters = parameterString.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parameters.Length; i++)
                parameters[i] = parameters[i].Replace(" ", string.Empty);

            return parameters;
        }

        private static bool ParseSetConditionFlags([CanBeNull] string args)
        {
            if (string.IsNullOrEmpty(args))
                return false;

            return args == "s";
        }

        private static EDataSize ParseSize([CanBeNull] string args)
        {
            if (string.IsNullOrEmpty(args))
                return EDataSize.Word;

            switch (args)
            {
                case "b":
                    return EDataSize.Byte;
                default:
                    return EDataSize.Word;
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

        public static ERegister ParseRegister(string regString)
        {
            ERegister reg;
            if (!Enum.TryParse(regString, true, out reg))
                throw new ArgumentException("Invalid register");

            return reg;
        }

        public static T ParseImmediate<T>(string parameter)
        {
            if (parameter.StartsWith("#", StringComparison.Ordinal))
                parameter = parameter.Substring(1);

            if (parameter.StartsWith("0x", StringComparison.Ordinal))
            {
                var value = long.Parse(parameter.Substring(2), System.Globalization.NumberStyles.HexNumber);
                return (T)Convert.ChangeType(value, typeof(T));
            }

            return (T)Convert.ChangeType(long.Parse(parameter), typeof(T));
        }


        public static bool ParseShiftInstruction(string parameter, ref EShiftInstruction shiftInst, ref byte shiftCount, ref ERegister rs)
        {
            if (parameter.Length < 4)
                throw new ArgumentException("Invalid Shiftinstruction");

            if (!Enum.TryParse(parameter.Substring(0, 3), true, out shiftInst))
                throw new ArgumentException("Invalid Shiftinstruction");

            parameter = parameter.Substring(3, parameter.Length - 3);

            if (Enum.TryParse(parameter, true, out rs))
                return true;

            shiftCount = ParseImmediate<byte>(parameter);
            if (shiftCount > 64) throw new ArgumentOutOfRangeException();

            return false;
        }

        public static void ParseShiftInstruction(string parameter, ref EShiftInstruction shiftInst, ref byte shiftCount)
        {
            if (parameter.Length < 4)
                throw new ArgumentException("Invalid Shiftinstruction");

            if (!Enum.TryParse(parameter.Substring(0, 3), true, out shiftInst))
                throw new ArgumentException("Invalid Shiftinstruction");

            parameter = parameter.Substring(3, parameter.Length - 3);
            shiftCount = ParseImmediate<byte>(parameter);
            if (shiftCount > 64) throw new ArgumentOutOfRangeException();
        }
    }
}
