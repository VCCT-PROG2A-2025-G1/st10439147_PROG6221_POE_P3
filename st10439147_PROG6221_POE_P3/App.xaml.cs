using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace st10439147_PROG6221_POE_P3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Show welcome window first
            bool navigated = WelcomePage.ShowWelcome();

            if (!navigated)
            {
                Shutdown(); // User closed welcome window
            }
        }
    }
}