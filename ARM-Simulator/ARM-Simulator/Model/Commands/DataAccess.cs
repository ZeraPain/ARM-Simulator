using System;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class DataAccess : Base
    {
        public DataAccess(ECondition condition, EMemOpcode opcode, string[] parameters)
        {
            Arithmetic = false;
            Condition = condition;
            _opcode = opcode;
            Immediate = 0;
            Decoded = false;
            _postIndex = false;
            Parse(parameters);
        }

        public DataAccess(ECondition condition, EMemOpcode opcode, bool writeBack, bool postIndex, ERegister? rd, ERegister? rn, ERegister? rm, short immediate, EShiftInstruction? shiftInst, byte shiftCount)
        {
            Arithmetic = false;
            Condition = condition;
            _opcode = opcode;
            Rd = rd;
            Rn = rn;
            Rm = rm;
            _writeBack = writeBack;
            _postIndex = postIndex;
            Immediate = immediate;
            ShiftInst = shiftInst;
            ShiftCount = shiftCount;
            Decoded = true;
        }

        private void ParseSource(string sourceString)
        {
            var source = sourceString.Split(',');
            if (source.Length > 3)
                throw new ArgumentException("Invalid syntax");

            Rn = Parser.ParseRegister(source[0]);

            if (source.Length > 1)
            {
                if (source[1].StartsWith("#"))
                    Immediate = Parser.ParseImmediate(source[1], 12);
                else
                    Rm = Parser.ParseRegister(source[1]);
            }

            if (source.Length > 2)
            {
                if (Rm != null)
                    Parser.ParseShiftInstruction(source[2], ref ShiftInst, ref ShiftCount);
                else
                    throw new ArgumentException("Invalid syntax");
            }   
        }

        public sealed override void Parse(string[] parameters)
        {
            if (Decoded)
                throw new Exception("Cannot parse a decoded command");

            // Check Parameter Count
            if (parameters.Length != 2 && parameters.Length != 3)
                throw new ArgumentException("Invalid parameter count");

            // Parse Destination Register
            if (!parameters[0].EndsWith(","))
                throw new ArgumentException("Invalid syntax");
            Rd = Parser.ParseRegister(parameters[0].Substring(0, parameters[0].Length - 1));

            // Parse Source Address
            ParseSource(parameters[1]);

            if (parameters.Length == 3)
            {
                if (parameters[2].Equals("!"))
                    _writeBack = true;
                else if (parameters[2].StartsWith(","))
                {
                    parameters[2] = parameters[2].Substring(1);

                    if (parameters[2].StartsWith("#"))
                    {
                        Immediate = Parser.ParseImmediate(parameters[2], 12);
                    }
                    else
                        Rm = Parser.ParseRegister(parameters[2]);

                    _writeBack = true;
                    _postIndex = true;
                } 
                else
                    throw new ArgumentException("Invalid syntax");
            }

            Decoded = true;
        }

        public override void Execute(Core armCore)
        {
            if (!Decoded)
                throw new Exception("Cannot execute an undecoded command");

            if (!CheckConditions(armCore.GetRegValue(ERegister.Cpsr)))
                return;

            int value;

            if (Rm != null)
            {
                // Get Register which may be shifted
                value = armCore.GetRegValue(Rm);
                Shift.ShiftValue(ref value, ShiftInst, ShiftCount);
            }
            else
            {
                value = Immediate;
            }

            switch (_opcode)
            {
                case EMemOpcode.Ldr:
                    armCore.SetRegValue(Rd,
                        _postIndex
                            ? armCore.Ram.ReadInt((uint) armCore.GetRegValue(Rn))
                            : armCore.Ram.ReadInt((uint) (armCore.GetRegValue(Rn) + value)));
                    break;
                case EMemOpcode.Str:
                    armCore.Ram.WriteInt(
                        _postIndex
                            ? (uint)armCore.GetRegValue(Rn)
                            : (uint)(armCore.GetRegValue(Rn) + value),
                            armCore.GetRegValue(Rd));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_writeBack)
                armCore.SetRegValue(Rn, armCore.GetRegValue(Rn) + value);
        }
    }
}
