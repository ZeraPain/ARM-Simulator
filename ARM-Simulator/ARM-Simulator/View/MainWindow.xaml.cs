using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Win32;

namespace ARM_Simulator.View
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow:Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

       



        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            var start = TxtEditor.Document.ContentStart;
            var end = TxtEditor.Document.ContentEnd;
            var isEmpty = start.GetOffsetToPosition(end);

          if (isEmpty > 2)
            {
                var result = MessageBox.Show("Would you like to save your File?", "Arm Simulator",
                    MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        MenuSave_OnClick();
                        break;
                    case MessageBoxResult.No:
                        TxtEditor.Document.Blocks.Clear();
                        break;
                }
            }
        }

        private void MenuSave_OnClick()
        {
            var saveFile = new SaveFileDialog
            {
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
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

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog
            {
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
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

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
         //Save File via Shortcut 
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        // Actually  == Save As
        private void MenuSave_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFile = new SaveFileDialog
            {
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
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
