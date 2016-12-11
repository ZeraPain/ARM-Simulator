using System;
using System.Collections.Generic;
using System.IO;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Commands;
using ARM_Simulator.Resources;

namespace ARM_Simulator.Model
{
    public class Parser
    {
        private readonly Dictionary<string, int> _labels; // Offset, Labelname
        private int _offset;

        public Parser()
        {
            _labels = new Dictionary<string, int>();
        }

        public List<int> ParseFile(string file)
        {
            var commandList = new List<string>();
            var hFile = File.ReadAllLines(file);
            
            foreach (var hLine in hFile)
            {
                var line = hLine.Trim(' ', '\t');

                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("@") || line.StartsWith("//"))
                    continue;

                if (line.EndsWith(":"))  // label
                {
                    var label = line.Substring(0, line.Length - 1);
                    _labels.Add(label, commandList.Count);
                    continue;
                }

                commandList.Add(line);
            }

            var source = new List<int>();
            for (_offset = 0; _offset < commandList.Count; _offset++)
                source.Add(ParseLine(commandList[_offset]).Encode());
            
            return source;
        }

        public ICommand ParseLine(string commandLine)
        {
            var index = commandLine.IndexOf(' ');
            if (index == -1)
                index = commandLine.IndexOf('\t');

            if (index == -1 || commandLine.Length < index + 1)
                throw new ArgumentException("Invalid Syntax");

            var commandString = commandLine.Substring(0, index);
            var parameterString = commandLine.Substring(index).Trim(' ', '\t');

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

            throw new ArgumentException("Unable to parse an invalid Command");
        }

        public int GetEntryPoint()
        {
            if (!_labels.ContainsKey("main"))
                throw new Exception("Cannot find entry point");

            return _labels["main"] * 0x4;
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

        private static ICommand ParseDataAccess(string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            var args = cmdString.Substring(3, cmdString.Length - 3).ToLower();
            cmdString = cmdString.Substring(0, 3).ToLower();

            var conditionFlags = ParseCondition(ref args);
            var size = ParseSize(args);
            var parameters = ParseParameters(parameterString, new[] { '[', ']' });

            switch (cmdString)
            {
                case "str":
                    return new DataAccess(conditionFlags, false, size, parameters);
                case "ldr":
                    return new DataAccess(conditionFlags, true, size, parameters);
            }

            return null;
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
                    return new Blocktransfer(condition, false, parameters);
                case "ldm":
                    return new Blocktransfer(condition, true, parameters);
            }

            return null;
        }

        private ICommand ParseJump(string cmdString, string parameterString)
        {
            cmdString = cmdString.ToLower();

            if (cmdString.StartsWith("b"))
            {
                if (!_labels.ContainsKey(parameterString))
                    throw new ArgumentException("Unknown Label");

                var offset = _labels[parameterString] - _offset - 2; // -2 -> Pipeline

                if (cmdString.Length < 2)
                    return new Jump(ECondition.Always, EJump.Branch, offset);

                cmdString = cmdString.Substring(1, cmdString.Length - 1);
                EJump jump;

                switch (cmdString)
                {
                    case "l":
                        cmdString = cmdString.Substring(1, cmdString.Length - 1);
                        jump = EJump.BranchLink;
                        break;
                    case "x":
                        cmdString = cmdString.Substring(1, cmdString.Length - 1);
                        jump = EJump.BranchExchange;
                        break;
                    default:
                        jump = EJump.Branch;
                        break;
                }

                var conditions = ParseCondition(ref cmdString);
                return new Jump(conditions, jump, offset);
            }

            return null;
        }

        private static ICommand ParseMultiply(string cmdString, string parameterString)
        {
            if (cmdString.Length <= 2)
                return null;

            var multiplication = EMultiplication.None;
            if (cmdString.Length >= 3)
            {
                if (!Enum.TryParse(cmdString.Substring(0, 3).ToLower(), true, out multiplication) && cmdString.Length >= 5)
                {
                    if (!Enum.TryParse(cmdString.Substring(0, 5).ToLower(), true, out multiplication))
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
            var parameters = ParseParameters(parameterString, new[] { ',' });
            return new Multiply(conditionFlags, multiplication, setConditionFlags, parameters);
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

            return args == "s";
        }

        private static ESize ParseSize(string args)
        {
            if (string.IsNullOrEmpty(args))
                return ESize.Word;

            switch (args)
            {
                case "b":
                    return ESize.Byte;
                default:
                    return ESize.Word;
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
            if (parameter.StartsWith("#0x"))
            {
                var value = long.Parse(parameter.Substring(3), System.Globalization.NumberStyles.HexNumber);
                return (T)Convert.ChangeType(value, typeof(T));
            }
                
            if (parameter.StartsWith("#"))
                parameter = parameter.Substring(1);

            return (T)Convert.ChangeType(long.Parse(parameter), typeof(T));
        }

        public static bool ParseShiftInstruction(string parameter, ref EShiftInstruction shiftInst, ref byte shiftCount, ref ERegister rs)
        {
            if (parameter.Length < 4)
                throw new ArgumentException("Invalid Shiftinstruction");

            if (!Enum.TryParse(parameter.Substring(0, 3), true, out shiftInst))
                throw new ArgumentException("Invalid Shiftinstruction");

            parameter = parameter.Substring(3, parameter.Length - 3);
            if (!Enum.TryParse(parameter, true, out rs))
            {
                shiftCount = ParseImmediate<byte>(parameter);
                if (shiftCount > 64) throw new ArgumentOutOfRangeException();
                return false;
            }

            return true;
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
