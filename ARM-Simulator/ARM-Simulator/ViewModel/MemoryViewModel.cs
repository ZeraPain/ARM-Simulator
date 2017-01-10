using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ARM_Simulator.Annotations;
using ARM_Simulator.Model.Components;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.ViewModel
{
    internal class MemoryViewModel
    {
        public ObservableCollection<ObservableMemoryStream> MemoryView { get; protected set; }
        private readonly Memory _memory;

        public MemoryViewModel(Memory memory)
        {
            _memory = memory;
            _memory.PropertyChanged += Update;
            _memory.AllowUnsafeCode = true;

            MemoryView = new ObservableCollection<ObservableMemoryStream>();
        }

        public void Update(object sender, [CanBeNull] PropertyChangedEventArgs e)
        {
            var property = e?.PropertyName;

            switch (property)
            {
                case nameof(_memory.Ram):
                    UpdateMemoryView();
                    break;
                default:
                    UpdateMemoryView();
                    break;
            }
        }

        public void UpdateMemoryView()
        {
            var data = _memory.Ram;

            for (var i = 0; i < data.Length/32; i++)
            {
                var baseAddress = (uint)i*32;
                var memoryBytes = new byte[32];
                Array.Copy(data, baseAddress, memoryBytes, 0, memoryBytes.Length);

                if (MemoryView.Count <= i)
                {
                    MemoryView.Add(new ObservableMemoryStream() { BaseAddress = baseAddress, MemoryBytes = memoryBytes });
                }
                else
                {
                    MemoryView[i].BaseAddress = baseAddress;
                    MemoryView[i].MemoryBytes = memoryBytes;
                }
            }
        }
    }
}
