using System;
using System.Collections.Generic;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Components
{
    public class Core
    {
        private readonly Dictionary<Register, int> _registers;
        private readonly Decoder _decoder;
        internal Memory Ram { get; }

        private int _fetch;
        private ICommand _decode;

        public Core(Memory ram)
        {
            _registers = new Dictionary<Register, int>
            {
                {Register.R0, 0},
                {Register.R1, 0},
                {Register.R2, 0},
                {Register.R3, 0},
                {Register.R4, 0},
                {Register.R5, 0},
                {Register.R6, 0},
                {Register.R7, 0},
                {Register.R8, 0},
                {Register.R9, 0},
                {Register.R10, 0},
                {Register.R11, 0},
                {Register.R12, 0},
                {Register.Lr, 0},
                {Register.Sp, ram.GetRamSize()},
                {Register.Pc, 0},
                {Register.Cpsr, 0x13 }
            };

            _decoder = new Decoder();
            Ram = ram;
        }

        public void SetRegValue(Register? reg, int value)
        {
            if (reg == null || !_registers.ContainsKey((Register)reg))
                throw new Exception("Invalid Register was requested");

            _registers[(Register)reg] = value;
        }

        public int GetRegValue(Register? reg)
        {
            if (reg == null || !_registers.ContainsKey((Register)reg))
                throw new Exception("Invalid Register was requested");

            return _registers[(Register)reg];
        }

        public void SetNzcvFlags(Flags mask, Flags flags)
        {
            var state = GetRegValue(Register.Cpsr);
            state &= ~(mask.Value << 28); // Clear affected status bits
            state |= flags.Value << 28; // Set affected status bits

            SetRegValue(Register.Cpsr, state);
        }

        public void Tick()
        {
            // Pipeline
            _decode?.Execute(this); // Execute
            _decode = _decoder.Decode(_fetch); // Decode
            _fetch = Ram.ReadInt((uint)(GetRegValue(Register.Pc) + 0x8)); // Fetch

            SetRegValue(Register.Pc, GetRegValue(Register.Pc) + 0x4);
        }


        // Used for Unit tests, skipped pipeline
        public void TestCommand(ICommand command)
        {
            var cmd = _decoder.Decode(command.Encode());
            cmd.Execute(this);
        }
    }
}
