using System.ComponentModel;
using System.Runtime.CompilerServices;
using ARM_Simulator.Annotations;
using ARM_Simulator.Resources;

namespace ARM_Simulator.ViewModel.Observables
{
    public class ObservableRegister : INotifyPropertyChanged
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _value;

        public string Value
        {
            get { return _value; }
            set
            {
                if (_value == value) return;
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
