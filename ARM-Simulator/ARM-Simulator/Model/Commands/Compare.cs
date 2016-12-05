using System;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Compare : Base
    {
        public Compare(ECondition condition, EOpcode opcode, string[] parameters)
        {
            Arithmetic = true;
            Condition = condition;
            Opcode = opcode;
            Decoded = false;
            Parse(parameters);
        }

        public Compare(ECondition condition, EOpcode opcode, ERegister? rn, ERegister? rm, short immediate, EShiftInstruction? shiftInst, byte shiftCount)
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
            Parser.ParseOperand2(parameters[1], parameters.Length == 3 ? parameters[2] : null, ref Rm, ref Immediate, ref ShiftInst, ref ShiftCount);

            Decoded = true;
        }

        public override void Execute(Core armCore)
        {
            if (!Decoded)
                throw new Exception("Cannot execute an undecoded command");

            if (!CheckConditions(armCore.GetRegValue(ERegister.Cpsr)))
                return;

            int value;

            if (Rm != null)
            {
                // Get Register which may be shifted
                value = armCore.GetRegValue(Rm);
                Shift.ShiftValue(ref value, ShiftInst, ShiftCount);
            }
            else
            {
                value = Immediate;
            }

            switch (Opcode)
            {
                case EOpcode.Cmp:
                    armCore.SetNzcvFlags(new Flags(true, true, true, true),
                        Substract.CheckFlags(armCore.GetRegValue(Rn), value));
                    break;
                case EOpcode.Cmn:
                    armCore.SetNzcvFlags(new Flags(true, true, true, true),
                        Add.CheckFlags(armCore.GetRegValue(Rn), value));
                    break;
                default:
                    throw new ArgumentException("Invalid EOpcode");
            }
        }
    }
}
