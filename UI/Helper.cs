using Rules._EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UI.Main;

namespace UI
{
    static class Helper
    {
        public static T FindAnchestor<T>(DependencyObject obj) where T:DependencyObject
        {
            while(obj != null)
            {
                if (obj is T)
                    return (T)obj;

                obj = VisualTreeHelper.GetParent(obj);

            } 

            return null;
        }

        public static T FindChild<T>(DependencyObject obj) where T:DependencyObject
        {
            while(obj != null)
            {
                if (obj is T)
                    return (T)obj;

                obj = VisualTreeHelper.GetChild(obj, 0);
            }

            return null;
        }

        public static void PrintEntitiesTree(Canvas canvas, EntityNode entity)
        {
            PrintEntitiesTree_bis(canvas, new Point(400, 30), entity);
        }

        private static FrameworkElement PrintEntitiesTree_bis(Canvas canvas, Point Position, EntityNode entity)
        {
            Border EntityBlock = EntityNodeTextBlock.CreateContainer(entity);
            Canvas.SetLeft(EntityBlock, Position.X);
            Canvas.SetTop(EntityBlock, Position.Y);
            canvas.Children.Add(EntityBlock);

            if(entity.Children != null)
            {
                FrameworkElement ChildBlock;
                Point ChildPos = new Point(Position.X / entity.Children.Count, Position.Y + EntityNodeTextBlock.tbEntityNodeMinHeight * 2);
                foreach(EntityNode child in entity.Children)
                {
                    ChildBlock = PrintEntitiesTree_bis(canvas, ChildPos, child);
                    EntityNodeLine NewLine = EntityNodeLine.CreateMyLine(entity, child, EntityBlock, ChildBlock);

                    canvas.Children.Add(NewLine);
                    ChildPos.X += EntityNodeTextBlock.tbEntityNodeMinWidth + Position.X / entity.Children.Count;
                }
            }
            
            return EntityBlock;
        }
    }
}
