using System.Windows.Controls;
using System.Windows.Input;
using ARM_Simulator.ViewModel;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.View
{
    /// <summary>
    /// Interaction logic for ShowBreakpoints.xaml
    /// </summary>
    public partial class ShowBreakpoints
    {
        // initalize ShowBreakpoint Window
        public ShowBreakpoints()
        {
            InitializeComponent();
        }

        // Breakpoints can be removed on double click
        private void ListViewBreakpoint_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as SimulatorViewModel;
            var listView = sender as ListView;

            if (viewModel == null || listView == null)
                return;

            var observableCommand = listView.SelectedItem as ObservableCommand;
            if (observableCommand == null)
                return;
           
            viewModel.CoreVm.ToggleBreakPoint(observableCommand.Address);
        }
    }
}
