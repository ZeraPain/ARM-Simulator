using ARM_Simulator.Model;

namespace ARM_Simulator.Interfaces
{
    public interface ICommand
    {
        bool Decode();
        bool Execute(ArmCore armCore);
    }
}
