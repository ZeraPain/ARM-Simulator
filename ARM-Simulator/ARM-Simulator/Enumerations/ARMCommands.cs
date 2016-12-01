using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARM_Simulator.Enumerations
{
    struct MOV
    {
        public Register DestReg { get; }
        public Register SrcReg { get; }
        public int Immediate { get; }
        public ShiftInstruction ShiftInst { get; }
        public byte ShiftCount { get; }

        public MOV(Register destReg, int immediate)
        {
            DestReg = destReg;
            SrcReg = Register.None;
            Immediate = immediate;
            ShiftInst = ShiftInstruction.None;
            ShiftCount = 0;
        }

        public MOV(Register destReg, Register srcReg, ShiftInstruction shiftInst, byte shiftCount)
        {
            DestReg = destReg;
            SrcReg = srcReg;
            Immediate = 0;
            ShiftInst = shiftInst;
            ShiftCount = shiftCount;
        }

    }

    enum ARMCommands
    {
        None,
        MOV

    }

    enum ShiftInstruction
    {
        None,
        LSR,
        LSL,
        ROR,
        ROL
    }
}
