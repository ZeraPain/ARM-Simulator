using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ARM_Simulator.Annotations;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.ViewModel
{
    public class CoreViewModel
    {
        public ObservableCollection<ObservableCommand> CommandList { get; set; }
        public ObservableCollection<ObservableRegister> RegisterList { get; set; }
        public Core ArmCore { get; set; }

        public bool DisplayFetch { get; set; }
        public bool DisplayDecode { get; set; }
        public bool DisplayExecute { get; set; }

        public CoreViewModel(Core core)
        {
            ArmCore = core;
            ArmCore.PropertyChanged += Update;
            RegisterList = new ObservableCollection<ObservableRegister>();
            CommandList = new ObservableCollection<ObservableCommand>();

            DisplayFetch = true;
            DisplayDecode = true;
            DisplayExecute = true;

            UpdateRegisterList();
        }

        private void Update(object sender, [NotNull] PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Registers")
            {
                UpdateRegisterList();
            }
            else if (e.PropertyName == "PipelineStatus")
            {
                UpdatePipelineStatus();
            }
        }

        private void UpdateRegisterList()
        {
            for (var i = 0; i < ArmCore.Registers.Count; i++)
            {
                var register = ArmCore.Registers.ElementAt(i);
                if (RegisterList.Any(reg => reg.Name == register.Key))
                    RegisterList[i].Value = register.Value.ToString();
                else
                    RegisterList.Add(new ObservableRegister { Name = register.Key, Value = register.Value.ToString() });
            }
        }

        private void UpdatePipelineStatus()
        {
            foreach (var t in CommandList)
                t.Status = EPipeline.None;

            var status = ArmCore.PipelineStatus;
            foreach (var x in status)
            {
                if (x.Value < 0)
                    continue;

                var index = x.Value / 4;
                if (index >= CommandList.Count)
                    continue;

                if ((x.Key == EPipeline.Fetch) && DisplayFetch) CommandList[index].Status = EPipeline.Fetch;
                if ((x.Key == EPipeline.Decode) && DisplayDecode) CommandList[index].Status = EPipeline.Decode;
                if ((x.Key == EPipeline.Execute) && DisplayExecute) CommandList[index].Status = EPipeline.Execute;
            }
        }

        public void UpdateList([NotNull] IEnumerable<Command> cmdList)
        {
            CommandList.Clear();
            foreach (var x in cmdList)
                CommandList.Add(new ObservableCommand()
                {
                    Breakpoint = x.Breakpoint,
                    Commandline = x.Commandline,
                    Label = x.Label,
                    Status = x.Status
                });
        }

        public void ToggleBreakPoint(object parameter)
        {
            var index = parameter as int?;

            if ((index >= 0) && (index < CommandList.Count))
                CommandList[(int)index].Breakpoint = !CommandList[(int)index].Breakpoint;
        }
    }
}
