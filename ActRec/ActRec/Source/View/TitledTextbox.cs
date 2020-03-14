using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class TitledTextbox : TitledControl
    {
        public TitledTextbox(string startingTitle)
            : this(startingTitle, new Editor())
        {
        }

        public TitledTextbox(string startingTitle, Editor textBox)
            : base(startingTitle)
        {
            // create a text field
            this.textField = textBox;
            this.contentLayout = new TextboxLayout(this.textField);
            this.SetContent(this.contentLayout);
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

        Editor textField;
        TextboxLayout contentLayout;
    }
}
