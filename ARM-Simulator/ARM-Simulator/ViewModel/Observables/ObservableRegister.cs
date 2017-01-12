using System.ComponentModel;
using System.Runtime.CompilerServices;
using ARM_Simulator.Annotations;
using ARM_Simulator.Resources;

namespace ARM_Simulator.ViewModel.Observables
{
    public class ObservableRegister : INotifyPropertyChanged
    {
        // variable holding one of the declared registers
        private ERegister _register;
        public ERegister Register
        {
            get { return _register; }
            set
            {
                _register = value;
                OnPropertyChanged(nameof(Register));
            }
        }

        // varibale holding the value of the register
        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public ObservableRegister(ERegister register, int value)
        {
            Register = register;
            Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
