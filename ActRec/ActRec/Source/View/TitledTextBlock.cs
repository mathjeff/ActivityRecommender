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
            this.TitleLayout.AlignHorizontally(TextAlignment.Start);

            // create a text field
            this.textField = new TextblockLayout();
            this.textField.AlignHorizontally(TextAlignment.Center);
            this.SetContent(this.textField);
        }
        public string Text
        {
            get
            {
                return this.textField.getText();
            }
            set
            {
                this.textField.setText(value);
            }
        }
        
        TextblockLayout textField;
    }
}
