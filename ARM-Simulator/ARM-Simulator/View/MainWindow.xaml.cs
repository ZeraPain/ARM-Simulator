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

        public MainWindow()
        {
            InitializeComponent();
            ArmSimulator = new Simulator();
            _file = "../../Resources/source.S";


            ListViewRegister.ItemsSource = ArmSimulator.ArmCore.Registers;

            var hFile = File.ReadAllLines(_file);

            TxtEditor.Document.LineHeight = 0.1f;
            foreach (var line in hFile)
            {
                TxtEditor.AppendText(line + "\n");
            }

        }


        #region Helper

        public bool IsEmpty()
        {
            var start = TxtEditor.Document.ContentStart;
            var end = TxtEditor.Document.ContentEnd;
            var length = start.GetOffsetToPosition(end);
            return length > 2;
        }

        public void MenuSave_OnClick()
        {
            var saveFile = new SaveFileDialog
            {
                Filter = "Assembly files (*.S)|*.S|All files (*.*)|*.*"
            };

            if (saveFile.ShowDialog() != true) return;
            try
            {
                var fileStream = new FileStream(saveFile.FileName, FileMode.Create);
                var range = new TextRange(TxtEditor.Document.ContentStart, TxtEditor.Document.ContentEnd);
                range.Save(fileStream, DataFormats.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Error: Saving your File!");
            }
        }
        #endregion

        #region Click-Functions

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEmpty())
                return;

            var result = MessageBox.Show("Would you like to save your File?", "Arm Simulator", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    MenuSave_OnClick();
                    break;
                case MessageBoxResult.No:
                    //TxtEditor.Document.Blocks.Clear();
                    break;
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog
            {
                Filter = "Assembly files (*.S)|*.S|All files (*.*)|*.*"
            };

            if (openFile.ShowDialog() != true) return;
            try
            {
                var fileStream = new FileStream(openFile.FileName, FileMode.Open);
                var range = new TextRange(TxtEditor.Document.ContentStart,
                    TxtEditor.Document.ContentEnd);
                range.Load(fileStream, DataFormats.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Error: Loading your File");
            }
        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            TxtEditor.Visibility = Visibility.Hidden;
            ListViewCode.Visibility = Visibility.Visible;
            CommandList = ArmSimulator.LoadFile(_file);
            ListViewCode.ItemsSource = CommandList;
        }

        private void BtnStep_Click(object sender, RoutedEventArgs e)
        {
            if (_running)
                return;

            ArmSimulator.ArmCore.Tick();
            UpdateView();
        }

        private void UpdateViewElements()
        {
            foreach (var x in CommandList) x.Status = EPipeline.None;

            var status = ArmSimulator.ArmCore.PipelineStatus;
            foreach (var x in status)
            {
                if (x.Value == -1)
                    continue;

                var line = x.Value / 4;
                if (line >= CommandList.Count)
                    continue;

                CommandList[line].Status = x.Key;
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

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            _running = true;
            _runThread = new Thread(Run);
            _runThread.Start();
        }

        private void Run()
        {
            while (_running)
            {
                ArmSimulator.ArmCore.Tick();

                var pcExe = ArmSimulator.ArmCore.PipelineStatus[EPipeline.Execute];
                for (var i = 0; i < CommandList.Count; i++) // TODO: fix this dirty part
                {
                    if (CommandList[i].Breakpoint && i*4 == pcExe)
                    {
                        _running = false;
                    }
                }
            }
            UpdateView();
        }
        #endregion

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
            _running = false;
            _runThread?.Join();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            ListViewCode.Visibility = Visibility.Hidden;
            TxtEditor.Visibility = Visibility.Visible;       
        }

        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            var saveFile = new SaveFileDialog
            {
                Filter = "Assembly files (*.S)|*.S|All files (*.*)|*.*"
            };

            if (saveFile.ShowDialog() != true) return;
            try
            {
                var fileStream = new FileStream(saveFile.FileName, FileMode.Create);
                var range = new TextRange(TxtEditor.Document.ContentStart, TxtEditor.Document.ContentEnd);
                range.Save(fileStream, DataFormats.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Error: Saving your File!");
            }
        }
    }
}
