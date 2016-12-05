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

            var conditionFlags = (ECondition)bitReader.ReadBits(28, 4);
            var blockTransfer = bitReader.ReadBits(27, 1) == 1;

            if (!blockTransfer)
            {
                var arithmetic = bitReader.ReadBits(26, 1) == 0;
                var useImmediate = bitReader.ReadBits(25, 1) > 0;
                var rn = (ERegister)bitReader.ReadBits(16, 4);
                var rd = (ERegister)bitReader.ReadBits(12, 4);

                short immediate = 0;
                byte shiftCount = 0;
                EShiftInstruction? shiftInst = null;
                ERegister? rm = null;

                if (useImmediate)
                {
                    immediate = (short)bitReader.ReadBits(0, 12);
                }
                else
                {
                    shiftCount = (byte)bitReader.ReadBits(7, 5);
                    shiftInst = (EShiftInstruction)bitReader.ReadBits(5, 2);
                    rm = (ERegister)bitReader.ReadBits(0, 4);
                }

                if (arithmetic)
                {
                    var opCode = (EOpcode) bitReader.ReadBits(21, 4);
                    var setConditionFlags = bitReader.ReadBits(20, 1) == 1;

                    switch (opCode)
                    {
                        case EOpcode.Add:
                            return new Add(conditionFlags, EOpcode.Add, setConditionFlags, rd, rn, rm, immediate, shiftInst, shiftCount);
                        case EOpcode.Sub:
                        case EOpcode.Rsb:
                            return new Substract(conditionFlags, opCode, setConditionFlags, rd, rn, rm, immediate, shiftInst, shiftCount);
                        case EOpcode.Mov:
                        case EOpcode.Mvn:
                            return new Move(conditionFlags, opCode, setConditionFlags, rd, rm, immediate, shiftInst, shiftCount);
                        case EOpcode.And:
                        case EOpcode.Eor:
                        case EOpcode.Orr:
                        case EOpcode.Bic:
                            return new Logical(conditionFlags, opCode, setConditionFlags, rd, rn, rm, immediate, shiftInst, shiftCount);
                        case EOpcode.Cmp:
                        case EOpcode.Cmn:
                            return new Compare(conditionFlags, opCode, rn, rm, immediate, shiftInst, shiftCount);
                        case EOpcode.Tst:
                        case EOpcode.Teq:
                            return new Test(conditionFlags, opCode, rn, rm, immediate, shiftInst, shiftCount);
                    }
                }
                else
                {
                    var postIndex = bitReader.ReadBits(24, 1) == 1;
                    var writeBack = bitReader.ReadBits(21, 1) == 1;
                    var opCode = (EMemOpcode) bitReader.ReadBits(20, 1);

                    return new DataAccess(conditionFlags, opCode, writeBack, postIndex, rd, rn, rm, immediate, shiftInst, shiftCount);
                }
            }

            return null;
        }
    }
}
