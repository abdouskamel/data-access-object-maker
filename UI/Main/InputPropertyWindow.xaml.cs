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

namespace UI.Main
{
    /// <summary>
    /// Logique d'interaction pour InputPropertyWindow.xaml
    /// </summary>
    
    public class AddObjectEventArgs
    {
        public bool ItsOk = true;
        public bool Changed = false;
        public string Message;
    }

    public partial class InputPropertyWindow : Window
    {
        public delegate void AddPropertyEventHandler(object sender, AddObjectEventArgs Args);
        public event AddPropertyEventHandler AddProperty;

        public InputPropertyWindow()
        {
            InitializeComponent();

            cbDbType.ItemsSource = Enum.GetNames(typeof(System.Data.DbType));
            cbDbType.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if(tbPropertyName.Text.Length == 0)
            {
                MessageBox.Show("Veuillez indiquer le nom de la propriété.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(AddProperty != null)
            {
                AddObjectEventArgs args = new AddObjectEventArgs();
                AddProperty(this, args);
                if (!args.ItsOk)
                {
                    MessageBox.Show(args.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                else
                    Close();
            }
        }
    }
}
