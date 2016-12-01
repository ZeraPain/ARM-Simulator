using System;
using ARM_Simulator.Enumerations;

namespace ARM_Simulator.ViewModel
{
    public class ARMDecoder
    {
        struct Command
        {
            public ARMCommands Opcode { get; }
            public string[] Parameters { get; }

            public Command(ARMCommands opcode, string[] parameters)
            {
                Opcode = opcode;
                Parameters = parameters;
            }
        }

        public void Test()
        {
            Decode("mov r1, r2, #0ff");
        }

        
        public void Decode(string command)
        {
            var cmd = ParseCommand(command);

            switch (cmd.Opcode)
            {
                case ARMCommands.None:
                     throw new ArgumentException("Unknown command");
                case ARMCommands.MOV:
                    Decode_MOV(cmd);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Command ParseCommand(string command)
        {
            var index = command.IndexOf(' ');
            if (index == -1)
                index = command.IndexOf('\t');

            if (index == -1 || command.Length < index + 1)
                throw new ArgumentException("Invalid Syntax");

            ARMCommands opcode;
            Enum.TryParse(command.Substring(0, index), true, out opcode);
            if (opcode == ARMCommands.None)
                throw new ArgumentException("Unable to parse an invalid ARM command");

            var parameters = command.Substring(index).Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parameters.Length; i++)
            {
                parameters[i] = parameters[i].Replace(" ", string.Empty);
            }

            return new Command(opcode, parameters);
        }

        private void GetShiftInstruction(string parameter, ref ShiftInstruction shiftInst, ref byte shiftCount)
        {
            var shiftParameters = parameter.Split('#');
            if (shiftParameters.Length != 2)
                throw new ArgumentException("Invalid Shiftinstruction");

            Enum.TryParse(shiftParameters[0], true, out shiftInst);
            if (shiftInst == ShiftInstruction.None)
                throw new ArgumentException("Invalid Shiftinstruction");

            shiftCount = byte.Parse(shiftParameters[1]);
        }

        private int GetImmediate(string parameter)
        {
            return parameter.StartsWith("#0x") ? int.Parse(parameter.Substring(3), System.Globalization.NumberStyles.HexNumber) : int.Parse(parameter.Substring(1));
        }

        private MOV Decode_MOV(Command cmd)
        {
            // Check Parameter Count
            var parameters = cmd.Parameters;
            if (parameters.Length != 2 && parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Check Destination Register
            Register destReg;
            Enum.TryParse(parameters[0], true, out destReg);
            if (destReg == Register.None)
                throw new ArgumentException("Invalid destination register");

            // Want to move an immediate?
            if (parameters[1].StartsWith("#"))
            {
                if (parameters.Length != 2)
                    throw new ArgumentException("Invalid parameter count");

                var immediate = GetImmediate(parameters[1]);

                return new MOV(destReg, immediate);
            }

            // Check Source Register
            Register srcReg;
            Enum.TryParse(parameters[0], true, out srcReg);
            if (srcReg == Register.None)
                throw new ArgumentException("Invalid source register");

            var shiftInst = ShiftInstruction.None;
            byte shiftCount = 0;

            if (parameters.Length == 3)
                GetShiftInstruction(parameters[2], ref shiftInst, ref shiftCount);

            return new MOV(destReg, srcReg, shiftInst, shiftCount);
        }
    }
}
