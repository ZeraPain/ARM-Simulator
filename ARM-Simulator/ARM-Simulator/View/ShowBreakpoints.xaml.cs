using System.Windows.Controls;
using System.Windows.Input;
using ARM_Simulator.ViewModel;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ShowBreakpoints
    {
        
        public ShowBreakpoints()
        {
            InitializeComponent();
        }

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
