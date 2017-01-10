using System;
using System.Collections.Generic;
using System.Reflection;
using ARM_Simulator.Annotations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class DataAccess : ICommand
    {
        protected ECondition Condition;
        protected bool Load;
        protected bool PreIndex;
        protected bool Up;
        protected bool WriteBack;
        protected readonly EDataSize DataSize;
        protected ERegister Rn;
        protected ERegister Rd;

        // Immediate
        protected short Immediate;

        // shift rm
        protected byte ShiftCount;
        protected ERegister Rm;
        protected EShiftInstruction ShiftInst;

        protected EOffset Offset;
        protected bool Decoded;

        protected bool Linked;
        protected string Label;

        public DataAccess(ECondition condition, bool load, EDataSize dataSize, [NotNull] string parameterString)
        {
            Condition = condition;
            Load = load;
            DataSize = dataSize;
            Decoded = false;
            Linked = false;
            PreIndex = true;
            Up = true;
            Parse(parameterString);
        }

        public DataAccess(ECondition condition, bool load, bool preIndex, bool unsigned, bool writeBack, EDataSize dataSize, ERegister rn, ERegister rd, short immediate)
        {
            Offset = EOffset.Immediate;
            Condition = condition;
            Load = load;
            PreIndex = preIndex;
            Up = unsigned;
            WriteBack = writeBack;
            DataSize = dataSize;
            Rn = rn;
            Rd = rd;
            Immediate = immediate;
            Decoded = true;
            Linked = true;
        }

        public DataAccess(ECondition condition, bool load, bool preIndex, bool unsigned, bool writeBack, EDataSize dataSize, ERegister rn, ERegister rd, byte shiftCount, EShiftInstruction shiftInst, ERegister rm)
        {
            Offset = EOffset.ImmediateShiftRm;
            Condition = condition;
            Load = load;
            PreIndex = preIndex;
            Up = unsigned;
            WriteBack = writeBack;
            DataSize = dataSize;
            Rn = rn;
            Rd = rd;
            ShiftCount = shiftCount;
            ShiftInst = shiftInst;
            Rm = rm;
            Decoded = true;
            Linked = true;
        }

        private void ParseSource([NotNull] string sourceString)
        {
            var source = sourceString.Split(',');
            if (source.Length > 3) throw new TargetParameterCountException();

            Rn = Parser.ParseRegister(source[0]);
            if (source.Length == 1) return;

            if (source[1].StartsWith("#", StringComparison.Ordinal))
            {
                Immediate = Parser.ParseImmediate<short>(source[1]);
                if (Immediate < 0)
                {
                    Up = false;
                    Immediate *= -1;
                }

                if (Immediate > 4096) throw new ArgumentOutOfRangeException();

                Offset = EOffset.Immediate;
            }
            else
            {
                Rm = Parser.ParseRegister(source[1]);
                if (source.Length > 2) Parser.ParseShiftInstruction(source[2], ref ShiftInst, ref ShiftCount);

                Offset = EOffset.ImmediateShiftRm;
            }
        }

        private void ParseLabelAccess([NotNull] string parameterString)
        {
            var parameters = Parser.ParseParameters(parameterString, new[] { ',' });
            if (parameters.Length != 2) throw new TargetParameterCountException();

            Rd = Parser.ParseRegister(parameters[0]);
            Rn = ERegister.Pc;
            Label = parameters[1];
            Decoded = true;
            Linked = false;
        }

        public void Parse([NotNull] string parameterString)
        {
            // Check if a label might be accessed
            if ((parameterString.IndexOf("[", StringComparison.Ordinal) == -1) && (parameterString.IndexOf("]", StringComparison.Ordinal) == -1))
            {
                ParseLabelAccess(parameterString);
                return;
            }

            var parameters = Parser.ParseParameters(parameterString, new[] {'[', ']'});
            if ((parameters.Length != 2) && (parameters.Length != 3)) throw new TargetParameterCountException();
            if (!parameters[0].EndsWith(",")) throw new ArgumentException();

            Rd = Parser.ParseRegister(parameters[0].Substring(0, parameters[0].Length - 1));

            // Parse second parameter (source)
            ParseSource(parameters[1]);

            // Parse third parameter (writeback, postindex)
            if (parameters.Length == 3)
            {
                if (parameters[2].Equals("!")) // Writeback
                {
                    WriteBack = true;
                }
                else if (parameters[2].StartsWith(",", StringComparison.Ordinal)) // PostIndex
                {
                    parameters[2] = parameters[2].Substring(1);

                    if (parameters[2].StartsWith("#", StringComparison.Ordinal))
                    {
                        Immediate = Parser.ParseImmediate<short>(parameters[2]);
                        if (Immediate < 0)
                        {
                            Up = false;
                            Immediate *= -1;
                        }

                        if (Immediate >= 4096) throw new ArgumentOutOfRangeException();
                    }
                    else
                    {
                        Rm = Parser.ParseRegister(parameters[2]);
                    }

                    WriteBack = true;
                    PreIndex = false;
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            Decoded = true;
            Linked = true;
        }

        public byte[] Encode()
        {
            if (!Decoded || !Linked) throw new InvalidOperationException();

            var bw = new BitWriter();

            bw.WriteBits((int)Condition, 28, 4); // Condition
            bw.WriteBits(1, 26, 1);
            bw.WriteBits(Offset == EOffset.ImmediateShiftRm ? 1 : 0, 25, 1);
            bw.WriteBits(PreIndex ? 1 : 0, 24, 1);
            bw.WriteBits(Up ? 1 : 0, 23, 1);
            bw.WriteBits(DataSize == EDataSize.Byte ? 1 : 0, 22, 1);
            bw.WriteBits(WriteBack ? 1 : 0, 21, 1);
            bw.WriteBits(Load ? 1 : 0, 20, 1);
            bw.WriteBits((int)Rn, 16, 4);
            bw.WriteBits((int)Rd, 12, 4);

            switch (Offset)
            {
                case EOffset.None:
                case EOffset.Immediate:
                    bw.WriteBits(Immediate, 0, 12);
                    break;
                case EOffset.ImmediateShiftRm:
                    bw.WriteBits(ShiftCount, 7, 5);
                    bw.WriteBits((int)ShiftInst, 5, 2);
                    bw.WriteBits((int)Rm, 0, 4);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return bw.GetValue();
        }

        public void Link(Dictionary<string, int> commandTable, Dictionary<string, int> dataTable, int commandOffset)
        {
            if (Linked) return;
            if (!commandTable.ContainsKey(Label)) throw new Exception("Unknown Label: " + Label);

            var offset = commandTable[Label] - commandOffset - 0x8; // Pipelining!!
            Immediate = (short) offset;
            if (Immediate < 0)
            {
                Up = false;
                Immediate *= -1;
            }

            Linked = true;
        }

        public void Execute([NotNull] Core armCore)
        {
            if (!Decoded || !Linked) throw new InvalidOperationException();
            if (!Helper.CheckConditions(Condition, armCore.GetRegValue(ERegister.Cpsr))) return;

            int value;

            switch (Offset)
            {
                case EOffset.Immediate:
                    value = Immediate;
                    break;
                case EOffset.ImmediateShiftRm:
                    value = armCore.GetRegValue(Rm);
                    Helper.ShiftValue(ref value, ShiftInst, ShiftCount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var addr = PreIndex ?
                Up ? (uint)(armCore.GetRegValue(Rn) + value) : (uint)(armCore.GetRegValue(Rn) - value)
                : (uint)armCore.GetRegValue(Rn);

            if (Load)
            {
                switch (DataSize)
                {
                    case EDataSize.Word:
                        armCore.SetRegValue(Rd, armCore.Ram.ReadInt(addr));
                        break;
                    case EDataSize.Byte:
                        armCore.SetRegValue(Rd, armCore.Ram.ReadByte(addr));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                switch (DataSize)
                {
                    case EDataSize.Word:
                        armCore.Ram.WriteInt(addr, armCore.GetRegValue(Rd));
                        break;
                    case EDataSize.Byte:
                        armCore.Ram.WriteByte(addr, (byte)armCore.GetRegValue(Rd));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (WriteBack) armCore.SetRegValue(Rn, Up ? armCore.GetRegValue(Rn) + value : armCore.GetRegValue(Rn) - value);
        }

        public int GetCommandSize(int align) => (int) Math.Pow(2, align);
    }
}
