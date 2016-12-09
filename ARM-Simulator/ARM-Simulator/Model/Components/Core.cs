﻿using System;
using System.Collections.Generic;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Components
{
    public class Core
    {
        private readonly Dictionary<ERegister, int> _registers;
        private readonly Decoder _decoder;
        internal Memory Ram { get; }

        private int? _fetch;
        private bool _jump;
        private ICommand _decode;

        private int _cpsr;

        public Core(Memory ram)
        {
            _registers = new Dictionary<ERegister, int>
            {
                {ERegister.R0, 0},
                {ERegister.R1, 0},
                {ERegister.R2, 0},
                {ERegister.R3, 0},
                {ERegister.R4, 0},
                {ERegister.R5, 0},
                {ERegister.R6, 0},
                {ERegister.R7, 0},
                {ERegister.R8, 0},
                {ERegister.R9, 0},
                {ERegister.R10, 0},
                {ERegister.R11, 0},
                {ERegister.R12, 0},
                {ERegister.Sp, ram.GetRamSize()},
                {ERegister.Lr, 0},
                {ERegister.Pc, 0}
            };

            _cpsr = 0x13;
            _jump = false;
            _decoder = new Decoder();
            Ram = ram;
        }

        public void SetRegValue(ERegister reg, int value)
        {
            if (!_registers.ContainsKey(reg))
                throw new Exception("Invalid Register was requested");

            if (reg == ERegister.Pc && value != GetRegValue(ERegister.Pc) + 0x4) 
                _jump = true;

            _registers[reg] = value;
        }

        public int GetRegValue(ERegister? reg)
        {
            if (reg == null || !_registers.ContainsKey((ERegister)reg))
                throw new Exception("Invalid Register was requested");

            return _registers[(ERegister)reg];
        }

        public void SetNzcvFlags(Flags mask, Flags flags)
        {
            _cpsr &= ~(mask.Value << 28); // Clear affected status bits
            _cpsr |= flags.Value << 28; // Set affected status bits
        }

        public int GetCpsr()
        {
            return _cpsr;
        }

        public void Tick()
        {
            // Pipeline
            var fetch = Ram.ReadInt((uint)GetRegValue(ERegister.Pc)); // Fetch
            var decode = _decoder.Decode(_fetch); // Decode
            _decode?.Execute(this); // Execute

            if (!_jump)
            {
                _fetch = fetch;
                _decode = decode;
                SetRegValue(ERegister.Pc, GetRegValue(ERegister.Pc) + 0x4);
            }
            else
            {
                _fetch = null;
                _decode = null;
                _jump = false;
            }
        }

        // Used for Unit tests, skipped pipeline
        public void TestCommand(ICommand command)
        {
            var cmd = _decoder.Decode(command.Encode());
            cmd?.Execute(this);
        }
    }
}
