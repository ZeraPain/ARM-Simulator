﻿using System;
using System.Collections.Generic;
using ARM_Simulator.Enumerations;

namespace ARM_Simulator.Model
{
    public class ArmCore
    {
        private readonly Dictionary<ArmRegister, int> _registers;
        private readonly ArmPipeline _pipeline;

        public ArmCore()
        {
            _registers = new Dictionary<ArmRegister, int>
            {
                {ArmRegister.R0, 0},
                {ArmRegister.R1, 0},
                {ArmRegister.R2, 0},
                {ArmRegister.R3, 0},
                {ArmRegister.R4, 0},
                {ArmRegister.R5, 0},
                {ArmRegister.R6, 0},
                {ArmRegister.R7, 0},
                {ArmRegister.R8, 0},
                {ArmRegister.R9, 0},
                {ArmRegister.R10, 0},
                {ArmRegister.R11, 0},
                {ArmRegister.Cpsr, 0x13 }
            };

            _pipeline = new ArmPipeline();
        }

        public void SetRegValue(ArmRegister reg, int value)
        {
            if (!_registers.ContainsKey(reg))
                throw new Exception("Invalid Register was requested");

            _registers[reg] = value;
        }

        public int GetRegValue(ArmRegister reg)
        {
            if (!_registers.ContainsKey(reg))
                throw new Exception("Invalid Register was requested");

            return _registers[reg];
        }

        public void SetNzcvFlags(Flags mask, Flags flags)
        {
            var state = GetRegValue(ArmRegister.Cpsr);
            state &= ~(mask.Value << 28); // Clear affected status bits
            state |= flags.Value << 28; // Set affected status bits

            SetRegValue(ArmRegister.Cpsr, state);
        }

        public void Tick(string fetchCommand)
        {
            _pipeline.Tick(fetchCommand)?.Execute(this);
        }

        public void DebugCommand()
        {

        }

        // Only needed for Unit tests!
        public void DirectExecute(string command)
        {
            _pipeline.ForceDecode(command).Execute(this);
        }
    }
}
