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
            // Show welcome window first
            if (WelcomePage.ShowWelcome())
            {
                // User clicked Enter - proceed to main app
                NextPage NextPage = new NextPage();
                NextPage.Show();
            }
            else
            {
                // User closed welcome window - exit app
                Shutdown();
            }

            base.OnStartup(e);
        }
    }

}
