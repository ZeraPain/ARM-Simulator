using System.ComponentModel;
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

        private string _memorystring;
        public string MemoryString
        {
            get
            {
                return _memorystring;
            }
            set
            {
                if (_memorystring == value) return;
                _memorystring = value;
                OnPropertyChanged(nameof(MemoryString));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
