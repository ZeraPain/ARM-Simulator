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
using ARM_Simulator.Resources;
using ARM_Simulator.View;
using Microsoft.Win32;

namespace ARM_Simulator.ViewModel
{
    internal class SimulatorViewModel : INotifyPropertyChanged
    {
        private bool _running;
        private Thread _runThread;
        private ShowBreakpoints _subWindow;

        public ObservableCollection<string> ErrorMessages { get; } = new ObservableCollection<string>();

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
            TickCommand = new DelegateCommand(Tick);
            ContinueCommand = new DelegateCommand(Continue);
            PauseCommand = new DelegateCommand(Pause);
            ExitCommand = new DelegateCommand(Exit);
            SyntaxCommand = new DelegateCommand(SyntaxCheck);
            ShowBreakpointsCommand = new DelegateCommand(ShowBreakpoints);
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

        private void Run(object parameter)
        {
            
            if (ErrorMessages.Any()) ErrorMessages.Clear();
            try
            {
                var cmdlist = ArmSimulator.LoadFile(_file);
                CoreVm.UpdateList(cmdlist);
                DebugMode = true;
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex.Message);   
            }
        }

        private void Stop(object parameter)
        {
            if (!DebugMode) return;
            _running = false;

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

        private static void Exit(object parameter) => Application.Current.Shutdown();  

        private void SyntaxCheck(object parameter)
        {
            if(ErrorMessages.Any()) ErrorMessages.Clear();
            try
            {
                var parser = new Parser(_file);
                foreach (var commandLine in parser.CommandList)
                {
                    Parser.ParseLine(commandLine);
                }
                
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex.Message);
            }
        }

        private void RunThread()
        {
            while (_running)
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    (Action)(() =>
                    {
                        _running = CoreVm.ArmCore.Tick();
                        if (IsBreakPoint()) _running = false;
                    }));
            }
        }

        private bool IsBreakPoint()
        {
            var pogramCounter = CoreVm.ArmCore.PipelineStatus[EPipeline.Execute];
            return CoreVm.CommandList.Where((cmd, index) => cmd.Breakpoint && index*4 == pogramCounter).Any();
        }

        private void ShowBreakpoints(object parameter)
        {
            if (_subWindow == null)
            {
                _subWindow = new ShowBreakpoints { DataContext = this };
                _subWindow.Show();
            }
            else
            {
                _subWindow.Close();
                _subWindow = null;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
