using System;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Logical : Base
    {
        public Logical(ECondition condition, EOpcode opcode, bool setConditionFlags, string[] parameters)
        {
            Arithmetic = true;
            Condition = condition;
            Opcode = opcode;
            SetConditionFlags = setConditionFlags;
            Decoded = false;
            Parse(parameters);
        }

        public Logical(ECondition condition, EOpcode opcode, bool setConditionFlags, ERegister? rd, ERegister? rn, ERegister? rm, short immediate, EShiftInstruction? shiftInst, byte shiftCount)
        {
            Arithmetic = true;
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

        public sealed override void Parse(string[] parameters)
        {
            if (Decoded)
                throw new Exception("Cannot parse a decoded command");

            // Check Parameter Count
            if (parameters.Length != 3 && parameters.Length != 4)
                throw new ArgumentException("Invalid parameter count");

            // Parse Destination Register / Source Register
            Rd = Parser.ParseRegister(parameters[0]);
            Rn = Parser.ParseRegister(parameters[1]);

            // Check for Rm or 8 bit immediate
            Parser.ParseOperand2(parameters[2], ref Rm, ref Immediate);

            // Check for Shift Instruction
            if (Rm != null && parameters.Length == 4)
                Parser.ParseShiftInstruction(parameters[3], ref ShiftInst, ref ShiftCount);

            Decoded = true;
        }

        public override void Execute(Core armCore)
        {
            if (!Decoded)
                throw new Exception("Cannot execute an undecoded command");

            if (!CheckConditions(armCore.GetRegValue(ERegister.Cpsr)))
                return;

            // Get Register which may be shifted
            var value = armCore.GetRegValue(Rm);
            var carry = Shift.ShiftValue(ref value, ShiftInst, ShiftCount);

            switch (Opcode)
            {
                case EOpcode.And:
                    value = armCore.GetRegValue(Rn) & value;
                    break;
                case EOpcode.Eor:
                    value = armCore.GetRegValue(Rn) ^ value;
                    break;
                case EOpcode.Orr:
                    value = armCore.GetRegValue(Rn) | value;
                    break;
                case EOpcode.Bic:
                    value = armCore.GetRegValue(Rn) & ~value;
                    break;
                default:
                    throw new ArgumentException("Invalid EOpcode");
            }

            if (SetConditionFlags)
            {
                armCore.SetNzcvFlags(new Flags(true, true, true, false),
                        new Flags(value < 0, value == 0, carry, false));
            }

            armCore.SetRegValue(Rd, value);
        }
    }
}
