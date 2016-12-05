using System;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Move : Base
    {
        public Move(ECondition condition, EOpcode opcode, bool setConditionFlags, string[] parameters)
        {
            Arithmetic = true;
            Condition = condition;
            Opcode = opcode;
            SetConditionFlags = setConditionFlags;
            Decoded = false;
            Parse(parameters);
        }

        public Move(ECondition condition, EOpcode? opcode, bool setConditionFlags, ERegister? rd, ERegister? rm, short immediate, EShiftInstruction? shiftInst, byte shiftCount)
        {
            Arithmetic = true;
            Condition = condition;
            Opcode = opcode;
            SetConditionFlags = setConditionFlags;
            Rd = rd;
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

            // Check for valid parameter count
            if (parameters.Length != 2 && parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Parse Rd
            Rd = Parser.ParseRegister(parameters[0]);

            if (!SetConditionFlags && Opcode == EOpcode.Mov) // Not valid for Mvn
            {
                if (parameters[1].StartsWith("#"))
                {
                    if (parameters.Length != 2)
                        throw new ArgumentException("Invalid parameter count");

                    // Parse 16 bit immediate
                    Immediate = Parser.ParseImmediate(parameters[1], 12);

                    Decoded = true;
                    return;
                }
            }

            // Check for Rm or 8 bit immediate
            Parser.ParseOperand2(parameters[1], ref Rm, ref Immediate);

            // Check for shift instruction
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

            // calculate with Rm
            if (Rm != null)
            {
                // Get value and shift if requested
                var value = armCore.GetRegValue(Rm);
                var carry = Shift.ShiftValue(ref value, ShiftInst, ShiftCount);

                // XOR if MVN or MVNS
                if (Opcode == EOpcode.Mvn)
                {
                    value ^= -1; // XOR with 0xFFFFFFFF
                }

                // Set status flags if requested
                if (SetConditionFlags)
                {
                    armCore.SetNzcvFlags(new Flags(true, true, true, false),
                        new Flags(value < 0, value == 0, carry, false));
                }
                   
                // Write value to Rd
                armCore.SetRegValue(Rd, value);
                return;
            }

            // XOR immediate if requested
            if (Opcode == EOpcode.Mvn)
            {
                Immediate ^= -1; // XOR with 0xFFFFFFFF
            }

            // Set status flags if requested
            if (SetConditionFlags)
            {
                armCore.SetNzcvFlags(new Flags(true, true, true, false),
                    new Flags(Immediate < 0, Immediate == 0, false, false));
            }

            // Write immediate to Rd
            armCore.SetRegValue(Rd, Immediate);
        }
    }
}
