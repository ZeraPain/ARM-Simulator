using ARM_Simulator.Interfaces;

namespace ARM_Simulator.Model
{
    public class Pipeline
    {
        private string _fetch;
        private ICommand _decode;
        private ICommand _execute;

        private readonly Parser _armDecoder;

        public Pipeline()
        {
            _armDecoder = new Parser();
            _fetch = null;
            _decode = null;
            _execute = null;
        }

        public ICommand Tick(string fetchCommand)
        {
            if (_decode != null)
                _execute = _decode;

            if (_fetch != null)
                _decode = _armDecoder.ParseLine(_fetch);

            _fetch = fetchCommand == string.Empty ? null : fetchCommand;

            return _execute;
        }

        // Only needed for Unit tests!
        public ICommand ForceDecode(string fetchCommand)
        {
            return _armDecoder.ParseLine(fetchCommand);
        }
    }
}
