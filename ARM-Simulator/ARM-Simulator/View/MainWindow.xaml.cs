using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using ARM_Simulator.Model;
using ARM_Simulator.ViewModel;
using Microsoft.Win32;

namespace ARM_Simulator.View
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public Simulator ArmSimulator { get; protected set; }
        public MemoryViewModel MemoryVm { get; protected set; }
        public CoreViewModel CoreVm { get; protected set; }

        private bool _running;
        private Thread _runThread;
        private string _file;
        private bool _debugmode;

        public MainWindow()
        {
            InitializeComponent();
            ArmSimulator = new Simulator();
            MemoryVm = new MemoryViewModel(ArmSimulator.Memory);
            CoreVm = new CoreViewModel(ArmSimulator.ArmCore);
        }

        public void ToggleDebugMode(bool active)
        {
            _debugmode = active;

            if (active)
            {
                DebugMode.Visibility = Visibility.Visible;
                EditMode.Visibility = Visibility.Collapsed;
                ListViewCode.ItemsSource = CoreVm.CommandList;
                ListViewRegister.ItemsSource = ArmSimulator.ArmCore.Registers;
                ListViewMemory.ItemsSource = MemoryVm.MemoryView;
            }
            else
            {
                DebugMode.Visibility = Visibility.Collapsed;
                EditMode.Visibility = Visibility.Visible;
                ListViewCode.ItemsSource = null;
                ListViewRegister.ItemsSource = null;
                ListViewMemory.ItemsSource = null;
            }
        }

        private void UpdateViewElements()
        {
            // Code View Update
            CoreVm.Update();
            ListViewCode.Items.Refresh();

            // Register View Update
            ListViewRegister.Items.Refresh();

            // Memory View Update
            MemoryVm.Update();
            ListViewMemory.Items.Refresh();
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
                if (CoreVm.IsBreakPoint()) _running = false;
            }

            UpdateView();
        }

        public void SaveFile()
        {
            if (string.IsNullOrEmpty(_file))
            {
                var saveFile = new SaveFileDialog
                {
                    Filter = "Assembly files (*.S)|*.S|All files (*.*)|*.*"
                };

                if (saveFile.ShowDialog() != true)
                    return;

                _file = saveFile.FileName;
            }

            var docStream = new FileStream(_file, FileMode.OpenOrCreate);
            System.Windows.Markup.XamlWriter.Save(TxtEditor.Document, docStream);
            docStream.Close();
        }

        #region Click-Functions
        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_file))
            {
                MessageBox.Show("Please load/save file before running", "File missing");
                return;
            }

            CoreVm.CommandList = ArmSimulator.LoadFile(_file);

            ToggleDebugMode(true);
            UpdateView();
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

            CoreVm.ToggleBreakPoint(index);
            ListViewCode.Items.Refresh();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (!_running || !_debugmode) return;

            _running = false;
            _runThread?.Join();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_running)
            {
                _running = false;
                _runThread?.Join();
            }

            ToggleDebugMode(false);     
        }

        private void BtnNewFile_Click(object sender, RoutedEventArgs e)
        {
            _file = null;
            TxtEditor.Document.Blocks.Clear();
        }

        private void BtnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog
            {
                Filter = "Assembly files (*.S)|*.S|All files (*.*)|*.*"
            };

            if (openFile.ShowDialog() != true || !File.Exists(openFile.FileName))
                return;

            _file = openFile.FileName;
            var range = new TextRange(TxtEditor.Document.ContentStart, TxtEditor.Document.ContentEnd);
            var fStream = new FileStream(_file, FileMode.OpenOrCreate);
            range.Load(fStream, DataFormats.Text);
            fStream.Close();
        }

        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }
        #endregion

        private void TxtEditor_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SaveFile();
            }
        }
    }
}
