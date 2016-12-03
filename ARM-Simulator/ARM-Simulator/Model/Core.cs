using System;
using System.Collections.Generic;
using ARM_Simulator.Enumerations;

namespace ARM_Simulator.Model
{
    public class Core
    {
        private readonly Dictionary<Register, int> _registers;
        private readonly Pipeline _pipeline;
        private readonly Decoder _decoder;

        public Core()
        {
            _registers = new Dictionary<Register, int>
            {
                {Register.R0, 0},
                {Register.R1, 0},
                {Register.R2, 0},
                {Register.R3, 0},
                {Register.R4, 0},
                {Register.R5, 0},
                {Register.R6, 0},
                {Register.R7, 0},
                {Register.R8, 0},
                {Register.R9, 0},
                {Register.R10, 0},
                {Register.R11, 0},
                {Register.Cpsr, 0x13 }
            };

            _pipeline = new Pipeline();
            _decoder = new Decoder();
        }

        public void SetRegValue(Register? reg, int value)
        {
            if (reg == null || !_registers.ContainsKey((Register)reg))
                throw new Exception("Invalid Register was requested");

            _registers[(Register)reg] = value;
        }

        public int GetRegValue(Register? reg)
        {
            if (reg == null || !_registers.ContainsKey((Register)reg))
                throw new Exception("Invalid Register was requested");

            return _registers[(Register)reg];
        }

        public void SetNzcvFlags(Flags mask, Flags flags)
        {
            var state = GetRegValue(Register.Cpsr);
            state &= ~(mask.Value << 28); // Clear affected status bits
            state |= flags.Value << 28; // Set affected status bits

            SetRegValue(Register.Cpsr, state);
        }

        public void Tick(string fetchCommand)
        {
            _pipeline.Tick(fetchCommand)?.Execute(this);
        }

        public void DebugCommand()
        {
            DirectExecute("mov r1, #2");
            DirectExecute("mvn r2, #2");
            DirectExecute("tst r1, r2");

            DirectExecute("mov r1, #8");
            DirectExecute("movs r0, r1, lsl#29");

            DirectExecute("mov r2, #0x4");
            DirectExecute("mov r3, r2, lsl#28");

            DirectExecute("add r4, r3, r3");

            DirectExecute("mov r0, #3");
            DirectExecute("movs r1, #5");
            DirectExecute("mvn r2, #3");
            DirectExecute("teq r0, r1");

            DirectExecute("mov r2, #0x4");
            DirectExecute("mov r3, r2, lsl#28");

            DirectExecute("mov r0, r1, lsl #2");
            DirectExecute("adds r1, r1, r0, LSL#28");
            DirectExecute("adds r1, r1, r0, LSL#28");

            DirectExecute("adds r2, r2, #1");
        }

        // Only needed for Unit tests!
        public void DirectExecute(string command)
        {
            var test = _pipeline.ForceDecode(command);
            var cmd = _decoder.Decode(test.GetBitCommand());
            cmd.Execute(this);
        }
    }
}
