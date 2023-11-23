using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if(e.Args.Length != 0)
            {
                if(Path.GetExtension(e.Args[0]) != ".daom")
                {
                    MessageBox.Show("Veuillez fournir un fichier de type .daom","Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Shutdown(1);
                }

                else
                {
                    try
                    {
                        Main.MainWindow win = new Main.MainWindow(e.Args[0]);
                        win.Show();
                    }

                    catch(Exception)
                    {
                        MessageBox.Show("Fichier .daom corrompu.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Shutdown(1);
                    }
                }
            }

            else
            {
                Start.StartWindow win = new Start.StartWindow();
                win.Show();
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Une erreur inconnue s'est produite : " + e.Exception.Message, "Erreur critique", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
