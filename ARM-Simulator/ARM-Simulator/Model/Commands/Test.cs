using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;

namespace ARM_Simulator.Model.Commands
{
    internal class Test : ICommand
    {
        // Required
        private Command _command;
        private ArmRegister _rn;

        // Optional
        private ArmRegister _rm;
        private short _immediate;
        private ShiftInstruction _shiftInst;
        private byte _shiftCount;
        private bool _decoded;

        public Test(Command command)
        {
            _command = command;
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
            if (parameters.Length != 2 && parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Parse Source Register
            _rn = ArmDecoder.ParseRegister(parameters[0]);

            // Check for Rm or 8 bit immediate
            ArmDecoder.ParseOperand2(parameters[1], ref _rm, ref _immediate);

            // Check for Shift Instruction
            if (_rm != ArmRegister.None && parameters.Length == 3)
                ArmDecoder.ParseShiftInstruction(parameters[2], ref _shiftInst, ref _shiftCount);

            _decoded = true;
            return true;
        }

        public bool Execute(ArmCore armCore)
        {
            if (!_decoded)
                throw new Exception("Cannot execute an undecoded command");

            int value;

            if (_rm != ArmRegister.None)
            {
                // Get Register which may be shifted
                value = armCore.GetRegValue(_rm);
                var carry = Shift.ShiftValue(ref value, _shiftInst, _shiftCount);

                switch (_command.Opcode)
                {
                    case ArmOpCode.Tst:
                        value = armCore.GetRegValue(_rn) & value;
                        break;
                    case ArmOpCode.Teq:
                        value = armCore.GetRegValue(_rn) ^ value;
                        break;
                    default:
                        throw new ArgumentException("Invalid Opcode");
                }

                // Set condition flags
                armCore.SetNzcvFlags(new Flags(true, true, true, false),
                    new Flags(value < 0, value == 0, carry, false));

                return true;
            }

            switch (_command.Opcode)
            {
                case ArmOpCode.Tst:
                    value = armCore.GetRegValue(_rn) & _immediate;
                    break;
                case ArmOpCode.Teq:
                    value = armCore.GetRegValue(_rn) ^ _immediate;
                    break;
                default:
                    throw new ArgumentException("Invalid Opcode");
            }

            // Set condition flags
            armCore.SetNzcvFlags(new Flags(true, true, true, false),
                    new Flags(value < 0, value == 0, false, false));

            return true;
        }
    }
}
