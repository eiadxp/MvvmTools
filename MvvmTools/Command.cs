using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MvvmTools.Commands;

namespace MvvmTools
{
    /// <summary>
    /// A static class that contains many methods to create commands.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All <c>Create()</c> method overloads returns and <see cref="ActionCommand"/> that will be executed synchronously 
    /// (in the same thread of the calling function), while <c>CreateAsync()</c> overloads will create <see cref="TaskCommand"/> that will be
    ///  executed asynchronously (on another thread).
    /// </para>
    /// </remarks>
    public static class Command
    {
        #region Internals
        internal static MemberTypes SetCanExecute(string memberName, object source, out Func<object, bool> canExecute)
        {
            if (source != null)
            {
                foreach (var item in source.GetType().GetMembers().Where(m => m.Name == memberName))
                {
                    if (item is PropertyInfo property && property.PropertyType == typeof(bool) && property.GetIndexParameters().Length == 0)
                    {
                        var propertyDelegate = (Func<bool>)property.GetMethod.CreateDelegate(typeof(Func<bool>), source);
                        canExecute = p => propertyDelegate();
                        return MemberTypes.Property;
                    }
                    else if (item is MethodInfo method && method.ReturnType == typeof(bool))
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1)
                        {
                            // TODO: Performance enhance can be done.
                            canExecute = p => (bool)method.Invoke(source, new object[] { p });
                            return MemberTypes.Method;
                        }
                        else if (parameters.Length == 0)
                        {
                            // TODO: Performance enhance can be done.
                            canExecute = p => (bool)method.Invoke(source, null);
                            return MemberTypes.Method;
                        }
                    }
                }
            }
            canExecute = null;
            return MemberTypes.Custom;
        }

        static Func<object, bool> CreateCanExecuteFunc(Func<bool> canExecute) => canExecute == null ? null : new Func<object, bool>(p => canExecute());
        static Func<object, bool> CreateCanExecuteFunc<T>(Func<T, bool> canExecute) => canExecute == null ? null : new Func<object, bool>(p => canExecute((T)p));
        #endregion
        #region ActionCommands
        public static ActionCommand Create(Action execute, Func<bool> canExecute = null, INotifyPropertyChanged source = null, string propertyName = null) => new ActionCommand(p => execute(), CreateCanExecuteFunc(canExecute), source, propertyName);
        public static ActionCommand Create(Action execute, string canExecuteMember, INotifyPropertyChanged source) => new ActionCommand(p => execute(), canExecuteMember, source);

        public static ActionCommand Create<TParameter>(Action<TParameter> execute, Func<TParameter, bool> canExecute, INotifyPropertyChanged source = null, string propertyName = null) => new ActionCommand(p => execute((TParameter)p), p => canExecute((TParameter)p), source, propertyName);
        public static ActionCommand Create<TParameter>(Action<TParameter> execute, Func<bool> canExecute = null, INotifyPropertyChanged source = null, string propertyName = null) => new ActionCommand(p => execute((TParameter)p), CreateCanExecuteFunc(canExecute), source, propertyName);
        public static ActionCommand Create<TParameter>(Action<TParameter> execute, string canExecute, INotifyPropertyChanged source) => new ActionCommand(p => execute((TParameter)p), canExecute, source);

        public static ActionCommand CreateFromFunc<T>(Func<T> execute, Func<bool> canExecute = null, INotifyPropertyChanged source = null, string propertyName = null) => new ActionCommand(p => execute(), CreateCanExecuteFunc(canExecute), source, propertyName);
        public static ActionCommand CreateFromFunc<T>(Func<T> execute, string canExecuteMember, INotifyPropertyChanged source) => new ActionCommand(p => execute(), canExecuteMember, source);

        public static ActionCommand CreateFromFunc<T, TParameter>(Action<TParameter> execute, Func<TParameter, bool> canExecute, INotifyPropertyChanged source = null, string propertyName = null) => new ActionCommand(p => execute((TParameter)p), p => canExecute((TParameter)p), source, propertyName);
        public static ActionCommand CreateFromFunc<T, TParameter>(Action<TParameter> execute, Func<bool> canExecute = null, INotifyPropertyChanged source = null, string propertyName = null) => new ActionCommand(p => execute((TParameter)p), CreateCanExecuteFunc(canExecute), source, propertyName);
        public static ActionCommand CreateFromFunc<T, TParameter>(Action<TParameter> execute, string canExecute, INotifyPropertyChanged source) => new ActionCommand(p => execute((TParameter)p), canExecute, source);
        #endregion
        #region TaskCommands
        public static TaskCommand CreateAsync(Func<Task> execute, Func<bool> canExecute = null, INotifyPropertyChanged source = null, string propertyName = null) => new TaskCommand(p => execute(), CreateCanExecuteFunc(canExecute), source, propertyName);
        public static TaskCommand CreateAsync(Func<Task> execute, string canExecuteMember, INotifyPropertyChanged source) => new TaskCommand(p => execute(), canExecuteMember, source);
        public static TaskCommand CreateAsync<TParameter>(Func<TParameter, Task> execute, Func<bool> canExecute = null, INotifyPropertyChanged source = null, string propertyName = null) => new TaskCommand(p => execute((TParameter)p), CreateCanExecuteFunc(canExecute), source, propertyName);
        public static TaskCommand CreateAsync<TParameter>(Func<TParameter, Task> execute, Func<TParameter, bool> canExecute, INotifyPropertyChanged source = null, string propertyName = null) => new TaskCommand(p => execute((TParameter)p), CreateCanExecuteFunc(canExecute), source, propertyName);
        public static TaskCommand CreateAsync<TParameter>(Func<TParameter, Task> execute, string canExecuteMember, INotifyPropertyChanged source) => new TaskCommand(p => execute((TParameter)p), canExecuteMember, source);

        public static TaskCommand CreateAsync<TParameter>(Action<TParameter> execute, Func<TParameter, bool> canExecute, INotifyPropertyChanged source = null, string propertyName = null) => new TaskCommand(p => Task.Run(() => execute((TParameter)p)), CreateCanExecuteFunc(canExecute), source, propertyName);
        public static TaskCommand CreateAsync<TParameter>(Action<TParameter> execute, Func<bool> canExecute = null, INotifyPropertyChanged source = null, string propertyName = null) => new TaskCommand(p => Task.Run(() => execute((TParameter)p)), CreateCanExecuteFunc(canExecute), source, propertyName);
        public static TaskCommand CreateAsync<TParameter>(Action<TParameter> execute, string canExecute, INotifyPropertyChanged source) => new TaskCommand(p => Task.Run(() => execute((TParameter)p)), canExecute, source);
        #endregion
    }
}
