﻿using System;
using System.Windows.Input;

namespace ARM_Simulator.Commands
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _action;
        public DelegateCommand(Action<object> action)
        {
            _action = action;
        }

        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
