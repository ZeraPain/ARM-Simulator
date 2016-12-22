using System.Collections.Generic;
using System.Linq;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;

namespace ARM_Simulator.ViewModel
{
    public class CoreViewModel
    {
        public List<Command> CommandList { get; set; }
        private readonly Core _core;

        public CoreViewModel(Core core)
        {
            _core = core;
        }

        public void Update()
        {
            foreach (var x in CommandList) x.Status = EPipeline.None;

            var status = _core.PipelineStatus;
            foreach (var x in status)
            {
                if (x.Value < 0)
                    continue;

                var index = x.Value / 4;
                if (index >= CommandList.Count)
                    continue;

                CommandList[index].Status = x.Key;
            }
        }

        public void ToggleBreakPoint(int index)
        {
            if (index >= 0 && index < CommandList.Count)
            {
                CommandList[index].Breakpoint = !CommandList[index].Breakpoint;
            }
        }

        public bool IsBreakPoint()
        {
            var pogramCounter = _core.PipelineStatus[EPipeline.Execute];
            return CommandList.Where((t, i) => t.Breakpoint && i*4 == pogramCounter).Any();
        }
    }
}
