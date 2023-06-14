using DeLightWPF.Utilities;
using System.Windows;

namespace DeLightWPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            GlobalSettings.Load();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            GlobalSettings.Save();
        }
    }
}
