using System;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Test : Base
    {
        public Test(ECondition condition, EOpcode opcode, string[] parameters)
        {
            Arithmetic = true;
            Condition = condition;
            Opcode = opcode;
            Decoded = false;
            Parse(parameters);
        }

        public Test(ECondition condition, EOpcode opcode, ERegister? rn, ERegister? rm, short immediate, EShiftInstruction? shiftInst, byte shiftCount)
        {
            Arithmetic = true;
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
            Parser.ParseOperand2(parameters[1], ref Rm, ref Immediate);

            // Check for Shift Instruction
            if (Rm != null && parameters.Length == 3)
                Parser.ParseShiftInstruction(parameters[2], ref ShiftInst, ref ShiftCount);

            Decoded = true;
        }

        public override void Execute(Core armCore)
        {
            if (!Decoded)
                throw new Exception("Cannot execute an undecoded command");

            if (!CheckConditions(armCore.GetRegValue(ERegister.Cpsr)))
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
