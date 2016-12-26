using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ARM_Simulator.Annotations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Commands;
using ARM_Simulator.Resources;

namespace ARM_Simulator.Model
{
    public class Parser
    {
        private Dictionary<string, int> _textLabels; // text section offset, Labelname
        private List<string> _textList;

        private Dictionary<string, int> _dataLabels; // data section offset, Labelname
        private MemoryStream _memoryStream;
        private BinaryWriter _dataWriter;

        private enum EFileStruct
        {
            Undefined,
            Text,
            Data,
            Comm
        }

        public Parser([CanBeNull] string path = null)
        {
            ParseFile(path);
        }

        private void ParseFile([CanBeNull] string path)
        {
            if (path == null) return;

            var hFile = File.ReadAllLines(path);

            _textList = new List<string>();
            _textLabels = new Dictionary<string, int>();

            _memoryStream = new MemoryStream();
            _dataWriter = new BinaryWriter(_memoryStream);
            _dataLabels = new Dictionary<string, int>();

            var currentSection = EFileStruct.Undefined;

            foreach (var hLine in hFile)
            {
                var line = hLine;

                // Check for comments
                var commentIndex = line.IndexOf("@", StringComparison.Ordinal);
                if (commentIndex > -1) line = line.Substring(0, commentIndex);

                // Remove leading, ending spaces, tabs
                line = line.Trim(' ', '\t');
                if (string.IsNullOrEmpty(line))
                    continue;

                // Check for sections
                if (line.StartsWith(".", StringComparison.Ordinal))
                {
                    Enum.TryParse(line.Substring(1), true, out currentSection);
                    continue;
                }

                switch (currentSection)
                {
                    case EFileStruct.Undefined:
                        break;
                    case EFileStruct.Text:
                        ParseTextSection(line);
                        break;
                    case EFileStruct.Data:
                        ParseDataSection(line);
                        break;
                    case EFileStruct.Comm:
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
                _textLabels.Add(label, _textList.Count);

                if (line.Length > labelIndex + 1)
                    line = line.Substring(labelIndex + 1).Trim(' ', '\t');
                else
                    return;
            }

            // Add new Command
            _textList.Add(line);
        }

        private void ParseDataSection([NotNull] string line)
        {
            var lineSplit = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (lineSplit.Length != 3)
                throw new ArgumentException("Invalid Syntax (parameter count)");

            if (!lineSplit[0].EndsWith(":"))
                throw new ArgumentException("Invalid Syntax (label name)");

            _dataLabels.Add(lineSplit[0].Substring(0, lineSplit[0].Length -1), (int)_dataWriter.BaseStream.Length);

            switch (lineSplit[1])
            {
                case ".byte":
                    _dataWriter.Write(lineSplit[2].StartsWith("0x", StringComparison.Ordinal)
                        ? BitConverter.GetBytes(byte.Parse(lineSplit[2].Substring(2),
                            System.Globalization.NumberStyles.HexNumber))
                        : BitConverter.GetBytes(byte.Parse(lineSplit[2])));
                    break;
                case ".word":
                    _dataWriter.Write(lineSplit[2].StartsWith("0x", StringComparison.Ordinal)
                        ? BitConverter.GetBytes(int.Parse(lineSplit[2].Substring(2),
                            System.Globalization.NumberStyles.HexNumber))
                        : BitConverter.GetBytes(int.Parse(lineSplit[2])));
                    break;
            }
        }

        [NotNull]
        public byte[] EncodeTextSection() // Compile and link
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    for (var i = 0; i < _textList.Count; i++) // Linking of labels
                        binaryWriter.Write(ParseLine(_textList[i], i).Encode());

                    return memoryStream.ToArray();
                }
            }
        }

        [CanBeNull]
        public byte[] EncodeDataSection() => _memoryStream?.ToArray();

        [NotNull]
        public List<Command> GetCommandList()
        {
            if (!_textLabels.ContainsKey("main"))
                throw new Exception("Cannot find entry point");

            var commandList = new List<Command>();
            for (var i = 0; i < _textList.Count; i++)
            {
                var command = new Command
                {
                    Status = i == _textLabels["main"] ? EPipeline.Fetch : EPipeline.None,
                    Breakpoint = false,
                    Commandline = _textList[i]
                };

                var index = i;
                foreach (var label in _textLabels.Where(label => label.Value == index))
                    command.Label = label.Key;

                commandList.Add(command);
            }

            return commandList;
        }

        [NotNull]
        public ICommand ParseLine([NotNull] string commandLine, int lineNumber)
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

            if ((index == -1) || (commandLine.Length < index + 1) || commandLine.EndsWith(","))
                throw new ArgumentException("Invalid Syntax");

            var commandString = commandLine.Substring(0, index);
            var parameterString = commandLine.Substring(index).Trim(' ', '\t');

            try
            {
                var arithmetic = ParseArithmetic(commandString, parameterString);
                if (arithmetic != null)
                    return arithmetic;

                var dataaccess = ParseDataAccess(commandString, parameterString);
                if (dataaccess != null)
                    return dataaccess;

                var blocktransfer = ParseBlockTransfer(commandString, parameterString);
                if (blocktransfer != null)
                    return blocktransfer;

                var jump = ParseJump(commandString, parameterString, lineNumber);
                if (jump != null)
                    return jump;

                var multiply = ParseMultiply(commandString, parameterString);
                if (multiply != null)
                    return multiply;

                throw new ArgumentException("Unable to parse an invalid Command");
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message + ": " + commandLine);
            }
            catch (FormatException ex)
            {
                throw new FormatException(ex.Message + ": " + commandLine);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ": " + commandLine);
            }
        }

        public int GetEntryPoint()
        {
            if (!_textLabels.ContainsKey("main"))
                throw new Exception("Cannot find entry point");

            return _textLabels["main"] * 0x4;
        }

        [CanBeNull]
        private static ICommand ParseArithmetic([NotNull] string cmdString, string parameterString)
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

        [CanBeNull]
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

        [CanBeNull]
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
                default:
                    return null;
            }
        }

        [CanBeNull]
        private ICommand ParseJump(string cmdString, string parameterString, int lineNumber)
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

                if (!_textLabels.ContainsKey(parameterString))
                    throw new ArgumentException("Unknown Label");

                var conditions = ParseCondition(ref cmdString);
                var offset = _textLabels[parameterString] - lineNumber - 2; // -2 -> Pipeline

                return new Jump(conditions, link ? EJump.BranchLink : EJump.Branch, offset);
            }

            return null;
        }

        [CanBeNull]
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
                parameters[i] = parameters[i].Replace(" ", string.Empty);

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
            if (parameter.StartsWith("#0x", StringComparison.Ordinal))
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
