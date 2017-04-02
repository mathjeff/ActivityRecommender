using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
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
            this.dateBox.KeyDown += dateBox_KeyDown;

            // Numeric inputs only
            InputScope inputScope = new InputScope();
            InputScopeName inputScopeName = new InputScopeName();
            inputScopeName.NameValue = InputScopeNameValue.Number;
            inputScope.Names.Add(inputScopeName);
            this.dateBox.InputScope = inputScope;

            this.dateBox.TextChanged += new TextChangedEventHandler(dateBox_TextChanged);
            this.SetContent(new TextboxLayout(this.dateBox));
            
            // Use a dateFormat of "yyyy-MM-ddTHH:mm:ss";
            this.dateFormat.Add(new DateCharacter('y', true));
            this.dateFormat.Add(new DateCharacter('y', true));
            this.dateFormat.Add(new DateCharacter('y', true));
            this.dateFormat.Add(new DateCharacter('y', true));
            this.dateFormat.Add(new DateCharacter('-', false));
            this.dateFormat.Add(new DateCharacter('M', true));
            this.dateFormat.Add(new DateCharacter('M', true));
            this.dateFormat.Add(new DateCharacter('-', false));
            this.dateFormat.Add(new DateCharacter('d', true));
            this.dateFormat.Add(new DateCharacter('d', true));
            this.dateFormat.Add(new DateCharacter('T', false));
            this.dateFormat.Add(new DateCharacter('H', true));
            this.dateFormat.Add(new DateCharacter('H', true));
            this.dateFormat.Add(new DateCharacter(':', false));
            this.dateFormat.Add(new DateCharacter('m', true));
            this.dateFormat.Add(new DateCharacter('m', true));
            this.dateFormat.Add(new DateCharacter(':', false));
            this.dateFormat.Add(new DateCharacter('s', true));
            this.dateFormat.Add(new DateCharacter('s', true));

            // initialize date
            this.SetDate(DateTime.Now);

            // update color
            this.updateDateColor();
        }

        void dateBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                this.doBackspace();
                e.Handled = true;
            }
            else
            {
                this.addFillerCharacters();
                Char newchar = (Char)(e.Key - Key.D0 + '0');
                this.dateBox.Text += newchar;
                this.dateBox.SelectionStart = this.dateBox.Text.Length;
                this.dateBox.SelectionLength = 0;
                e.Handled = true;
            }
        }
        void doBackspace()
        {
            // delete an entire block of characters
            while (true)
            {
                if (this.dateBox.Text.Length <= 0)
                    break;
                this.dateBox.Text = this.dateBox.Text.Remove(this.dateBox.Text.Length - 1, 1);
                // delete through the previous special character
                int length = this.dateBox.Text.Length;
                if (length <= 0 || length >= this.dateFormat.Count)
                    break;
                if (!this.dateFormat[length].IsMutable)
                {
                    // We've deleted all of the current block; so now stop
                    break;
                }
            }
            this.dateBox.SelectionStart = this.dateBox.Text.Length;
            this.dateBox.SelectionLength = 0;
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
            return DateTime.TryParse(this.dateBox.Text, out result);
        }

        private String getDateFormatString()
        {
            String format = "";
            foreach (DateCharacter character in this.dateFormat)
            {
                format += character.Character;
            }
            return format;
        }
        public void SetDate(DateTime when)
        {
            string text = when.ToString(this.getDateFormatString());
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
        private void addFillerCharacters()
        {
            while (true)
            {
                int length = this.dateBox.Text.Length;
                if (length < this.dateFormat.Count())
                {
                    if (!this.dateFormat[length].IsMutable)
                    {
                        this.dateBox.Text += this.dateFormat[length].Character;
                        this.dateBox.SelectionStart = this.dateBox.Text.Length;
                        this.dateBox.SelectionLength = 0;
                    }
                    else
                        break;
                }
                else
                {
                    break;
                }
            }
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
        //String dateFormat = "yyyy-MM-ddTHH:mm:ss";
        List<DateCharacter> dateFormat = new List<DateCharacter>();

    }
}

class DateCharacter
{
    public DateCharacter(Char character, bool isMutable)
    {
        this.Character = character;
        this.IsMutable = isMutable;
    }
    public Char Character;
    public bool IsMutable;
}