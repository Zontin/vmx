using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace vmx.Helper
{
    internal class RelayCommand : ICommand
    {
        private readonly Action<object> command;
        private readonly Func<object, bool> canExecute;

        public RelayCommand(Action<object> command, Func<object, bool> canExecute = null)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            this.command = command;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (canExecute == null)
            {
                return true;
            }
            return canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            command(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
