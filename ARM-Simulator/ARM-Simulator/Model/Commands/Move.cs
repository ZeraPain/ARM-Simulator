using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Move : ICommand
    {
        // Required
        private Opcode? _opcode;
        private bool _setConditionFlags;
        private Register? _rd;

        // Optional
        private Register? _rm;
        private short _immediate;
        private ShiftInstruction? _shiftInst;
        private byte _shiftCount;
        private bool _decoded;

        public Move()
        {
            _setConditionFlags = false;
            _opcode = null;
            _rd = null;
            _rm = null;
            _immediate = 0;
            _shiftInst = null;
            _shiftCount = 0;
            _decoded = false;
        }

        public Move(Opcode? opcode, bool setConditionFlags, Register? rd, Register? rm, short immediate, ShiftInstruction? shiftInst, byte shiftCount)
        {
            _opcode = opcode;
            _setConditionFlags = setConditionFlags;
            _rd = rd;
            _rm = rm;
            _immediate = immediate;
            _shiftInst = shiftInst;
            _shiftCount = shiftCount;
            _decoded = true;
        }

        public bool Parse(Command command)
        {
            var parameters = command.Parameters;
            _opcode = command.Opcode;
            _setConditionFlags = command.SetConditionFlags;

            // Check for valid parameter count
            if (parameters.Length != 2 && parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Parse Rd
            _rd = Parser.ParseRegister(parameters[0]);

            if (!_setConditionFlags && command.Opcode == Opcode.Mov) // Not valid for Mvn
            {
                if (parameters[1].StartsWith("#"))
                {
                    if (parameters.Length != 2)
                        throw new ArgumentException("Invalid parameter count");

                    // Parse 16 bit immediate
                    _immediate = Parser.ParseImmediate(parameters[1], 12);

                    _decoded = true;
                    return true;
                }
            }

            // Check for Rm or 8 bit immediate
            Parser.ParseOperand2(parameters[1], ref _rm, ref _immediate);

            // Check for shift instruction
            if (_rm != null && parameters.Length == 3)
                Parser.ParseShiftInstruction(parameters[2], ref _shiftInst, ref _shiftCount);

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
            if (_opcode != null) bw.WriteBits((int)_opcode, 21, 4); // Opcode
            bw.WriteBits(_setConditionFlags ? 1 : 0, 20, 1); // Set condition codes
            bw.WriteBits(0, 16, 4); // Set Rn to 0
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

            // calculate with Rm
            if (_rm != null)
            {
                // Get value and shift if requested
                var value = armCore.GetRegValue(_rm);
                var carry = Shift.ShiftValue(ref value, _shiftInst, _shiftCount);

                // XOR if MVN or MVNS
                if (_opcode == Opcode.Mvn)
                {
                    value ^= -1; // XOR with 0xFFFFFFFF
                }

                // Set status flags if requested
                if (_setConditionFlags)
                {
                    armCore.SetNzcvFlags(new Flags(true, true, true, false),
                        new Flags(value < 0, value == 0, carry, false));
                }
                   
                // Write value to Rd
                armCore.SetRegValue(_rd, value);
                return true;
            }

            // XOR immediate if requested
            if (_opcode == Opcode.Mvn)
            {
                _immediate ^= -1; // XOR with 0xFFFFFFFF
            }

            // Set status flags if requested
            if (_setConditionFlags)
            {
                armCore.SetNzcvFlags(new Flags(true, true, true, false),
                    new Flags(_immediate < 0, _immediate == 0, false, false));
            }

            // Write immediate to Rd
            armCore.SetRegValue(_rd, _immediate);
            return true;
        }
    }
}
