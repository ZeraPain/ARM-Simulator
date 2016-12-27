using System;
using System.Collections.Generic;
using System.Linq;
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
            if (Decoded)
                throw new Exception("Cannot parse a decoded command");

            var parameters = Parser.ParseParameters(parameterString, new[] { '{', '}' });

            // Check Parameter Count
            if (parameters.Length != 2)
                throw new ArgumentException("Invalid parameter count");

            // Parse Destination Register
            if (!parameters[0].EndsWith(","))
                throw new ArgumentException("Invalid syntax");

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
                        if (endReg < startReg)
                            throw new ArgumentException("Invalid syntax");

                        for (var i = startReg; i <= endReg; i++)
                        {
                            RegisterList |= 1 << (int)i;
                        }
                        break;
                    default:
                        throw new ArgumentException("Invalid syntax");
                }
            }

            Decoded = true;
        }

        public int Encode()
        {
            if (!Decoded)
                throw new Exception("Cannot convert an undecoded command");

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

        public void Execute(Core armCore)
        {
            if (!Decoded)
                throw new Exception("Cannot execute an undecoded command");

            if (!Helper.CheckConditions(Condition, armCore.GetCpsr()))
                return;

            var basisAdr = armCore.GetRegValue(Rn);

            if (Load)
            {
                for (var i = 0; i < 16; i++)
                {
                    if ((RegisterList & (1 << i)) == 0)
                        continue;

                    var memValue = armCore.Ram.ReadInt((uint) basisAdr);
                    armCore.SetRegValue((ERegister) i, memValue);
                    basisAdr += 4;
                }
            }
            else
            {
                for (var i = 15; i >= 0; i--)
                {
                    if ((RegisterList & (1 << i)) == 0)
                        continue;

                    basisAdr -= 4;
                    var regValue = armCore.GetRegValue((ERegister)i);
                    armCore.Ram.WriteInt((uint)basisAdr, regValue);
                }
            }
            
            if (WriteBack)
                armCore.SetRegValue(Rn, basisAdr);
        }
    }
}
