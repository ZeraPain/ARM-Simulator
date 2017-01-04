using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using ARM_Simulator.ViewModel;
using Microsoft.Win32;

namespace ARM_Simulator.View
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly SimulatorViewModel _viewModel;
        private ShowBreakpoints _subWindow;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as SimulatorViewModel;
            LoadFile("../../Resources/source.S");
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            SavingDialog();
            //this closes all Windows of the Application
            Application.Current.Shutdown();
        }

        private void ListViewCode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;

            var index = ListViewCode.SelectedIndex;
            if ((index < 0) || (index >= ListViewCode.Items.Count))
                return;

            _viewModel.CoreVm.ToggleBreakPoint(index);
        }

        private void BtnNewFile_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            _viewModel.File = null;
            RichTextBoxEditor.Document.Blocks.Clear();
        }

        private void LoadFile(string path)
        {
            if (_viewModel == null) return;
            if (!File.Exists(path)) return;

            _viewModel.File = path;
            var range = new TextRange(RichTextBoxEditor.Document.ContentStart, RichTextBoxEditor.Document.ContentEnd);
            var fStream = new FileStream(_viewModel.File, FileMode.OpenOrCreate);
            range.Load(fStream, DataFormats.Text);
            fStream.Close();
        }

        private void BtnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog
            {
                Filter = "Assembly files (*.S)|*.S|All files (*.*)|*.*"
            };

            if (openFile.ShowDialog() != true || !File.Exists(openFile.FileName))
                return;

            LoadFile(openFile.FileName);
        }

        private void SavingDialog()
        {
            var content = new TextRange(RichTextBoxEditor.Document.ContentStart, RichTextBoxEditor.Document.ContentEnd).Text
                    .TrimEnd(' ', '\r', '\n', '\t').Replace("\r\n", "\n").Split('\n');
            var areEqual = File.ReadAllLines(_viewModel.File).SequenceEqual(content);

            if (!areEqual)
            {
                var result = MessageBox.Show("Do you want to save your changes?", "Save File", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        _viewModel.SaveFile(RichTextBoxEditor.Document);
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }

        private void RichTextBoxEditor_OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.S) && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                _viewModel.SaveFile(RichTextBoxEditor.Document);
            }
        }

        private void ButtonShowBreakpoints_OnClick(object sender, RoutedEventArgs e)
        {
            if (_subWindow == null)
            {
                _subWindow = new ShowBreakpoints {DataContext = _viewModel};
                _subWindow.Show();
            }
            else
            {
                _subWindow.Close();
                _subWindow = null;
            }
        }
    }
}
