﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
            _viewModel?.LoadFile("../../Resources/source.S", RichTextBoxEditor.Document);
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

        private void SavingDialog()
        {
            var content = new TextRange(RichTextBoxEditor.Document.ContentStart, RichTextBoxEditor.Document.ContentEnd).Text
                    .TrimEnd(' ', '\r', '\n', '\t').Replace("\r\n", "\n").Split('\n');

            if (_viewModel.File == null || !File.ReadAllLines(_viewModel.File).SequenceEqual(content))
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
                var newValue = Parser.ParseImmediate<uint>(cell.Text);

                _viewModel.ArmSimulator.Memory.WriteUint(address, newValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
    }
}
