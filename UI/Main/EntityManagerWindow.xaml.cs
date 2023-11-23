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
using Rules._EntityModel;
using System.Data;
using System.Collections.ObjectModel;

namespace UI.Main
{
    /// <summary>
    /// Logique d'interaction pour EntityManagerWindow.xaml
    /// </summary>
    public partial class EntityManagerWindow : Window
    {
        AddObjectEventArgs Args;

        public EntityModel Entity { get; protected set; }
        string[] EntitiesList;

        ObservableCollection<PropertyModel> Properties;

        public delegate void AddEntityEventHandler(object sender, AddObjectEventArgs args);
        public event AddEntityEventHandler AddEntity;

        public EntityManagerWindow(EntityModel entity = null, string[] EntitiesList = null)
        {
            InitializeComponent();

            if(entity != null)
            {
                Title = tbEntityName.Text = entity.Name;
                if(entity.Base != null)
                    tbBaseName.Text = entity.Base.Name;

                Properties = new ObservableCollection<PropertyModel>(entity.Properties.Values);
                Entity = entity;
            }

            else
            {
                Properties = new ObservableCollection<PropertyModel>();
                tbEntityName.IsReadOnly = false;
            }

            dgProperties.SetBinding(DataContextProperty, new Binding { Source = Properties });
            this.EntitiesList = EntitiesList;

            Args = new AddObjectEventArgs();
        }

        private void btnAddProperty_Click(object sender, RoutedEventArgs e)
        {
            InputPropertyWindow win = new InputPropertyWindow();
            win.AddProperty += AddPropertyHandler;
            win.ShowDialog();
        }

        private void btnAddEntity_Click(object sender, RoutedEventArgs e)
        {
            if (Entity == null)
            {
                if (tbEntityName.Text.Length == 0)
                {
                    MessageBox.Show("Veuillez préciser un nom pour l'entité.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Entity = new EntityModel(tbEntityName.Text, null, false);
                foreach (PropertyModel prop in Properties)
                    Entity.Properties.Add(prop.Name, prop);
            }

            if (AddEntity != null)
            {
                Args.ItsOk = true;
                AddEntity(this, Args);

                if (!Args.ItsOk)
                {
                    MessageBox.Show(Args.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    Entity = null;
                }

                else
                    Close();
            }
        }

        private void AddPropertyHandler(object sender, AddObjectEventArgs Args)
        {
            InputPropertyWindow win = sender as InputPropertyWindow;
            PropertyModel NewProp;

            foreach(PropertyModel prop in Properties)
            {
                if(win.tbPropertyName.Text == prop.Name)
                {
                    Args.ItsOk = false;
                    Args.Message = "Une propriété avec le même nom existe déjà.";
                    return;
                }
            }

            if(win.tbCLRType.Text.Length != 0)
            {
                Type CLRType;
                try
                {
                    CLRType = Type.GetType(win.tbCLRType.Text, true, true);
                    DbType DbType;
                    Enum.TryParse((string)win.cbDbType.SelectedItem, out DbType);
                    NewProp = new PropertyModel(win.tbPropertyName.Text, false, DbType, CLRType);
                }
                
                catch(Exception)
                {
                    if(EntitiesList == null || !EntitiesList.Contains(win.tbCLRType.Text))
                    {
                        Args.ItsOk = false;
                        Args.Message = "Type CLR inconnu.";
                        return;
                    }

                    NewProp = new PropertyModel
                    {
                        Name = win.tbPropertyName.Text,
                        Inherited = false,
                        CLRType = win.tbCLRType.Text,
                        DbType = DbType.Object
                    };
                }
            }

            else
            {
                DbType DbType;
                Enum.TryParse((string)win.cbDbType.SelectedItem, out DbType);
                NewProp = new PropertyModel(win.tbPropertyName.Text, false, DbType);
            }

            if (win.tbDbName.Text.Length != 0)
                NewProp.DbName = win.tbDbName.Text;

            Properties.Add(NewProp);
            if (Entity != null)
                Entity.Properties.Add(NewProp.Name, NewProp);

            this.Args.Changed = true;
        }
    }
}
