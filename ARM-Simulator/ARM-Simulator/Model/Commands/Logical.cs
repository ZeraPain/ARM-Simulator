using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;

namespace ARM_Simulator.Model.Commands
{
    internal class Logical : ICommand
    {
        // Required
        private Command _command;
        private ArmRegister _rd;
        private ArmRegister _rn;

        // Optional
        private ArmRegister _rm;
        private short _immediate;
        private ShiftInstruction _shiftInst;
        private byte _shiftCount;
        private bool _decoded;

        public Logical(Command command)
        {
            _command = command;
            _rd = ArmRegister.None;
            _rn = ArmRegister.None;
            _rm = ArmRegister.None;
            _immediate = 0;
            _shiftInst = ShiftInstruction.None;
            _shiftCount = 0;
            _decoded = false;
        }

        public bool Decode()
        {
            var parameters = _command.Parameters;

            // Check Parameter Count
            if (parameters.Length != 3 && parameters.Length != 4)
                throw new ArgumentException("Invalid parameter count");

            // Parse Destination Register / Source Register
            _rd = ArmDecoder.ParseRegister(parameters[0]);
            _rn = ArmDecoder.ParseRegister(parameters[1]);

            // Check for Rm or 8 bit immediate
            ArmDecoder.ParseOperand2(parameters[2], ref _rm, ref _immediate);

            // Check for Shift Instruction
            if (_rm != ArmRegister.None && parameters.Length == 4)
                ArmDecoder.ParseShiftInstruction(parameters[3], ref _shiftInst, ref _shiftCount);

            _decoded = true;
            return true;
        }

        public bool Execute(ArmCore armCore)
        {
            if (!_decoded)
                throw new Exception("Cannot execute an undecoded command");

            // Get Register which may be shifted
            var value = armCore.GetRegValue(_rm);
            var carry = Shift.ShiftValue(ref value, _shiftInst, _shiftCount);

            switch (_command.Opcode)
            {
                case ArmOpCode.And:
                case ArmOpCode.Ands:
                    value = armCore.GetRegValue(_rn) & value;
                    break;
                case ArmOpCode.Eor:
                case ArmOpCode.Eors:
                    value = armCore.GetRegValue(_rn) ^ value;
                    break;
                case ArmOpCode.Orr:
                case ArmOpCode.Orrs:
                    value = armCore.GetRegValue(_rn) | value;
                    break;
                case ArmOpCode.Orn:
                case ArmOpCode.Orns:
                    value = armCore.GetRegValue(_rn) | ~value;
                    break;
                case ArmOpCode.Bic:
                case ArmOpCode.Bics:
                    value = armCore.GetRegValue(_rn) & ~value;
                    break;
                default:
                    throw new ArgumentException("Invalid Opcode");
            }

            if (_command.Opcode == ArmOpCode.Ands)
            {
                armCore.SetNzcvFlags(new Flags(true, true, true, false),
                        new Flags(value < 0, value == 0, carry, false));
            }

            armCore.SetRegValue(_rd, value);
            return true;
        }
    }
}
