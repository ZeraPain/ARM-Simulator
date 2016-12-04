using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Interfaces
{
    public interface ICommand
    {
        bool Parse(Command command);
        bool Execute(Core armCore);
        int Encode();
    }
}
