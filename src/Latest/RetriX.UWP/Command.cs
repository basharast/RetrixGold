using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RetriX.UWP
{
    internal class Command : ICommand
    {
        public Action MainAction;
        public bool ExecuteState = true;

        public Command(Action action)
        {
            MainAction = action;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return ExecuteState;
        }

        public void Execute(object parameter)
        {
            MainAction.Invoke();
        }
    }
}
