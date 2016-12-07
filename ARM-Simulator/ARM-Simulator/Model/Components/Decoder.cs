using System;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Commands;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Components
{
    internal class Decoder
    {
        private ECondition _condition;
        private EOperation _operation;

        private ICommand DecodeArithmetic(BitReader br)
        {
            var useImmediate = br.ReadBits(25, 1) != 0;
            
            var opCode = (EOpcode)br.ReadBits(21, 4);
            var setConditionFlags = br.ReadBits(20, 1) == 1;
            var rn = (ERegister)br.ReadBits(16, 4);
            var rd = (ERegister)br.ReadBits(12, 4);

            var immediate = 0;
            byte shiftCount;

            EShiftInstruction? shiftInst = null;
            ERegister? rm = null;

            if (useImmediate)
            {
                shiftCount = (byte) br.ReadBits(8, 4);
                immediate = (byte)br.ReadBits(0, 8);
                Helper.ShiftValue(ref immediate, EShiftInstruction.Ror, shiftCount);
            }
            else
            {
                shiftCount = (byte) br.ReadBits(7, 5);
                shiftInst = (EShiftInstruction) br.ReadBits(5, 2);
                rm = (ERegister) br.ReadBits(0, 4);
            }

            switch (opCode)
            {
                case EOpcode.Add:
                case EOpcode.Sub:
                case EOpcode.Rsb:
                case EOpcode.Mov:
                case EOpcode.Mvn:
                case EOpcode.And:
                case EOpcode.Eor:
                case EOpcode.Orr:
                case EOpcode.Bic:
                case EOpcode.Cmp:
                case EOpcode.Cmn:
                case EOpcode.Tst:
                case EOpcode.Teq:
                    return new Arithmetic(_condition, opCode, setConditionFlags, rd, rn, rm, immediate, shiftInst,
                        shiftCount);
                default:
                    throw new ArgumentException("Invalid Opcode");
            }
        }

        private ICommand DecodeDataAccess(BitReader br)
        {
            var useImmediate = br.ReadBits(25, 1) == 0;

            var postIndex = br.ReadBits(24, 1) == 0;
            var size = (ESize) br.ReadBits(22, 1);
            var writeBack = br.ReadBits(21, 1) == 1;
            var requestType = (ERequestType)br.ReadBits(20, 1);
            var rn = (ERegister) br.ReadBits(16, 4);
            var rd = (ERegister) br.ReadBits(12, 4);

            var immediate = 0;
            byte shiftCount = 0;

            EShiftInstruction? shiftInst = null;
            ERegister? rm = null;

            if (useImmediate)
            {
                immediate = br.ReadBits(0, 12);
            }
            else
            {
                shiftCount = (byte) br.ReadBits(7, 5);
                shiftInst = (EShiftInstruction) br.ReadBits(5, 2);
                rm = (ERegister) br.ReadBits(0, 4);
            }

            return new DataAccess(_condition, requestType, size, writeBack, postIndex, rd, rn, rm, immediate, shiftInst,
                shiftCount);
        }

        private ICommand DecodeBlockTransfer(BitReader br)
        {
            var postIndex = br.ReadBits(24, 1) == 0;
            var writeBack = br.ReadBits(21, 1) == 1;
            var requestType = (ERequestType)br.ReadBits(20, 1);
            var rn = (ERegister)br.ReadBits(16, 4);
            var immediate = br.ReadBits(0, 16);

            return new Blocktransfer(_condition, requestType, writeBack, postIndex, rn, immediate);
        }

        public ICommand Decode(int command)
        {
            if (command == 0)
                return null;

            var br = new BitReader(command);

            _condition = (ECondition)br.ReadBits(28, 4);
            _operation = (EOperation)br.ReadBits(26, 2);

            switch (_operation)
            {
                case EOperation.Arithmetic:
                    return DecodeArithmetic(br);
                case EOperation.DataAccess:
                    return DecodeDataAccess(br);
                case EOperation.Blocktransfer:
                    return DecodeBlockTransfer(br);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
