using System;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Helper
    {
        public static bool CheckConditions(ECondition condition, int cpsr)
        {
            var n = (cpsr & (1 << 31)) != 0;
            var z = (cpsr & (1 << 30)) != 0;
            var c = (cpsr & (1 << 29)) != 0;
            var v = (cpsr & (1 << 28)) != 0;

            switch (condition)
            {
                case ECondition.Equal:
                    if (z) return true;
                    break;
                case ECondition.NotEqual:
                    if (!z) return true;
                    break;
                case ECondition.CarrySet:
                    if (c) return true;
                    break;
                case ECondition.CarryClear:
                    if (!c) return true;
                    break;
                case ECondition.Minus:
                    if (n) return true;
                    break;
                case ECondition.Plus:
                    if (!n) return true;
                    break;
                case ECondition.OverflowSet:
                    if (v) return true;
                    break;
                case ECondition.OverflowClear:
                    if (!v) return true;
                    break;
                case ECondition.Higher:
                    if (c && !z) return true;
                    break;
                case ECondition.LowerOrSame:
                    if (!c || z) return true;
                    break;
                case ECondition.GreaterEqual:
                    if (n == v) return true;
                    break;
                case ECondition.LessThan:
                    if (n != v) return true;
                    break;
                case ECondition.GreaterThan:
                    if (!z && (n == v)) return true;
                    break;
                case ECondition.LessEqual:
                    if (z && (n != v)) return true;
                    break;
                case ECondition.Always:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

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
