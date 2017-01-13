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

        // initialize MainWindow with data context of SimualtorViewModel
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as SimulatorViewModel;
            _viewModel?.LoadFile("../../Resources/source.S", RichTextBoxEditor.Document);
        }

        // Breakpoints can be removed on double click 
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
            // User can save the file by hitting CTRL + S
            if ((e.Key == Key.S) && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                _viewModel.SaveFile(RichTextBoxEditor.Document);
            }
        }

        //editing the memory cells
        private void DataGridMemory_OnCellEditEnding(object sender, [NotNull] DataGridCellEditEndingEventArgs e)
        {
            var cell = e.EditingElement as TextBox;
            var row = cell?.DataContext as ObservableMemoryStream;
            if (row == null)
                return;

            try
            {
                var index = e.Column.DisplayIndex;
                // Calculate the modifies address
                var address = (uint) (row.BaseAddress + (index - 1) * 4);

                if (_viewModel.ShowAsByte)
                {
                    // Split the string by the bytes and add them in order
                    var newValue = new byte[4];
                    var split = cell.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length > 4) throw new ArgumentException();

                    for (var i = split.Length - 1; i >= 0; i--)
                    {
                        if (_viewModel.ShowAsHexadecimal)
                            newValue[i] = unchecked((byte) long.Parse(split[i], NumberStyles.HexNumber));
                        else
                            newValue[i] = unchecked((byte) long.Parse(split[i]));
                    }


                    _viewModel.ArmSimulator.Memory.Write(address, newValue);
                }
                else
                {
                    // User entered and word value, no matter if its hex (0x) or dec
                    var newValue = unchecked((int)Parser.ParseImmediate<long>(cell.Text));
                    _viewModel.ArmSimulator.Memory.WriteInt(address, newValue);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var border = RibbonApplicationMenuEdit.Template.FindName("MainPaneBorder", RibbonApplicationMenuEdit) as Border;
            var grid = border?.Parent as Grid;
            if (grid != null) grid.ColumnDefinitions[2].Width = new GridLength(0);
        }
    }
}
