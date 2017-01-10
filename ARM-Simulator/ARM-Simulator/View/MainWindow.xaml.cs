using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ARM_Simulator.Annotations;
using ARM_Simulator.Model;
using ARM_Simulator.ViewModel;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.View
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly SimulatorViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as SimulatorViewModel;
            //_viewModel?.LoadFile("../../Resources/source.S", RichTextBoxEditor.Document);
        }

        private void ListViewCode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as ListView;

            if (_viewModel == null || listView == null)
                return;

            var observableCommand = listView.SelectedItem as ObservableCommand;
            if (observableCommand == null)
                return;

            _viewModel.CoreVm.ToggleBreakPoint(observableCommand.Address);
        }

        private void RichTextBoxEditor_OnKeyDown(object sender, [NotNull] KeyEventArgs e)
        {
            if ((e.Key == Key.S) && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                _viewModel.SaveFile(RichTextBoxEditor.Document);
            }
        }

        private void DataGrid_OnCellEditEnding(object sender, [NotNull] DataGridCellEditEndingEventArgs e)
        {
            var cell = e.EditingElement as TextBox;
            var row = cell?.DataContext as ObservableMemoryStream;
            if (row == null)
                return;

            try
            {
                var index = e.Column.DisplayIndex;
                var address = (uint) (row.BaseAddress + (index - 1) * 4);

                if (_viewModel.MemoryVm.ShowAsByte)
                {
                    var newValue = new byte[4];
                    var split = cell.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length > 4) throw new ArgumentException();

                    for (var i = split.Length - 1; i >= 0; i--)
                    {
                        if (_viewModel.MemoryVm.ShowAsSigned)
                        {
                            newValue[i] = _viewModel.MemoryVm.ShowAsHexadecimal
                                ? (byte)sbyte.Parse(split[i], NumberStyles.HexNumber)
                                : (byte)sbyte.Parse(split[i]);
                        }
                        else
                        {
                            newValue[i] = _viewModel.MemoryVm.ShowAsHexadecimal
                                ? byte.Parse(split[i], NumberStyles.HexNumber)
                                : byte.Parse(split[i]);
                        }
                    }

                    _viewModel.ArmSimulator.Memory.Write(address, newValue);
                }
                else
                {
                    if (_viewModel.MemoryVm.ShowAsSigned)
                    {
                        var newValue = Parser.ParseImmediate<int>(cell.Text);
                        _viewModel.ArmSimulator.Memory.WriteInt(address, newValue);
                    }
                    else
                    {
                        var newValue = Parser.ParseImmediate<uint>(cell.Text);
                        _viewModel.ArmSimulator.Memory.WriteUint(address, newValue);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
    }
}
