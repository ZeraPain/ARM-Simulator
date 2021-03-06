﻿using System;
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
        // Using static variables so that they can be accessed by the memory and register converters
        public static bool StaticShowAsHexadecimal;
        public static bool StaticShowAsByte;
        public static bool StaticShowAsSigned;

        private bool _running;
        private Thread _runThread;
        private ShowBreakpoints _subWindow;
        private string[] _programCode;

        public ObservableCollection<string> ErrorMessages { get; protected set; }

        // Variable holding the current file which is used 
        private string _file;
        public string File
        {
            get { return _file; }
            set { _file = value; OnPropertyChanged(); }
        }

        // Variable holding the current state wether we are in edit mode or debug mode
        private bool _debugmode;
        public bool DebugMode
        {
            get { return _debugmode; }
            protected set { _debugmode = value; OnPropertyChanged(); }
        }

        //Variable holding the current state wether disyplay numeric type is hexadecimal or decimal
        public bool ShowAsHexadecimal
        {
            get { return StaticShowAsHexadecimal; }
            set
            {
                StaticShowAsHexadecimal = value;
                MemoryVm.Update(this, null);
                CoreVm.Update(this, null);
            }
        }

        //Variable holding the current state wether diayplay numeric type is byte or word
        public bool ShowAsByte
        {
            get { return StaticShowAsByte; }
            set
            {
                StaticShowAsByte = value;
                MemoryVm.Update(this, null);
                CoreVm.Update(this, null);
                OnPropertyChanged();
            }
        }

        //Variable holding the current state wether display numeric type is signed or unsigned
        public bool ShowAsSigned
        {
            get { return StaticShowAsSigned; }
            set
            {
                StaticShowAsSigned = value;
                MemoryVm.Update(this, null);
                CoreVm.Update(this, null);
            }
        }

        public Simulator ArmSimulator { get; protected set; }
        public MemoryViewModel MemoryVm { get; protected set; }
        public CoreViewModel CoreVm { get; protected set; }

        // ICommands for edtior and exit
        public ICommand NewFileCommand { get; protected set; }
        public ICommand SaveFileCommand { get; protected set; }
        public ICommand SaveFileAsCommand { get; protected set; }
        public ICommand LoadFileCommand { get; protected set; }
        public ICommand ExitCommand { get; protected set; }

        // special ICommands used for ARM simualtion
        public ICommand RunCommand { get; protected set; }
        public ICommand RestartCommand { get; protected set; }
        public ICommand StopCommand { get; protected set; }
        public ICommand TickCommand { get; protected set; }
        public ICommand ContinueCommand { get; protected set; }
        public ICommand PauseCommand { get; protected set; }
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
            SaveFileAsCommand = new DelegateCommand(SaveFileAs);
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

            // default values for display numeric type
            ShowAsHexadecimal = true;
            ShowAsSigned = false;
            ShowAsByte = false;
        }

        //opens a blank document
        private void NewFile(object parameter)
        {
            var document = parameter as FlowDocument;
            if (document == null) return;

            File = null;
            document.Blocks.Clear();
        }

        // open a file loading dialog to set the path and load the content of a document
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

        // loading a file with a given path
        public void LoadFile(string path, FlowDocument document)
        {
            if (!System.IO.File.Exists(path)) return;

            File = path;
            var range = new TextRange(document.ContentStart, document.ContentEnd);
            var fStream = new FileStream(File, FileMode.OpenOrCreate);
            range.Load(fStream, DataFormats.Text);
            fStream.Close();
        }

        private void SaveFileAs(object parameter)
        {
            var document = parameter as FlowDocument;
            if (document == null) return;

            var content = new TextRange(document.ContentStart, document.ContentEnd).Text
                    .TrimEnd(' ', '\r', '\n', '\t').Replace("\r\n", "\n").Split('\n');

            Save(content, true);
        }

        public void SaveFile(object parameter)
        {
            var document = parameter as FlowDocument;
            if (document == null) return;

            var content = new TextRange(document.ContentStart, document.ContentEnd).Text
                    .TrimEnd(' ', '\r', '\n', '\t').Replace("\r\n", "\n").Split('\n');

            Save(content);
        }

        private void Save(string[] lines, bool newFile = false)
        {
            if (string.IsNullOrEmpty(File) || newFile)
            {
                var saveFile = new SaveFileDialog
                {
                    Filter = "Assembly files (*.S)|*.S|All files (*.*)|*.*"
                };

                if (saveFile.ShowDialog() != true)
                    return;

                File = saveFile.FileName;
            }

            System.IO.File.WriteAllLines(File, lines);
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
                // load the current program code to the arm core, also compile / link
                var cmdlist = ArmSimulator.LoadFile(_programCode);
                // set the commandlist to display it in the listview
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

            // kill the current thread if its still alive
            if (_runThread?.IsAlive == true)
                _runThread.Abort();

            _programCode = null;
            DebugMode = false;
        }

        private void Tick(object parameter)
        {
            if (!DebugMode) return;

            CoreVm.Core.Tick();
        }

        // execute the whole program 
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
                // gets the current content of the editor
                var content = new TextRange(document.ContentStart, document.ContentEnd).Text
                    .TrimEnd(' ', '\r', '\n', '\t').Replace("\r\n", "\n").Split('\n');

                // try to parse, compile and link the current program code within a virtual core (not the real core!)
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
                // invoke so that our listviews will get updated
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
                    {
                        success = CoreVm.Core.Tick();
                        if (CoreVm.IsBreakPoint()) _running = false;
                    }));
            }
        }

        private void ShowBreakpoints(object parameter)
        {
            if (_subWindow == null || !_subWindow.IsVisible)
            {    
                // opens a subwindow with all breakpoints of the current data context   
                _subWindow = new ShowBreakpoints { DataContext = this };
                _subWindow.Show();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
