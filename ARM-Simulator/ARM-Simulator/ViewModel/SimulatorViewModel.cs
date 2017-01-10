using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using ARM_Simulator.Annotations;
using ARM_Simulator.Commands;
using ARM_Simulator.Model;
using ARM_Simulator.Model.Components;
using ARM_Simulator.View;
using Microsoft.Win32;

namespace ARM_Simulator.ViewModel
{
    internal class SimulatorViewModel : INotifyPropertyChanged
    {
        private bool _running;
        private Thread _runThread;
        private ShowBreakpoints _subWindow;
        private string[] _programCode;

        public ObservableCollection<string> ErrorMessages { get; protected set; }

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

        private bool _debugmode;
        public bool DebugMode
        {
            get { return _debugmode; }
            protected set
            {
                if (_debugmode == value) return;
                _debugmode = value;
                OnPropertyChanged();
            }
        }

        public Simulator ArmSimulator { get; protected set; }
        public MemoryViewModel MemoryVm { get; protected set; }
        public CoreViewModel CoreVm { get; protected set; }

        public ICommand NewFileCommand { get; protected set; }
        public ICommand SaveFileCommand { get; protected set; }
        public ICommand LoadFileCommand { get; protected set; }

        public ICommand RunCommand { get; protected set; }
        public ICommand RestartCommand { get; protected set; }
        public ICommand StopCommand { get; protected set; }
        public ICommand TickCommand { get; protected set; }
        public ICommand ContinueCommand { get; protected set; }
        public ICommand PauseCommand { get; protected set; }
        public ICommand ExitCommand { get; protected set; }
        public ICommand SyntaxCommand { get; protected set; }
        public ICommand ShowBreakpointsCommand { get; protected set; }

        public SimulatorViewModel()
        {
            DebugMode = false;

            ArmSimulator = new Simulator();
            MemoryVm = new MemoryViewModel(ArmSimulator.Memory);
            CoreVm = new CoreViewModel(ArmSimulator.ArmCore);

            NewFileCommand = new DelegateCommand(NewFile);
            SaveFileCommand = new DelegateCommand(SaveFile);
            LoadFileCommand = new DelegateCommand(LoadFileDialog);
            StopCommand = new DelegateCommand(Stop);
            RunCommand = new DelegateCommand(Run);
            RestartCommand = new DelegateCommand(Restart);
            TickCommand = new DelegateCommand(Tick);
            ContinueCommand = new DelegateCommand(Continue);
            PauseCommand = new DelegateCommand(Pause);
            ExitCommand = new DelegateCommand(Exit);
            SyntaxCommand = new DelegateCommand(SyntaxCheck);
            ShowBreakpointsCommand = new DelegateCommand(ShowBreakpoints);

            ErrorMessages = new ObservableCollection<string>();
        }

        private void NewFile(object parameter)
        {
            var document = parameter as FlowDocument;
            if (document == null) return;

            File = null;
            document.Blocks.Clear();
        }

        private void LoadFileDialog(object parameter)
        {
            var document = parameter as FlowDocument;
            if (document == null) return;

            var openFile = new OpenFileDialog
            {
                Filter = "Assembly files (*.S)|*.S|All files (*.*)|*.*"
            };

            if (openFile.ShowDialog() != true || !System.IO.File.Exists(openFile.FileName))
                return;

            LoadFile(openFile.FileName, document);
        }

        public void LoadFile(string path, FlowDocument document)
        {
            if (!System.IO.File.Exists(path)) return;

            File = path;
            var range = new TextRange(document.ContentStart, document.ContentEnd);
            var fStream = new FileStream(File, FileMode.OpenOrCreate);
            range.Load(fStream, DataFormats.Text);
            fStream.Close();
        }

        public void SaveFile(object parameter)
        {
            var document = parameter as FlowDocument;
            if (document == null) return;

            if (string.IsNullOrEmpty(File))
            {
                var saveFile = new SaveFileDialog
                {
                    Filter = "Assembly files (*.S)|*.S|All files (*.*)|*.*"
                };

                if (saveFile.ShowDialog() != true)
                    return;

                File = saveFile.FileName;
            }

            var content = new TextRange(document.ContentStart, document.ContentEnd).Text
                    .TrimEnd(' ', '\r', '\n', '\t').Replace("\r\n", "\n").Split('\n');
            System.IO.File.WriteAllLines(File, content);
        }

        private void Start()
        {
            ErrorMessages.Clear();

            if (_programCode == null)
            {
                ErrorMessages.Add("No program was loaded!");
                return;
            }

            try
            {
                var cmdlist = ArmSimulator.LoadFile(_programCode);
                CoreVm.UpdateList(cmdlist);
                DebugMode = true;
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex.Message);
            }
        }

        private void Run(object parameter)
        {
            var document = parameter as FlowDocument;
            if (document == null) return;

            _programCode = new TextRange(document.ContentStart, document.ContentEnd).Text.TrimEnd(' ', '\r', '\n', '\t').Replace("\r\n", "\n").Split('\n');
            Start();
        }

        private void Restart(object parameter) => Start();

        public void Stop(object parameter)
        {
            if (!DebugMode) return;
            _running = false;

            if (_runThread?.IsAlive == true)
                _runThread.Abort();

            _programCode = null;
            DebugMode = false;
        }

        private void Tick(object parameter)
        {
            if (!DebugMode) return;

            CoreVm.ArmCore.Tick();
        }

        private void Continue(object paramter)
        {
            if (!DebugMode || _running || (_runThread?.IsAlive == true)) return;

            _running = true;
            _runThread = new Thread(RunThread);
            _runThread.Start();
        }

        private void Pause(object parameter)
        {
            if (!DebugMode) return;
            _running = false;
        }

        private void Exit(object parameter)
        {
            SavingDialog(parameter);
            Stop(null);
            Application.Current.Shutdown();
        }

        private void SavingDialog(object parameter)
        {
            var document = parameter as FlowDocument;
            if (document == null) return;

            var content = new TextRange(document.ContentStart, document.ContentEnd).Text
                    .TrimEnd(' ', '\r', '\n', '\t').Replace("\r\n", "\n").Split('\n');

            if ((File != null && System.IO.File.ReadAllLines(File).SequenceEqual(content)) || content.Length < 2)
                return;

            var result = MessageBox.Show("Do you want to save your changes?", "Save File", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveFile(document);
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        private void SyntaxCheck(object parameter)
        {
            var document = parameter as FlowDocument;
            if (document == null) return;

            ErrorMessages.Clear();

            try
            {
                var content = new TextRange(document.ContentStart, document.ContentEnd).Text
                    .TrimEnd(' ', '\r', '\n', '\t').Replace("\r\n", "\n").Split('\n');

                var parser = new Parser();
                parser.ParseFile(content);
                var linker = new Linker(new Memory(0x40000, 0x10000), parser);
                linker.CompileAndLink();

                MessageBox.Show("Everything seems to be correct!", "Syntax Check");
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex.Message);
            }
        }

        private void RunThread()
        {
            var success = true;
            while (_running && success)
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    (Action)(() =>
                    {
                        success = CoreVm.ArmCore.Tick();
                        if (CoreVm.IsBreakPoint()) _running = false;
                    }));
            }
        }

        private void ShowBreakpoints(object parameter)
        {
            if (_subWindow == null || !_subWindow.IsVisible)
            {
                _subWindow = new ShowBreakpoints { DataContext = this };
                _subWindow.Show();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
