using System;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Arithmetic : ICommand
    {
        // required
        protected ECondition Condition;
        protected EOpcode Opcode;
        protected bool SetConditionFlags;
        protected ERegister Rn;
        protected ERegister Rd;

        // rotate immediate
        protected byte Rotate;
        protected byte Immediate;

        // shift rm
        protected byte ShiftCount;
        protected ERegister Rm;
        protected ERegister Rs;
        protected EShiftInstruction ShiftInst;

        protected EOperand2 Operand2;
        protected bool Decoded;

        public Arithmetic(ECondition condition, EOpcode opcode, bool setConditionFlags, string[] parameters)
        {
            Condition = condition;
            Opcode = opcode;
            SetConditionFlags = setConditionFlags;
            Decoded = false;
            Parse(parameters);
        }

        public Arithmetic(ECondition condition, EOpcode opcode, bool setConditionFlags, ERegister rn, ERegister rd, byte rotate, byte immediate)
        {
            Operand2 = EOperand2.RotateImmediate;
            Condition = condition;
            Opcode = opcode;
            SetConditionFlags = setConditionFlags;
            Rn = rn;
            Rd = rd;
            Rotate = rotate;
            Immediate = immediate;
            Decoded = true;
        }

        public Arithmetic(ECondition condition, EOpcode opcode, bool setConditionFlags, ERegister rn, ERegister rd, byte shiftCount, EShiftInstruction shiftInst, ERegister rm)
        {
            Operand2 = EOperand2.ImmediateShiftRm;
            Condition = condition;
            Opcode = opcode;
            SetConditionFlags = setConditionFlags;
            Rn = rn;
            Rd = rd;
            ShiftCount = shiftCount;
            ShiftInst = shiftInst;
            Rm = rm;
            Decoded = true;
        }

        public Arithmetic(ECondition condition, EOpcode opcode, bool setConditionFlags, ERegister rn, ERegister rd, ERegister rs, EShiftInstruction shiftInst, ERegister rm)
        {
            Operand2 = EOperand2.RsShiftRm;
            Condition = condition;
            Opcode = opcode;
            SetConditionFlags = setConditionFlags;
            Rn = rn;
            Rd = rd;
            Rs = rs;
            ShiftInst = shiftInst;
            Rm = rm;
            Decoded = true;
        }

        private EOperand2 ParseOperand2(string operand2, string shiftValue)
        {
            if (operand2.StartsWith("#", StringComparison.Ordinal)) // use immediate
            {
                Immediate = Parser.ParseImmediate<byte>(operand2);
                ShiftCount = 0;
                if (!string.IsNullOrEmpty(shiftValue)) // rotate right
                {
                    Rotate = Parser.ParseImmediate<byte>(shiftValue);
                    if (Rotate >= 16)
                        throw new ArgumentOutOfRangeException();
                }

                return EOperand2.RotateImmediate;
            }

            Rm = Parser.ParseRegister(operand2); // use register

            if (!string.IsNullOrEmpty(shiftValue) && Parser.ParseShiftInstruction(shiftValue, ref ShiftInst, ref ShiftCount, ref Rs))
            {
                return EOperand2.RsShiftRm;
            }

            return EOperand2.ImmediateShiftRm;
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

                    Operand2 = ParseOperand2(parameters[2], parameters.Length == 4 ? parameters[3] : null);
                    break;
                case EOpcode.Mov:
                case EOpcode.Mvn:
                    // Check for valid parameter count
                    if (parameters.Length != 2 && parameters.Length != 3)
                        throw new ArgumentException("Invalid parameter count");

                    // Parse Rd
                    Rd = Parser.ParseRegister(parameters[0]);

                    Operand2 = ParseOperand2(parameters[1], parameters.Length == 3 ? parameters[2] : null);
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

                    Operand2 = ParseOperand2(parameters[1], parameters.Length == 3 ? parameters[2] : null);
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

            if (Operand2 == EOperand2.RotateImmediate)
                bw.WriteBits(1, 25, 1);

            bw.WriteBits((int)Opcode, 21, 4); // Opcode
            bw.WriteBits(SetConditionFlags ? 1 : 0, 20, 1); // Set condition flags

            bw.WriteBits((int)Rn, 16, 4); // Rn
            bw.WriteBits((int)Rd, 12, 4); // Rd

            // Operand2
            switch (Operand2)
            {
                case EOperand2.RotateImmediate:
                    bw.WriteBits(Rotate, 8, 4);
                    bw.WriteBits(Immediate, 0, 8);
                    break;
                case EOperand2.ImmediateShiftRm:
                    bw.WriteBits(ShiftCount, 7, 5);
                    bw.WriteBits((int)ShiftInst, 5, 2);
                    bw.WriteBits(0, 4, 1);
                    bw.WriteBits((int)Rm, 0, 4);
                    break;
                case EOperand2.RsShiftRm:
                    bw.WriteBits((int)Rs, 8, 4);
                    bw.WriteBits(0, 7, 1);
                    bw.WriteBits((int)ShiftInst, 5, 2);
                    bw.WriteBits(1, 4, 1);
                    bw.WriteBits((int)Rm, 0, 4);
                    break;
            }

            return bw.GetValue();
        }

        private int Calculation(Core armCore, int value)
        {
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

            return result;
        }

        public void Execute(Core armCore)
        {
            if (!Decoded)
                throw new Exception("Cannot execute an undecoded command");

            if (!Helper.CheckConditions(Condition, armCore.GetCpsr()))
                return;

            var value = 0;
            var carry = false;

            switch (Operand2)
            {
                case EOperand2.RotateImmediate:
                    value = Immediate;
                    Helper.ShiftValue(ref value, EShiftInstruction.Ror, Rotate);
                    break;
                case EOperand2.ImmediateShiftRm:
                    value = armCore.GetRegValue(Rm);
                    carry = Helper.ShiftValue(ref value, ShiftInst, ShiftCount);
                    break;
                case EOperand2.RsShiftRm:
                    value = armCore.GetRegValue(Rm);
                    carry = Helper.ShiftValue(ref value, ShiftInst, (byte)armCore.GetRegValue(Rs));
                    break;
            }

            var result = Calculation(armCore, value);

            if (!SetConditionFlags)
                return;

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
