﻿using System;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;

namespace ARM_Simulator.Model.Commands
{
    internal class Add : ICommand
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

        public Add(Command command)
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

            // Check parameter count
            if (parameters.Length != 3 && parameters.Length != 4)
                throw new ArgumentException("Invalid parameter count");

            // Parse Rd, Rn
            _rd = ArmDecoder.ParseRegister(parameters[0]);
            _rn = ArmDecoder.ParseRegister(parameters[1]);

            switch (_command.Opcode)
            {     
                case ArmOpCode.Adds:
                    // Check for Rm or 8 bit immediate
                    ArmDecoder.ParseOperand2(parameters[2], ref _rm, ref _immediate);
                    break;
                case ArmOpCode.Add:
                    // Check if an immediate needs to be moved
                    if (parameters[2].StartsWith("#"))
                    {
                        if (parameters.Length != 3)
                            throw new ArgumentException("Invalid parameter count");

                        // Parse 12 bit immediate
                        _immediate = ArmDecoder.ParseImmediate(parameters[2], 12);

                        _decoded = true;
                        return true;
                    }

                    // Check for Rm or 8 bit immediate
                    ArmDecoder.ParseOperand2(parameters[2], ref _rm, ref _immediate);
                    break;
                default:
                    throw new ArgumentException("Invalid Opcode");
            }

            // Check for shift instruction
            if (_rm != ArmRegister.None && parameters.Length == 4)
                ArmDecoder.ParseShiftInstruction(parameters[3], ref _shiftInst, ref _shiftCount);

            _decoded = true;
            return true;
        }

        public bool Execute(ArmCore armCore)
        {
            if (!_decoded)
                throw new Exception("Cannot execute an undecoded command");

            int value;

            // Add two registers
            if (_rm != ArmRegister.None)
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

            if (_command.Opcode == ArmOpCode.Adds)
            {
                armCore.SetNzcvFlags(new Flags(true, true, true, true),
                    CheckFlags(armCore.GetRegValue(_rn), value));
            }

            armCore.SetRegValue(_rd, armCore.GetRegValue(_rn) + value);
            return true;
        }

        public static Flags CheckFlags(int oldValue, int addValue)
        {
            var newValue = (long)oldValue + addValue;

            var n = (int) newValue < 0;
            var z = (int) newValue == 0;
            var c = (newValue & 0x100000000) > 0;
            var v = (oldValue >= 0) && n;

            return new Flags(n, z, c, v);
        }
    }
}
