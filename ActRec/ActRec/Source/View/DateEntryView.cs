using System;
using System.Collections.Generic;
using System.Linq;
using VisiPlacement;
using Xamarin.Forms;

// the DateEntryView class allows the user to select a date
namespace ActivityRecommendation
{
    // a DateEntryView is a small button that displays a DateTime. If a DateEntryView is clicked, it brings up a fullscreen editing view
    class DateEntryView : ContainerLayout, OnBack_Listener
    {
        public static bool Parse(string text, out DateTime result)
        {
            if (text != null)
            {
                if (text.Length == 4)
                {
                    // allow parsing a 4-digit number as a year
                    text = text + "-01";
                }
                if (text.Length == 13)
                {
                    // Allow skipping specifying the number of minutes and instead treating them as 0
                    // For example, "2019-01-02T03" will be treated as "2019-01-02T03:00"
                    text = text + ":00";
                }
            }
            return DateTime.TryParse(text, out result);
        }

        public DateEntryView(string title, LayoutStack layoutStack)
        {
            this.layoutStack = layoutStack;
            this.chooseDate_button = new Button();
            this.chooseDate_button.Clicked += ChooseDate_button_Clicked;
            this.title = title;
            this.SubLayout = new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(new TextblockLayout(title).AlignHorizontally(TextAlignment.Center))
                .AddLayout(ButtonLayout.WithoutBevel(this.chooseDate_button))
                .Build();

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

            this.implView = new FullscreenDateEntryView(title, this.dateFormat, layoutStack);
        }

        private void ChooseDate_button_Clicked(object sender, EventArgs e)
        {
            // If the user is pressing this button, then they want to change the date
            // So, one or more components of the date are wrong
            // So, we automatically do some backspacing before showing the screen to the user, so the user doesn't need to immediately delete the minutes+seconds
            string suggestedDateText = this.getDateText();
            int length = 13; // yyyy-MM-ddTHH
            if (suggestedDateText.Length > length)
                suggestedDateText = suggestedDateText.Substring(0, length);
            this.implView.DateText = suggestedDateText;
            this.implView.NowText = DateTime.Now.ToString(this.getDateFormatString()) + " is the current time.";
            this.layoutStack.AddLayout(this.implView, this.title, this);
        }

        public void Add_TextChanged_Handler(EventHandler<TextChangedEventArgs> h)
        {
            if (h == null)
            {
                throw new ArgumentException("cannot add null textchanged_handler to " + this);
            }
            this.textChanged_handlers.Add(h);
        }

        private string getDateText()
        {
            return this.dateText;
        }

        public void appearInvalid()
        {
            this.chooseDate_button.BackgroundColor = Color.Red;
        }
        public void appear_defaultValid()
        {
            this.chooseDate_button.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
        }
        public void appearConcerned()
        {
            this.chooseDate_button.BackgroundColor = Color.Yellow;
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
            return Parse(this.getDateText(), out result);
        }

        private string getDateFormatString()
        {
            string format = "";
            foreach (DateCharacter character in this.dateFormat)
            {
                format += character.Character;
            }
            return format;
        }
        public void SetDate(DateTime when)
        {
            this.setDateText(when.ToString(this.getDateFormatString()));
        }
        // sets the day only (not the time)
        public void SetDay(DateTime day)
        {
            this.setDateText(day.ToString("yyyy-MM-dd"));
        }
        public void Placeholder(string text)
        {
            this.placeholder = text;
            this.updateButtonText();
        }

        public void setDateText(string newText)
        {
            string oldText = this.dateText;
            this.dateText = newText;
            if (newText != oldText)
            {
                this.updateButtonText();
                // call handlers
                TextChangedEventArgs args = new TextChangedEventArgs(oldText, newText);
                foreach (EventHandler<TextChangedEventArgs> handler in this.textChanged_handlers)
                {
                    handler.Invoke(this.chooseDate_button, args);
                }

            }
        }
        private void updateButtonText()
        {
            if (this.IsDateValid())
            {
                this.chooseDate_button.Text = this.dateText;
                this.appear_defaultValid();
            }
            else
            {
                if ((this.dateText == null || this.dateText == "") && this.placeholder != null)
                {
                    this.chooseDate_button.Text = this.placeholder;
                    this.appear_defaultValid();
                }
                else
                {
                    this.chooseDate_button.Text = this.dateText;
                    this.appearInvalid();
                }
            }
        }

        public void OnBack(LayoutChoice_Set layout)
        {
            this.setDateText(this.implView.DateText);
        }


        List<EventHandler<TextChangedEventArgs>> textChanged_handlers = new List<EventHandler<TextChangedEventArgs>>(2);
        private LayoutStack layoutStack;
        List<DateCharacter> dateFormat = new List<DateCharacter>();
        Button chooseDate_button;
        FullscreenDateEntryView implView;
        private string placeholder = null;
        private string dateText = "";
        private string title;
    }

    class FullscreenDateEntryView : ContainerLayout
    {
        public FullscreenDateEntryView(string title, List<DateCharacter> dateFormat, LayoutStack layoutStack)
        {
            this.dateFormat = dateFormat;

            this.dateLayout = new TextblockLayout("");
            this.nowLayout = new TextblockLayout("");

            dateLayout.ScoreIfEmpty = true;
            GridLayout mainGrid = GridLayout.New(new BoundProperty_List(4), new BoundProperty_List(1), LayoutScore.Zero);
            mainGrid.AddLayout(nowLayout);
            mainGrid.AddLayout(new TextblockLayout("Enter " + title + ":"));
            mainGrid.AddLayout(dateLayout);

            GridLayout buttonGrid = GridLayout.New(BoundProperty_List.Uniform(4), BoundProperty_List.Uniform(3), LayoutScore.Zero);
            buttonGrid.AddLayout(this.makeButtonNumber(1));
            buttonGrid.AddLayout(this.makeButtonNumber(2));
            buttonGrid.AddLayout(this.makeButtonNumber(3));

            buttonGrid.AddLayout(this.makeButtonNumber(4));
            buttonGrid.AddLayout(this.makeButtonNumber(5));
            buttonGrid.AddLayout(this.makeButtonNumber(6));

            buttonGrid.AddLayout(this.makeButtonNumber(7));
            buttonGrid.AddLayout(this.makeButtonNumber(8));
            buttonGrid.AddLayout(this.makeButtonNumber(9));

            LayoutChoice_Set helpWindow = new HelpWindowBuilder()
                .AddMessage("This screen enables you to enter " + title + " using the format " + this.getDateFormatString() + ".")
                .AddMessage("Press the backspace button (the '<-') to remove any incorrect characters (it will remove several at a time).")
                .AddMessage("Then use the keypad to enter new digits to use in the date/time.")
                .AddMessage("Filler characters like '-', 'T', and ':' will be automatically added for you.")
                .AddMessage("Press your device's Back button when finished.")
                .Build();

            buttonGrid.AddLayout(new HelpButtonLayout(helpWindow, layoutStack));
            buttonGrid.AddLayout(this.makeButtonNumber(0));
            buttonGrid.AddLayout(this.makeBackspaceButton());

            mainGrid.AddLayout(buttonGrid);
            this.DateText = DateText;

            this.SubLayout = mainGrid;

        }

        public void Add_TextChanged_Handler(EventHandler<TextChangedEventArgs> h)
        {
            if (h == null)
            {
                throw new ArgumentException("cannot add null textchanged_handler to " + this);
            }
            this.textChanged_handlers.Add(h);
        }


        public string DateText
        {
            get
            {
                string text = this.dateLayout.getText();
                if (text == null)
                    text = "";
                return text;
            }
            set
            {
                this.dateLayout.setText(value);
                this.updateValidity();
            }
        }

        public string NowText
        {
            set
            {
                this.nowLayout.setText(value);
            }
        }
        private ButtonLayout makeButtonNumber(int value)
        {
            while (this.buttons.Count <= value)
            {
                this.buttons.Add(null);
            }
            Button button = new Button();
            button.Text = value.ToString();
            this.buttons[value] = button;
            button.Clicked += this.numberButton_click;
            return new ButtonLayout(button);
        }


        private void numberButton_click(object sender, EventArgs e)
        {
            int digit = 0;
            for (int i = 0; i < this.buttons.Count; i++)
            {
                if (sender == this.buttons[i])
                {
                    digit = i;
                    break;
                }
            }
            this.addDigit(digit);
        }

        private void addDigit(int digit)
        {
            string newText = this.addFillerCharacters(this.DateText) + digit.ToString();
            if (newText.Length <= this.dateFormat.Count())
                this.DateText = newText;
        }

        private string addFillerCharacters(string text)
        {
            while (true)
            {
                int length = text.Length;
                if (length < this.dateFormat.Count())
                {
                    if (!this.dateFormat[length].IsMutable)
                    {
                        text += this.dateFormat[length].Character;

                    }
                    else
                        break;
                }
                else
                {
                    break;
                }
            }
            return text;
        }


        private ButtonLayout makeBackspaceButton()
        {
            Button backspaceButton = new Button();
            backspaceButton.Text = "<-";
            backspaceButton.Clicked += BackspaceButton_Clicked;
            return new ButtonLayout(backspaceButton);
        }
        private void BackspaceButton_Clicked(object sender, EventArgs e)
        {
            this.doBackspace();
        }

        private void doBackspace()
        {
            this.DateText = this.doBackspace(this.DateText);
        }
        private string doBackspace(string text)
        {
            // delete an entire block of characters
            while (true)
            {
                if (text.Length <= 0)
                    break;
                text = text.Remove(text.Length - 1, 1);
                // delete through the previous special character
                int length = text.Length;
                if (length <= 0 || length >= this.dateFormat.Count)
                    break;
                if (!this.dateFormat[length].IsMutable)
                {
                    // We've deleted all of the current block; so now stop
                    break;
                }
            }
            return text;

        }


        public void updateValidity()
        {
            if (this.isDateValid())
                this.dateLayout.setBackgroundColor(Color.FromRgba(0, 0, 0, 0));
            else
                this.dateLayout.setBackgroundColor(Color.Red);
        }

        private bool isDateValid()
        {
            DateTime result;
            return this.Parse(out result);
        }

        private bool Parse(out DateTime result)
        {
            return DateEntryView.Parse(this.DateText, out result);
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


        TextblockLayout nowLayout;
        TextblockLayout dateLayout;
        List<Button> buttons = new List<Button>();
        List<DateCharacter> dateFormat = new List<DateCharacter>();
        List<EventHandler<TextChangedEventArgs>> textChanged_handlers = new List<EventHandler<TextChangedEventArgs>>(2);
    }



    // Unfortunately, TextBoxDateEntryView doesn't work well on Android.
    // The Label on Android doesn't like it when its keyboard is Numeric but its content contains a letter,
    // and this situation causes the textbox to lose focus and the keyboard to revert to alphanumeric
    class TextBoxDateEntryView : TitledControl
    {
        public TextBoxDateEntryView(string startingTitle)
        {
            // create the title
            this.SetTitle(startingTitle);

            // create the box to store the date
            this.dateBox = new Entry();
            this.dateBox.Keyboard = Keyboard.Numeric;
            //Entry entry = new Entry();
            //this.dateBox.Keyboard = Keyboard.Telephone;

            //this.dateBox.KeyDown += dateBox_KeyDown;

            // Numeric inputs only
            /*InputScope inputScope = new InputScope();
            InputScopeName inputScopeName = new InputScopeName();
            inputScopeName.NameValue = InputScopeNameValue.Number;
            inputScope.Names.Add(inputScopeName);
            this.dateBox.InputScope = inputScope;*/

            //LayoutChoice_Set content = new TextboxLayout(this.dateBox);
            LayoutChoice_Set content = new SinglelineTextboxLayout(this.dateBox);

            this.SetContent(content);
            //this.SetContent(new LeafLayout(new DatePicker()));
            //this.SetContent(new LeafLayout(new DatePicker(), new LayoutDimensions(100, 30, LayoutScore.Zero)));
            
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

            this.dateBox.TextChanged += new EventHandler<TextChangedEventArgs>(dateBox_TextChanged);

        }

        string doBackspace(string text)
        {
            // delete an entire block of characters
            while (true)
            {
                if (text.Length <= 0)
                    break;
                text = text.Remove(text.Length - 1, 1);
                // delete through the previous special character
                int length = text.Length;
                if (length <= 0 || length >= this.dateFormat.Count)
                    break;
                if (!this.dateFormat[length].IsMutable)
                {
                    // We've deleted all of the current block; so now stop
                    break;
                }
            }
            return text;

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
            return DateEntryView.Parse(this.getDisplayText(), out result);
        }
        private string getDisplayText()
        {
            return this.dateBox.Text;
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
            this.previousFormattedText = text;
            this.dateBox.Text = text;
        }
        // sets the day only (not the time)
        public void SetDay(DateTime day)
        {
            this.dateBox.Text = day.ToString("yyyy.MM.dd");
        }


        public void Add_TextChanged_Handler(EventHandler<TextChangedEventArgs> h)
        {
            if (h == null)
            {
                throw new ArgumentException("cannot add null textchanged_handler to " + this);
            }
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
            this.dateBox.BackgroundColor = Color.LightGray;
        }

        // alters the appearance to indicate that the given date is not valid
        public void appearInvalid()
        {
            this.dateBox.BackgroundColor = Color.Red;
        }
        private string addFillerCharacters(string text)
        {
            while (true)
            {
                int length = text.Length;
                if (length < this.dateFormat.Count())
                {
                    if (!this.dateFormat[length].IsMutable)
                    {
                        text += this.dateFormat[length].Character;

                    }
                    else
                        break;
                }
                else
                {
                    break;
                }
            }
            return text;
        }
        void dateBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // skip duplicate firings of the event
            //string text = this.getText();
            //if (text == this.latestText)
            //    return;
            //this.latestText = text;


            string prevText = this.previousFormattedText;
            string newText = e.NewTextValue;
            if (prevText != newText)
            {
                // add or remove filler characters
                string desiredText;
                if (newText.Length < prevText.Length)
                {
                    desiredText = this.doBackspace(prevText);
                }
                else
                {
                    string prevTextWithFiller = this.addFillerCharacters(prevText);
                    desiredText = prevTextWithFiller + newText.Substring(prevText.Length);
                }
                if (desiredText != this.previousFormattedText)
                {
                    this.previousFormattedText = desiredText;
                    this.dateBox.Text = desiredText;
                    this.dateBox.Focus();
                }
            }

            // update color
            this.updateDateColor();

            // call handlers
            foreach (EventHandler<TextChangedEventArgs> handler in this.textChanged_handlers)
            {
                handler.Invoke(sender, e);
            }
        }

        //Label titleBox;
        //Editor dateBox;
        Entry dateBox;
        List<EventHandler<TextChangedEventArgs>> textChanged_handlers = new List<EventHandler<TextChangedEventArgs>>(2);
        //String dateFormat = "yyyy-MM-ddTHH:mm:ss";
        List<DateCharacter> dateFormat = new List<DateCharacter>();
        //string latestText = null;
        string previousFormattedText;

    }
}

class DateCharacter
{
    public DateCharacter(Char displayCharacter, bool isMutable)
    {
        this.Character = displayCharacter;
        this.IsMutable = isMutable;
    }
    public Char Character;
    public bool IsMutable;
}