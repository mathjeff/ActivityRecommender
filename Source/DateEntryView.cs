using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using VisiPlacement;

// the DateEntryView class allows the user to select a date
namespace ActivityRecommendation
{
    class DateEntryView : TitledControl
    {
        public DateEntryView(string startingTitle)
        {
            this.textChanged_handlers = new List<TextChangedEventHandler>();

            // create the title
            this.SetTitle(startingTitle);

            // create the box to store the date
            this.dateBox = new TextBox();
            //this.dateBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            //this.dateBox.Width = 150;
            //this.dateBox.Height = 30;
            this.dateBox.TextChanged += new TextChangedEventHandler(dateBox_TextChanged);
            this.SetContent(new TextboxLayout(this.dateBox));

            //this.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            // initialize date
            this.SetDate(DateTime.Now);

            // update color
            this.updateDateColor();
        }
        
        public bool IsDateValid()
        {
            DateTime result;
            return this.Parse(out result);
        }
        
        public DateTime GetDate()
        {
            DateTime result;
            bool valid = this.Parse(out result);
            if (!valid)
                throw new FormatException("Invalid date");
            return result;
        }

        private bool Parse(out DateTime result)
        {
            // example date: "YYYY-MM-DDThh:mm:ss"
            return DateTime.TryParse(this.dateBox.Text, out result);
        }
        
        public void SetDate(DateTime when)
        {
            string text = when.ToString("yyyy-MM-ddTHH:mm:ss");
            this.dateBox.Text = text;
        }

        public void Add_TextChanged_Handler(TextChangedEventHandler h)
        {
            this.textChanged_handlers.Add(h);
        }

        void updateDateColor()
        {
            if (this.IsDateValid())
                this.appearValid();
            else
                this.appearInvalid();
        }

        // alters the appearance to indicate that the given date is not valid
        public void appearValid()
        {
            this.dateBox.Background = new SolidColorBrush(Colors.White);
        }

        // alters the appearance to indicate that the given date is not valid
        public void appearInvalid()
        {
            this.dateBox.Background = new SolidColorBrush(Colors.Red);
        }
        void dateBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.updateDateColor();
            foreach (TextChangedEventHandler handler in this.textChanged_handlers)
            {
                handler.Invoke(sender, e);
            }
        }

        //TextBlock titleBox;
        TextBox dateBox;
        List<TextChangedEventHandler> textChanged_handlers;

    }
}
