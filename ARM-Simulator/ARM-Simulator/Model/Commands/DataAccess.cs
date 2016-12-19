using System;
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
        protected bool Unsigned;
        protected bool WriteBack;
        protected readonly ESize Size;
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

        public DataAccess(ECondition condition, bool load, ESize size, string[] parameters)
        {
            Condition = condition;
            Load = load;
            Size = size;
            Decoded = false;
            PreIndex = true;
            Parse(parameters);
        }

        public DataAccess(ECondition condition, bool load, bool preIndex, bool unsigned, bool writeBack, ESize size, ERegister rn, ERegister rd, short immediate)
        {
            Offset = EOffset.Immediate;
            Condition = condition;
            Load = load;
            PreIndex = preIndex;
            Unsigned = unsigned;
            WriteBack = writeBack;
            Size = size;
            Rn = rn;
            Rd = rd;
            Immediate = immediate;
            Decoded = true;
        }

        public DataAccess(ECondition condition, bool load, bool preIndex, bool unsigned, bool writeBack, ESize size, ERegister rn, ERegister rd, byte shiftCount, EShiftInstruction shiftInst, ERegister rm)
        {
            Offset = EOffset.ImmediateShiftRm;
            Condition = condition;
            Load = load;
            PreIndex = preIndex;
            Unsigned = unsigned;
            WriteBack = writeBack;
            Size = size;
            Rn = rn;
            Rd = rd;
            ShiftCount = shiftCount;
            ShiftInst = shiftInst;
            Rm = rm;
            Decoded = true;
        }

        private void ParseSource(string sourceString)
        {
            var source = sourceString.Split(',');
            if (source.Length > 3)
                throw new ArgumentException("Invalid syntax");

            Rn = Parser.ParseRegister(source[0]);

            if (source.Length > 1)
            {
                if (source[1].StartsWith("#"))
                {
                    Immediate = Parser.ParseImmediate<short>(source[1]);
                    if (Immediate > 4096) throw new ArgumentOutOfRangeException();
                    Offset = EOffset.Immediate;
                }
                else
                {
                    Rm = Parser.ParseRegister(source[1]);
                    if (source.Length > 2)
                    {
                        Parser.ParseShiftInstruction(source[2], ref ShiftInst, ref ShiftCount);
                    }
                    Offset = EOffset.ImmediateShiftRm;
                }    
            }
        }

        public void Parse(string[] parameters)
        {
            if (Decoded)
                throw new Exception("Cannot parse a decoded command");

            // Check Parameter Count
            if (parameters.Length != 2 && parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Parse Source Register
            if (!parameters[0].EndsWith(","))
                throw new ArgumentException("Invalid syntax");

            // Resize
            parameters[0] = parameters[0].Substring(0, parameters[0].Length - 1);

            Rd = Parser.ParseRegister(parameters[0]);

            // Parse Source Address
            ParseSource(parameters[1]);

            if (parameters.Length == 2)
            {
                Decoded = true;
                return;
            }

            // Parse third parameter
            if (parameters[2].Equals("!"))
            {
                WriteBack = true;
            }
            else if (parameters[2].StartsWith(","))
            {
                parameters[2] = parameters[2].Substring(1);

                if (parameters[2].StartsWith("#"))
                {
                    Immediate = Parser.ParseImmediate<short>(parameters[2]);
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
                throw new ArgumentException("Invalid syntax");
            }

            Decoded = true;
        }

        public int Encode()
        {
            if (!Decoded)
                throw new Exception("Cannot convert an undecoded command");

            var bw = new BitWriter();

            bw.WriteBits((int)Condition, 28, 4); // Condition
            bw.WriteBits(1, 26, 1);

            bw.WriteBits(PreIndex ? 1 : 0, 24, 1);
            bw.WriteBits(Unsigned ? 1 : 0, 23, 1);

            if (Offset == EOffset.ImmediateShiftRm)
                bw.WriteBits(1, 22, 1);

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

        public void Execute(Core armCore)
        {
            if (!Decoded)
                throw new Exception("Cannot execute an undecoded command");

            if (!Helper.CheckConditions(Condition, armCore.GetCpsr()))
                return;

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

            var addr = PreIndex ? (uint)(armCore.GetRegValue(Rn) + value) : (uint)armCore.GetRegValue(Rn);

            if (Load)
            {
                switch (Size)
                {
                    case ESize.Word:
                        armCore.SetRegValue(Rd, armCore.Ram.ReadInt(addr));
                        break;
                    case ESize.Byte:
                        armCore.SetRegValue(Rd, armCore.Ram.ReadByte(addr));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                switch (Size)
                {
                    case ESize.Word:
                        armCore.Ram.WriteInt(addr, armCore.GetRegValue(Rd));
                        break;
                    case ESize.Byte:
                        armCore.Ram.WriteByte(addr, (byte)armCore.GetRegValue(Rd));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (WriteBack) armCore.SetRegValue(Rn, armCore.GetRegValue(Rn) + value);
        }
    }
}
