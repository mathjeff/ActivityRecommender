using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ActivityRecommendation
{
    class TitledTextbox : TitledControl
    {
        public TitledTextbox(string startingTitle)
            : base(startingTitle)
        {
            
            // create a text field
            this.textField = new ResizableTextBox();
            //this.textField.TextWrapping = System.Windows.TextWrapping.Wrap;
            //this.textField.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.PutItem(this.textField, 1, 0);
        }
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            System.Windows.Size result = base.MeasureOverride(constraint);
            return result;
        }
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size arrangeSize)
        {
            System.Windows.Size result = base.ArrangeOverride(arrangeSize);
            return result;
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

        ResizableTextBox textField;
    }
}
