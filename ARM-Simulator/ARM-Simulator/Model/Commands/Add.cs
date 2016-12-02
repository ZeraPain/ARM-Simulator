using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;

namespace ARM_Simulator.Model.Commands
{
    internal class Add : ICommand
    {
        private readonly string[] _parameters;
        private ArmRegister _destReg;
        private ArmRegister _srcReg1;
        private ArmRegister _srcReg2;
        private int _immediate;
        private ShiftInstruction _shiftInst;
        private byte _shiftCount;
        private bool _decoded;

        public Add(string[] parameters)
        {
            _parameters = parameters;
            _destReg = ArmRegister.None;
            _srcReg1 = ArmRegister.None;
            _srcReg2 = ArmRegister.None;
            _immediate = 0;
            _shiftInst = ShiftInstruction.None;
            _shiftCount = 0;
            _decoded = false;
        }

        public bool Decode()
        {
            // Check Parameter Count
            if (_parameters.Length != 3 && _parameters.Length != 4)
                throw new ArgumentException("Invalid parameter count");

            // Check Destination Register
            Enum.TryParse(_parameters[0], true, out _destReg);
            if (_destReg == ArmRegister.None)
                throw new ArgumentException("Invalid destination register");

            // Check Source Register1
            Enum.TryParse(_parameters[1], true, out _srcReg1);
            if (_srcReg1 == ArmRegister.None)
                throw new ArgumentException("Invalid source register");

            // Want to add an immediate?
            if (_parameters[2].StartsWith("#"))
            {
                if (_parameters.Length != 3)
                    throw new ArgumentException("Invalid parameter count");

                _immediate = ArmDecoder.ParseImmediate(_parameters[2]);

                _decoded = true;
                return true;
            }

            // Check Source Register2
            Enum.TryParse(_parameters[2], true, out _srcReg2);
            if (_srcReg2 == ArmRegister.None)
                throw new ArgumentException("Invalid source register");

            // Check for Shift Instruction
            if (_parameters.Length == 4)
                ArmDecoder.ParseShiftInstruction(_parameters[3], ref _shiftInst, ref _shiftCount);

            _decoded = true;
            return true;
        }

        public bool Execute(ArmCore armCore)
        {
            if (!_decoded)
                throw new Exception("Cannot execute an undecoded command");

            var value = armCore.GetRegValue(_srcReg1);

            if (_srcReg2 != ArmRegister.None)
            {
                value = armCore.GetRegValue(_srcReg2);
            }

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

            if (_srcReg2 != ArmRegister.None)
            {
                armCore.SetRegValue(_destReg, armCore.GetRegValue(_srcReg1) + value);
            }
            else
            {
                // Use Immediate
                armCore.SetRegValue(_destReg, value + _immediate);
            }

            return true;
        }
    }
}
