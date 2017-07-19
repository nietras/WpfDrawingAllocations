﻿using System;
using System.Windows.Input;

namespace WpfDrawingAllocations.Common
{
    public class ActionCommand : ICommand
    {
        private Action _action;
        private bool _canExecute;

        public ActionCommand(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }
    }
}
