using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UI.Main
{
    using Rules._EntityModel;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    public class EntityNodeTextBlock : TextBlock
    {
        public EntityNodeTextBlock() : base()
        {
            
        }

        public EntityNodeTextBlock(EntityNode Entity) : base()
        {
            this.EntityNode = Entity;
        }

        // Dependency Property : Entity
        public EntityNode EntityNode
        {
            get
            {
                return (EntityNode)GetValue(EntityProperty);
            }

            set
            {
                SetValue(EntityProperty, value);
            }
        }

        public static readonly DependencyProperty EntityProperty =
            DependencyProperty.Register("Entity", typeof(EntityNode), typeof(EntityNodeTextBlock),
                new UIPropertyMetadata(null, new PropertyChangedCallback(OnEntityPropertyChanged)));

        private static void OnEntityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue != null)
            {
                ((EntityNodeTextBlock)d).Text = ((EntityNode)e.NewValue).Value.Name;
            }
        }

        public static readonly double tbEntityNodeMinWidth = 100d, tbEntityNodeMinHeight = 30d;

        public static Border CreateContainer(EntityNode node)
        {
            EntityNodeTextBlock entity_text = new EntityNodeTextBlock(node);

            entity_text.FontSize = 20;
            entity_text.HorizontalAlignment = HorizontalAlignment.Center;
            entity_text.VerticalAlignment = VerticalAlignment.Center;

            Border entity_block = new Border();
            entity_block.BorderBrush = Brushes.Black;
            entity_block.BorderThickness = new Thickness(1);
            entity_block.Background = Brushes.White;

            entity_block.MinWidth = tbEntityNodeMinWidth;
            entity_block.MinHeight = tbEntityNodeMinHeight;
            entity_block.Cursor = Cursors.Hand;
            entity_block.Child = entity_text;

            TextBlock ToolTip = new TextBlock();

            if (node.Value.Properties.Count == 0)
                ToolTip.Text = "Aucune propriété.";

            else
            {
                StringBuilder ToolTipText = new StringBuilder();

                foreach (PropertyModel prop in node.Value.Properties.Values)
                {
                    ToolTipText.Append(string.Format("| {0} | DbType :  {1} | CLRType : {2}\n", prop.Name, prop.DbType, prop.CLRType));
                }

                ToolTipText.Remove(ToolTipText.Length - 1, 1);
                ToolTip.Text = ToolTipText.ToString();
                ToolTip.FontSize = 16;
            }

            entity_block.ToolTip = ToolTip;
            return entity_block;
        }
    }
}
