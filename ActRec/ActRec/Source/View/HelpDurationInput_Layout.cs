using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    // Allows the user to specify how much help they received while doing something
    class HelpDurationInput_Layout : ContainerLayout, OnBack_Listener
    {
        public HelpDurationInput_Layout(LayoutStack layoutStack)
        {
            this.layoutStack = layoutStack;

            Button button = new Button();
            button.Clicked += Button_Clicked;

            this.buttonLayout = ButtonLayout.WithoutBevel(button);
            this.SubLayout = buttonLayout;

            this.detailsLayout = new HelpDurationInput_DetailsLayout();

            this.updateButtonText();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(this.detailsLayout, "Have help?", this);
        }

        public void OnBack(LayoutChoice_Set other)
        {
            this.updateButtonText();
        }

        private void updateButtonText()
        {
            this.buttonLayout.setText("Have help? " + this.detailsLayout.Summarize());
        }
        public TimeSpan GetHelpDuration(TimeSpan duration)
        {
            return this.detailsLayout.GetHelpDuration(duration);
        }
        public double GetHelpFraction(TimeSpan duration)
        {
            TimeSpan help = this.GetHelpDuration(duration);
            return help.TotalSeconds / (duration.TotalSeconds + help.TotalSeconds);
        }

        private HelpDurationInput_DetailsLayout detailsLayout;
        private LayoutStack layoutStack;
        private ButtonLayout buttonLayout;
    }

    class HelpDurationInput_DetailsLayout : ContainerLayout
    {
        public HelpDurationInput_DetailsLayout()
        {
            this.typeBox = new VisiPlacement.CheckBox("as a multiple of your effort:", "as a number of minutes:");
            this.typeBox.Updated += TypeBox_Clicked;
            this.minutesDurationLayout = new MinutesDurationLayout();
            this.ratioLayout = new RatioLayout();
            this.gridLayout = GridLayout.New(BoundProperty_List.Uniform(3), new BoundProperty_List(1), LayoutScore.Zero);
            this.gridLayout.AddLayout(new TextblockLayout("Enter help received"));
            this.gridLayout.AddLayout(this.typeBox);
            this.SubLayout = this.gridLayout;
            this.updateSublayout();
        }

        public string Summarize()
        {
            return this.impl.Summarize();
        }
        public TimeSpan GetHelpDuration(TimeSpan participationDuration)
        {
            return this.impl.Compute(participationDuration);
        }

        private void TypeBox_Clicked(SingleSelect singleSelect)
        {
            this.updateSublayout();
        }

        private DurationHelpInterface impl
        {
            get
            {
                if (this.typeBox.Checked)
                    return this.minutesDurationLayout;
                return this.ratioLayout;
            }
        }

        private void updateSublayout()
        {
            this.gridLayout.PutLayout(this.impl.GetLayout(), 0, 2);
        }
        VisiPlacement.CheckBox typeBox;
        List<DurationHelpInterface> options = new List<DurationHelpInterface>();
        RatioLayout ratioLayout;
        MinutesDurationLayout minutesDurationLayout;
        GridLayout gridLayout;
    }

    interface DurationHelpInterface
    {
        string Summarize();
        TimeSpan Compute(TimeSpan participationDuration);
        LayoutChoice_Set GetLayout();
    }

    class MinutesDurationLayout : DurationHelpInterface
    {
        public MinutesDurationLayout()
        {
            this.textBox = new Editor();
            this.textBox.Text = "0";
            this.textBox.Keyboard = Keyboard.Numeric;
            this.textBox.TextChanged += Editor_TextChanged;
            this.textboxLayout = new TextboxLayout(this.textBox);
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(this.Text, out double result))
                this.appearValid();
            else
                this.appearInvalid();
        }

        private void appearInvalid()
        {
            this.textBox.BackgroundColor = Color.Red;
        }

        private void appearValid()
        {
            this.textBox.BackgroundColor = Color.White;
        }

        private string Text
        {
            get
            {
                return this.textBox.Text;
            }
        }
        public string Summarize()
        {
            return this.Text + "m";
        }
        public TimeSpan Compute(TimeSpan participationDuration)
        {
            double numMinutes;
            double.TryParse(this.Text, out numMinutes);
            return TimeSpan.FromMinutes(numMinutes);
        }
        public LayoutChoice_Set GetLayout()
        {
            return this.textboxLayout;
        }
        TextboxLayout textboxLayout;
        Editor textBox;
    }

    class RatioLayout : ContainerLayout, DurationHelpInterface
    {
        public RatioLayout()
        {
            this.textBox = new Editor();
            this.textBox.Text = "0";
            this.textBox.Keyboard = Keyboard.Numeric;
            this.textBox.TextChanged += Editor_TextChanged;
            this.textboxLayout = new TextboxLayout(this.textBox);
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(this.Text, out double result))
                this.appearValid();
            else
                this.appearInvalid();
        }

        private void appearInvalid()
        {
            this.textBox.BackgroundColor = Color.Red;
        }

        private void appearValid()
        {
            this.textBox.BackgroundColor = Color.White;
        }

        private string Text
        {
            get
            {
                return this.textBox.Text;
            }
        }
        public string Summarize()
        {
            return this.Text + "x";
        }
        public TimeSpan Compute(TimeSpan participationDuration)
        {
            double ratio;
            double.TryParse(this.Text, out ratio);
            double otherNumMinutes = participationDuration.TotalMinutes;
            return TimeSpan.FromMinutes(otherNumMinutes * ratio);
        }
        public LayoutChoice_Set GetLayout()
        {
            return this.textboxLayout;
        }
        TextboxLayout textboxLayout;
        Editor textBox;
    }
}
