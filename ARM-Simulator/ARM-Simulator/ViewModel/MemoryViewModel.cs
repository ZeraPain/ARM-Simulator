using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ARM_Simulator.Annotations;
using ARM_Simulator.Model.Components;

namespace ARM_Simulator.ViewModel
{
    public class MemoryViewModel
    {
        public class MemoryStream
        {
            public string BaseAddress { get; set; }
            public string MemoryString { get; set; }
        }

        public ObservableCollection<MemoryStream> MemoryView { get; protected set; }

        private readonly Memory _memory;

        public MemoryViewModel(Memory memory)
        {
            _memory = memory;
            _memory.PropertyChanged += Update;
            MemoryView = new ObservableCollection<MemoryStream>();
        }

        private void Update(object sender, [NotNull] PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Ram":
                    UpdateMemoryView();
                    break;
            }
        }

        private void UpdateMemoryView()
        {
            MemoryView.Clear();
            var data = _memory.Ram;

            for (var i = 0; i < data.Length / 32; i++)
            {
                var baseAddr = i*32;
                var datarow = new byte[32];
                Array.Copy(data, baseAddr, datarow, 0, 32);
                MemoryView.Add(new MemoryStream
                {
                    BaseAddress = baseAddr.ToString("X8"),
                    MemoryString = BitConverter.ToString(datarow).Replace("-", " ")
                });
            }
        }
    }
}
