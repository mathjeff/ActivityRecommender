using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ActivityRecommendation
{
    class TitledTextblock : TitledControl
    {
        public TitledTextblock(string startingTitle)
            : base(startingTitle)
        {
            this.TitleBlock.HorizontalAlignment = HorizontalAlignment.Left;

            // create a text field
            this.textField = new ResizableTextBlock();
            this.textField.TextAlignment = System.Windows.TextAlignment.Center;
            //this.textField.TextWrapping = System.Windows.TextWrapping.Wrap;
            this.SetContent(this.textField);
            this.PutItem(this.textField, 1, 0);
        }
        public string Text
        {
            get
            {
                return this.textField.Text;
            }
            set
            {
                this.textField.Text = value;
            }
        }

        ResizableTextBlock textField;
    }
}
