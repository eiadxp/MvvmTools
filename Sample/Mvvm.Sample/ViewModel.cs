using System;
using System.Linq;

namespace MvvmTools.Sample
{
    public class ViewModel
    {
        static public Action<object> ShowMessage { get; set; } = (o) => System.Diagnostics.Debug.WriteLine(o ?? "Empty object");

        public Model[] Models { get; set; } = new[] { new Model("John"), new Model("Eve"), new Model("Charlie"), new Model("Eiad") };

        public void ShowModelsCount() => ShowMessage(Models?.Length);
        public void ShowModelId(Model model) => ShowMessage(model?.Id);
        public void ShowModelName(Model model) => ShowMessage(model?.Name);
        public void ShowModelName(int id) => ShowMessage(Models.FirstOrDefault(m => m.Id == id)?.Name);
    }
}
