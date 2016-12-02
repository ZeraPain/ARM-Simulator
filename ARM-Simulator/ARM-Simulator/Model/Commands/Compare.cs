using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;

namespace ARM_Simulator.Model.Commands
{
    internal class Compare : ICommand
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

        public Compare(Command command)
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
                Shift.ShiftValue(ref value, _shiftInst, _shiftCount);
            }
            else
            {
                value = _immediate;
            }

            switch (_command.Opcode)
            {
                case ArmOpCode.Cmp:
                    armCore.SetNzcvFlags(new Flags(true, true, true, true),
                        Substract.CheckFlags(armCore.GetRegValue(_rn), value));
                    break;
                case ArmOpCode.Cmn:
                    armCore.SetNzcvFlags(new Flags(true, true, true, true),
                        Add.CheckFlags(armCore.GetRegValue(_rn), value));
                    break;
                default:
                    throw new ArgumentException("Invalid Opcode");
            }

            return true;
        }
    }
}
