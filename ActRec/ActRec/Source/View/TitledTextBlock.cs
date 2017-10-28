using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class TitledTextblock : TitledControl
    {
        public TitledTextblock(string startingTitle)
            : base(startingTitle)
        {
            this.TitleBlock.HorizontalTextAlignment = TextAlignment.Start;

            // create a text field
            this.textField = new Label();
            this.textField.HorizontalTextAlignment = TextAlignment.Center;
            //this.textField.TextWrapping = System.Windows.TextWrapping.Wrap;
            this.SetContent(new TextblockLayout(this.textField));
            //this.PutItem(this.textField, 1, 0);
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
        
        Label textField;
    }
}
