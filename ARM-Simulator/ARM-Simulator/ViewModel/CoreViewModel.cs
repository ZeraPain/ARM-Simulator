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
    public class CoreViewModel
    {
        public ObservableCollection<ObservableCommand> CommandList { get; set; }

        public Core Core { get; protected set; }

        public bool DisplayFetch { get; set; }
        public bool DisplayDecode { get; set; }
        public bool DisplayExecute { get; set; }

        public ICommand RemoveBreakpointsCommand { get; protected set; }

        public CoreViewModel(Core core)
        {
            Core = core;
            Core.PropertyChanged += Update;
            CommandList = new ObservableCollection<ObservableCommand>();

            DisplayFetch = true;
            DisplayDecode = true;
            DisplayExecute = true;

            RemoveBreakpointsCommand = new DelegateCommand(RemoveBreakpoints);
        }

        public void Update(object sender, [CanBeNull] PropertyChangedEventArgs e)
        {
            var property = e?.PropertyName;

            switch (property)
            {
                case nameof(Core.PipelineStatus):
                    UpdatePipelineStatus();
                    break;
                case nameof(Core.RegisterList):
                    //UpdateRegisterList();
                    break;
                default:
                    UpdatePipelineStatus();
                    UpdateRegisterList();
                    break;
            }
        }

        private void UpdateRegisterList()
        {
            foreach (var register in Core.RegisterList)
                register.Value = register.Value;
        }

        private void UpdatePipelineStatus()
        {
            foreach (var t in CommandList)
                t.Status = EPipeline.None;

            var status = Core.PipelineStatus;
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
        }

        public bool IsBreakPoint() => CommandList.Any(command => command.Address == Core.PipelineStatus[EPipeline.Execute] && command.Breakpoint);

        private void RemoveBreakpoints(object parameter)
        {
            foreach (var t in CommandList)
                t.Breakpoint = false;
        }
    }
}
