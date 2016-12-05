using System;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Substract : Base
    {
        public Substract(ECondition condition, EOpcode opcode, bool setConditionFlags, string[] parameters)
        {
            Arithmetic = true;
            Condition = condition;
            Opcode = opcode;
            SetConditionFlags = setConditionFlags;
            Decoded = false;
            Parse(parameters);
        }

        public Substract(ECondition condition, EOpcode? opcode, bool setConditionFlags, ERegister? rd, ERegister? rn, ERegister? rm, short immediate, EShiftInstruction? shiftInst, byte shiftCount)
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

            // Check parameter count
            if (parameters.Length != 3 && parameters.Length != 4)
                throw new ArgumentException("Invalid parameter count");

            // Parse Rd, Rn
            Rd = Parser.ParseRegister(parameters[0]);
            Rn = Parser.ParseRegister(parameters[1]);

            if (!SetConditionFlags)
            {
                if (parameters[2].StartsWith("#"))
                {
                    if (parameters.Length != 3)
                        throw new ArgumentException("Invalid parameter count");

                    // Parse 12 bit immediate
                    Immediate = Parser.ParseImmediate(parameters[2], 12);

                    Decoded = true;
                    return;
                }
            }

            // Check for Rm or 8 bit immediate
            Parser.ParseOperand2(parameters[2], ref Rm, ref Immediate);

            // Check for shift instruction
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

            int value;

            // Add two registers
            if (Rm != null)
            {
                // Get Register which may be shifted
                value = armCore.GetRegValue(Rm);
                Shift.ShiftValue(ref value, ShiftInst, ShiftCount);
            }
            // Add immediate to Rd
            else
            {
                value = Immediate;
            }

            if (SetConditionFlags)
            {
                if (Opcode == EOpcode.Rsb)
                {
                    armCore.SetNzcvFlags(new Flags(true, true, true, true),
                        CheckFlags(value, armCore.GetRegValue(Rn)));
                }
                else
                {
                    armCore.SetNzcvFlags(new Flags(true, true, true, true),
                        CheckFlags(armCore.GetRegValue(Rn), value));
                }
            }

            if (Opcode == EOpcode.Rsb)
            {
                armCore.SetRegValue(Rd, value - armCore.GetRegValue(Rn));
            }
            else
            {
                armCore.SetRegValue(Rd, armCore.GetRegValue(Rn) - value);
            }
        }

        public static Flags CheckFlags(int regValue, int addValue)
        {
            var newValue = (long)regValue + (~addValue + 1); // 2k complement

            var n = (int)newValue < 0;
            var z = (int)newValue == 0;
            var c = (newValue & 0x100000000) > 0;
            var v = newValue < int.MinValue || newValue > int.MaxValue;

            return new Flags(n, z, c, v);
        }
    }
}
