using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARM_Simulator.ViewModel
{
    public class ARMDecoder
    {
        public void Decode(string command)
        {
            var cmdSplit = command.Split(new []{ ' '}, StringSplitOptions.RemoveEmptyEntries);
            if (cmdSplit.Length == 0)
                return;

        }
    }
}
