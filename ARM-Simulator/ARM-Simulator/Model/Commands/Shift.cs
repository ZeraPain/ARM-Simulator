using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;

namespace ARM_Simulator.Model.Commands
{
    internal class Shift : ICommand
    {
        // Required
        private Command _command;
        private ArmRegister _rd;
        private ArmRegister _rm;
        private ShiftInstruction _shiftInst;
        private byte _shiftCount;

        // Optional
        private bool _decoded;

        public Shift(Command command)
        {
            _command = command;
            _rd = ArmRegister.None;
            _rm = ArmRegister.None;
            _shiftInst = ShiftInstruction.None;
            _shiftCount = 0;
            _decoded = false;
        }

        public bool Decode()
        {
            var parameters = _command.Parameters;

            // Check Parameter Count
            if (parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Parse Destination Register / Source Register
            _rd = ArmDecoder.ParseRegister(parameters[0]);
            _rm = ArmDecoder.ParseRegister(parameters[1]);
            if (parameters[2].StartsWith("#"))
            {
                _shiftCount = (byte)ArmDecoder.ParseImmediate(parameters[2], 12);
            }

            switch (_command.Opcode)
            {
                case ArmOpCode.Lsl:
                case ArmOpCode.Lsls:
                    _shiftInst = ShiftInstruction.Lsl;
                    break;
                case ArmOpCode.Lsr:
                case ArmOpCode.Lsrs:
                    _shiftInst = ShiftInstruction.Lsr;
                    break;
                case ArmOpCode.Ror:
                case ArmOpCode.Rors:
                    _shiftInst = ShiftInstruction.Ror;
                    break;
                case ArmOpCode.Asr:
                case ArmOpCode.Asrs:
                    _shiftInst = ShiftInstruction.Asr;
                    break;
                default:
                    throw new ArgumentException("Invalid Opcode");
            }

            _decoded = true;
            return true;
        }

        public bool Execute(ArmCore armCore)
        {
            if (!_decoded)
                throw new Exception("Cannot execute an undecoded command");

            // Get Register which may be shifted
            var value = armCore.GetRegValue(_rm);
            var carry = ShiftValue(ref value, _shiftInst, _shiftCount);

            if (_command.Opcode == ArmOpCode.Lsls || _command.Opcode == ArmOpCode.Lsrs || _command.Opcode == ArmOpCode.Rors || _command.Opcode == ArmOpCode.Asrs)
            {
                armCore.SetNzcvFlags(new Flags(true, true, true, false),
                    new Flags(value < 0, value == 0, carry, false));
            }

            armCore.SetRegValue(_rd, value);
            return true;
        }

        public static bool ShiftValue(ref int value, ShiftInstruction shiftInstruction, byte shiftCount)
        {
            var carry = 0;

            switch (shiftInstruction)
            {
                case ShiftInstruction.None:
                    break;
                case ShiftInstruction.Lsr:
                    carry = shiftCount > 32 ? 0 : value & (1 << (shiftCount - 1));
                    value = value >> shiftCount;
                    break;
                case ShiftInstruction.Lsl:
                    carry = shiftCount > 32 ? 0 : value & (1 << (32 - shiftCount));
                    value = value << shiftCount;
                    break;
                case ShiftInstruction.Ror:
                    carry = shiftCount > 32 ? 0 : value & (1 << (shiftCount - 1));
                    value = (value >> shiftCount) | (value << (32 - shiftCount));
                    break;
                case ShiftInstruction.Asr:
                    carry = shiftCount > 32 ? 0 : value & (1 << (shiftCount - 1));
                    value = value >> shiftCount;
                    value |= -1 << (32 - shiftCount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return carry > 0;
        }
    }
}
