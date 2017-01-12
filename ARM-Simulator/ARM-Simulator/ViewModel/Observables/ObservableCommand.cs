using System.ComponentModel;
using System.Runtime.CompilerServices;
using ARM_Simulator.Annotations;
using ARM_Simulator.Resources;

namespace ARM_Simulator.ViewModel.Observables
{
    public class ObservableCommand : INotifyPropertyChanged
    {
        // variable holding the pipelining status of a command
        private EPipeline _status;
        public EPipeline Status
        {
            get { return _status; }
            set
            {
                if (_status == value) return;
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        // variable holding a bool value, it the command is marked as breakpoint
        private bool _breakpoint;
        public bool Breakpoint
        {
            get { return _breakpoint; }
            set
            {
                if (_breakpoint == value) return;
                _breakpoint = value;
                OnPropertyChanged(nameof(Breakpoint));
            }
        }

        // variable holding the address of the command in the programm section
        private uint _address;
        public uint Address
        {
            get { return _address; }
            set
            {
                if (_address == value) return;
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        // varibale holding a  label for the command
        private string _label;
        public string Label
        {
            get { return _label; }
            set
            {
                if (_label == value) return;
                _label = value;
                OnPropertyChanged(nameof(Label));
            }
        }
        
        // string representing the command
        private string _commandline;
        public string Commandline
        {
            get { return _commandline; }
            set
            {
                if (_commandline == value) return;
                _commandline = value;
                OnPropertyChanged(nameof(Commandline));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
