using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmTools.Sample.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            MvvmTools.Sample.ViewModel.ShowMessage = (o) => MessageBox.Show((o?.ToString()) ?? "Empty message");
            base.OnStartup(e);
        }
    }
}
