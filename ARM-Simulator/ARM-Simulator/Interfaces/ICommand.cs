using ARM_Simulator.Enumerations;
using ARM_Simulator.Model;

namespace ARM_Simulator.Interfaces
{
    public interface ICommand
    {
        bool Decode(Command command);
        bool Execute(Core armCore);
        int GetBitCommand();
    }
}
