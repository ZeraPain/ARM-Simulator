using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using ARM_Simulator.Model;
using ARM_Simulator.Resources;
using Microsoft.Win32;

namespace ARM_Simulator.View
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public Simulator ArmSimulator { get; protected set; }
        public List<Command> CommandList { get; protected set; }

        private bool _running;
        private Thread _runThread;
        private string _file;
        private bool _debugmode;

        public MainWindow()
        {
            InitializeComponent();
            ArmSimulator = new Simulator();
        }

        public void ToggleDebugMode(bool active)
        {
            _debugmode = active;

            if (active)
            {
                DebugMode.Visibility = Visibility.Visible;
                EditMode.Visibility = Visibility.Collapsed;
                ListViewRegister.ItemsSource = ArmSimulator.ArmCore.Registers;
            }
            else
            {
                DebugMode.Visibility = Visibility.Collapsed;
                EditMode.Visibility = Visibility.Visible;
                ListViewRegister.ItemsSource = null;
            }
        }

        private void UpdateViewElements()
        {
            foreach (var x in CommandList) x.Status = EPipeline.None;

            var status = ArmSimulator.ArmCore.PipelineStatus;
            foreach (var x in status)
            {
                if (x.Value < 0)
                    continue;

                var index = x.Value / 4;
                if (index >= CommandList.Count)
                    continue;

                CommandList[index].Status = x.Key;
            }
            ListViewCode.Items.Refresh();
            ListViewRegister.Items.Refresh();
        }

        private void UpdateView()
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                UpdateViewElements();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(UpdateViewElements));
            }
        }

        private void Run()
        {
            while (_running)
            {
                ArmSimulator.ArmCore.Tick();

                var pcExe = ArmSimulator.ArmCore.PipelineStatus[EPipeline.Execute];
                for (var i = 0; i < CommandList.Count; i++) // TODO: fix this dirty part
                {
                    if (CommandList[i].Breakpoint && i * 4 == pcExe)
                    {
                        _running = false;
                    }
                }
            }
            UpdateView();
        }

        #region Click-Functions
        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            if (_file == null)
            {
                MessageBox.Show("Please load a file before running", "File missing");
                return;
            }

            CommandList = ArmSimulator.LoadFile(_file);
            ListViewCode.ItemsSource = CommandList;

            ToggleDebugMode(true);
        }

        private void BtnStep_Click(object sender, RoutedEventArgs e)
        {
            if (_running || !_debugmode) return;

            ArmSimulator.ArmCore.Tick();
            UpdateView();
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            if (_running || !_debugmode) return;

            _running = true;
            _runThread = new Thread(Run);
            _runThread.Start();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var index = ListViewCode.SelectedIndex;
            if (index < 0 || index >= ListViewCode.Items.Count)
                return;

            CommandList[index].Breakpoint = !CommandList[index].Breakpoint;
            ListViewCode.ItemsSource = null;
            ListViewCode.ItemsSource = CommandList;
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (!_running || !_debugmode) return;

            _running = false;
            _runThread?.Join();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            ToggleDebugMode(false);     
        }

        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            var saveFile = new SaveFileDialog
            {
                Filter = "Assembly files (*.S)|*.S|All files (*.*)|*.*"
            };

            if (saveFile.ShowDialog() != true)
                return;

            var richText = new TextRange(TxtEditor.Document.ContentStart, TxtEditor.Document.ContentEnd).Text.Replace("\r", "");
            File.WriteAllLines(saveFile.FileName, richText.Split('\n'));
        }

        private void BtnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog
            {
                Filter = "Assembly files (*.S)|*.S|All files (*.*)|*.*"
            };

            if (openFile.ShowDialog() != true)
                return;

            _file = openFile.FileName;
            foreach (var line in File.ReadAllLines(_file))
                TxtEditor.AppendText(line + "\n");
        }
        #endregion

        private void TxtEditor_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_file == null)
                return;

            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var richText = new TextRange(TxtEditor.Document.ContentStart, TxtEditor.Document.ContentEnd).Text.Replace("\r", "");
                File.WriteAllLines(_file, richText.Split('\n'));
            }
        }
    }
}
