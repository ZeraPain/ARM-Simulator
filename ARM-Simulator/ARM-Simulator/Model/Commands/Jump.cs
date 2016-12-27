using System;
using System.Collections.Generic;
using ARM_Simulator.Annotations;
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

        protected bool Linked;
        protected int Offset;
        protected string Label;


        public Jump(ECondition conditions, EJump jump, string label)
        {
            Linked = false;
            Condition = conditions;
            JumpType = jump;
            Label = label;
        }

        public Jump(ECondition conditions, EJump jump, int offset)
        {
            Linked = false;
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

        public void Parse(string parameterString)
        {
            throw new NotImplementedException();
        }

        public void Execute([NotNull] Core armCore)
        {
            if (!Helper.CheckConditions(Condition, armCore.GetCpsr()))
                return;

            switch (JumpType)
            {
                case EJump.Branch:
                    armCore.Jump(armCore.GetRegValue(ERegister.Pc) + Offset * 4);
                    break;
                case EJump.BranchLink:
                    armCore.SetRegValue(ERegister.Lr, armCore.GetRegValue(ERegister.Pc) - 0x4);
                    armCore.Jump(armCore.GetRegValue(ERegister.Pc) + Offset * 4);
                    break;
                case EJump.BranchExchange:
                    armCore.Jump(armCore.GetRegValue(Rm));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int Encode()
        {
            if (!Linked)
                throw new Exception("Cannot encode an unlinked command");

            var bw = new BitWriter();

            bw.WriteBits((int)Condition, 28, 4); // Condition

            switch (JumpType)
            {
                case EJump.BranchLink:
                    bw.WriteBits(1, 24, 1);
                    goto case EJump.Branch;
                case EJump.Branch:
                    bw.WriteBits(1, 27, 1);
                    bw.WriteBits(1, 25, 1);
                    bw.WriteBits(Offset, 0, 24);
                    break;
                case EJump.BranchExchange:
                    bw.WriteBits(1, 24, 1);
                    bw.WriteBits(1, 21, 1);
                    bw.WriteBits(1, 4, 1);
                    bw.WriteBits((int)Rm, 0, 4);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return bw.GetValue();
        }

        public void Link(Dictionary<string, int> commandTable, Dictionary<string, int> dataTable, int commandOffset)
        {
            if (Linked)
                return;

            if (!commandTable.ContainsKey(Label))
                throw new Exception("Invalid Label: " + Label);

            Offset = commandTable[Label] - commandOffset - 2;
            Linked = true;
        }
    }
}
