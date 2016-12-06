﻿using System;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Test : Base
    {
        public Test(ECondition condition, EOpcode opcode, string[] parameters)
        {
            Operation = EOperation.Arithmetic;
            Condition = condition;
            Opcode = opcode;
            Decoded = false;
            Parse(parameters);
        }

        public Test(ECondition condition, EOpcode opcode, ERegister? rn, ERegister? rm, int immediate, EShiftInstruction? shiftInst, byte shiftCount)
        {
            Operation = EOperation.Arithmetic;
            Condition = condition;
            Opcode = opcode;
            Rn = rn;
            Rm = rm;
            Immediate = immediate;
            ShiftInst = shiftInst;
            ShiftCount = shiftCount;
            Decoded = true;
        }

        public sealed override void Parse(string[] parameters)
        {
            if (Decoded)
                throw new Exception("Cannot parse a decoded command");

            // Check Parameter Count
            if (parameters.Length != 2 && parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Parse Source Register
            Rn = Parser.ParseRegister(parameters[0]);

            // Check for Rm or 8 bit immediate
            Parser.ParseOperand2(parameters[1], parameters.Length == 3 ? parameters[2] : null, ref Rm, ref Immediate, ref ShiftInst, ref ShiftCount);

            Decoded = true;
        }

        public override int Encode()
        {
            return EncodeArithmetic();
        }

        public override void Execute(Core armCore)
        {
            if (!Decoded)
                throw new Exception("Cannot execute an undecoded command");

            if (!CheckConditions(armCore.GetCpsr()))
                return;

            var value = Rm != null ? armCore.GetRegValue(Rm) : Immediate;
            var carry = Rm != null && Shift.ShiftValue(ref value, ShiftInst, ShiftCount);

            switch (Opcode)
            {
                case EOpcode.Tst:
                    value = armCore.GetRegValue(Rn) & value;
                    break;
                case EOpcode.Teq:
                    value = armCore.GetRegValue(Rn) ^ value;
                    break;
                default:
                    throw new ArgumentException("Invalid EOpcode");
            }

            // Set condition flags
            armCore.SetNzcvFlags(new Flags(true, true, true, false),
                    new Flags(value < 0, value == 0, carry, false));
        }
    }
}
