using System.Collections.Generic;
using ARM_Simulator.Enumerations;

namespace ARM_Simulator.Interfaces
{
    public interface ICommand
    {
        bool Decode();
        bool Execute(Dictionary<ArmRegister, int> registers);
    }
}
