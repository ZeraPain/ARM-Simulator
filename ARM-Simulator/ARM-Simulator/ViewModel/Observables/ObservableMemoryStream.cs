using System.ComponentModel;
using System.Runtime.CompilerServices;
using ARM_Simulator.Annotations;

namespace ARM_Simulator.ViewModel.Observables
{
    public class ObservableMemoryStream : INotifyPropertyChanged
    {
        private uint _baseAddress;
        public uint BaseAddress
        {
            get { return _baseAddress; }
            set
            {
                _baseAddress = value;
                OnPropertyChanged(nameof(BaseAddress));
            }
        }

        private byte[] _memoryBytes;
        public byte[] MemoryBytes
        {
            get { return _memoryBytes; }
            set
            {
                _memoryBytes = value;
                OnPropertyChanged(nameof(MemoryBytes));
            }
        }

        public ObservableMemoryStream()
        {
            _memoryBytes = new byte[32];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
