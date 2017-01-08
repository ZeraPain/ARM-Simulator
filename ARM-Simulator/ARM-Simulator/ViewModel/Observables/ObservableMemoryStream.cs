using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ARM_Simulator.Annotations;

namespace ARM_Simulator.ViewModel.Observables
{
    public class ObservableMemoryStream : INotifyPropertyChanged
    {
        private string _baseAddress;
        public string BaseAddress
        {
            get
            {
                return _baseAddress;
            }
            set
            {
                if (_baseAddress == value) return;
                _baseAddress = value;
                OnPropertyChanged(nameof(BaseAddress));
            }
        }

        private string[] _memoryOffset;

        public string[] MemoryOffset
        {
            get { return _memoryOffset; }
            set
            {
                if (_memoryOffset.SequenceEqual(value)) return;
                _memoryOffset = value;
                OnPropertyChanged();
            }
        }

        private string _ascii;

        public string Ascii
        {
            get { return _ascii; }
            set
            {
                if (_ascii == value) return;
                _ascii = value;
                OnPropertyChanged(nameof(Ascii));
            }
        }

        public ObservableMemoryStream()
        {
            _memoryOffset = new string[8];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
