using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI.Main
{
    class CloseableTabItem : TabItem
    {
        CloseableHeader CloseableHeader;

        public CloseableTabItem()
        {
            CloseableHeader = new CloseableHeader();
            Header = CloseableHeader;

            CloseableHeader.CloseButton.Click += CloseButton_Click;
        }

        public delegate void HeaderClosedHandler(object sender);
        public event HeaderClosedHandler HeaderClosed;

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderClosed(this);
            ((TabControl)Parent).Items.Remove(this);
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            CloseableHeader.CloseButton.Visibility = Visibility.Visible;
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            CloseableHeader.CloseButton.Visibility = Visibility.Hidden;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            CloseableHeader.CloseButton.Visibility = Visibility.Visible;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if(!IsSelected)
                CloseableHeader.CloseButton.Visibility = Visibility.Hidden;
        }

        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }

            set
            {
                SetValue(TitleProperty, value);
            }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CloseableTabItem),
                new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnTitlePropertyChanged)));

        private static void OnTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue != null)
            {
                ((CloseableTabItem)d).CloseableHeader.TabTitleLabel.Content = e.NewValue;
            }
        }
    }
}
