using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

// the DateEntryView class allows the user to select a date
namespace ActivityRecommendation
{
    class DateEntryView : TitledControl
    {
        public DateEntryView(string startingTitle)
        {
            this.textchangedHandlers = new List<TextChangedEventHandler>();

            // create the title
            this.SetTitle(startingTitle);

            // create the box to store the date
            this.dateBox = new ResizableTextBox();
            this.dateBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            //this.dateBox.Width = 150;
            //this.dateBox.Height = 30;
            this.dateBox.AddTextChangedHandler(new TextChangedEventHandler(dateBox_TextChanged));
            this.SetContent(this.dateBox);

            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            // initialize date
            this.SetDate(DateTime.Now);

            // update color
            this.updateDateColor();
        }
        
        public bool IsDateValid()
        {
            try
            {
                this.GetDate();
            }
            catch (System.FormatException)
            {
                return false;
            }
            return true;
        }
        
        public DateTime GetDate()
        {
            // example date: "YYYY-MM-DDThh:mm:ss"
            string text = this.dateBox.Text;
            DateTime result;
            result = DateTime.Parse(text);
            return result;
        }
        
        public void SetDate(DateTime when)
        {
            string text = when.ToString("yyyy-MM-ddTHH:mm:ss");
            this.dateBox.Text = text;
        }

        public void AddTextchangedHandler(TextChangedEventHandler h)
        {
            this.textchangedHandlers.Add(h);
        }

        void updateDateColor()
        {
            if (this.IsDateValid())
            {
                this.dateBox.Background = Brushes.White;
            }
            else
            {
                this.dateBox.Background = Brushes.Red;
            }
        }

        void dateBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.updateDateColor();
            foreach (TextChangedEventHandler handler in this.textchangedHandlers)
            {
                handler.Invoke(sender, e);
            }
        }

        //TextBlock titleBox;
        ResizableTextBox dateBox;
        List<TextChangedEventHandler> textchangedHandlers;

    }
}
