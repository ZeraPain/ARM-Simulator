using System;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Arithmetic : ICommand
    {
        protected ECondition Condition;
        protected EOpcode? Opcode;
        protected bool SetConditionFlags;
        protected ERegister? Rd;
        protected ERegister? Rn;
        protected ERegister? Rm;
        protected int Immediate;
        protected EShiftInstruction? ShiftInst;
        protected byte ShiftCount;
        protected bool Decoded;

        public Arithmetic(ECondition condition, EOpcode opcode, bool setConditionFlags, string[] parameters)
        {
            Condition = condition;
            Opcode = opcode;
            SetConditionFlags = setConditionFlags;
            Decoded = false;
            Parse(parameters);
        }

        public Arithmetic(ECondition condition, EOpcode opcode, bool setConditionFlags, ERegister? rd, ERegister? rn, ERegister? rm, int immediate, EShiftInstruction? shiftInst, byte shiftCount)
        {
            Condition = condition;
            Opcode = opcode;
            SetConditionFlags = setConditionFlags;
            Rd = rd;
            Rn = rn;
            Rm = rm;
            Immediate = immediate;
            ShiftInst = shiftInst;
            ShiftCount = shiftCount;
            Decoded = true;
        }

        public void Parse(string[] parameters)
        {
            if (Decoded)
                throw new Exception("Cannot parse a decoded command");

            switch (Opcode)
            {
                case EOpcode.Add:
                case EOpcode.Sub:
                case EOpcode.Rsb:
                case EOpcode.And:
                case EOpcode.Eor:
                case EOpcode.Orr:
                case EOpcode.Bic:
                    // Check parameter count
                    if (parameters.Length != 3 && parameters.Length != 4)
                        throw new ArgumentException("Invalid parameter count");

                    // Parse Rd, Rn
                    Rd = Parser.ParseRegister(parameters[0]);
                    Rn = Parser.ParseRegister(parameters[1]);

                    Parser.ParseOperand2(parameters[2], parameters.Length == 4 ? parameters[3] : null, ref Rm,
                        ref Immediate, ref ShiftInst, ref ShiftCount);
                    break;
                case EOpcode.Mov:
                case EOpcode.Mvn:
                    // Check for valid parameter count
                    if (parameters.Length != 2 && parameters.Length != 3)
                        throw new ArgumentException("Invalid parameter count");

                    // Parse Rd
                    Rd = Parser.ParseRegister(parameters[0]);

                    Parser.ParseOperand2(parameters[1], parameters.Length == 3 ? parameters[2] : null, ref Rm,
                        ref Immediate, ref ShiftInst, ref ShiftCount);
                    break;
                case EOpcode.Tst:
                case EOpcode.Teq:
                case EOpcode.Cmp:
                case EOpcode.Cmn:
                    // Check for valid parameter count
                    if (parameters.Length != 2 && parameters.Length != 3)
                        throw new ArgumentException("Invalid parameter count");

                    // Parse Rn
                    Rn = Parser.ParseRegister(parameters[0]);

                    Parser.ParseOperand2(parameters[1], parameters.Length == 3 ? parameters[2] : null, ref Rm,
                        ref Immediate, ref ShiftInst, ref ShiftCount);
                    break;
            }

            Decoded = true;
        }

        public int Encode()
        {
            if (!Decoded)
                throw new Exception("Cannot convert an undecoded command");

            var bw = new BitWriter();

            bw.WriteBits((int)Condition, 28, 4); // Condition
            bw.WriteBits(0, 26, 2); // Operation

            bw.WriteBits(Rm != null ? 0 : 1, 25, 1); // Bool use immediate
            if (Opcode != null) bw.WriteBits((int)Opcode, 21, 4); // EOpcode
            bw.WriteBits(SetConditionFlags ? 1 : 0, 20, 1); // Set condition flags

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
                bw.WriteBits(ShiftCount, 8, 4);
                bw.WriteBits(Immediate, 0, 8);
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
            var carry = false;

            // Add two registers
            if (Rm != null)
            {
                // Get Register which may be shifted
                value = armCore.GetRegValue(Rm);
                carry = Helper.ShiftValue(ref value, ShiftInst, ShiftCount);
            }
            // Add immediate to Rd
            else
            {
                value = Immediate;
            }

            var result = 0;

            switch (Opcode)
            {
                case EOpcode.Add:
                    result = armCore.GetRegValue(Rn) + value;
                    armCore.SetRegValue(Rd, result);
                    break;
                case EOpcode.Sub:
                    result = armCore.GetRegValue(Rn) - value;
                    armCore.SetRegValue(Rd, result);
                    break;
                case EOpcode.Rsb:
                    result = value - armCore.GetRegValue(Rn);
                    armCore.SetRegValue(Rd, result);
                    break;
                case EOpcode.Mov:
                    result = value;
                    armCore.SetRegValue(Rd, result);
                    break;
                case EOpcode.Mvn:
                    result = value ^ -1;
                    armCore.SetRegValue(Rd, result);
                    break;
                case EOpcode.And:
                    result = armCore.GetRegValue(Rn) & value;
                    armCore.SetRegValue(Rd, result);
                    break;
                case EOpcode.Eor:
                    result = armCore.GetRegValue(Rn) ^ value;
                    armCore.SetRegValue(Rd, result);
                    break;
                case EOpcode.Orr:
                    result = armCore.GetRegValue(Rn) | value;
                    armCore.SetRegValue(Rd, result);
                    break;
                case EOpcode.Bic:
                    result = armCore.GetRegValue(Rn) & ~value;
                    armCore.SetRegValue(Rd, result);
                    break;
                case EOpcode.Tst:
                    result = armCore.GetRegValue(Rn) & value;
                    break;
                case EOpcode.Teq:
                    result = armCore.GetRegValue(Rn) ^ value;
                    break;
            }

            if (SetConditionFlags)
            {
                switch (Opcode)
                {
                    case EOpcode.Cmp:
                    case EOpcode.Sub:
                        armCore.SetNzcvFlags(new Flags(true, true, true, true),
                            CheckFlags(armCore.GetRegValue(Rn), ~value + 1));
                        break;
                    case EOpcode.Rsb:
                        armCore.SetNzcvFlags(new Flags(true, true, true, true),
                            CheckFlags(value, ~armCore.GetRegValue(Rn) + 1));
                        break;
                    case EOpcode.Cmn:
                    case EOpcode.Add:
                        armCore.SetNzcvFlags(new Flags(true, true, true, true),
                            CheckFlags(armCore.GetRegValue(Rn), value));
                        break;
                    case EOpcode.And:
                    case EOpcode.Eor:
                    case EOpcode.Tst:
                    case EOpcode.Teq:
                    case EOpcode.Orr:
                    case EOpcode.Mov:
                    case EOpcode.Bic:
                    case EOpcode.Mvn:
                        armCore.SetNzcvFlags(new Flags(true, true, true, false),
                            new Flags(result < 0, result == 0, carry, false));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static Flags CheckFlags(int regValue, int addValue)
        {
            var newValue = (long)regValue + addValue;

            var n = (int)newValue < 0;
            var z = (int)newValue == 0;
            var c = (newValue & 0x100000000) > 0;
            var v = newValue < int.MinValue || newValue > int.MaxValue;

            return new Flags(n, z, c, v);
        }
    }
}
