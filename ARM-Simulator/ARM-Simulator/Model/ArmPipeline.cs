using System.Collections.Generic;
using ARM_Simulator.Enumerations;
using ARM_Simulator.Interfaces;

namespace ARM_Simulator.Model
{
    public class ArmPipeline
    {
        private string _fetch;
        private ICommand _decode;
        private ICommand _execute;

        private readonly ArmDecoder _armDecoder;
        private readonly Dictionary<ArmRegister, int> _registers;

        public ArmPipeline(Dictionary<ArmRegister, int> registers)
        {
            _fetch = null;
            _decode = null;
            _execute = null;

            _armDecoder = new ArmDecoder();
            _registers = registers;
        }

        public void Step(string fetchCommand)
        {
            if (_decode != null)
                _execute = _decode;

            if (_fetch != null)
                _decode = _armDecoder.Decode(_fetch);

            _fetch = fetchCommand;

            _execute?.Execute(_registers);
        }

        public void DirectExecute(string command)
        {
            var cmd = _armDecoder.Decode(command);
            cmd.Execute(_registers);
        }
    }
}
