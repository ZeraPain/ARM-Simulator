using System;
using System.Collections.Generic;
using ARM_Simulator.Model.Components;

namespace ARM_Simulator.ViewModel
{
    public class MemoryViewModel
    {
        public Dictionary<string, string> MemoryView { get; protected set; }
        private readonly Memory _memory;

        public MemoryViewModel(Memory memory)
        {
            MemoryView = new Dictionary<string, string>();
            _memory = memory;
            Update();
        }

        public void Update()
        {
            MemoryView.Clear();
            var data = _memory.Read(0x0, _memory.GetRamSize());

            for (var i = 0; i < data.Length / 32; i++)
            {
                var baseAddr = i*32;
                var datarow = new byte[32];
                Array.Copy(data, baseAddr, datarow, 0, 32);
                MemoryView.Add(baseAddr.ToString("X8"), BitConverter.ToString(datarow).Replace("-", " "));
            }
        }
    }
}
