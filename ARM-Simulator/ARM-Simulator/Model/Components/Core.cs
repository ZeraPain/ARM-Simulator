using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ARM_Simulator.Annotations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Resources;

namespace ARM_Simulator.Model.Components
{
    public class Core : INotifyPropertyChanged
    {
        public Dictionary<EPipeline, int> PipelineStatus { get; protected set; }
        public Dictionary<ERegister, int> Registers { get; protected set; }
        
        internal Memory Ram { get; }

        private int? _fetch;
        private bool _jump;
        private ICommand _decode;

        private int _cpsr;
        private readonly Decoder _decoder;

        public Core(Memory ram)
        {
            _decoder = new Decoder();
            Ram = ram;
            Reset();
        }

        public void Reset()
        {
            _fetch = null;
            _decode = null;
            _jump = false;
            _cpsr = 0x13;

            Registers = new Dictionary<ERegister, int>
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
                {ERegister.Sp, Ram.GetRamSize()},
                {ERegister.Lr, 0},
                {ERegister.Pc, 0}
            };

            PipelineStatus = new Dictionary<EPipeline, int>
            {
                {EPipeline.Fetch, -1},
                {EPipeline.Decode, -1},
                {EPipeline.Execute, -1}
            };
        }

        public void SetRegValue(ERegister reg, int value)
        {
            if (!Registers.ContainsKey(reg))
                throw new Exception("Invalid Register was requested");

            Registers[reg] = value;
            OnPropertyChanged(nameof(Registers));
        }

        public void SetEntryPoint(int address)
        {
            Registers[ERegister.Pc] = address;
            PipelineStatus[EPipeline.Fetch] = Registers[ERegister.Pc];
        }

        public void Jump(int address)
        {
            Registers[ERegister.Pc] = address;
            _jump = true;
        }

        public int GetRegValue(ERegister? reg)
        {
            if ((reg == null) || !Registers.ContainsKey((ERegister)reg))
                throw new Exception("Invalid Register was requested");

            return Registers[(ERegister)reg];
        }

        public void SetNzcvFlags(Flags mask, Flags flags)
        {
            _cpsr &= ~(mask.Value << 28); // Clear affected status bits
            _cpsr |= flags.Value << 28; // Set affected status bits
        }

        public int GetCpsr() => _cpsr;

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
                Registers[ERegister.Pc] += 0x4;
            }
            else
            {
                _fetch = null;
                _decode = null;
                _jump = false;
            }

            PipelineStatus[EPipeline.Execute] = PipelineStatus[EPipeline.Decode];
            PipelineStatus[EPipeline.Decode] = PipelineStatus[EPipeline.Fetch];
            PipelineStatus[EPipeline.Fetch] = Registers[ERegister.Pc];
            OnPropertyChanged(nameof(PipelineStatus));
        }

        // Used for Unit tests, skipped pipeline
        public void TestCommand([NotNull] ICommand command)
        {
            var cmd = _decoder.Decode(command.Encode());
            cmd?.Execute(this);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
