using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using ARM_Simulator.Annotations;
using ARM_Simulator.Commands;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.ViewModel
{
    public class CoreViewModel : ContextMenuHandler
    {
        public ObservableCollection<ObservableCommand> CommandList { get; set; }
        public ObservableCollection<ObservableRegister> RegisterList { get; set; }
        public ObservableCollection<ObservableCommand> BreakpointCommandList { get; set; }
        public Core ArmCore { get; set; }

        public bool DisplayFetch { get; set; }
        public bool DisplayDecode { get; set; }
        public bool DisplayExecute { get; set; }

        public ICommand RemoveBreakpointsCommand { get; protected set; }

        public CoreViewModel(Core core)
        {
            ArmCore = core;
            ArmCore.PropertyChanged += Update;
            RegisterList = new ObservableCollection<ObservableRegister>();
            CommandList = new ObservableCollection<ObservableCommand>();
            BreakpointCommandList = new ObservableCollection<ObservableCommand>();

            DisplayFetch = true;
            DisplayDecode = true;
            DisplayExecute = true;

            ContextMenuUpdate = UpdateRegisterList;

            RemoveBreakpointsCommand = new DelegateCommand(RemoveBreakpoints);

            InitRegisterList();
        }

        private void Update(object sender, [NotNull] PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Registers")
                UpdateRegisterList();
            else if (e.PropertyName == "PipelineStatus")
                UpdatePipelineStatus();
        }

        public void UpdateShowBreakpoints()
        {
            if (BreakpointCommandList.Any())
                BreakpointCommandList.Clear();

            if (CommandList == null) return;

            foreach (var command in CommandList.Where(command => command.Breakpoint))
                BreakpointCommandList.Add(command);
        }

        private void InitRegisterList()
        {
            for (var i = 0; i < ArmCore.Registers.Count; i++)
            {
                var register = ArmCore.Registers.ElementAt(i);
                RegisterList.Add(new ObservableRegister { Name = register.Key.ToString(), Value = register.Value.ToString() });
            }

            RegisterList.Add(new ObservableRegister() { Name = "CPSR", Value = ArmCore.Cpsr.ToString()});
        }

        private void UpdateRegisterList()
        {
            for (var i = 0; i < ArmCore.Registers.Count; i++)
            {
                var register = ArmCore.Registers.ElementAt(i);
                var value = register.Value;
                if (ShowAsByte) value = (byte) value;

                var valueString = value.ToString();
                if (ShowAsUnsigned) valueString = ((uint)value).ToString();
                if (ShowAsHexadecimal) valueString = "0x" + value.ToString("X");

                if (RegisterList.Any(reg => reg.Name == register.Key.ToString()))
                    RegisterList[i].Value = valueString;
                else
                    RegisterList.Add(new ObservableRegister { Name = register.Key.ToString(), Value = valueString });
            }

            foreach (var register in RegisterList)
            {
                if (register.Name == "CPSR")
                {
                    var value = ArmCore.Cpsr;
                    if (ShowAsByte) value = (byte)value;

                    var valueString = value.ToString();
                    if (ShowAsUnsigned) valueString = ((uint)value).ToString();
                    if (ShowAsHexadecimal) valueString = "0x" + value.ToString("X");

                    register.Value = valueString;
                }
            }
        }

        private void UpdatePipelineStatus()
        {
            foreach (var t in CommandList)
                t.Status = EPipeline.None;

            var status = ArmCore.PipelineStatus;
            foreach (var x in status.Where(x => x.Value >= 0))
            {
                switch (x.Key)
                {
                    case EPipeline.Fetch:
                        if (DisplayFetch) SetPipelineStatus(x.Key, x.Value);
                        break;
                    case EPipeline.Decode:
                        if (DisplayDecode) SetPipelineStatus(x.Key, x.Value);
                        break;
                    case EPipeline.Execute:
                        if (DisplayExecute) SetPipelineStatus(x.Key, x.Value);
                        break;
                }
            }
        }

        private void SetPipelineStatus(EPipeline status, int address)
        {
            foreach (var command in CommandList.Where(command => command.Address == address))
                command.Status = status;
        }

        public void UpdateList([NotNull] List<ObservableCommand> cmdList)
        {
            CommandList.Clear();
            foreach (var x in cmdList)
                CommandList.Add(x);
        }

        public void ToggleBreakPoint(uint address)
        {
            foreach (var command in CommandList.Where(command => command.Address == address))
                command.Breakpoint = !command.Breakpoint;

            UpdateShowBreakpoints();
        }

        public bool IsBreakPoint() => CommandList.Any(command => command.Address == ArmCore.PipelineStatus[EPipeline.Execute] && command.Breakpoint);

        private void RemoveBreakpoints(object parameter)
        {
           BreakpointCommandList.Clear();
            foreach (var t in CommandList)
                t.Breakpoint = false;
        }
    }
}
