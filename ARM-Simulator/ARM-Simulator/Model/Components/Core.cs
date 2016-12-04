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
        private readonly Memory _ram;

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
                {Register.Sp, 0},
                {Register.Pc, 0},
                {Register.Cpsr, 0x13 }
            };

            _decoder = new Decoder();
            _ram = ram;
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
            Execute();
            Decode();
            Fetch();

            SetRegValue(Register.Pc, GetRegValue(Register.Pc) + 4);
        }

        private void Fetch()
        {
            _fetch = _ram.ReadInt((uint)(GetRegValue(Register.Pc) + 8));
        }

        private void Decode()
        {
            _decode = _decoder.Decode(_fetch);
        }

        private void Execute()
        {
            _decode?.Execute(this);
        }

        // Necessary for Unit tests
        public void TestCommand(ICommand command)
        {
            var cmd = _decoder.Decode(command.Encode());
            cmd.Execute(this);
        }
    }
}
