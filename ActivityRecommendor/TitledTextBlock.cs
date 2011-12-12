using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ActivityRecommendation
{
    class TitledTextblock : DisplayGrid
    {
        public TitledTextblock(string startingTitle)
            : base(2, 1)
        {
            // create a title
            this.titleBlock = new TextBlock();
            this.titleBlock.Text = startingTitle;
            this.titleBlock.Height = 20;
            this.RowDefinitions[0].Height = new System.Windows.GridLength(30);
            this.AddItem(this.titleBlock);

            // create a text field
            this.textField = new TextBlock();
            this.textField.TextAlignment = System.Windows.TextAlignment.Center;
            this.SetItem(this.textField, 1, 0);
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
        /*
        public void SetText(string newText)
        {
            this.textField.Text = newText;
        }
        public string GetText()
        {
            return this.textField.Text;
        }*/

        TextBlock titleBlock;
        TextBlock textField;
    }
}
