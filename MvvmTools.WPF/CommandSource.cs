using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MvvmTools.WPF
{
    public class CommandSource : DependencyObject, ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => (Command == null) || Command.CanExecute(UseOwnParameter ? Parameter : parameter);
        public void Execute(object parameter) => Command?.Execute(UseOwnParameter ? Parameter : parameter);

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(CommandSource), 
                new PropertyMetadata(null, CommandChanged));
        private static void CommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CommandSource source)) return;
            if (e.OldValue is ICommand oldCommand) oldCommand.CanExecuteChanged -= source.Command_CanExecuteChanged;
            if (e.NewValue is ICommand newCommand) newCommand.CanExecuteChanged += source.Command_CanExecuteChanged;
            source.CanExecuteChanged?.Invoke(source, EventArgs.Empty);
        }
        void Command_CanExecuteChanged(object sender, EventArgs e) => CanExecuteChanged?.Invoke(this, e);

        public object Parameter
        {
            get { return (object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(CommandSource), new PropertyMetadata(null));

        public bool UseOwnParameter
        {
            get { return (bool)GetValue(UseOwnParameterProperty); }
            set { SetValue(UseOwnParameterProperty, value); }
        }
        public static readonly DependencyProperty UseOwnParameterProperty =
            DependencyProperty.Register("UseOwnParameter", typeof(bool), typeof(CommandSource), new PropertyMetadata(false));
    }
}
