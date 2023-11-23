using Rules._EntityModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace UI.Main
{
    [Serializable]
    public class CanvasElementSerialization
    {
        public Point LeftTop { get; set; }

        public CanvasElementSerialization(UIElement element)
        {
            LeftTop = new Point(Canvas.GetLeft(element), Canvas.GetTop(element));
        }
    }

    [Serializable]
    public class EntityNodeTextBlockSerialization
    {
        public Point LeftTop { get; set; }

        public EntityNode TheEntity { get; set; }

        public EntityNodeTextBlockSerialization(Border element)
        {
            LeftTop = new Point(Canvas.GetLeft(element), Canvas.GetTop(element));
            TheEntity = UI.Helper.FindChild<EntityNodeTextBlock>(element).EntityNode;
        }
    }

    [Serializable]
    public class EntityNodeLineSerialization
    {
        public EntityNode Base { get; set; }

        public EntityNode Child { get; set; }

        public Point BasePos { get; set; }

        public Point ChildPos { get; set; }

        public EntityNodeLineSerialization(EntityNodeLine element)
        {
            Base = element.BaseEntity;
            Child = element.ChildEntity;

            BasePos = element.Pos1;
            ChildPos = element.Pos2;
        }
    }

    [Serializable]
    public class CanvasSerialization
    {
        public object[] Elements { get; protected set; }

        public CanvasSerialization(Canvas canvas)
        {
            ArrayList Elements = new ArrayList();

            for (int i = 0; i < canvas.Children.Count; ++i)
            {
                if (canvas.Children[i] is Border)
                    Elements.Add(new EntityNodeTextBlockSerialization((Border)canvas.Children[i]));

                else
                    Elements.Add(new EntityNodeLineSerialization((EntityNodeLine)canvas.Children[i]));
            }

            this.Elements = Elements.ToArray();
        }
    }
}
