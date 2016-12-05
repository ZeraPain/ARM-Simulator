using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class DataAccess : ICommand
    {
        // Required
        private readonly MemOpcode? _opcode;
        private Register? _rd;
        private Register? _rn;
        private bool _writeBack;
        private bool _postIndex;

        // Optional
        private readonly string[] _parameters;
        private Register? _rm;
        private short _immediate;
        private ShiftInstruction? _shiftInst;
        private byte _shiftCount;
        private bool _decoded;

        public DataAccess(MemOpcode opcode, string[] parameters)
        {
            _opcode = opcode;
            _parameters = parameters;
            _immediate = 0;
            _decoded = false;
            _postIndex = false;
            Parse();
        }

        public DataAccess(MemOpcode opcode, bool writeBack, bool postIndex, Register? rd, Register? rn, Register? rm, short immediate, ShiftInstruction? shiftInst, byte shiftCount)
        {
            _opcode = opcode;
            _rd = rd;
            _rn = rn;
            _rm = rm;
            _writeBack = writeBack;
            _postIndex = postIndex;
            _immediate = immediate;
            _shiftInst = shiftInst;
            _shiftCount = shiftCount;
            _decoded = true;
        }

        private void ParseSource(string sourceString)
        {
            var source = sourceString.Split(',');
            if (source.Length > 3)
                throw new ArgumentException("Invalid syntax");

            _rn = Parser.ParseRegister(source[0]);

            if (source.Length > 1)
            {
                if (source[1].StartsWith("#"))
                    _immediate = Parser.ParseImmediate(source[1], 12);
                else
                    _rm = Parser.ParseRegister(source[1]);
            }

            if (source.Length > 2)
            {
                if (_rm != null)
                    Parser.ParseShiftInstruction(source[2], ref _shiftInst, ref _shiftCount);
                else
                    throw new ArgumentException("Invalid syntax");
            }   
        }

        public void Parse()
        {
            if (_decoded)
                throw new Exception("Cannot parse a decoded command");

            // Check Parameter Count
            if (_parameters.Length != 2 && _parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Parse Destination Register
            if (!_parameters[0].EndsWith(","))
                throw new ArgumentException("Invalid syntax");
            _rd = Parser.ParseRegister(_parameters[0].Substring(0, _parameters[0].Length - 1));

            // Parse Source Address
            ParseSource(_parameters[1]);

            if (_parameters.Length == 3)
            {
                if (_parameters[2].Equals("!"))
                    _writeBack = true;
                else if (_parameters[2].StartsWith(","))
                {
                    _parameters[2] = _parameters[2].Substring(1);

                    if (_parameters[2].StartsWith("#"))
                    {
                        _immediate = Parser.ParseImmediate(_parameters[2], 12);
                    }
                    else
                        _rm = Parser.ParseRegister(_parameters[2]);

                    _writeBack = true;
                    _postIndex = true;
                } 
                else
                    throw new ArgumentException("Invalid syntax");
            }

            _decoded = true;
        }

        public int Encode()
        {
            if (!_decoded)
                throw new Exception("Cannot convert an undecoded command");

            var bw = new BitWriter();

            bw.WriteBits(0, 28, 4); // Condition flags
            bw.WriteBits(0, 27, 1); // Empty
            bw.WriteBits(1, 26, 1); // Data Access
            bw.WriteBits(_rm != null ? 0 : 1, 25, 1); // Bool immediate?
            bw.WriteBits(_postIndex ? 1 : 0, 24, 1);
            bw.WriteBits(0, 23, 1); // Up / Down
            bw.WriteBits(1, 22, 1); // Unsigned
            bw.WriteBits(_writeBack ? 1 : 0, 21, 1);
            if (_opcode != null) bw.WriteBits((int)_opcode, 20, 1);
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

            if (_rm != null)
            {
                // Get Register which may be shifted
                value = armCore.GetRegValue(_rm);
                Shift.ShiftValue(ref value, _shiftInst, _shiftCount);
            }
            else
            {
                value = _immediate;
            }

            switch (_opcode)
            {
                case MemOpcode.Ldr:
                    
                    if (_postIndex)
                        armCore.SetRegValue(_rd, armCore.Ram.ReadInt((uint)armCore.GetRegValue(_rn)));
                    else
                        armCore.SetRegValue(_rd, armCore.Ram.ReadInt((uint)(armCore.GetRegValue(_rn) + value)));
                    break;
                case MemOpcode.Str:
                    if (_postIndex)
                        armCore.Ram.WriteInt((uint)armCore.GetRegValue(_rn), armCore.GetRegValue(_rd));
                    else
                        armCore.Ram.WriteInt((uint)(armCore.GetRegValue(_rn) + value), armCore.GetRegValue(_rd));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_writeBack)
                armCore.SetRegValue(_rn, armCore.GetRegValue(_rn) + value);

            return true;
        }

    }
}
