using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnapShot
{
    class SingleInstanceApplication : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create and show the application's main window
            var window = new MainWindow();
            window.Show();
        }

        public void Activate()
        {
            // Reactivate application's main window
            MainWindow.Show();
            MainWindow.Activate();
            if(MainWindow.WindowState==WindowState.Minimized)
            {
                MainWindow.WindowState = WindowState.Normal;
            }
        }
    }
}
