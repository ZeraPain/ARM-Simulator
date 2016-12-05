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
            sim.TestCommand("mov r0, #7");
            sim.TestCommand("mov r2, #0");
            sim.TestCommand("mov r1, #0x1");
            sim.TestCommand("mov r1, r1, lsl#16");
            sim.TestCommand("str r0, [r1, #100]!");
            sim.TestCommand("ldr r2, [r1]");


            var source = sim.Compile("source.txt");
            sim.Load(source);
            sim.Run();
        }
    }
}
