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

    internal enum ArmOpCode
    {
        None,
        Mov,
        Add
    }

    public enum ShiftInstruction
    {
        None,
        Lsr,
        Lsl,
        Ror,
        Rol
    }

    public enum ArmRegister
    {
        None, R0, R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11
    }

}
