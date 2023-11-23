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

namespace UI.Main
{
    using Rules;
    using Rules._EntityModel;
    using System.Collections.ObjectModel;
    using System.IO;

    [Serializable]
    public struct WorkAreaSerialization
    {
        public ObservableCollection<EntityModel> EntitiesList;
        public CanvasSerialization[] TheCanvas;
    }

    /// <summary>
    /// Interaction logic for WorkArea.xaml
    /// </summary>
    public partial class WorkArea : UserControl
    {
        DbAnalyzer TheDbAnalyzer;
        ObservableCollection<EntityModel> EntitiesList;
        bool WorkAreaChanged;

        EntityManagerWindow _EntityManagerWindow;

        public delegate void WorkAreaStateChangedHandler(object sender);
        public event WorkAreaStateChangedHandler WorkAreaStateChanged;

        public delegate void GenerateFilesHandler(object sender);
        public event GenerateFilesHandler GenerateFiles;

        public WorkArea(DbAnalyzer TheDbAnalyzer)
        {
            InitializeComponent();

            this.TheDbAnalyzer = TheDbAnalyzer;

            EntitiesList = new ObservableCollection<EntityModel>();
            foreach (EntityNode node in TheDbAnalyzer.EntitiesForest)
                EntitiesList.Add(node.Value);

            lvEntities.SetBinding(DataContextProperty, new Binding { Source = EntitiesList });
            btnAddTabItem_Click(this, EventArgs.Empty);

            WorkAreaChanged = false;
        }

        private void btnGenerateFiles_Click(object sender, RoutedEventArgs e)
        {
            if (GenerateFiles != null)
                GenerateFiles(this);
        }

        public WorkAreaSerialization GetSerialization()
        {
            WorkAreaSerialization res = new WorkAreaSerialization();
            res.EntitiesList = EntitiesList;

            res.TheCanvas = new CanvasSerialization[MainTabControl.Items.Count];
            for (int i = 0; i < res.TheCanvas.Length; ++i)
                res.TheCanvas[i] = new CanvasSerialization((Canvas)((CloseableTabItem)MainTabControl.Items[i]).Content);
            
            return res;
        }

        public void SetSerialization(WorkAreaSerialization ser, DbAnalyzer TheDbAnalyzer)
        {
            WorkAreaChanged = true;
            this.TheDbAnalyzer = TheDbAnalyzer;

            EntitiesList.Clear();
            foreach (EntityModel model in ser.EntitiesList)
                EntitiesList.Add(model);

            MainTabControl.Items.Clear();

            Canvas CurCanvas;
            Border tbEntityNode;
            EntityNodeTextBlockSerialization tbEntityNodeSer;
            EntityNodeLine lineEntityNode;
            EntityNodeLineSerialization lineEntityNodeSer;

            foreach(CanvasSerialization canvas in ser.TheCanvas)
            {
                btnAddTabItem_Click(this, EventArgs.Empty);
                CurCanvas = (Canvas)((CloseableTabItem)MainTabControl.Items[MainTabControl.Items.Count - 1]).Content;

                foreach(object element in canvas.Elements)
                {
                    tbEntityNodeSer = element as EntityNodeTextBlockSerialization;
                    if (tbEntityNodeSer != null)
                    {
                        tbEntityNode = EntityNodeTextBlock.CreateContainer(tbEntityNodeSer.TheEntity);
                        Canvas.SetLeft(tbEntityNode, tbEntityNodeSer.LeftTop.X);
                        Canvas.SetTop(tbEntityNode, tbEntityNodeSer.LeftTop.Y);

                        tbEntityNode.PreviewMouseLeftButtonDown += Entity_block_PreviewMouseLeftButtonDown;
                        tbEntityNode.PreviewMouseRightButtonDown += entity_block_PreviewMouseRightButtonDown;

                        CurCanvas.Children.Add(tbEntityNode);
                    }

                    else
                    {
                        lineEntityNodeSer = (EntityNodeLineSerialization)element;
                        lineEntityNode = new EntityNodeLine(lineEntityNodeSer.Base, lineEntityNodeSer.Child,
                            lineEntityNodeSer.BasePos, lineEntityNodeSer.ChildPos);

                        lineEntityNode.PreviewMouseLeftButtonDown += NewLine_PreviewMouseLeftButtonDown;

                        CurCanvas.Children.Add(lineEntityNode);
                    }
                }
            }
        }

        EntityNode ClickOneEntity, ClickTwoEntity;
        Border ClickOneContainer, ClickTwoContainer;

        private void Entity_block_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas MainCanvas = UI.Helper.FindAnchestor<Canvas>((DependencyObject)sender);
            if(tbRemove.IsChecked == true)
            {
                EntityNodeTextBlock tbEntityNode = UI.Helper.FindChild<EntityNodeTextBlock>((DependencyObject)sender);
                EntitiesList.Add(tbEntityNode.EntityNode.Value);
                MainCanvas.Children.Remove((UIElement)sender);

                EntityNodeLine tmp;
                int i = 0;
                while(i < MainCanvas.Children.Count)
                {
                    tmp = MainCanvas.Children[i] as EntityNodeLine;
                    if(tmp != null)
                    {
                        if(tmp.BaseEntity == tbEntityNode.EntityNode || tmp.ChildEntity == tbEntityNode.EntityNode)
                        {
                            NewLine_PreviewMouseLeftButtonDown(MainCanvas.Children[i], EventArgs.Empty);
                            continue;
                        }
                    }

                    i++;
                }

                WorkAreaChanged = true;
                if (WorkAreaStateChanged != null)
                    WorkAreaStateChanged(this);
            }

            else if(tbChildMaker.IsChecked == true)
            {
                if (ClickOneEntity == null)
                {
                    ClickOneContainer = (Border)sender;
                    ClickOneEntity = UI.Helper.FindChild<EntityNodeTextBlock>((DependencyObject)sender).EntityNode;
                }

                else if(ClickTwoEntity == null)
                {
                    ClickTwoContainer = (Border)sender;
                    ClickTwoEntity = UI.Helper.FindChild<EntityNodeTextBlock>((DependencyObject)sender).EntityNode;

                    if(ClickOneEntity == ClickTwoEntity)
                        MessageBox.Show("Une entité ne peut pas hériter d'elle même.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

                    else if(ClickTwoEntity.Father != null)
                        MessageBox.Show("Cette entité hérite déjà d'une autre entité.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

                    else if(ClickOneEntity.Father == ClickTwoEntity)
                        MessageBox.Show("Il y a déjà une relation entre ces deux entités.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

                    else
                    {
                        List<PropertyModel> props_list = new List<PropertyModel>();
                        Helper.GetInheritedProperties(ClickOneEntity.Value, ClickTwoEntity.Value, props_list);

                        if (props_list.Count == 0)
                            MessageBox.Show("Impossible de procéder à cet héritage.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

                        else
                        {
                            ClickOneEntity.AddChild(ClickTwoEntity, props_list);

                            EntityNodeLine NewLine = EntityNodeLine.CreateMyLine(ClickOneEntity, ClickTwoEntity, ClickOneContainer, ClickTwoContainer);
                            NewLine.PreviewMouseLeftButtonDown += NewLine_PreviewMouseLeftButtonDown;
                            MainCanvas.Children.Add(NewLine);

                            WorkAreaChanged = true;
                            if (WorkAreaStateChanged != null)
                                WorkAreaStateChanged(this);
                        }
                    }

                    ClickOneEntity = ClickTwoEntity = null;
                }
            }
        }

        EntityNodeTextBlock CurEditEntityBlock;
        Canvas CurEditEntityCanvas;

        void entity_block_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_EntityManagerWindow != null)
                _EntityManagerWindow.Focus();

            else
            {
                CurEditEntityBlock = UI.Helper.FindChild<EntityNodeTextBlock>((DependencyObject)sender);
                CurEditEntityCanvas = UI.Helper.FindAnchestor<Canvas>((DependencyObject)sender);

                string[] EntitiesStringList = new string[TheDbAnalyzer.EntitiesForest.Count];
                for (int i = 0; i < EntitiesStringList.Count(); ++i)
                    EntitiesStringList[i] = TheDbAnalyzer.EntitiesForest[i].Value.Name;

                _EntityManagerWindow = new EntityManagerWindow(CurEditEntityBlock.EntityNode.Value, EntitiesStringList);
                _EntityManagerWindow.AddEntity += _EntityManagerWindow_EditEntity;
                _EntityManagerWindow.Closed += _EntityManagerWindow_Closed;
                _EntityManagerWindow.Show();
            }
        }

        private void _EntityManagerWindow_EditEntity(object sender, AddObjectEventArgs args)
        {
            if(args.Changed && CurEditEntityBlock.EntityNode.Children != null)
            {
                CurEditEntityBlock.EntityNode.ClearChildren();

                var collection = CurEditEntityCanvas.Children;
                EntityNodeLine line;
                for (int i = 0; i < collection.Count; )
                {
                    line = collection[i] as EntityNodeLine;
                    if (line != null && line.BaseEntity == CurEditEntityBlock.EntityNode)
                        collection.Remove(line);

                    else
                        i++;
                }
            }
        }

        private void NewLine_PreviewMouseLeftButtonDown(object sender, EventArgs e)
        {
            if(tbRemove.IsChecked == true)
            {
                EntityNodeLine EntityLine = (EntityNodeLine)sender;

                EntityLine.BaseEntity.RemoveChild(EntityLine.ChildEntity);
                UI.Helper.FindAnchestor<Canvas>((DependencyObject)sender).Children.Remove(EntityLine);

                WorkAreaChanged = true;
                if (WorkAreaStateChanged != null)
                    WorkAreaStateChanged(this);
            }
        }

        /* _______________________________________TOOLBAR_________________________________________________ */
        private void btnGenerateTrees_Click(object sender, RoutedEventArgs e)
        {
            if(WorkAreaChanged == true)
            {
                MessageBoxResult res = 
                    MessageBox.Show("La génération automatique de l'arborescence effacera votre travail en cours.\nVoulez-vous effacer votre travail en cours ?",
                        "Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if(res != MessageBoxResult.Yes)
                    return;
            }

            TheDbAnalyzer.BuildInheritedEntitiesForest();
            if(TheDbAnalyzer.InheritedEntitiesForest.Count == TheDbAnalyzer.EntitiesForest.Count)
            {
                MessageBox.Show("Aucun héritage n'est possible entre vos entités.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MainTabControl.Items.Clear();
            EntitiesList.Clear();
            foreach (EntityNode root in TheDbAnalyzer.InheritedEntitiesForest)
            {
                if (root.Children != null)
                {
                    btnAddTabItem_Click(this, EventArgs.Empty);
                    Canvas CurCanvas = (Canvas)((TabItem)MainTabControl.Items[MainTabControl.Items.Count - 1]).Content;
                    UI.Helper.PrintEntitiesTree(CurCanvas, root);

                    foreach(UIElement element in CurCanvas.Children)
                    {
                        if (element is Border)
                        {
                            element.PreviewMouseLeftButtonDown += Entity_block_PreviewMouseLeftButtonDown;
                            element.PreviewMouseRightButtonDown += entity_block_PreviewMouseRightButtonDown;
                        }

                        else if (element is EntityNodeLine)
                            element.PreviewMouseLeftButtonDown += NewLine_PreviewMouseLeftButtonDown;
                    }
                }

                else
                    EntitiesList.Add(root.Value);
            }

            if (!MainTabControl.Items.IsEmpty)
                MainTabControl.SelectedIndex = 0;

            WorkAreaChanged = false;
        }

        private void btnAddTabItem_Click(object sender, EventArgs e)
        {
            CloseableTabItem NewTabItem = new CloseableTabItem();
            NewTabItem.Title = string.Format("Page {0}", MainTabControl.Items.Count + 1);
            NewTabItem.HeaderClosed += NewTabItem_HeaderClosed;

            Canvas ItemCanvas = new Canvas();
            ItemCanvas.Background = Brushes.CadetBlue;
            ItemCanvas.AllowDrop = true;
            ItemCanvas.DragEnter += MainCanvas_DragEnter;
            ItemCanvas.Drop += MainCanvas_Drop;

            NewTabItem.Content = ItemCanvas;
            MainTabControl.Items.Add(NewTabItem);

            MainTabControl.SelectedIndex = MainTabControl.Items.Count - 1;
            WorkAreaChanged = true;

            if(WorkAreaStateChanged != null)
                WorkAreaStateChanged(this);
        }

        private void btnAddEntity_Click(object sender, RoutedEventArgs e)
        {
            if (_EntityManagerWindow != null)
                _EntityManagerWindow.Focus();
            
            else
            {
                string[] EntitiesStringList = new string[TheDbAnalyzer.EntitiesForest.Count];
                for (int i = 0; i < EntitiesStringList.Count(); ++i)
                    EntitiesStringList[i] = TheDbAnalyzer.EntitiesForest[i].Value.Name;

                _EntityManagerWindow = new EntityManagerWindow(null, EntitiesStringList);
                _EntityManagerWindow.AddEntity += AddEntityHandler;
                _EntityManagerWindow.Closed += _EntityManagerWindow_Closed;
                _EntityManagerWindow.Show();
            }
        }

        void _EntityManagerWindow_Closed(object sender, EventArgs e)
        {
            _EntityManagerWindow = null;
        }

        private void AddEntityHandler(object sender, AddObjectEventArgs Args)
        {
            EntityModel NewEntity = ((EntityManagerWindow)sender).Entity;
            foreach(EntityNode node in TheDbAnalyzer.EntitiesForest)
            {
                if(node.Value.Name == NewEntity.Name)
                {
                    Args.ItsOk = false;
                    Args.Message = "Une entité portant ce nom existe déjà.";
                    return;
                }
            }

            TheDbAnalyzer.AddAnEntity(NewEntity);
            EntitiesList.Add(NewEntity);
        }

        private void NewTabItem_HeaderClosed(object sender)
        {
            Canvas canvas = (Canvas)((CloseableTabItem)sender).Content;
            foreach(UIElement element in canvas.Children)
            {
                if(element is Border)
                {
                    EntityNodeTextBlock block = UI.Helper.FindChild<EntityNodeTextBlock>((DependencyObject)element);
                    EntitiesList.Add(block.EntityNode.Value);
                }
            }
        }

        private void tbRemove_Checked(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.No;
        }

        private void tbChildMaker_Checked(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Pen;
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Arrow;
            ClickOneEntity = ClickTwoEntity = null;
        }

        /* _______________________________________DRAG & DROP_________________________________________________ */
        // Entities List Drag
        Point EntitesListDragStartPosition;
        private void lvEntities_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EntitesListDragStartPosition = e.GetPosition(null);
        }

        private void lvEntities_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Vector tmp_vec = e.GetPosition(null) - EntitesListDragStartPosition;
            if(e.LeftButton == MouseButtonState.Pressed &&
               Math.Abs(tmp_vec.X) > SystemParameters.MinimumHorizontalDragDistance &&
               Math.Abs(tmp_vec.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                ListViewItem lv_item = UI.Helper.FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if(lv_item != null)
                {
                    EntityModel entity = (EntityModel)(sender as ListView).ItemContainerGenerator.ItemFromContainer(lv_item);

                    DataObject dataobj = new DataObject("lvEntitiesItem", entity);
                    DragDrop.DoDragDrop(lv_item, dataobj, DragDropEffects.Move);
                }
            }
        }

        // Main Canvas Drop
        private void MainCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if(!e.Data.GetDataPresent("lvEntitiesItem") && !e.Data.GetDataPresent("MainCanvasItem"))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void MainCanvas_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent("lvEntitiesItem"))
            {
                EntityModel entity = (EntityModel)e.Data.GetData("lvEntitiesItem");
                EntitiesList.Remove(entity);
                Border entity_block = EntityNodeTextBlock.CreateContainer(new EntityNode(entity));

                entity_block.PreviewMouseLeftButtonDown += Entity_block_PreviewMouseLeftButtonDown;
                entity_block.PreviewMouseRightButtonDown += entity_block_PreviewMouseRightButtonDown;

                Canvas canvas = sender as Canvas;
                Point mousepos = e.GetPosition(canvas);

                Canvas.SetLeft(entity_block, mousepos.X);
                Canvas.SetTop(entity_block, mousepos.Y);
                canvas.Children.Add(entity_block);

                WorkAreaChanged = true;
                if (WorkAreaStateChanged != null)
                    WorkAreaStateChanged(this);
            }
        }
    }
}
