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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace UI.Main
{
    using Microsoft.Win32;
    using Rules;

    [Serializable]
    struct AppSerialization
    {
        public DbAnalyzer TheDbAnalyzer;
        public WorkAreaSerialization _WorkArea;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DbAnalyzer TheDbAnalyzer;
        WorkArea _WorkArea;

        string SavePath;
        bool Saved;

        public MainWindow(DbAnalyzer TheDbAnalyzer)
        {
            InitializeComponent();

            this.TheDbAnalyzer = TheDbAnalyzer;
            Title = Title.Insert(0, TheDbAnalyzer.DatabaseName);

            _WorkArea = new WorkArea(TheDbAnalyzer);
            _WorkArea.WorkAreaStateChanged += _WorkArea_WorkAreaStateChanged;
            _WorkArea.GenerateFiles += _WorkArea_GenerateFiles;
            DockPanel.SetDock(_WorkArea, Dock.Left);
            this.MainDock.Children.Add(_WorkArea);

            Saved = true;
        }

        public MainWindow(string DaomFilePath)
        {
            InitializeComponent();

            OpenNewSession(DaomFilePath);

            _WorkArea.WorkAreaStateChanged += _WorkArea_WorkAreaStateChanged;
            _WorkArea.GenerateFiles += _WorkArea_GenerateFiles;
            DockPanel.SetDock(_WorkArea, Dock.Left);
            this.MainDock.Children.Add(_WorkArea);
        }

        private void _WorkArea_GenerateFiles(object sender)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TheDbAnalyzer.GenerateTheDao(dialog.SelectedPath);
            }
        }

        public void OpenNewSession(string SavePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(SavePath, FileMode.Open, FileAccess.Read))
            {
                AppSerialization app_ser = (AppSerialization)formatter.Deserialize(fs);
                TheDbAnalyzer = app_ser.TheDbAnalyzer;

                if (_WorkArea == null)
                    _WorkArea = new WorkArea(TheDbAnalyzer);
                _WorkArea.SetSerialization(app_ser._WorkArea, TheDbAnalyzer);

                this.SavePath = SavePath;
                Saved = true;
            }
        }

        public bool VerifySavedSession()
        {
            if(!Saved)
            {
                MessageBoxResult res = System.Windows.MessageBox.Show("Votre session n'a pas été sauvegardée, voulez-vous la sauvegarder ?", 
                    "Sauvegarde", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (res == MessageBoxResult.Yes)
                {
                    miSaveSession_Click(this, EventArgs.Empty);
                    return true;
                }

                else if (res == MessageBoxResult.Cancel)
                    return false;
            }

            return true;
        }

        private void miOpenNewSession_Click(object sender, RoutedEventArgs e)
        {
            if (!VerifySavedSession())
                return;

            OpenFileDialog filedialog = new OpenFileDialog();
            filedialog.Filter = "DAOMaker Files (*.daom) | *.daom";

            if(filedialog.ShowDialog() == true)
            {
                try
                {
                    OpenNewSession(filedialog.FileName);
                }
                
                catch(Exception)
                {
                    System.Windows.MessageBox.Show("Impossible d'ouvrir le fichier .daom spécifié, ce fichier est invalide ou endommagé.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void miSaveSession_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SavePath))
            {
                miSaveSessionAs_Click(sender, e);
                return;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(SavePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                AppSerialization app_ser = new AppSerialization();
                app_ser.TheDbAnalyzer = TheDbAnalyzer;
                app_ser._WorkArea = _WorkArea.GetSerialization();

                formatter.Serialize(fs, app_ser);
                Saved = true;
            }
        }

        private void miSaveSessionAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog filedialog = new SaveFileDialog();
            filedialog.Filter = "DAOMaker Files (*.daom) | *.daom";

            if (filedialog.ShowDialog() == true)
            {
                SavePath = filedialog.FileName;
                miSaveSession_Click(sender, e);
            }
        }

        private void miExitSession_Click(object sender, RoutedEventArgs e)
        {
            if(VerifySavedSession())
            {
                (new Start.StartWindow()).Show();
                Close();
            }
        }

        private void miExitApp_Click(object sender, RoutedEventArgs e)
        {
            if(VerifySavedSession())
                Close();
        }

        private void _WorkArea_WorkAreaStateChanged(object sender)
        {
            Saved = false;
        }
    }
}
