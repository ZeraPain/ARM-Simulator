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
 
                _showAsByte = value;
                ContextMenuUpdate?.Invoke();
            }
        }

        protected ContextMenuHandler()
        {
            ShowAsHexadecimal = true;
            ShowAsUnsigned = true;
            ShowAsByte = false;
        }
    }
}
