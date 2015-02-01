using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using VisiPlacement;

namespace ActivityRecommendation
{
    class TitledTextbox : TitledControl
    {
        public TitledTextbox(string startingTitle)
            : base(startingTitle)
        {
            // create a text field
            this.textField = new TextBox();

            this.SetContent(new TextboxLayout(this.textField));
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

        TextBox textField;
    }
}
