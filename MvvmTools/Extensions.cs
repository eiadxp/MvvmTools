using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MvvmTools.Commands;

namespace MvvmTools
{
    public static class Extensions
    {
        internal static void LogWrite(this object obj, object message, string category = null)
        {
            // To be added later for logging.
            Configurations.Logging.Write(obj, message, category);
        }

        static string TryGetName<T>(Expression<Func<T, bool>> expression)
        {
            try
            {
                return (expression.Body as MemberExpression)?.Member?.Name;
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static ActionCommand CreateCommand(this INotifyPropertyChanged source, Action execute) => Command.Create(execute);
        public static ActionCommand CreateCommand(this INotifyPropertyChanged source, Action execute, Func<bool> canExecute) => Command.Create(execute, canExecute, source);
        public static ActionCommand CreateCommand<TSource>(this TSource source, Action execute, Expression<Func<TSource, bool>> canExecuteProperty) where TSource : INotifyPropertyChanged => Command.Create(execute, () => canExecuteProperty.Compile().Invoke(source), source, TryGetName(canExecuteProperty));
        public static ActionCommand CreateCommand(this INotifyPropertyChanged source, Action execute, string canExecuteMember) => Command.Create(execute, canExecuteMember, source);

        public static ActionCommand CreateCommand<T>(this INotifyPropertyChanged source, Action<T> execute) => Command.Create<T>(execute);
        public static ActionCommand CreateCommand<T>(this INotifyPropertyChanged source, Action<T> execute, Func<T, bool> canExecute) => Command.Create<T>(execute, canExecute, source);
        public static ActionCommand CreateCommand<T>(this INotifyPropertyChanged source, Action<T> execute, Func<bool> canExecute) => Command.Create<T>(execute, canExecute, source);
        public static ActionCommand CreateCommand<T, TSource>(this TSource source, Action<T> execute, Expression<Func<TSource, bool>> canExecuteProperty) where TSource : INotifyPropertyChanged => Command.Create<T>(execute, (p) => canExecuteProperty.Compile()(source), source, TryGetName(canExecuteProperty));
        public static ActionCommand CreateCommand<T>(this INotifyPropertyChanged source, Action<T> execute, string canExecute) => Command.Create<T>(execute, canExecute, source);

        public static TaskCommand CreateCommandAsync(this INotifyPropertyChanged source, Func<Task> execute) => Command.CreateAsync(execute);
        public static TaskCommand CreateCommandAsync(this INotifyPropertyChanged source, Func<Task> execute, Func<bool> canExecute) => Command.CreateAsync(execute, canExecute, source);
        public static TaskCommand CreateCommandAsync<TSource>(this TSource source, Func<Task> execute, Expression<Func<TSource, bool>> canExecuteProperty) where TSource : INotifyPropertyChanged => Command.CreateAsync(execute, () => canExecuteProperty.Compile().Invoke(source), source, TryGetName(canExecuteProperty));
        public static TaskCommand CreateCommandAsync(this INotifyPropertyChanged source, Func<Task> execute, string canExecuteMember) => Command.CreateAsync(execute, canExecuteMember, source);

        public static TaskCommand CreateCommandAsync<T>(this INotifyPropertyChanged source, Func<T, Task> execute) => Command.CreateAsync<T>(execute);
        public static TaskCommand CreateCommandAsync<T>(this INotifyPropertyChanged source, Func<T, Task> execute, Func<T, bool> canExecute) => Command.CreateAsync<T>(execute, canExecute, source);
        public static TaskCommand CreateCommandAsync<T>(this INotifyPropertyChanged source, Func<T, Task> execute, Func<bool> canExecute) => Command.CreateAsync<T>(execute, canExecute, source);
        public static TaskCommand CreateCommandAsync<T, TSource>(this TSource source, Func<T, Task> execute, Expression<Func<TSource, bool>> canExecuteProperty) where TSource : INotifyPropertyChanged => Command.CreateAsync<T>(execute, (p) => canExecuteProperty.Compile()(source), source, TryGetName(canExecuteProperty));
        public static TaskCommand CreateCommandAsync<T>(this INotifyPropertyChanged source, Func<T, Task> execute, string canExecute) => Command.CreateAsync<T>(execute, canExecute, source);

    }
}
