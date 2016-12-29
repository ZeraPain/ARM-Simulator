using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ARM_Simulator.Annotations;
using ARM_Simulator.Commands;
using ARM_Simulator.Model;
using ARM_Simulator.Model.Components;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.ViewModel
{
    internal class MemoryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ObservableMemoryStream> MemoryView { get; protected set; }
        private readonly Memory _memory;

        private string _file;
        public string File
        {
            get { return _file; }
            set
            {
                if (_file == value) return;
                _file = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand UnsafeCommand { get; protected set; }

        public MemoryViewModel(Memory memory)
        {
            _memory = memory;
            _memory.PropertyChanged += Update;

            MemoryView = new ObservableCollection<ObservableMemoryStream>();

           UnsafeCommand = new DelegateCommand(Unsafe);
        }

        private void Unsafe(object parameter)
        {
            //TODO
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
            var data = _memory.Ram;

            for (var i = 0; i < data.Length / 32; i++)
            {
                var baseAddr = i*32;
                var memoryDataBytes = new byte[32];
                Array.Copy(data, baseAddr, memoryDataBytes, 0, 32);

                var baseAddress = "0x" + baseAddr.ToString("X8");
                var memoryString = BitConverter.ToString(memoryDataBytes).Replace("-", " ");

                if (MemoryView.Count <= i)
                {
                    MemoryView.Add(new ObservableMemoryStream {BaseAddress = baseAddress, MemoryString = memoryString });
                }
                else
                {
                    MemoryView[i].BaseAddress = baseAddress;
                    MemoryView[i].MemoryString = memoryString;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
