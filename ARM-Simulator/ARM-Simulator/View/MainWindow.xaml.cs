using ARM_Simulator.Model;

namespace ARM_Simulator.View
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var sim = new Simulator();
            var source = sim.Compile("source.txt");
            sim.LoadSource(source);
            sim.Run();
        }
    }
}
