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
            : this(startingTitle, new TextBox())
        {
        }

        public TitledTextbox(string startingTitle, TextBox textBox)
            : base(startingTitle)
        {
            // create a text field
            this.textField = textBox;
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
