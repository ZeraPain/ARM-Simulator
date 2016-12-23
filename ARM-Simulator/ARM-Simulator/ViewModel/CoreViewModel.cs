using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ARM_Simulator.Annotations;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;

namespace ARM_Simulator.ViewModel
{
    public class CoreViewModel : ViewModelBase
    {
        public class Register
        {
            public ERegister Name { get; set; }
            public string Value { get; set; }
        }

        public ObservableCollection<Command> CommandList { get; set; }
        public ObservableCollection<Register> RegisterList { get; set; }
        public Core ArmCore { get; set; }

        public CoreViewModel(Core core)
        {
            ArmCore = core;
            ArmCore.PropertyChanged += Update;
            RegisterList = new ObservableCollection<Register>();
            CommandList = new ObservableCollection<Command>();

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
            RegisterList.Clear();
            foreach (var register in ArmCore.Registers)
                RegisterList.Add(new Register() { Name = register.Key, Value = register.Value.ToString() });
        }

        private void UpdatePipelineStatus()
        {
            var commanListCopy = new List<Command>(CommandList);
            foreach (var x in commanListCopy) x.Status = EPipeline.None;

            var status = ArmCore.PipelineStatus;
            foreach (var x in status)
            {
                if (x.Value < 0)
                    continue;

                var index = x.Value / 4;
                if (index >= commanListCopy.Count)
                    continue;

                commanListCopy[index].Status = x.Key;
            }

            UpdateList(commanListCopy);
        }

        public void UpdateList([NotNull] IEnumerable<Command> cmdList)
        {
            CommandList.Clear();
            foreach (var x in cmdList)
                CommandList.Add(x);
        }

        public void ToggleBreakPoint(object parameter)
        {
            var index = parameter as int?;

            if ((index >= 0) && (index < CommandList.Count))
                CommandList[(int)index].Breakpoint = !CommandList[(int)index].Breakpoint;
        }
    }
}
