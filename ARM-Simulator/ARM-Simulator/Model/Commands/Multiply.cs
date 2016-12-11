using System;
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

        protected bool Decoded;

        public Multiply(ECondition condition, EMultiplication multiplication, bool setConditionFlags, string[] parameters)
        {
            Condition = condition;
            Multiplication = multiplication;
            SetConditionFlags = setConditionFlags;
            Decoded = false;
            Parse(parameters);
        }

        public Multiply(ECondition condition, EMultiplication multiplication, bool setConditionFlags, ERegister rd, ERegister rs, ERegister rm)
        {
            Condition = condition;
            Multiplication = multiplication;
            SetConditionFlags = setConditionFlags;
            Rd = rd;
            Rs = rs;
            Rm = rm;
            Decoded = true;
        }

        public Multiply(ECondition condition, EMultiplication multiplication, bool setConditionFlags, ERegister rd, ERegister rn, ERegister rs, ERegister rm)
        {
            Condition = condition;
            Multiplication = multiplication;
            SetConditionFlags = setConditionFlags;
            Rd = rd;
            Rn = rn;
            Rs = rs;
            Rm = rm;
            Decoded = true;
        }

        public void Parse(string[] parameters)
        {
            if (Decoded)
                throw new Exception("Cannot parse a decoded command");

            switch (Multiplication)
            {
                case EMultiplication.Mul:
                    if (parameters.Length != 3)
                        throw new ArgumentException("Invalid parameter count");

                    Rd = Parser.ParseRegister(parameters[0]);
                    Rm = Parser.ParseRegister(parameters[1]);
                    Rs = Parser.ParseRegister(parameters[2]);
                    break;
                case EMultiplication.Mla:
                    if (parameters.Length != 4)
                        throw new ArgumentException("Invalid parameter count");

                    Rd = Parser.ParseRegister(parameters[0]);
                    Rm = Parser.ParseRegister(parameters[1]);
                    Rs = Parser.ParseRegister(parameters[2]);
                    Rn = Parser.ParseRegister(parameters[3]);
                    break;
                case EMultiplication.Smlal:
                    break;
                case EMultiplication.Smull:
                    break;
                case EMultiplication.Umlal:
                    break;
                case EMultiplication.UMull:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Decoded = true;
        }

        public int Encode()
        {
            if (!Decoded)
                throw new Exception("Cannot convert an undecoded command");

            var bw = new BitWriter();

            bw.WriteBits((int)Condition, 28, 4); // Condition

            switch (Multiplication)
            {
                case EMultiplication.Mul:
                    break;
                case EMultiplication.Mla:
                    bw.WriteBits(1, 21, 1);
                    bw.WriteBits((int)Rn, 12, 4);
                    break;
                case EMultiplication.Smlal:
                    break;
                case EMultiplication.Smull:
                    break;
                case EMultiplication.Umlal:
                    break;
                case EMultiplication.UMull:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            bw.WriteBits((int)Rd, 16, 4);
            bw.WriteBits((int)Rs, 8, 4);
            bw.WriteBits(1, 7, 1);
            bw.WriteBits(1, 4, 1);
            bw.WriteBits((int)Rm, 0, 4);

            return bw.GetValue();
        }

        public void Execute(Core armCore)
        {
            if (!Decoded)
                throw new Exception("Cannot execute an undecoded command");

            if (!Helper.CheckConditions(Condition, armCore.GetCpsr()))
                return;

            var lo = 0xFFFFFFFF;
            var hi = 0xFFFFFFFF00000000;
            ulong result;
            var value = 0;

            switch (Multiplication)
            {
                case EMultiplication.Mul:
                    result = (ulong)(armCore.GetRegValue(Rs) * armCore.GetRegValue(Rm));
                    value = (int) (result & lo);
                    break;
                case EMultiplication.Mla:
                    result = (ulong)(armCore.GetRegValue(Rs) * armCore.GetRegValue(Rm) + armCore.GetRegValue(Rn));
                    value = (int)(result & lo);
                    break;
                case EMultiplication.Smlal:
                    break;
                case EMultiplication.Smull:
                    break;
                case EMultiplication.Umlal:
                    break;
                case EMultiplication.UMull:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            armCore.SetRegValue(Rd, value);

            if (!SetConditionFlags)
                return;

            armCore.SetNzcvFlags(new Flags(true, true, false, false),
                       new Flags(value < 0, value == 0, false, false));
        }       
    }
}
