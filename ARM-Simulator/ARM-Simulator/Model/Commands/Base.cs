using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal abstract class Base : ICommand
    {
        protected bool Arithmetic;

        protected ECondition Condition;
        protected ERegister? Rd;
        protected ERegister? Rn;
        protected ERegister? Rm;
        protected short Immediate;
        protected EShiftInstruction? ShiftInst;
        protected byte ShiftCount;
        protected bool Decoded;

        // Extended for Arithmetic
        protected EOpcode? Opcode;
        protected bool SetConditionFlags;

        // Extended for Data Access
        protected EMemOpcode? _opcode;
        protected bool _writeBack;
        protected bool _postIndex;

        public abstract void Parse(string[] parameters);
        public abstract void Execute(Core armCore);

        public int Encode()
        {
            if (!Decoded)
                throw new Exception("Cannot convert an undecoded command");

            var bw = new BitWriter();

            bw.WriteBits((int)Condition, 28, 4); // Condition
            bw.WriteBits(0, 27, 1); // Empty

            if (Arithmetic)
            {
                bw.WriteBits(0, 26, 1); // Arithmetic
                bw.WriteBits(Rm != null ? 0 : 1, 25, 1); // Bool use immediate
                if (Opcode != null) bw.WriteBits((int)Opcode, 21, 4); // EOpcode
                bw.WriteBits(SetConditionFlags ? 1 : 0, 20, 1); // Set condition flags
            }
            else
            {
                bw.WriteBits(1, 26, 1); // Data Access
                bw.WriteBits(Rm != null ? 0 : 1, 25, 1); // Bool immediate?
                bw.WriteBits(_postIndex ? 1 : 0, 24, 1);
                bw.WriteBits(0, 23, 1); // Up / Down
                bw.WriteBits(1, 22, 1); // Unsigned
                bw.WriteBits(_writeBack ? 1 : 0, 21, 1);
                if (_opcode != null) bw.WriteBits((int)_opcode, 20, 1);
            }

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
                bw.WriteBits(Immediate, 0, 12);
            }

            return bw.GetValue();
        }

        protected bool CheckConditions(int cpsr)
        {
            var n = (cpsr & (1 << 31)) != 0;
            var z = (cpsr & (1 << 30)) != 0;
            var c = (cpsr & (1 << 29)) != 0;
            var v = (cpsr & (1 << 28)) != 0;

            switch (Condition)
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
    }
}
