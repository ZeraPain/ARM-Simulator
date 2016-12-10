using System;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Commands;
using ARM_Simulator.Resources;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Components
{
    internal class Decoder
    {
        private static ICommand DecodeArithmetic(int command)
        {
            var br = new BitReader(command);

            var conditions = (ECondition) br.ReadBits(28, 4);
            var opCode = (EOpcode) br.ReadBits(21, 4);
            var setConditionFlags = br.ReadBits(20, 1) == 1;
            var rn = (ERegister) br.ReadBits(16, 4);
            var rd = (ERegister) br.ReadBits(12, 4);

            if ((command & 0x0e000000) == 0x02000000) // 4 bit rotate, 8 bit immediate
            {
                var rotate = (byte) br.ReadBits(8, 4);
                var immediate = (byte) br.ReadBits(0, 8);
                return new Arithmetic(conditions, opCode, setConditionFlags, rn, rd, rotate, immediate);
            }

            if ((command & 0x0e000010) == 0x00000000) // 5 bit shiftvalue, 2 bit shift, 1 clear, 4 bit rm
            {
                var shiftCount = (byte) br.ReadBits(7, 5);
                var shiftInst = (EShiftInstruction) br.ReadBits(5, 2);
                var rm = (ERegister) br.ReadBits(0, 4);
                return new Arithmetic(conditions, opCode, setConditionFlags, rn, rd, shiftCount, shiftInst, rm);
            }

            if ((command & 0x0e000090) == 0x00000010) // 4 bit rs, 1 clear, 2 bit shift, 1 clear 4 bit rm 
            {
                var rs = (ERegister) br.ReadBits(8, 4);
                var shiftInst = (EShiftInstruction)br.ReadBits(5, 2);
                var rm = (ERegister)br.ReadBits(0, 4);
                return new Arithmetic(conditions, opCode, setConditionFlags, rn, rd, rs, shiftInst, rm);
            }

            return null;
        }

        private static ICommand DecodeBlockTransfer(int command)
        {
            var br = new BitReader(command);

            var conditions = (ECondition)br.ReadBits(28, 4);
            var writeBack = br.ReadBits(21, 1) == 1;
            var rn = (ERegister)br.ReadBits(16, 4);
            var regList = (short)br.ReadBits(0, 16);

            switch (command & 0x0e500000)
            {
                case 0x08000000: // STM Rm, reg List
                    return new Blocktransfer(conditions, false, writeBack, rn, regList);
                case 0x08100000: // LDM Rm, reg List
                    return new Blocktransfer(conditions, true, writeBack, rn, regList);
            }

            return null;
        }

        private static ICommand DecodeMul(int command)
        {
            switch (command & 0x0fe000f0)
            {
                case 0x00000090: // MUL
                    throw new NotImplementedException();
                case 0x00200090: // MLA
                    throw new NotImplementedException();
                case 0x00800090: // UMULL
                    throw new NotImplementedException();
                case 0x00a00090: // UMLAL
                    throw new NotImplementedException();
                case 0x00c00090: // SMULL
                    throw new NotImplementedException();
                case 0x00e00090: // SMLAL
                    throw new NotImplementedException();
            }

            return null;
        }

        private static ICommand DecodeSwap(int command)
        {
            switch (command & 0x0fc000f0)
            {
                case 0x01000090: // SWP
                    throw new NotImplementedException();
                case 0x01400090: // SWPB
                    throw new NotImplementedException();
            }

            return null;
        }

        private ICommand DecodeDataAccess(int command)
        {
            var br = new BitReader(command);

            var conditions = (ECondition) br.ReadBits(28, 4);
            var preIndex = br.ReadBits(24, 1) == 1;
            var unsigned = br.ReadBits(23, 1) == 1;
            var writeBack = br.ReadBits(21, 1) == 1;
            var rn = (ERegister)br.ReadBits(16, 4);
            var rd = (ERegister)br.ReadBits(12, 4);

            short immediate;
            byte shiftValue;
            EShiftInstruction shiftInst;
            ERegister rm;

            switch (command & 0x0e100000) // DataAccess
            {
                case 0x04100000: // LDR + offset
                    immediate = (short) br.ReadBits(0, 12);
                    return new DataAccess(conditions, true, preIndex, unsigned, writeBack, ESize.Word, rn, rd, immediate);
                case 0x06100000: // LDR + Rm shift value
                    shiftValue = (byte) br.ReadBits(7, 5);
                    shiftInst = (EShiftInstruction) br.ReadBits(5, 2);
                    rm = (ERegister) br.ReadBits(0, 4);
                    return new DataAccess(conditions, true, preIndex, unsigned, writeBack, ESize.Word, rn, rd, shiftValue, shiftInst, rm);
                case 0x04500000: // LDRB + offset
                    immediate = (short)br.ReadBits(0, 12);
                    return new DataAccess(conditions, true, preIndex, unsigned, writeBack, ESize.Byte, rn, rd, immediate);
                case 0x06500000: // LDRB + Rm shift value
                    shiftValue = (byte)br.ReadBits(7, 5);
                    shiftInst = (EShiftInstruction)br.ReadBits(5, 2);
                    rm = (ERegister)br.ReadBits(0, 4);
                    return new DataAccess(conditions, true, preIndex, unsigned, writeBack, ESize.Byte, rn, rd, shiftValue, shiftInst, rm);
                case 0x04000000: // STR + offset
                    immediate = (short)br.ReadBits(0, 12);
                    return new DataAccess(conditions, false, preIndex, unsigned, writeBack, ESize.Word, rn, rd, immediate);
                case 0x06000000: // STR + Rm shift value
                    shiftValue = (byte)br.ReadBits(7, 5);
                    shiftInst = (EShiftInstruction)br.ReadBits(5, 2);
                    rm = (ERegister)br.ReadBits(0, 4);
                    return new DataAccess(conditions, false, preIndex, unsigned, writeBack, ESize.Word, rn, rd, shiftValue, shiftInst, rm);
                case 0x04400000: // STRB + offset
                    immediate = (short)br.ReadBits(0, 12);
                    return new DataAccess(conditions, false, preIndex, unsigned, writeBack, ESize.Byte, rn, rd, immediate);
                case 0x06400000: // STRB + Rm shift value
                    shiftValue = (byte)br.ReadBits(7, 5);
                    shiftInst = (EShiftInstruction)br.ReadBits(5, 2);
                    rm = (ERegister)br.ReadBits(0, 4);
                    return new DataAccess(conditions, false, preIndex, unsigned, writeBack, ESize.Byte, rn, rd, shiftValue, shiftInst, rm);
            }

            switch (command & 0x0e1000f0) // Special DataAccess (thumb)
            {
                case 0x000000b0: // STRH + addr_mode
                    throw new NotImplementedException();
                case 0x001000b0: // LDRH + addr_mode
                    throw new NotImplementedException();
                case 0x001000d0: // LDRSB + addr_mode
                    throw new NotImplementedException();
                case 0x001000f0: // LDRSH + addr_mode
                    throw new NotImplementedException();
            }

            return null;
        }

        private static ICommand DecodeBranch(int command)
        {
            var br = new BitReader(command);

            var conditions = (ECondition)br.ReadBits(28, 4);

            switch (command & 0x0f000000)
            {
                case 0x0a000000: // Branch
                    var offset = br.ReadBits(0, 24, true);
                    return new Jump(conditions, EJump.Branch, offset);
            }

            return null;
        }

        private static ICommand DecodeMrs(int command)
        {
            switch (command & 0x0ff00000)
            {
                case 0x01000000: // MRS CPSR
                    throw new NotImplementedException();
                case 0x01400000: // MRS SPSR
                    throw new NotImplementedException();
                case 0x01200000: // MSR CPSR_<field>
                    throw new NotImplementedException();
                case 0x03200000: // MSR CPSR_f
                    throw new NotImplementedException();
                case 0x01600000: // MSR SPSR_<field>
                    throw new NotImplementedException();
                case 0x03600000: // MSR SPSR_f
                    throw new NotImplementedException();
            }

            return null;
        }

        public ICommand Decode(int? fetch)
        {
            if (fetch == null)
                return null;

            var command = (int) fetch;

            // 12 bit
            if (((command & 0xff000f0) ^ 0x01200010) == 0) // BX
                throw new NotImplementedException();

            // 11 bit
            var cmd = DecodeMul(command);
            if (cmd != null)
                return cmd;

            // 10 bit
            cmd = DecodeSwap(command);
            if (cmd != null)
                return cmd;

            // 8 bit
            cmd = DecodeDataAccess(command);
            if (cmd != null)
                return cmd;

            cmd = DecodeMrs(command);
            if (cmd != null)
                return cmd;

            // 5 bit
            cmd = DecodeBlockTransfer(command);
            if (cmd != null)
                return cmd;

            // 4 bit
            cmd = DecodeBranch(command);
            if (cmd != null)
                return cmd;

            // 3 bit
            cmd = DecodeArithmetic(command);
            if (cmd != null)
                return cmd;

            throw new ArgumentOutOfRangeException();
        }
    }
}
