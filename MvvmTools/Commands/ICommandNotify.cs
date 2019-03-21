namespace MvvmTools.Commands
{
    /// <summary>
    /// Defines a method to raise <see cref="System.Windows.Input.ICommand.CanExecuteChanged"/> event. 
    /// </summary>
    public interface ICommandNotify : System.Windows.Input.ICommand
    {
        /// <summary>
        /// Raise <see cref="System.Windows.Input.ICommand.CanExecuteChanged"/> event.
        /// </summary>
        void NotifyCanExecuteChanged();
    }
}
