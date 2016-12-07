using System;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class DataAccess : ICommand
    {
        protected ECondition Condition;
        protected ERegister? Rd;
        protected ERegister? Rn;
        protected ERegister? Rm;
        protected int Immediate;
        protected EShiftInstruction? ShiftInst;
        protected byte ShiftCount;
        protected bool Decoded;
        protected ERequestType? RequestType;
        protected bool WriteBack;
        protected bool PostIndex;
        protected readonly ESize Size;

        public DataAccess(ECondition condition, ERequestType opcode, ESize size, string[] parameters)
        {
            Condition = condition;
            RequestType = opcode;
            Size = size;
            Immediate = 0;
            Decoded = false;
            PostIndex = false;
            Parse(parameters);
        }

        public DataAccess(ECondition condition, ERequestType opcode,  ESize size, bool writeBack, bool postIndex, ERegister? rd, ERegister? rn, ERegister? rm, int immediate, EShiftInstruction? shiftInst, byte shiftCount)
        {
            Condition = condition;
            RequestType = opcode;
            Size = size;
            Rd = rd;
            Rn = rn;
            Rm = rm;
            WriteBack = writeBack;
            PostIndex = postIndex;
            Immediate = immediate;
            ShiftInst = shiftInst;
            ShiftCount = shiftCount;
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
                }
                else
                    Rm = Parser.ParseRegister(source[1]);
            }

            if (source.Length > 2)
            {
                if (Rm != null)
                    Parser.ParseShiftInstruction(source[2], ref ShiftInst, ref ShiftCount);
                else
                    throw new ArgumentException("Invalid syntax");
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
            Rd = Parser.ParseRegister(parameters[0].Substring(0, parameters[0].Length - 1));

            // Parse Source Address
            ParseSource(parameters[1]);

            if (parameters.Length == 3)
            {
                if (parameters[2].Equals("!"))
                    WriteBack = true;
                else if (parameters[2].StartsWith(","))
                {
                    parameters[2] = parameters[2].Substring(1);

                    if (parameters[2].StartsWith("#"))
                    {
                        Immediate = Parser.ParseImmediate<short>(parameters[2]);
                        if (Immediate > 4096) throw new ArgumentOutOfRangeException();
                    }
                    else
                        Rm = Parser.ParseRegister(parameters[2]);

                    WriteBack = true;
                    PostIndex = true;
                }
                else
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
            bw.WriteBits(1, 26, 2); // Operation

            bw.WriteBits(Rm != null ? 1 : 0, 25, 1); // Bool immediate?
            bw.WriteBits(PostIndex ? 0 : 1, 24, 1);
            bw.WriteBits(0, 23, 1); // Up / Down
            bw.WriteBits((int)Size, 22, 1); // byte or word
            bw.WriteBits(WriteBack ? 1 : 0, 21, 1);
            if (RequestType != null) bw.WriteBits((int)RequestType, 20, 1);

            if (Rn != null) bw.WriteBits((int)Rn, 16, 4); // 1st operand
            if (Rd != null) bw.WriteBits((int)Rd, 12, 4); // destination

            // Operand 2
            if (Rm != null)
            {
                if (ShiftInst != null)
                {
                    bw.WriteBits(ShiftCount, 7, 5);
                    bw.WriteBits((int)ShiftInst, 5, 2);
                    bw.WriteBits(0, 4, 1);
                }
                bw.WriteBits((int)Rm, 0, 4);
            }
            else
            {
                bw.WriteBits(Immediate, 0, 12);
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

            if (Rm != null)
            {
                // Get Register which may be shifted
                value = armCore.GetRegValue(Rm);
                Helper.ShiftValue(ref value, ShiftInst, ShiftCount);
            }
            else
            {
                value = Immediate;
            }

            switch (RequestType)
            {
                case ERequestType.Load:
                    armCore.SetRegValue(Rd,
                        PostIndex
                            ? armCore.Ram.ReadInt((uint) armCore.GetRegValue(Rn))
                            : armCore.Ram.ReadInt((uint) (armCore.GetRegValue(Rn) + value)));
                    break;
                case ERequestType.Store:
                    armCore.Ram.WriteInt(
                        PostIndex
                            ? (uint)armCore.GetRegValue(Rn)
                            : (uint)(armCore.GetRegValue(Rn) + value),
                            armCore.GetRegValue(Rd));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (WriteBack)
                armCore.SetRegValue(Rn, armCore.GetRegValue(Rn) + value);
        }
    }
}
