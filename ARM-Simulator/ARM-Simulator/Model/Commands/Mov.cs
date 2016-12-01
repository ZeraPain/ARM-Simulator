using System;
using System.Collections.Generic;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;

namespace ARM_Simulator.Model.Commands
{
    internal class Mov : ICommand
    {
        private readonly string[] _parameters;
        private ArmRegister _destReg;
        private ArmRegister _srcReg;
        private int _immediate;
        private ShiftInstruction _shiftInst;
        private byte _shiftCount;
        private bool _decoded;

        public Mov(string[] parameters)
        {
            _parameters = parameters;
            _decoded = false;
            _shiftInst = ShiftInstruction.None;
            _shiftCount = 0;
        }

        public bool Decode()
        {
            // Check Parameter Count
            if (_parameters.Length != 2 && _parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Check Destination Register
            Enum.TryParse(_parameters[0], true, out _destReg);
            if (_destReg == ArmRegister.None)
                throw new ArgumentException("Invalid destination register");

            // Want to move an immediate?
            if (_parameters[1].StartsWith("#"))
            {
                if (_parameters.Length != 2)
                    throw new ArgumentException("Invalid parameter count");

                _immediate = ArmDecoder.ParseImmediate(_parameters[1]);

                _decoded = true;
                return true;
            }

            // Check Source Register
            Enum.TryParse(_parameters[1], true, out _srcReg);
            if (_srcReg == ArmRegister.None)
                throw new ArgumentException("Invalid source register");

            // Check for Shift Instruction
            if (_parameters.Length == 3)
                ArmDecoder.ParseShiftInstruction(_parameters[2], ref _shiftInst, ref _shiftCount);

            _decoded = true;
            return true;
        }

        public bool Execute(Dictionary<ArmRegister, int> registers)
        {
            if (!_decoded)
                throw new Exception("Cannot execute an undecoded command");

            if (!registers.ContainsKey(_destReg))
                throw new Exception("Invalid Register was requested");

            if (_srcReg != ArmRegister.None) // Use Register
            {
                if (!registers.ContainsKey(_srcReg))
                    throw new Exception("Invalid Register was requested");

                var value = registers[_srcReg];

                switch (_shiftInst)
                {
                    case ShiftInstruction.None:
                        break;
                    case ShiftInstruction.Lsr:
                        value = value >> _shiftCount;
                        break;
                    case ShiftInstruction.Lsl:
                        value = value << _shiftCount;
                        break;
                    case ShiftInstruction.Ror:
                        value = (value >> _shiftCount) | (value << (32 - _shiftCount));
                        break;
                    case ShiftInstruction.Rol:
                        value = (value << _shiftCount) | (value >> (32 - _shiftCount));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                registers[_destReg] = value;
                return true;
            }

            // Use Immediate
            registers[_destReg] = _immediate;
            return true;
        }
    }
}
