using Rules._EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace UI.Main
{
    [Serializable]
    public class EntityNodeLine : UIElement
    {
        public EntityNode BaseEntity { get; protected set; }
        
        public EntityNode ChildEntity { get; protected set; }

        public Point Pos1 { get; protected set; }
        public Point Pos2 { get; protected set; }

        public EntityNodeLine(EntityNode BaseEntity, EntityNode ChildEntity, Point pos1, Point pos2)
        {
            this.BaseEntity = BaseEntity;
            this.ChildEntity = ChildEntity;

            Pos1 = pos1;
            Pos2 = pos2;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawLine(new Pen(Brushes.Black, 2), Pos1, Pos2);
        }

        public static EntityNodeLine CreateMyLine(EntityNode BaseEntity, EntityNode ChildEntity, FrameworkElement BaseContainer, FrameworkElement ChildContainer)
        {
            Point pos1 = new Point(), pos2 = new Point();
            pos1.X = Canvas.GetLeft(BaseContainer) + EntityNodeTextBlock.tbEntityNodeMinWidth / 2;
            pos1.Y = Canvas.GetTop(BaseContainer) + EntityNodeTextBlock.tbEntityNodeMinHeight;

            pos2.X = Canvas.GetLeft(ChildContainer) + EntityNodeTextBlock.tbEntityNodeMinWidth / 2;
            pos2.Y = Canvas.GetTop(ChildContainer);

            return new EntityNodeLine(BaseEntity, ChildEntity, pos1, pos2);
        }
    }
}
