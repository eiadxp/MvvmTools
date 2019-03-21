using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Xamarin.Forms;

namespace MvvmTools.XForms
{
    public class CommandSource : BindableObject, ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => (Command == null) || Command.CanExecute(UseOwnParameter ? Parameter : parameter);
        public void Execute(object parameter) => Command?.Execute(UseOwnParameter ? Parameter : parameter);

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(CommandSource), null, propertyChanged: CommandChanged);
        private static void CommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is CommandSource source)) return;
            if (oldValue is ICommand oldCommand) oldCommand.CanExecuteChanged -= source.Command_CanExecuteChanged;
            if (newValue is ICommand newCommand) newCommand.CanExecuteChanged += source.Command_CanExecuteChanged;
            source.CanExecuteChanged?.Invoke(source, EventArgs.Empty);
        }
        void Command_CanExecuteChanged(object sender, EventArgs e) => CanExecuteChanged?.Invoke(this, e);

        public object Parameter
        {
            get { return (object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }
        public static readonly BindableProperty ParameterProperty =
            BindableProperty.Create(nameof(Parameter), typeof(object), typeof(CommandSource), null);

        public bool UseOwnParameter
        {
            get { return (bool)GetValue(UseOwnParameterProperty); }
            set { SetValue(UseOwnParameterProperty, value); }
        }
        public static readonly BindableProperty UseOwnParameterProperty =
            BindableProperty.Create(nameof(UseOwnParameter), typeof(bool), typeof(CommandSource), null);
    }
}
