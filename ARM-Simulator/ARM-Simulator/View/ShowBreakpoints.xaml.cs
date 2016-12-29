using System.ComponentModel;

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

        private void ShowBreakpoints_OnClosing(object sender, CancelEventArgs e)
        {
           e.Cancel = true;
           BpWindow.Hide();
        }
    }
}
