using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Add : ICommand
    {
        // Required
        private bool _setConditionFlags;
        private Register? _rd;
        private Register? _rn;

        // Optional
        private Register? _rm;
        private short _immediate;
        private ShiftInstruction? _shiftInst;
        private byte _shiftCount;
        private bool _decoded;

        public Add()
        {
            _setConditionFlags = false;
            _rd = null;
            _rn = null;
            _rm = null;
            _immediate = 0;
            _shiftInst = null;
            _shiftCount = 0;
            _decoded = false;
        }

        public Add(bool setConditionFlags, Register? rd, Register? rn, Register? rm, short immediate, ShiftInstruction? shiftInst, byte shiftCount)
        {
            _setConditionFlags = setConditionFlags;
            _rd = rd;
            _rn = rn;
            _rm = rm;
            _immediate = immediate;
            _shiftInst = shiftInst;
            _shiftCount = shiftCount;
            _decoded = true;
        }

        public bool Parse(Command command)
        {
            var parameters = command.Parameters;
            _setConditionFlags = command.SetConditionFlags;

            // Check parameter count
            if (parameters.Length != 3 && parameters.Length != 4)
                throw new ArgumentException("Invalid parameter count");

            // Parse Rd, Rn
            _rd = Parser.ParseRegister(parameters[0]);
            _rn = Parser.ParseRegister(parameters[1]);

            if (!_setConditionFlags)
            {
                if (parameters[2].StartsWith("#"))
                {
                    if (parameters.Length != 3)
                        throw new ArgumentException("Invalid parameter count");

                    // Parse 12 bit immediate
                    _immediate = Parser.ParseImmediate(parameters[2], 12);

                    _decoded = true;
                    return true;
                }
            }

            // Check for Rm or 8 bit immediate
            Parser.ParseOperand2(parameters[2], ref _rm, ref _immediate);

            // Check for shift instruction
            if (_rm != null && parameters.Length == 4)
                Parser.ParseShiftInstruction(parameters[3], ref _shiftInst, ref _shiftCount);

            _decoded = true;
            return true;
        }

        public int Encode()
        {
            if (!_decoded)
                throw new Exception("Cannot convert an undecoded command");

            var bw = new BitWriter();
            
            bw.WriteBits(0, 28, 4); // Condition flags
            bw.WriteBits(0, 27, 1); // Empty
            bw.WriteBits(0, 26, 1); // Arithmetic
            bw.WriteBits(_rm != null ? 0 : 1, 25, 1); // Bool immediate?
            bw.WriteBits((int)Opcode.Add, 21, 4); // Opcode
            bw.WriteBits(_setConditionFlags ? 1 : 0, 20, 1); // Set condition codes
            if (_rn != null) bw.WriteBits((int)_rn, 16, 4); // 1st operand
            if (_rd != null) bw.WriteBits((int)_rd, 12, 4); // destination

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

            int value;

            // Add two registers
            if (_rm != null)
            {
                // Get Register which may be shifted
                value = armCore.GetRegValue(_rm);
                Shift.ShiftValue(ref value, _shiftInst, _shiftCount);
            }
            // Add immediate to Rd
            else
            {
                value = _immediate;
            }

            if (_setConditionFlags)
            {
                armCore.SetNzcvFlags(new Flags(true, true, true, true),
                    CheckFlags(armCore.GetRegValue(_rn), value));
            }

            armCore.SetRegValue(_rd, armCore.GetRegValue(_rn) + value);
            return true;
        }

        public static Flags CheckFlags(int regValue, int addValue)
        {
            var newValue = (long)regValue + addValue;

            var n = (int) newValue < 0;
            var z = (int) newValue == 0;
            var c = (newValue & 0x100000000) > 0;
            var v = newValue < int.MinValue || newValue > int.MaxValue;

            return new Flags(n, z, c, v);
        }
    }
}
