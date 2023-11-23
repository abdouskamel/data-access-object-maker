using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.Common;
using System.Data;

namespace UI.Start
{
    using Microsoft.Win32;
    using Rules;

    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        NamesWritingWindow NamesWritingWin;

        public StartWindow()
        {
            InitializeComponent();
            NamesWritingWin = new NamesWritingWindow();

            DataRowCollection providers = DbProviderFactories.GetFactoryClasses().Rows;
            string[] cbProvidersSource = new string[providers.Count];
            for(int i = 0; i < providers.Count; ++i)
                cbProvidersSource[i] = providers[i]["InvariantName"].ToString();

            cbProviders.SetBinding(DataContextProperty, new Binding { Source = cbProvidersSource, Mode = BindingMode.OneTime });
        }

        private void StartTheAnalyze(DbAnalyzer analyzer)
        {
            DbAnalyseArgs args = new DbAnalyseArgs();
            args.PropertiesForm = (NamesWritingForm)NamesWritingWin.cbPropertiesForm.SelectedIndex;
            args.MethodsForm = (NamesWritingForm)NamesWritingWin.cbMethodsForm.SelectedIndex;
            args.ClassesForm = (NamesWritingForm)NamesWritingWin.cbClassesForm.SelectedIndex;
            args.NamespacesForm = (NamesWritingForm)NamesWritingWin.cbNamespacesForm.SelectedIndex;

            try
            {
                analyzer.AnalyseTheDatabase(args);
                Main.MainWindow _main = new Main.MainWindow(analyzer);
                _main.Show();
                Close();
            }

            catch (DbConnectionException _e)
            {
                MessageBox.Show(_e.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //catch(Exception)
            //{
            //    MessageBox.Show("Une erreur inconnue s'est produite, veuillez vérifier que votre base de données est bien valide", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        private void btnOpenConnection_Click(object sender, RoutedEventArgs e)
        {
            string provider = cbProviders.SelectedItem.ToString(),
                   connection_string = tbConnectionString.Text;

            if(connection_string.Length == 0)
            {
                MessageBox.Show("Veuillez entrer une chaîne de connexion.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            StartTheAnalyze(new DbAnalyzer(provider, connection_string));
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            if(tbBddPath.Text.Length == 0)
            {
                MessageBox.Show("Veuillez spécifier le chemin et le nom de votre base de données.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(System.IO.Path.GetExtension(tbBddPath.Text) != ".daom")
                StartTheAnalyze(new DbAnalyzer(tbBddPath.Text, tbBddName.Text, tbBddPasswd.Password));

            else
            {
                try
                {
                    Main.MainWindow MainWindow = new Main.MainWindow(tbBddPath.Text);
                    MainWindow.Show();
                    Close();
                }

                catch(Exception)
                {
                    MessageBox.Show("Impossible d'ouvrir le fichier .daom spécifié, ce fichier est invalide ou endommagé.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnGetBddPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Files (*.daom, *.accdb, *.mdf)|*.daom;*.accdb;*.mdf|Databases (*.mdf, *.accdb)|*.mdf;*.accdb|DAOMaker File (*.daom)|*.daom|SQLExpress Database (*.mdf)|*.mdf|MS Access Database (*.accdb)|*.accdb";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if(dialog.ShowDialog() == true)
            {
                tbBddPath.Text = dialog.FileName;
                tbBddPasswd.IsEnabled = tbBddName.IsEnabled = 
                    sndTbSyntaxSelection.IsEnabled = System.IO.Path.GetExtension(dialog.FileName) != ".daom";
            }
        }

        private void tbSyntaxSelection_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NamesWritingWin.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (NamesWritingWin != null)
                NamesWritingWin.Close();
        }
    }
}
