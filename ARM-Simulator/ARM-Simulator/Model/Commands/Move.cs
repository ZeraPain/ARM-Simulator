using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;

namespace ARM_Simulator.Model.Commands
{
    internal class Move : ICommand
    {
        // Required
        private Command _command;
        private ArmRegister _rd;

        // Optional
        private ArmRegister _rm;
        private short _immediate;
        private ShiftInstruction _shiftInst;
        private byte _shiftCount;
        private bool _decoded;

        public Move(Command command)
        {
            _command = command;
            _rd = ArmRegister.None;
            _rm = ArmRegister.None;
            _immediate = 0;
            _shiftInst = ShiftInstruction.None;
            _shiftCount = 0;
            _decoded = false;
        }

        public bool Decode()
        {
            var parameters = _command.Parameters;

            // Check for valid parameter count
            if (parameters.Length != 2 && parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Parse Rd
            _rd = ArmDecoder.ParseRegister(parameters[0]);

            switch (_command.Opcode)
            {
                case ArmOpCode.Movs:
                case ArmOpCode.Mvn:
                case ArmOpCode.Mvns:
                    // Check for Rm or 8 bit immediate
                    ArmDecoder.ParseOperand2(parameters[1], ref _rm, ref _immediate);
                    break;
                case ArmOpCode.Mov:
                    // Check if an immediate needs to be moved
                    if (parameters[1].StartsWith("#"))
                    {
                        if (parameters.Length != 2)
                            throw new ArgumentException("Invalid parameter count");

                        // Parse 16 bit immediate
                        _immediate = ArmDecoder.ParseImmediate(parameters[1], 16);

                        _decoded = true;
                        return true;
                    }

                    // Check for Rm or 8 bit immediate
                    ArmDecoder.ParseOperand2(parameters[1], ref _rm, ref _immediate);
                    break;
                default:
                    throw new ArgumentException("Invalid Opcode");
            }

            // Check for shift instruction
            if (_rm != ArmRegister.None && parameters.Length == 3)
                ArmDecoder.ParseShiftInstruction(parameters[2], ref _shiftInst, ref _shiftCount);

            _decoded = true;
            return true;
        }

        public bool Execute(ArmCore armCore)
        {
            if (!_decoded)
                throw new Exception("Cannot execute an undecoded command");

            // calculate with Rm
            if (_rm != ArmRegister.None)
            {
                // Get value and shift if requested
                var value = armCore.GetRegValue(_rm);
                var carry = Shift.ShiftValue(ref value, _shiftInst, _shiftCount);

                // XOR if MVN or MVNS
                if (_command.Opcode == ArmOpCode.Mvn || _command.Opcode == ArmOpCode.Mvns)
                {
                    value ^= -1; // XOR with 0xFFFFFFFF
                }

                // Set status flags if requested
                if (_command.Opcode == ArmOpCode.Movs || _command.Opcode == ArmOpCode.Mvns)
                {
                    armCore.SetNzcvFlags(new Flags(true, true, true, false),
                        new Flags(value < 0, value == 0, carry, false));
                }
                   
                // Write value to Rd
                armCore.SetRegValue(_rd, value);
                return true;
            }

            // XOR immediate if requested
            if (_command.Opcode == ArmOpCode.Mvn || _command.Opcode == ArmOpCode.Mvns)
            {
                _immediate ^= -1; // XOR with 0xFFFFFFFF
            }

            // Write immediate to Rd
            armCore.SetRegValue(_rd, _immediate);
            return true;
        }
    }
}
