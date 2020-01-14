using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SteamFastSwitcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex = null;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string appName = System.Windows.Forms.Application.ProductName;
            bool createdNew;

            mutex = new Mutex(true, appName, out createdNew);
            if (!createdNew) System.Environment.Exit(-1);
        }
    }
}
