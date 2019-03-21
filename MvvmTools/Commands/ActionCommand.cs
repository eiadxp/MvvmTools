using System;
using System.ComponentModel;
using System.Windows.Input;

namespace MvvmTools.Commands
{
    /// <summary>
    /// A command that is used to expose methods to be executed synchronously.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this command to expose methods that do not return value.
    /// You can use a method that returns <c>True</c>/<c>False</c> as delegates for CanExecute.
    /// </para>
    /// <para>
    /// It is more easier to use static members in <see cref="Command"/> class or extension method for <see cref="INotifyPropertyChanged"/> 
    /// interface (Defined in <see cref="Extensions"/> class).
    /// </para>
    /// </remarks>
    public class ActionCommand : CommandBase
    {
        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <c>null</c>.</param>
        /// <returns><c>true></c> if this command can be executed; otherwise, <c>false</c>.</returns>
        public override bool CanExecute(object parameter)
        {
            if (canExecute == null) return true;
            return canExecute(parameter);
        }
        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <c>null</c>.</param>
        public override void Execute(object parameter)
        {
            execute?.Invoke(parameter);
        }

        readonly Action<object> execute;
        readonly Func<object, bool> canExecute;

        /// <summary>
        /// Creates command that expose a a single parameter method which does not return a value (<see cref="Action{Object}"/>), 
        /// and uses a single parameter function that returns <c>true</c>/<c>false</c> as CanExecute function.
        /// </summary>
        /// <param name="executeAction">A delegate to that method to be executed in the command.</param>
        /// <param name="canExecuteFunction">A delegate to a function that returns <c>bool</c> to be executed and returned in the <see cref="CanExecute(object)"/> function.</param>
        /// <param name="source">The object that implement <see cref="INotifyPropertyChanged"/>  interface that is used to raise <see cref="ICommand.CanExecuteChanged"/> event.</param>
        /// <param name="propertyName">Name of the property which when changed will cause the <see cref="ICommand.CanExecuteChanged"/> event to be raised.</param>
        /// <remarks>
        /// <para>
        /// If <paramref name="canExecuteFunction"/> is not set, <see cref="CanExecute(object)"/> will return always <c>true</c>.
        /// </para>
        /// <para>
        /// If the <paramref name="propertyName"/> is set, the command will monitor <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// until the <see cref="PropertyChangedEventArgs.PropertyName"/> equals the <paramref name="propertyName"/>, 
        /// where it will raise <see cref="ICommand.CanExecuteChanged"/> event.
        /// If the <paramref name="propertyName"/> is not set, the command <see cref="ICommand.CanExecuteChanged"/> event will be raised 
        /// whenever the <see cref="INotifyPropertyChanged.PropertyChanged"/> event of <paramref name="source"/> is raised.
        /// </para>
        /// <para>
        /// Passing <c>null</c> for <paramref name="source"/> will skip monitoring its <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </para>
        /// <para>
        /// If you want to use a property as <see cref="CanExecute(object)"/> result, 
        /// you can either put it inside a function delegate (and using its name as <paramref name="propertyName"/>) as in the examples,
        /// or you can use another constructor <see cref="ActionCommand.ActionCommand(Action{object}, string, INotifyPropertyChanged)"/>.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class DemoClass : INotifyPropertyChanged
        /// {
        ///     public event PropertyChangedEventHandler PropertyChanged;
        ///     
        ///     void Exec(object parameter)
        ///     {
        ///         // Your code
        ///     }
        ///     bool CanExec(object parameter) => true;
        ///     bool CanExecProperty { get; set; };
        ///     public ICommand ExecCommand { get; set; }
        ///     
        ///     public DemoClass()
        ///     {
        ///         // CanExecute will always returns true.
        ///         ExecCommand = new ActionCommand(Exec);
        ///         // CanExecute is set to function.
        ///         ExecCommand = new ActionCommand(Exec, CanExec);
        ///         // This will raise the command CanExecuteChanged whenever the DemoClass object raises the PropertyChanged event.
        ///         ExecCommand = new ActionCommand(Exec, CanExec, this);
        ///         // This will wrap the property CanExecProperty in a function, and raise the command CanExecuteChanged 
        ///         // whenever the DemoClass object raises the PropertyChanged event for the CanExecProperty property.
        ///         ExecCommand = new ActionCommand(Exec, o => CanExecProperty, this, nameof(CanExecProperty));
        ///     }
        /// }
        /// </code>
        /// </example>
        public ActionCommand(Action<object> executeAction, Func<object, bool> canExecuteFunction = null, INotifyPropertyChanged source = null, string propertyName = null)
        {
            execute = executeAction;
            canExecute = canExecuteFunction;
            SubscribeToSource(source, propertyName);
        }
        /// <summary>
        /// Creates command that expose a single parameter method which does not return a value (<see cref="Action{Object}"/>), 
        /// and uses a named property or function that returns <c>true</c>/<c>false</c> as CanExecute function.
        /// </summary>
        /// <param name="executeAction">A delegate to that method to be executed in the command.</param>
        /// <param name="canExecuteMember">The name of property or function to be used when CanExecute is called.</param>
        /// <param name="source">The object that implement <see cref="INotifyPropertyChanged"/>  interface that is used to raise <see cref="ICommand.CanExecuteChanged"/> event. This object will be used to find the <paramref name="canExecuteMember"/></param>
        /// <remarks>
        /// <paramref name="canExecuteMember"/> Should be a property that returns <c>bool</c>, a parameterless method that returns <c>bool</c>,
        /// or a single parameter function that returns <c>bool</c>.
        /// In case of properties, the command will raise <see cref="ICommand.CanExecuteChanged"/> 
        /// every time <paramref name="source"/> raises <see cref="INotifyPropertyChanged"/> for that property.
        /// In case of methods, the command will raise <see cref="ICommand.CanExecuteChanged"/> 
        /// every time <paramref name="source"/> raises <see cref="INotifyPropertyChanged"/> for any property.
        /// </remarks>
        /// <example>
        /// <code>
        /// public class DemoClass : INotifyPropertyChanged
        /// {
        ///     public event PropertyChangedEventHandler PropertyChanged;
        ///     
        ///     void Exec(object parameter)
        ///     {
        ///         // Your code
        ///     }
        ///     bool CanExec(object parameter) => true;
        ///     bool CanExecProperty { get; set; } = true;
        ///     public ICommand ExecCommand { get; set; }
        ///     
        ///     public DemoClass()
        ///     {
        ///         // This will raise the command CanExecuteChanged whenever the DemoClass object raises the PropertyChanged event.
        ///         // Because CanExec is a method.
        ///         ExecCommand = new ActionCommand(Exec, nameeof(CanExec), this);
        ///         // This will wrap the property CanDoSomthing2 in a function, and raise the command CanExecuteChanged 
        ///         // whenever the DemoClass object raises the PropertyChanged event for the CanExecProperty property.
        ///         ExecCommand = new ActionCommand(Exec, nameof(CanExecProperty), this);
        ///     }
        /// }
        /// </code>
        /// </example>
        public ActionCommand(Action<object> executeAction, string canExecuteMember, INotifyPropertyChanged source)
        {
            execute = executeAction;
            if (Command.SetCanExecute(canExecuteMember, source, out canExecute) == System.Reflection.MemberTypes.Property)
                SubscribeToSource(source, canExecuteMember);
            else
                SubscribeToSource(source);
        }
    }
}
