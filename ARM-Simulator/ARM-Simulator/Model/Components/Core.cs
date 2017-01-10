using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ARM_Simulator.Annotations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Resources;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.Model.Components
{
    public class Core : INotifyPropertyChanged
    {
        public Dictionary<EPipeline, int> PipelineStatus { get; protected set; }
        public ObservableCollection<ObservableRegister> RegisterList { get; protected set; }

        internal Memory Ram { get; }

        private int? _fetch;
        private bool _jump;
        private ICommand _decode;
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

            RegisterList = new ObservableCollection<ObservableRegister>()
            {
                new ObservableRegister(ERegister.R0, 0),
                new ObservableRegister(ERegister.R1, 0),
                new ObservableRegister(ERegister.R2, 0),
                new ObservableRegister(ERegister.R3, 0),
                new ObservableRegister(ERegister.R4, 0),
                new ObservableRegister(ERegister.R5, 0),
                new ObservableRegister(ERegister.R6, 0),
                new ObservableRegister(ERegister.R7, 0),
                new ObservableRegister(ERegister.R8, 0),
                new ObservableRegister(ERegister.R9, 0),
                new ObservableRegister(ERegister.R10, 0),
                new ObservableRegister(ERegister.R11, 0),
                new ObservableRegister(ERegister.R12, 0),
                new ObservableRegister(ERegister.Sp, Ram.GetRamSize()),
                new ObservableRegister(ERegister.Lr, 0),
                new ObservableRegister(ERegister.Pc, 0),
                new ObservableRegister(ERegister.Cpsr, 0x13)
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
            RegisterList[(int) reg].Value = value;
            OnPropertyChanged(nameof(RegisterList));
        }

        public void SetEntryPoint(int address)
        {
            if (address < 0) throw new ArgumentException("Cannot find entry point (main function)");

            SetRegValue(ERegister.Pc, address);
            SetPipelineStatus(EPipeline.Fetch, address);
        }

        private void SetPipelineStatus(EPipeline status, int value)
        {
            PipelineStatus[status] = value;
            OnPropertyChanged(nameof(PipelineStatus));
        }

        public void Jump(int address)
        {
            SetRegValue(ERegister.Pc, address);

            SetPipelineStatus(EPipeline.Fetch, -1);
            SetPipelineStatus(EPipeline.Decode, -1);
            SetPipelineStatus(EPipeline.Execute, -1);

            _jump = true;
        }

        public int GetRegValue(ERegister? reg)
        {
            if (reg == null)
                throw new Exception("Invalid Register was requested");

            return RegisterList[(int) reg].Value;
        }

        public void SetNzcvFlags(Flags mask, Flags flags)
        {
            var value = GetRegValue(ERegister.Cpsr);
            value &= ~(mask.Value << 28); // Clear affected status bits
            value |= flags.Value << 28; // Set affected status bits
            SetRegValue(ERegister.Cpsr, value);
        }

        public bool Tick()
        {
            try
            {
                // Pipeline
                var fetch = Ram.ReadInt((uint) GetRegValue(ERegister.Pc)); // Fetch
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

                SetPipelineStatus(EPipeline.Execute, PipelineStatus[EPipeline.Decode]);
                SetPipelineStatus(EPipeline.Decode, PipelineStatus[EPipeline.Fetch]);
                SetPipelineStatus(EPipeline.Fetch, GetRegValue(ERegister.Pc));
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
                return false;
            }
        }

        // Used for Unit tests, skipped pipeline
        public void TestCommand([NotNull] ICommand command)
        {
            var fetch = BitConverter.ToInt32(command.Encode(), 0);
            var cmd = _decoder.Decode(fetch);
            cmd?.Execute(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
