using CODE.Framework.Services.Client;
using G_PortalServer.Contract;
using G_PortalServer.Implementation;
using System.Threading;
using System.Windows;

namespace G_PortalServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex;

        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            ServiceGardenLocal.AddServiceHost(typeof(GameServerService), typeof(IGameServerService));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "G-PortalServer_Dashboard.WPF";

            _mutex = new Mutex(true, appName, out bool isOnly);
            if (!isOnly)
            {
                Current.Shutdown();
            }
            base.OnStartup(e);
        }
    }
}
