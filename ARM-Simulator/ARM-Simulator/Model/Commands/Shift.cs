using System;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Shift
    {
        public static bool ShiftValue(ref int value, EShiftInstruction? shiftInstruction, byte shiftCount)
        {
            var carry = 0;

            if (shiftCount == 0)
                return false;

            switch (shiftInstruction)
            {
                case null:
                    break;
                case EShiftInstruction.Lsr:
                    carry = shiftCount > 32 ? 0 : value & (1 << (shiftCount - 1));
                    value = value >> shiftCount;
                    break;
                case EShiftInstruction.Lsl:
                    carry = shiftCount > 31 ? 0 : value & (1 << (32 - shiftCount));
                    value = value << shiftCount;
                    break;
                case EShiftInstruction.Ror:
                    carry = shiftCount > 31 ? 0 : value & (1 << (shiftCount - 1));
                    value = (value >> shiftCount) | (value << (32 - shiftCount));
                    break;
                case EShiftInstruction.Asr:
                    carry = shiftCount > 32 ? 0 : value & (1 << (shiftCount - 1));
                    value = value >> shiftCount;
                    value |= -1 << (32 - shiftCount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return carry > 0;
        }
    }
}
