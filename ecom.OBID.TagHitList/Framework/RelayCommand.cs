using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ecom.TagHitList.Framework
{
    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Func<bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;

        }

        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            handler?.Invoke(this, new EventArgs());
        }

        #region ICommand IMplementation
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return (_canExecute == null) || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion

    }
}
