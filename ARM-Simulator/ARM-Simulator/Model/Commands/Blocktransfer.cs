using System;
using ARM_Simulator.Interfaces;
using ARM_Simulator.Model.Components;
using ARM_Simulator.Utilitiy;

namespace ARM_Simulator.Model.Commands
{
    internal class Blocktransfer : ICommand
    {
        protected ECondition Condition;
        protected ERegister? Rn;
        protected int Registerlist;
        protected bool Decoded;
        protected ERequestType? RequestType;
        protected bool WriteBack;
        protected bool PostIndex;

        public Blocktransfer(ECondition condition, ERequestType requestType, string[] parameters)
        {
            Condition = condition;
            RequestType = requestType;
            Registerlist = 0;
            Decoded = false;
            PostIndex = false;
            Parse(parameters);
        }

        public Blocktransfer(ECondition condition, ERequestType requestType, bool writeBack, bool postIndex, ERegister? rn, int registerlist)
        {
            Condition = condition;
            RequestType = requestType;
            Rn = rn;
            WriteBack = writeBack;
            PostIndex = postIndex;
            Registerlist = registerlist;
            Decoded = true;
        }

        public void Parse(string[] parameters)
        {
            if (Decoded)
                throw new Exception("Cannot parse a decoded command");

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

            foreach (var reg in regList)
            {
                var regRange = reg.Split('-');
                if (regRange.Length == 1)
                {
                    var register = Parser.ParseRegister(regRange[0]);
                    if (register != null) Registerlist |= 1 << (int)register;
                }
                else if (regRange.Length == 2)
                {
                    var startReg = Parser.ParseRegister(regRange[0]);
                    var endReg = Parser.ParseRegister(regRange[1]);
                    if (startReg != null && endReg != null)
                    {
                        if (endReg < startReg)
                            throw new ArgumentException("Invalid syntax");

                        for (var i = startReg; i <= endReg; i++)
                        {
                            Registerlist |= 1 << (int)i;
                        }
                    }
                }
                else
                {
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
            bw.WriteBits(2, 26, 2); // Operation

            bw.WriteBits(0, 25, 1); // Bool immediate?
            bw.WriteBits(PostIndex ? 0 : 1, 24, 1);
            bw.WriteBits(0, 23, 1); // Up / Down
            bw.WriteBits(0, 22, 1); // byte or word
            bw.WriteBits(WriteBack ? 1 : 0, 21, 1);
            if (RequestType != null) bw.WriteBits((int)RequestType, 20, 1);

            if (Rn != null) bw.WriteBits((int)Rn, 16, 4); // Basis register

            bw.WriteBits(Registerlist, 0, 16); // Register list

            return bw.GetValue();
        }

        public void Execute(Core armCore)
        {
            if (!Decoded)
                throw new Exception("Cannot execute an undecoded command");

            if (!Helper.CheckConditions(Condition, armCore.GetCpsr()))
                return;

            var basisAdr = armCore.GetRegValue(Rn);
            for (var i = 0; i < 16; i++)
            {
                if ((Registerlist & (1 << i)) != 0)
                {
                    switch (RequestType)
                    {
                        case ERequestType.Store:
                            var regValue = armCore.GetRegValue((ERegister) i);
                            armCore.Ram.WriteInt((uint)basisAdr, regValue);
                            basisAdr -= 4;
                            break;
                        case ERequestType.Load:
                            var memValue = armCore.Ram.ReadInt((uint)basisAdr);
                            armCore.SetRegValue((ERegister)i, memValue);
                            basisAdr += 4;
                            break;
                    }
                }
            }

            if (RequestType == ERequestType.Load)
            {
                
            }
            else if (RequestType == ERequestType.Store)
            {
                
            }
            //throw new NotImplementedException();
        }
    }
}
