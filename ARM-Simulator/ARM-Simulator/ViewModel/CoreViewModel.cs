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
        // This list contains all Commands including pipeline state, breakpoint and address to display in the listview
        public ObservableCollection<ObservableCommand> CommandList { get; protected set; }

        // Reference to the arm core
        public Core Core { get; protected set; }

        // Settable options rather to display a certain pipeline state or not
        public bool DisplayFetch { get; set; }
        public bool DisplayDecode { get; set; }
        public bool DisplayExecute { get; set; }

        // Command to remove all existing breakpoints
        public ICommand RemoveBreakpointsCommand { get; protected set; }

        public CoreViewModel(Core core)
        {
            Core = core;
            CommandList = new ObservableCollection<ObservableCommand>();
            RemoveBreakpointsCommand = new DelegateCommand(RemoveBreakpoints);

            // Subscribe to the inotifyproperty changed to update the registers / pipeline status on changes made by the core
            Core.PropertyChanged += Update;        

            // default values for visualize pipelining 
            DisplayFetch = true;
            DisplayDecode = true;
            DisplayExecute = true;
        }

        // This method is used to update the current display state of registers and/or pipeline status
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
            // force complete refresh of bindings which is used to change the numberic display type by the registervalueconverter
            foreach (var register in Core.RegisterList)
                register.Value = register.Value;
        }

        private void UpdatePipelineStatus()
        {
            // clear all pipeline states
            foreach (var t in CommandList)
                t.Status = EPipeline.None;

            // get the current pipeline state and set them if required
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
            // set the pipeline state on a certain address
            foreach (var command in CommandList.Where(command => command.Address == address))
                command.Status = status;
        }

        public void UpdateList([NotNull] List<ObservableCommand> cmdList)
        {
            // set the current commandlist to be displayed in the listview
            CommandList.Clear();
            foreach (var x in cmdList)
                CommandList.Add(x);
        }

        public void ToggleBreakPoint(uint address)
        {
            // activate or deactivate a breakpoint at a certain address
            foreach (var command in CommandList.Where(command => command.Address == address))
                command.Breakpoint = !command.Breakpoint;
        }

        // returns true if the current command is used as a breakpoint (during the execute state)
        public bool IsBreakPoint() => CommandList.Any(command => command.Address == Core.PipelineStatus[EPipeline.Execute] && command.Breakpoint);

        private void RemoveBreakpoints(object parameter)
        {
            // remove all existing breakpoints
            foreach (var t in CommandList)
                t.Breakpoint = false;
        }
    }
}
