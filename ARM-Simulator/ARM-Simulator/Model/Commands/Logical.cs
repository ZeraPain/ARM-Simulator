using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;

namespace ARM_Simulator.Model.Commands
{
    internal class Logical : ICommand
    {
        // Required
        private Opcode? _opcode;
        private bool _setConditionFlags;
        private Register? _rd;
        private Register? _rn;

        // Optional
        private Register? _rm;
        private short _immediate;
        private ShiftInstruction? _shiftInst;
        private byte _shiftCount;
        private bool _decoded;

        public Logical()
        {
            _opcode = null;
            _setConditionFlags = false;
            _rd = null;
            _rn = null;
            _rm = null;
            _immediate = 0;
            _shiftInst = null;
            _shiftCount = 0;
            _decoded = false;
        }

        public Logical(Opcode opcode, bool setConditionFlags, Register? rd, Register? rn, Register? rm, short immediate, ShiftInstruction? shiftInst, byte shiftCount)
        {
            _opcode = opcode;
            _setConditionFlags = setConditionFlags;
            _rd = rd;
            _rn = rn;
            _rm = rm;
            _immediate = immediate;
            _shiftInst = shiftInst;
            _shiftCount = shiftCount;
            _decoded = true;
        }

        public bool Decode(Command command)
        {
            var parameters = command.Parameters;
            _opcode = command.Opcode;
            _setConditionFlags = command.SetConditionFlags;

            // Check Parameter Count
            if (parameters.Length != 3 && parameters.Length != 4)
                throw new ArgumentException("Invalid parameter count");

            // Parse Destination Register / Source Register
            _rd = Parser.ParseRegister(parameters[0]);
            _rn = Parser.ParseRegister(parameters[1]);

            // Check for Rm or 8 bit immediate
            Parser.ParseOperand2(parameters[2], ref _rm, ref _immediate);

            // Check for Shift Instruction
            if (_rm != null && parameters.Length == 4)
                Parser.ParseShiftInstruction(parameters[3], ref _shiftInst, ref _shiftCount);

            _decoded = true;
            return true;
        }

        public int GetBitCommand()
        {
            if (!_decoded)
                throw new Exception("Cannot convert an undecoded command");

            var bw = new BitWriter();

            bw.WriteBits(0, 28, 4); // Condition flags
            bw.WriteBits(0, 27, 1); // Empty
            bw.WriteBits(0, 26, 1); // Arithmetic
            bw.WriteBits(_rm != null ? 0 : 1, 25, 1); // Bool immediate?
            if (_opcode != null) bw.WriteBits((int)_opcode, 21, 4); // Opcode
            bw.WriteBits(_setConditionFlags ? 1 : 0, 20, 1); // Set condition codes
            bw.WriteBits((int)_rn, 16, 4); // 1st operand
            bw.WriteBits((int)_rd, 12, 4); // destination

            if (_rm != null)
            {
                if (_shiftInst != null)
                {
                    bw.WriteBits(_shiftCount, 7, 5);
                    bw.WriteBits((int)_shiftInst, 5, 2);
                    bw.WriteBits(0, 4, 1);
                }
                bw.WriteBits((int)_rm, 0, 4);
            }
            else
            {
                bw.WriteBits(_immediate, 0, 12);
            }

            return bw.GetValue();
        }

        public bool Execute(Core armCore)
        {
            if (!_decoded)
                throw new Exception("Cannot execute an undecoded command");

            // Get Register which may be shifted
            var value = armCore.GetRegValue(_rm);
            var carry = Shift.ShiftValue(ref value, _shiftInst, _shiftCount);

            switch (_opcode)
            {
                case Opcode.And:
                    value = armCore.GetRegValue(_rn) & value;
                    break;
                case Opcode.Eor:
                    value = armCore.GetRegValue(_rn) ^ value;
                    break;
                case Opcode.Orr:
                    value = armCore.GetRegValue(_rn) | value;
                    break;
                case Opcode.Bic:
                    value = armCore.GetRegValue(_rn) & ~value;
                    break;
                default:
                    throw new ArgumentException("Invalid Opcode");
            }

            if (_setConditionFlags)
            {
                armCore.SetNzcvFlags(new Flags(true, true, true, false),
                        new Flags(value < 0, value == 0, carry, false));
            }

            armCore.SetRegValue(_rd, value);
            return true;
        }
    }
}
