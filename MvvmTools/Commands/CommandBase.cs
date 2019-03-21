using System;
using System.ComponentModel;

namespace MvvmTools.Commands
{
    /// <summary>
    /// The base class of all commands in this package.
    /// </summary>
    public abstract class CommandBase : ICommandNotify
    {
        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>C:\Users\Eiad Al-Khanshour\source\repos\ClinicalOffice.Commands\ClinicalOffice.Commands\Core\CommandBase.cs
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <c>null</c>.</param>
        /// <returns><c>true</c> if this command can be executed; otherwise, <c>false</c>.</returns>
        public abstract bool CanExecute(object parameter);
        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <c>null</c>.</param>
        public abstract void Execute(object parameter);

        /// <summary>
        /// Raise <see cref="System.Windows.Input.ICommand.CanExecuteChanged"/> event.
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Subscribes to <see cref="INotifyPropertyChanged.PropertyChanged"/> event of <paramref name="source"/> 
        /// to raise <see cref="CanExecuteChanged"/> event in the current command.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <remarks>
        /// <para>
        /// When <paramref name="propertyName"/> is <c>null</c>, white space, or empty, the command will raise 
        /// <see cref="CanExecuteChanged"/> event every time <paramref name="source"/> raises 
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> events.
        /// </para>
        /// If <paramref name="propertyName"/> is set, the command will raise <see cref="CanExecuteChanged"/> when <paramref name="source"/> 
        /// raises <see cref="INotifyPropertyChanged.PropertyChanged"/> for that property.
        /// </remarks>
        protected virtual void SubscribeToSource(INotifyPropertyChanged source, string propertyName = null)
        {
            if (source == null) return;
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                source.PropertyChanged += (sender, e) => NotifyCanExecuteChanged();
            }
            else
            {
                source.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == propertyName) NotifyCanExecuteChanged();
                };
            }
        }
    }
}
