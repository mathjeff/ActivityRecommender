/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;


namespace ActivityRecommendation
{
    class ResizableTextBox
    {
        public ResizableTextBox()
        {
            this.textBox = new TextBox();
            this.textBox.TextWrapping = TextWrapping.Wrap;
            this.textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.textBox.Background = null;
            this.Background = Brushes.White;
            this.SetContent(this.textBox, new Resizability(1, 1));
        }
        public void AddTextChangedHandler(EventHandler<TextChangedEventArgs> h)
        {
            this.textBox.TextChanged += h;
        }
        public string Text
        {
            get
            {
                return this.textBox.Text;
            }
            set
            {
                this.textBox.Text = value;
            }
        }
        public TextAlignment TextAlignment
        {
            set
            {
                this.textBox.TextAlignment = value;
            }
        }

        private TextBox textBox;
    }
}
*/