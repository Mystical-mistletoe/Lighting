using System.Windows;

namespace Lighting
{
    public sealed partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoginWindow login = new LoginWindow();
            login.Show();
        }
    }
}
