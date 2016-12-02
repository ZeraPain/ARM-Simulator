namespace ARM_Simulator.Enumerations
{
    internal struct Command
    {
        public ArmOpCode Opcode { get; }
        public string[] Parameters { get; }

        public Command(ArmOpCode opcode, string[] parameters)
        {
            Opcode = opcode;
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

    internal enum ArmOpCode
    {
        Nop,
        Mov, Movs, Mvn, Mvns,
        Add, Adds,
        Sub, Subs, Rsb, Rsbs,
        And, Ands, Eor, Eors, Orr, Orrs, Orn, Orns, Bic, Bics,
        Lsl, Lsls, Lsr, Lsrs, Ror, Rors, Asr, Asrs,
        Tst, Teq,
        Cmp, Cmn
    }

    public enum ShiftInstruction
    {
        None, Lsl, Lsr, Ror, Asr
    }

    public enum ArmRegister
    {
        None, R0, R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11,
        Cpsr
    }

}
