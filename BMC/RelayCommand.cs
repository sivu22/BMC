using System;
using System.Windows.Input;

namespace BMC
{
    class RelayCommand: ICommand
    {
        readonly Action<object> execute;
        readonly Predicate<object> canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute) => (this.execute, this.canExecute) = (execute, canExecute);
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }
}
