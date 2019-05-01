using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;

namespace MvvmTools.Sample
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            ShowModelsCountCommand = Command.Create(ShowModelsCount);
            ShowModelIdCommand = Command.Create<Model>(ShowModelId);
        }
        static public Action<object> ShowMessage { get; set; } = (o) => System.Diagnostics.Debug.WriteLine(o ?? "Empty object");

        public List<Model> Models { get; set; }

        public void ShowModelsCount() => ShowMessage(Models?.Count);
        public ICommand ShowModelsCountCommand { get; private set; }
        public void ShowModelId(Model model) => ShowMessage(model?.Id);
        public ICommand ShowModelIdCommand { get; private set; }
        public void ShowModelName(Model model) => ShowMessage(model?.Name);
        public void ShowModelName(int id) => ShowMessage(Models.FirstOrDefault(m => m.Id == id)?.Name);
        public void LoadData()
        {
            Models = new List<Model>(new[] { new Model("John"), new Model("Eve"), new Model("Charlie"), new Model("Eiad") });
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Models)));
            ShowMessage("Data loaded");
        }
    }
}
