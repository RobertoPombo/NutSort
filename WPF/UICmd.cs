using System.Windows.Input;

namespace GTRC_WPF
{
    public class UICmd(Action<object> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public void Execute(object? parameter)
        {
            execute?.Invoke(parameter);
        }

        public bool CanExecute(object? parameter) { return true; }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
