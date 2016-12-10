using System;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Jump : ICommand
    {
        protected ECondition Condition;
        protected EJump JumpType;

        protected ERegister Rm;
        protected int Offset;

        public Jump(ECondition conditions, EJump jump, int offset)
        {
            Condition = conditions;
            JumpType = jump;
            Offset = offset;
        }

        public Jump(ECondition conditions, EJump jump, ERegister rm)
        {
            Condition = conditions;
            JumpType = jump;
            Rm = rm;
        }

        public void Parse(string[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Execute(Core armCore)
        {
            armCore.SetRegValue(ERegister.Pc, armCore.GetRegValue(ERegister.Pc) + Offset * 4);
        }

        public int Encode()
        {
            var bw = new BitWriter();

            bw.WriteBits((int)Condition, 28, 4); // Condition

            switch (JumpType)
            {
                case EJump.Branch:
                    bw.WriteBits(1, 27, 1);
                    bw.WriteBits(0, 26, 1);
                    bw.WriteBits(1, 25, 1);
                    bw.WriteBits(0, 24, 1);
                    bw.WriteBits(Offset, 0, 24);
                    break;
                case EJump.BranchLink:
                    bw.WriteBits(1, 27, 1);
                    bw.WriteBits(0, 26, 1);
                    bw.WriteBits(1, 25, 1);
                    bw.WriteBits(1, 24, 1);
                    bw.WriteBits(Offset, 0, 24);
                    break;
                case EJump.BranchExchange:
                    bw.WriteBits(0, 27, 1);
                    bw.WriteBits(0, 26, 1);
                    bw.WriteBits(0, 25, 1);
                    bw.WriteBits(1, 24, 1);
                    bw.WriteBits(0, 23, 1);
                    bw.WriteBits(0, 22, 1);
                    bw.WriteBits(1, 21, 1);
                    bw.WriteBits(0, 20, 1);
                    bw.WriteBits((int)Rm, 0, 4);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return bw.GetValue();
        }
    }
}
