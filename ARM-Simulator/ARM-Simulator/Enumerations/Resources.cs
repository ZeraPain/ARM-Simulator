namespace ARM_Simulator.Enumerations
{
    public struct Command
    {
        public Opcode? Opcode { get; }
        public bool SetConditionFlags { get; }
        public string[] Parameters { get; }

        public Command(Opcode? opcode, bool setConditionFlags, string[] parameters)
        {
            Opcode = opcode;
            SetConditionFlags = setConditionFlags;
            Parameters = parameters;
        }
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

    public enum Opcode
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

    public enum ShiftInstruction
    {
        Lsl = 0,
        Lsr = 1,
        Ror = 2,
        Asr = 3
    }

    public enum Register
    {
        R0,
        R1,
        R2,
        R3,
        R4,
        R5,
        R6,
        R7,
        R8,
        R9,
        R10,
        R11,
        Cpsr
    }
}
