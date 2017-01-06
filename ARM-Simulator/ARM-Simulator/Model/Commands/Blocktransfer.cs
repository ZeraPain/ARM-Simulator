using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ARM_Simulator.Annotations;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Resources;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Blocktransfer : ICommand
    {
        protected ECondition Condition;
        protected bool Load;
        protected bool WriteBack;
        protected ERegister Rn;
        protected int RegisterList;

        protected bool Decoded;

        public Blocktransfer(ECondition condition, bool load, string parameterString)
        {
            Condition = condition;
            Load = load;
            Decoded = false;
            Parse(parameterString);
        }

        public Blocktransfer(ECondition condition, bool load, bool writeBack, ERegister rn, short regList)
        {
            Condition = condition;
            Load = load;
            WriteBack = writeBack;
            Rn = rn;
            RegisterList = regList;
            Decoded = true;
        }

        public void Parse([NotNull] string parameterString)
        {
            if (Decoded) throw new InvalidOperationException();

            var parameters = Parser.ParseParameters(parameterString, new[] { '{', '}' });

            if (parameters.Length != 2) throw new TargetParameterCountException();
            if (!parameters[0].EndsWith(",")) throw new ArgumentException();

            parameters[0] = parameters[0].Substring(0, parameters[0].Length - 1);

            if (parameters[0].EndsWith("!"))
            {
                parameters[0] = parameters[0].Substring(0, parameters[0].Length - 1);
                WriteBack = true;
            }

            Rn = Parser.ParseRegister(parameters[0]);

            // Parse Register List
            var regList = parameters[1].Split(',');

            foreach (var regRange in regList.Select(reg => reg.Split('-')))
            {
                switch (regRange.Length)
                {
                    case 1:
                        var register = Parser.ParseRegister(regRange[0]);
                        RegisterList |= 1 << (int)register;
                        break;
                    case 2:
                        var startReg = Parser.ParseRegister(regRange[0]);
                        var endReg = Parser.ParseRegister(regRange[1]);
                        if (endReg < startReg) throw new ArgumentException();

                        for (var i = startReg; i <= endReg; i++)
                            RegisterList |= 1 << (int)i;

                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            Decoded = true;
        }

        public int Encode()
        {
            if (!Decoded) throw new InvalidOperationException();

            var bw = new BitWriter();

            bw.WriteBits((int)Condition, 28, 4); // Condition
            bw.WriteBits(1, 27, 1);
            bw.WriteBits(WriteBack ? 1 : 0, 21, 1);
            bw.WriteBits(Load ? 1 : 0, 20, 1);
            bw.WriteBits((int)Rn, 16, 4);
            bw.WriteBits(RegisterList, 0, 16);

            return bw.GetValue();
        }

        public void Link(Dictionary<string, int> commandTable, Dictionary<string, int> dataTable, int commandOffset)
        {

        }

        public void Execute([NotNull] Core armCore)
        {
            if (!Decoded) throw new InvalidOperationException();
            if (!Helper.CheckConditions(Condition, armCore.Cpsr)) return;

            var baseAddress = armCore.GetRegValue(Rn);

            if (Load)
            {
                for (var i = 0; i < 16; i++)
                {
                    if ((RegisterList & (1 << i)) == 0)
                        continue;

                    var memValue = armCore.Ram.ReadInt((uint)baseAddress);
                    armCore.SetRegValue((ERegister) i, memValue);
                    baseAddress += 4;
                }
            }
            else
            {
                for (var i = 15; i >= 0; i--)
                {
                    if ((RegisterList & (1 << i)) == 0)
                        continue;

                    baseAddress -= 4;
                    var regValue = armCore.GetRegValue((ERegister)i);
                    armCore.Ram.WriteInt((uint)baseAddress, regValue);
                }
            }

            if (WriteBack) armCore.SetRegValue(Rn, baseAddress);
        }
    }
}
