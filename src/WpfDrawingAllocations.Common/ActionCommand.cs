using System;
using System.Windows.Input;

namespace WpfDrawingAllocations.Common
{
    public class ActionCommand : ICommand
    {
        static readonly EventArgs m_eventArgs = new EventArgs();
        readonly Action _action;
        bool _canExecute;

        public ActionCommand(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public void SetCanExecute(bool canExecute)
        {
            _canExecute = canExecute;
            CanExecuteChanged?.Invoke(this, m_eventArgs);
        }

        public bool CanExecute(object parameter) => _canExecute;

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) => _action();
    }
}
