using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmTools.Commands
{
    /// <summary>
    /// A command that is used to expose async methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this command to expose async methods that returns a <see cref="Task"/>.
    /// You can use a method that returns <c>True</c>/<c>False</c> as delegates for CanExecute.
    /// </para>
    /// <para>
    /// It is more easier to use static members in <see cref="Command"/> class or extension method for <see cref="INotifyPropertyChanged"/> 
    /// interface (Defined in <see cref="Extensions"/> class).
    /// </para>
    /// </remarks>
    public class TaskCommand : CommandBase, ICommand
    {
        /// <summary>
        /// Determine weather the task returned from async method will be started automatically when <see cref="execute"/> is called.
        /// </summary>
        public bool AutoStartTask { get; set; } = true;

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <c>null</c>.</param>
        /// <returns><c>true></c> if this command can be executed; otherwise, <c>false</c>.</returns>
        public override bool CanExecute(object parameter)
        {
            if (canExecute == null) return true;
            return canExecute.Invoke(parameter);
        }
        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <c>null</c>.</param>
        /// <remarks>
        /// If the <see cref="Task.Status"/> of the task returned by underlaying delegate has a value of <see cref="TaskStatus.Created"/>,
        /// and the value of <see cref="AutoStartTask"/> property is <c>true</c>,
        /// the task will be started by calling its <see cref="Task.Start()"/> method.
        /// </remarks>
        public override void Execute(object parameter)
        {
            var task = execute?.Invoke(parameter);
            if (task == null) return;
            if (task.Status == TaskStatus.Created && AutoStartTask) task.Start();
        }

        readonly Func<object, Task> execute;
        readonly Func<object, bool> canExecute;

        /// <summary>
        /// Creates command that expose a single parameter async method, 
        /// and uses a parameterless function that returns <c>true</c>/<c>false</c> as CanExecute function.
        /// </summary>
        /// <param name="executeTask">A delegate to async method to be executed in the command.</param>
        /// <param name="canExecuteFunction">A delegate to a single parameter function that returns <c>bool</c> to be executed and returned in the <see cref="CanExecute(object)"/> function.</param>
        /// <param name="source">The object that implement <see cref="INotifyPropertyChanged"/>  interface that is used to raise <see cref="ICommand.CanExecuteChanged"/> event.</param>
        /// <param name="propertyName">Name of the property which when changed will cause the <see cref="ICommand.CanExecuteChanged"/> event to be raised.</param>
        /// <remarks>
        /// <para>
        /// If <paramref name="canExecuteFunction"/> is null or not specified, both <paramref name="propertyName"/> and <paramref name="source"/>
        /// will be ignored, and <see cref="CanExecute(object)"/> will return <c>true</c> always.
        /// </para>
        /// <para>
        /// If <paramref name="propertyName"/>, <paramref name="source"/>, and <paramref name="canExecuteFunction"/> are set, 
        /// the command will monitor <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// until the <see cref="PropertyChangedEventArgs.PropertyName"/> equals the <paramref name="propertyName"/>, 
        /// where it will raise <see cref="ICommand.CanExecuteChanged"/> event.
        /// If the <paramref name="propertyName"/> is not set, and both <paramref name="source"/> and <paramref name="canExecuteFunction"/> are set, 
        /// the command <see cref="ICommand.CanExecuteChanged"/> event will be raised 
        /// whenever the <see cref="INotifyPropertyChanged.PropertyChanged"/> event of <paramref name="source"/> is raised.
        /// </para>
        /// <para>
        /// Passing <c>null</c> for <paramref name="source"/> will skip monitoring its <see cref="INotifyPropertyChanged.PropertyChanged"/>,
        /// and will ignore <paramref name="propertyName"/>.
        /// </para>
        /// <para>
        /// If you want to use a property as <see cref="CanExecute(object)"/> result, 
        /// you can either put it inside a function delegate (and using its name as <paramref name="propertyName"/>) as in the examples,
        /// or you can use another constructor <see cref="TaskCommand.TaskCommand(Func{object, Task}, string, INotifyPropertyChanged)"/>.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class DemoClass : INotifyPropertyChanged
        /// {
        ///     public event PropertyChangedEventHandler PropertyChanged;
        ///     
        ///     Task Exec(object parameter)
        ///     {
        ///         // Your code
        ///     }
        ///     bool CanExec(object parameter) => true;
        ///     bool CanExecProperty { get; set; } = true;
        ///     public ICommand ExecCommand { get; set; }
        ///     
        ///     public DemoClass()
        ///     {
        ///         // It will create a command to execute Exec, and returns true always for CanExecute.
        ///         ExecCommand = new TaskCommand(Exec);
        ///         // Here CanExecute will return the result of CanExec().
        ///         ExecCommand = new TaskCommand(Exec, CanExec);
        ///         // This will raise the command CanExecuteChanged whenever the DemoClass object raises the PropertyChanged event.
        ///         ExecCommand = new TaskCommand(Exec, CanExec, this);
        ///         // This will wrap the property CanExecProperty in a function, and raise the command CanExecuteChanged 
        ///         // whenever the DemoClass object raises the PropertyChanged event for the CanExecProperty property.
        ///         ExecCommand = new TaskCommand(Exec, p => CanExecProperty, this, nameof(CanExecProperty));
        ///     }
        /// }
        /// </code>
        /// </example>
        public TaskCommand(Func<object, Task> executeTask, Func<object, bool> canExecuteFunction = null, INotifyPropertyChanged source = null, string propertyName = null)
        {
            execute = executeTask;
            canExecute = canExecuteFunction;
            SubscribeToSource(source, propertyName);
        }
        /// <summary>
        /// Creates command that expose parameterless async method, 
        /// and uses a named property or function that returns <c>true</c>/<c>false</c> as CanExecute function.
        /// </summary>
        /// <param name="executeTask">A delegate to async method to be executed in the command.</param>
        /// <param name="canExecuteMember">The name of property or function to be used when CanExecute is called.</param>
        /// <param name="source">The object that implement <see cref="INotifyPropertyChanged"/>  interface that is used to raise <see cref="ICommand.CanExecuteChanged"/> event. This object will be used to find the <paramref name="canExecuteMember"/></param>
        /// <remarks>
        /// <paramref name="canExecuteMember"/> Should be a property that returns <c>bool</c> or a parameterless method that returns <c>bool</c>.
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
        ///     Task DoSomthing1()
        ///     {
        ///         // Your code
        ///     }
        ///     bool CanDoSomthing1() => true;
        ///     public ICommand DoSomthing1Command { get; set; }
        ///     
        ///     Task DoSomthing2()
        ///     {
        ///         // Your code
        ///     }
        ///     bool CanDoSomthing2 { get; set; } = true;
        ///     public ICommand DoSomthing2Command { get; set; }
        ///     
        ///     public DemoClass()
        ///     {
        ///         // This will raise the command CanExecuteChanged whenever the DemoClass object raises the PropertyChanged event.
        ///         // Because CanDoSomthing1 is a method.
        ///         DoSomthing1Command = new TaskCommand(DoSomthing1, nameeof(CanDoSomthing1), this);
        ///         // This will wrap the property CanDoSomthing2 in a function, and raise the command CanExecuteChanged 
        ///         // whenever the DemoClass object raises the PropertyChanged event for the CanDoSomthing2 property.
        ///         // Because CanDoSomthing2 is a property.
        ///         DoSomthing2Command = new TaskCommand(DoSomthing2, nameof(CanDoSomthing2), this);
        ///     }
        /// }
        /// </code>
        /// </example>
        public TaskCommand(Func<object, Task> executeTask, string canExecuteMember, INotifyPropertyChanged source) : this(executeTask)
        {
            if (Command.SetCanExecute(canExecuteMember,source, out canExecute) == System.Reflection.MemberTypes.Property)
                SubscribeToSource(source, canExecuteMember);
            else
                SubscribeToSource(source);
        }
    }
}
