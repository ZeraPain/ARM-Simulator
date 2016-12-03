using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Commands;

namespace ARM_Simulator.Model
{
    internal class Decoder
    {
        public ICommand Decode(int command)
        {
            var bitReader = new BitReader(command);

            var conditionFlags = (short)bitReader.ReadBits(28, 4);
            var arithmetic = bitReader.ReadBits(26, 1) == 0;

            if (arithmetic)
            {
                var useImmediate = bitReader.ReadBits(25, 1) > 0;
                var opCode = (Opcode)bitReader.ReadBits(21, 4);
                var setConditionFlags = bitReader.ReadBits(20, 1) == 1;
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

                switch (opCode)
                {
                    case Opcode.Add:
                        return new Add(setConditionFlags, rd, rn, rm, immediate, shiftInst, shiftCount);
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

            return null;
        }
    }
}
