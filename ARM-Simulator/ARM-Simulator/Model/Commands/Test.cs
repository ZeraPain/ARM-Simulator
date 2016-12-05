﻿using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Test : ICommand
    {
        // Required
        private readonly Opcode? _opcode;
        private Register? _rn;

        // Optional
        private readonly string[] _parameters;
        private Register? _rm;
        private short _immediate;
        private ShiftInstruction? _shiftInst;
        private byte _shiftCount;
        private bool _decoded;

        public Test(Opcode opcode, string[] parameters)
        {
            _opcode = opcode;
            _parameters = parameters;
            _decoded = false;
            Parse();
        }

        public Test(Opcode opcode, Register? rn, Register? rm, short immediate, ShiftInstruction? shiftInst, byte shiftCount)
        {
            _opcode = opcode;
            _rn = rn;
            _rm = rm;
            _immediate = immediate;
            _shiftInst = shiftInst;
            _shiftCount = shiftCount;
            _decoded = true;
        }

        public void Parse()
        {
            if (_decoded)
                throw new Exception("Cannot parse a decoded command");

            // Check Parameter Count
            if (_parameters.Length != 2 && _parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Parse Source Register
            _rn = Parser.ParseRegister(_parameters[0]);

            // Check for Rm or 8 bit immediate
            Parser.ParseOperand2(_parameters[1], ref _rm, ref _immediate);

            // Check for Shift Instruction
            if (_rm != null && _parameters.Length == 3)
                Parser.ParseShiftInstruction(_parameters[2], ref _shiftInst, ref _shiftCount);

            _decoded = true;
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
            bw.WriteBits(1, 20, 1); // Set condition codes
            if (_rn != null) bw.WriteBits((int)_rn, 16, 4); // 1st operand
            bw.WriteBits(0, 12, 4); // Set Rd to 0

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

            var value = _rm != null ? armCore.GetRegValue(_rm) : _immediate;
            var carry = _rm != null && Shift.ShiftValue(ref value, _shiftInst, _shiftCount);

            switch (_opcode)
            {
                case Opcode.Tst:
                    value = armCore.GetRegValue(_rn) & value;
                    break;
                case Opcode.Teq:
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
    }
}
