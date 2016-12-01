using System.Collections.Generic;
using ARM_Simulator.Enumerations;

namespace ARM_Simulator.Model
{
    public class ArmCore
    {
        public Dictionary<ArmRegister, int> Registers { get; }
        public ArmPipeline Pipeline { get; }

        public ArmCore()
        {
            Registers = new Dictionary<ArmRegister, int>();
            Registers.Add(ArmRegister.R0, 0);
            Registers.Add(ArmRegister.R1, 0);
            Registers.Add(ArmRegister.R2, 0);
            Registers.Add(ArmRegister.R3, 0);
            Registers.Add(ArmRegister.R4, 0);
            Registers.Add(ArmRegister.R5, 0);
            Registers.Add(ArmRegister.R6, 0);
            Registers.Add(ArmRegister.R7, 0);
            Registers.Add(ArmRegister.R8, 0);
            Registers.Add(ArmRegister.R9, 0);
            Registers.Add(ArmRegister.R10, 0);
            Registers.Add(ArmRegister.R11, 0);

            Pipeline = new ArmPipeline(Registers);
        }

        public void Test()
        {
            Pipeline.Step("mov r0, #12");
            Pipeline.Step("mov r1, #2");
            Pipeline.Step("mov r0, r1, lsl #2");
            Pipeline.Step("mov r0, r0");
            Pipeline.Step("mov r0, r0");
            Pipeline.Step("mov r0, r0");
            Pipeline.Step("mov r0, r0");
            Pipeline.Step("mov r0, r0");
            Pipeline.Step("mov r0, r0");
        }
    }
}
