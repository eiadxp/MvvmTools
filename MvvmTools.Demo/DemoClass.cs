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
            return true;
        }
        readonly Action<string> ShowMessage;
        public DemoClass(Action<string> showMessage = null)
        {
            ShowMessage = showMessage ?? (s => System.Diagnostics.Debug.WriteLine(s));
            EnableAllCommands = true;
            CreateCommands();
        }

        void CreateCommands()
        {
            ExecCommand = Command.Create(Exec, nameof(CanExec), this);
            ExecWithParameterCommand = this.CreateCommand<string>(ExecWithParameter, nameof(CanExecWithParameter));
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

        public void Exec() => ShowMessage("This is a parameterless function.");
        bool _CanExec;
        public bool CanExec { get => _CanExec; set => Set(ref _CanExec, value); }
        public ICommand ExecCommand { get; set; }

        public void ExecWithParameter(string parameter) => ShowMessage("This is a parameter function:\n\n" + parameter);
        bool _CanExecWithParameter;
        public bool CanExecWithParameter { get => _CanExecWithParameter; set => Set(ref _CanExecWithParameter, value); }
        public ICommand ExecWithParameterCommand { get; set; }
    }
}
