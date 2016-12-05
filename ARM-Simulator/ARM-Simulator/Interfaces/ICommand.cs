using ARM_Simulator.Model.Components;

namespace ARM_Simulator.Interfaces
{
    public interface ICommand
    {
        void Parse(string[] parameters);
        void Execute(Core armCore);
        int Encode();
    }
}
