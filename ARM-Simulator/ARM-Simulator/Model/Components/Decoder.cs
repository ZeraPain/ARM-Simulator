using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Commands;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Components
{
    internal class Decoder
    {
        public ICommand Decode(int command)
        {
            if (command == 0)
                return null;

            var bitReader = new BitReader(command);

            var conditionFlags = (short)bitReader.ReadBits(28, 4);
            var blockTransfer = bitReader.ReadBits(27, 1) == 1;

            if (!blockTransfer)
            {
                var arithmetic = bitReader.ReadBits(26, 1) == 0;
                var useImmediate = bitReader.ReadBits(25, 1) > 0;
                var rn = (Register)bitReader.ReadBits(16, 4);
                var rd = (Register)bitReader.ReadBits(12, 4);

                short immediate = 0;
                byte shiftCount = 0;
                ShiftInstruction? shiftInst = null;
                Register? rm = null;

                if (useImmediate)
                {
                    immediate = (short)bitReader.ReadBits(0, 12);
                }
                else
                {
                    shiftCount = (byte)bitReader.ReadBits(7, 5);
                    shiftInst = (ShiftInstruction)bitReader.ReadBits(5, 2);
                    rm = (Register)bitReader.ReadBits(0, 4);
                }

                if (arithmetic)
                {
                    var opCode = (Opcode) bitReader.ReadBits(21, 4);
                    var setConditionFlags = bitReader.ReadBits(20, 1) == 1;

                    switch (opCode)
                    {
                        case Opcode.Add:
                            return new Add(Opcode.Add, setConditionFlags, rd, rn, rm, immediate, shiftInst, shiftCount);
                        case Opcode.Sub:
                        case Opcode.Rsb:
                            return new Substract(opCode, setConditionFlags, rd, rn, rm, immediate, shiftInst, shiftCount);
                        case Opcode.Mov:
                        case Opcode.Mvn:
                            return new Move(opCode, setConditionFlags, rd, rm, immediate, shiftInst, shiftCount);
                        case Opcode.And:
                        case Opcode.Eor:
                        case Opcode.Orr:
                        case Opcode.Bic:
                            return new Logical(opCode, setConditionFlags, rd, rn, rm, immediate, shiftInst, shiftCount);
                        case Opcode.Cmp:
                        case Opcode.Cmn:
                            return new Compare(opCode, rn, rm, immediate, shiftInst, shiftCount);
                        case Opcode.Tst:
                        case Opcode.Teq:
                            return new Test(opCode, rn, rm, immediate, shiftInst, shiftCount);
                    }
                }
                else
                {
                    var postIndex = bitReader.ReadBits(24, 1) == 1;
                    var writeBack = bitReader.ReadBits(21, 1) == 1;
                    var opCode = (MemOpcode) bitReader.ReadBits(20, 1);

                    return new DataAccess(opCode, writeBack, postIndex, rd, rn, rm, immediate, shiftInst, shiftCount);
                }
            }

            return null;
        }
    }
}
