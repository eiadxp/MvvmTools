using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MvvmTools.Demo
{
    public class DemoClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        bool Set<T>(ref T field, T newValue, [CallerMemberName] string property = "")
        {
            if (Equals(field, newValue)) return false;
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            (ExecWithParameterAndConditionCommand as Commands.ICommandNotify)?.NotifyCanExecuteChanged();
            return true;
        }
        readonly Action<string> ShowMessage;
        public DemoClass() : this(null) { }
        public DemoClass(Action<string> showMessage = null)
        {
            ShowMessage = showMessage ?? (s => System.Diagnostics.Debug.WriteLine(s));
            EnableAllCommands = true;
            CreateCommands();
        }

        void CreateCommands()
        {
            _Parameter = "Test";
            ExecCommand = Command.Create(Exec, nameof(CanExec), this);
            ExecWithParameterCommand = this.CreateCommand<string>(ExecWithParameter, nameof(CanExecWithParameter));
            ExecWithParameterAndConditionCommand = this.CreateCommand<string>(ExecWithParameterAndCondition, nameof(CanExecWithParameterAndCondition));
        }

        bool _EnableAllCommands;
        public bool EnableAllCommands
        {
            get { return _EnableAllCommands; }
            set
            {
                Set(ref _EnableAllCommands, value);
                CanExec = value;
                CanExecWithParameter = value;
            }
        }

        private string _Parameter;
        public string Parameter { get => _Parameter; set => Set(ref _Parameter, value); }


        public void Exec() => ShowMessage("This is a parameterless function.");
        bool _CanExec;
        public bool CanExec { get => _CanExec; set => Set(ref _CanExec, value); }
        public ICommand ExecCommand { get; set; }

        public void ExecWithParameter(string parameter) => ShowMessage("This is a parameter function:\n\n" + parameter);
        bool _CanExecWithParameter;
        public bool CanExecWithParameter { get => _CanExecWithParameter; set => Set(ref _CanExecWithParameter, value); }
        public ICommand ExecWithParameterCommand { get; set; }

        public void ExecWithParameterAndCondition(string parameter) => ShowMessage("This is a parameter function that execute if parameter value is:\n\n" + parameter);
        public bool CanExecWithParameterAndCondition(string parameter) => parameter == "mvvmtools";
        public ICommand ExecWithParameterAndConditionCommand { get; set; }
    }
}
