namespace ARM_Simulator.Resources
{
    public class Command
    {
        public EPipeline Status { get; set; }
        public bool Breakpoint { get; set; }
        public string Label { get; set; }
        public string Commandline { get; set; }
    }

    public struct Flags
    {
        public byte Value { get; }

        public Flags(bool n, bool z, bool c, bool v)
        {
            Value = 0;
            if (n) Value |= 1 << 3;
            if (z) Value |= 1 << 2;
            if (c) Value |= 1 << 1;
            if (v) Value |= 1 << 0;
        }
    }

    public enum EPipeline
    {
        None,
        Fetch,
        Decode,
        Execute    
    }

    public enum EOperand2
    {
        None,
        RotateImmediate,
        ImmediateShiftRm,
        RsShiftRm
    }

    public enum EOffset
    {
        None,
        Immediate,
        ImmediateShiftRm
    }

    public enum ESize
    {
        Word = 0,
        Byte = 1
    }

    public enum EJump
    {
        Branch = 0,
        BranchLink = 1,
        BranchExchange = 2
    }

    public enum EOpcode
    {
        And = 0,
        Eor = 1,
        Sub = 2,
        Rsb = 3,
        Add = 4,
        Tst = 8,
        Teq = 9,
        Cmp = 10,
        Cmn = 11,
        Orr = 12,
        Mov = 13,
        Bic = 14,
        Mvn = 15
    }

    public enum EMultiplication
    {
        None,
        Mul,
        Mla,
        Smlal,
        Smull,
        Umlal,
        UMull,
    }

    public enum EShiftInstruction
    {
        Lsl = 0,
        Lsr = 1,
        Ror = 2,
        Asr = 3
    }

    public enum ERegister
    {
        R0 = 0,
        R1 = 1,
        R2 = 2,
        R3 = 3,
        R4 = 4,
        R5 = 5,
        R6 = 6,
        R7 = 7,
        R8 = 8,
        R9 = 9,
        R10 = 10,
        R11 = 11,
        R12 = 12,
        Lr = 13,
        Sp = 14,
        Pc = 15
    }

    public enum ECondition
    {
        Equal = 0,
        NotEqual = 1,
        CarrySet = 2,
        CarryClear = 3,
        Minus = 4,
        Plus = 5,
        OverflowSet = 6,
        OverflowClear = 7,
        Higher = 8,
        LowerOrSame = 9,
        GreaterEqual = 10,
        LessThan = 11,
        GreaterThan = 12,
        LessEqual = 13,
        Always = 14
    }
}
