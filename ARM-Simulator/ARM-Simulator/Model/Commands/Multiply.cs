using System;
using System.Collections.Generic;
using System.Reflection;
using ARM_Simulator.Annotations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Multiply : ICommand
    {
        protected ECondition Condition;
        protected EMultiplication Multiplication;
        protected bool SetConditionFlags;
        protected ERegister Rd;
        protected ERegister Rn;
        protected ERegister Rm;
        protected ERegister Rs;
        protected ERegister RdHi;
        protected ERegister RdLo;

        protected bool Decoded;

        public Multiply(ECondition condition, EMultiplication multiplication, bool setConditionFlags, string parameterString)
        {
            Condition = condition;
            Multiplication = multiplication;
            SetConditionFlags = setConditionFlags;
            Decoded = false;
            Parse(parameterString);
        }

        public Multiply(ECondition condition, bool setConditionFlags, ERegister rd, ERegister rs, ERegister rm)
        {
            Condition = condition;
            Multiplication = EMultiplication.Mul;
            SetConditionFlags = setConditionFlags;
            Rd = rd;
            Rs = rs;
            Rm = rm;
            Decoded = true;
        }

        public Multiply(ECondition condition, bool setConditionFlags, ERegister rd, ERegister rn, ERegister rs, ERegister rm)
        {
            Condition = condition;
            Multiplication = EMultiplication.Mla;
            SetConditionFlags = setConditionFlags;
            Rd = rd;
            Rn = rn;
            Rs = rs;
            Rm = rm;
            Decoded = true;
        }

        public Multiply(ECondition condition, EMultiplication multiplication, bool setConditionFlags, ERegister rdhi, ERegister rdlo, ERegister rs, ERegister rm)
        {
            Condition = condition;
            Multiplication = multiplication;
            SetConditionFlags = setConditionFlags;
            RdHi = rdhi;
            RdLo = rdlo;
            Rs = rs;
            Rm = rm;
            Decoded = true;
        }

        public void Parse([NotNull] string parameterString)
        {
            if (Decoded) throw new InvalidOperationException();

            var parameters = Parser.ParseParameters(parameterString, new[] { ',' });

            switch (Multiplication)
            {
                case EMultiplication.Mul:
                    if (parameters.Length != 3) throw new TargetParameterCountException();

                    Rd = Parser.ParseRegister(parameters[0]);
                    Rm = Parser.ParseRegister(parameters[1]);
                    Rs = Parser.ParseRegister(parameters[2]);
                    break;
                case EMultiplication.Mla:
                    if (parameters.Length != 4) throw new TargetParameterCountException();

                    Rd = Parser.ParseRegister(parameters[0]);
                    Rm = Parser.ParseRegister(parameters[1]);
                    Rs = Parser.ParseRegister(parameters[2]);
                    Rn = Parser.ParseRegister(parameters[3]);
                    break;
                case EMultiplication.Smlal:
                case EMultiplication.Smull:
                case EMultiplication.Umlal:
                case EMultiplication.UMull:
                    if (parameters.Length != 4) throw new TargetParameterCountException();

                    RdLo = Parser.ParseRegister(parameters[0]);
                    RdHi = Parser.ParseRegister(parameters[1]);
                    Rm = Parser.ParseRegister(parameters[2]);
                    Rs = Parser.ParseRegister(parameters[3]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Decoded = true;
        }

        public byte[] Encode()
        {
            if (!Decoded) throw new InvalidOperationException();

            var bw = new BitWriter();

            bw.WriteBits((int)Condition, 28, 4); // Condition

            switch (Multiplication)
            {
                case EMultiplication.Mul:
                    bw.WriteBits((int)Rd, 16, 4);
                    break;
                case EMultiplication.Mla:
                    bw.WriteBits(1, 21, 1);
                    bw.WriteBits((int)Rd, 16, 4);
                    bw.WriteBits((int)Rn, 12, 4);
                    break;
                case EMultiplication.Smlal:
                    bw.WriteBits(1, 21, 1);
                    goto case EMultiplication.Smull;
                case EMultiplication.Smull:
                    bw.WriteBits(1, 23, 1);
                    bw.WriteBits(1, 22, 1);
                    bw.WriteBits((int)RdHi, 16, 4);
                    bw.WriteBits((int)RdLo, 12, 4);
                    break;
                case EMultiplication.Umlal:
                    bw.WriteBits(1, 21, 1);
                    goto case EMultiplication.UMull;
                case EMultiplication.UMull:
                    bw.WriteBits(1, 23, 1);
                    bw.WriteBits((int)RdHi, 16, 4);
                    bw.WriteBits((int)RdLo, 12, 4);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            bw.WriteBits((int)Rs, 8, 4);
            bw.WriteBits(1, 7, 1);
            bw.WriteBits(1, 4, 1);
            bw.WriteBits((int)Rm, 0, 4);

            return bw.GetValue();
        }

        public void Link(Dictionary<string, int> commandTable, Dictionary<string, int> dataTable, int commandOffset)
        {

        }

        public void Execute([NotNull] Core armCore)
        {
            if (!Decoded) throw new InvalidOperationException();
            if (!Helper.CheckConditions(Condition, armCore.Cpsr)) return;

            const uint lo = 0xFFFFFFFF;
            const ulong hi = 0xFFFFFFFF00000000;
            ulong result;

            switch (Multiplication)
            {
                case EMultiplication.Mul:
                    result = (ulong)((long)armCore.GetRegValue(Rs) * armCore.GetRegValue(Rm));
                    armCore.SetRegValue(Rd, (int)(result & lo));
                    break;
                case EMultiplication.Mla:
                    result = (ulong)((long)armCore.GetRegValue(Rs) * armCore.GetRegValue(Rm) + armCore.GetRegValue(Rn));
                    armCore.SetRegValue(Rd, (int)(result & lo));
                    break;
                case EMultiplication.Smlal:
                    result = (ulong)((long)armCore.GetRegValue(Rs) * armCore.GetRegValue(Rm));
                    armCore.SetRegValue(RdLo, armCore.GetRegValue(RdLo) + (int)(result & lo));
                    armCore.SetRegValue(RdHi, armCore.GetRegValue(RdHi) + (int)((result & hi) >> 32));
                    break;
                case EMultiplication.Smull:
                    result = (ulong)((long)armCore.GetRegValue(Rs) * armCore.GetRegValue(Rm));
                    armCore.SetRegValue(RdLo, (int)(result & lo));
                    armCore.SetRegValue(RdHi, (int)((result & hi) >> 32));
                    break;
                case EMultiplication.Umlal:
                    result = (ulong)(uint)armCore.GetRegValue(Rs) * (uint)armCore.GetRegValue(Rm);
                    armCore.SetRegValue(RdLo, armCore.GetRegValue(RdLo) + (int)(result & lo));
                    armCore.SetRegValue(RdHi, armCore.GetRegValue(RdHi) + (int)((result & hi) >> 32));
                    break;
                case EMultiplication.UMull:
                    result = (ulong)(uint)armCore.GetRegValue(Rs) * (uint)armCore.GetRegValue(Rm);
                    armCore.SetRegValue(RdLo, (int)(result & lo));
                    armCore.SetRegValue(RdHi, (int)((result & hi) >> 32));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (SetConditionFlags)
            {
                armCore.SetNzcvFlags(new Flags(true, true, false, false), //C, V are not set for ARMv5 and higher
                    new Flags((long)result < 0, result == 0, false, false));
            }
        }
    }
}
