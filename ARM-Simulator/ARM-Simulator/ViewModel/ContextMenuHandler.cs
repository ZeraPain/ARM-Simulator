namespace ARM_Simulator.ViewModel
{
    public abstract class ContextMenuHandler
    {
        protected delegate void ContextMenuUpdateFunction();
        protected ContextMenuUpdateFunction ContextMenuUpdate;

        private bool _showAsHexadecimal;
        public bool ShowAsHexadecimal
        {
            get { return _showAsHexadecimal; }
            set
            {
                if (_showAsHexadecimal == value) return;
                _showAsHexadecimal = value;
                ContextMenuUpdate?.Invoke();
            }
        }

        private bool _showAsUnsigned;
        public bool ShowAsUnsigned
        {
            get { return _showAsUnsigned; }
            set
            {
                if (_showAsUnsigned == value) return;
                _showAsUnsigned = value;
                ContextMenuUpdate?.Invoke();
            }
        }

        private bool _showAsByte;
        public bool ShowAsByte
        {
            get { return _showAsByte; }
            set
            {
                if (_showAsByte == value) return;
                _showAsByte = value;
                ContextMenuUpdate?.Invoke();
            }
        }

        protected ContextMenuHandler()
        {
            _showAsHexadecimal = false;
            _showAsUnsigned = false;
            _showAsByte = false;
        }
    }
}
