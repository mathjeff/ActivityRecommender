using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using VisiPlacement;

namespace ActivityRecommendation
{
    class ActivityNameEntryBox : TitledControl
    {
        public ActivityNameEntryBox(string startingTitle) : base(startingTitle)
        {
            GridLayout content = GridLayout.New(BoundProperty_List.Uniform(2), new BoundProperty_List(1), LayoutScore.Zero);

            this.nameBox = new TextBox();
            this.nameBox.TextChanged += new TextChangedEventHandler(nameBox_TextChanged);
            this.nameBox.KeyDown += new System.Windows.Input.KeyEventHandler(nameBox_PreviewKeyDown);
            content.AddLayout(new TextboxLayout(this.nameBox));

            this.suggestionBlock = new TextBlock();
            content.AddLayout(new TextblockLayout(this.suggestionBlock));

            this.UpdateSuggestions();

            base.SetContent(new LayoutCache(content));

        }
        public void AddTextChangedHandler(TextChangedEventHandler h)
        {
            this.nameBox.TextChanged += h;
        }
        void nameBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((this.SuggestionText != this.nameBox.Text) && (this.SuggestionText != ""))
            {
                // if there is text to fill in, then check whether they pushed a key that indicates a request for an autocomplete
                if (e.Key == Key.Enter || e.Key == Key.Tab)
                {
                    // automatically fill the suggestion text into the box
                    this.nameBox.Text = this.suggestionBlock.Text;
                    e.Handled = true;
                }
            }
        }
        public string NameText
        {
            get
            {
                return this.nameBox.Text;
            }
            set
            {
                this.nameBox.Text = value;
                this.UpdateSuggestions();
            }
        }
        public ActivityDatabase Database
        {
            set
            {
                this.database = value;
            }
        }
        public bool NameMatchesSuggestion
        {
            get
            {
                return (this.NameText == this.SuggestionText);
            }
        }
        string SuggestionText
        {
            get
            {
                return this.suggestionBlock.Text;
            }
        }
        // this function gets called right after the text changes
        void nameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateSuggestions();
            if (this.NameMatchesSuggestion)
            {
                if (this.NameMatchedSuggestion != null)
                    this.NameMatchedSuggestion.Invoke(sender, e);
            }
        }
        // update the UI based on a change in the text that the user has typed
        void UpdateSuggestions()
        {
            // default to a white background for the suggestion box
            //this.suggestionBlock.Background = Brushes.White;
            ActivityDescriptor descriptor = this.ActivityDescriptor;
            if (descriptor == null)
            {
                this.suggestionBlock.Text = null;
                return;
            }
            Activity activity = this.database.ResolveDescriptor(descriptor);
            if (activity != null)
            {
                if (activity.Name == this.nameBox.Text)
                {
                    // if this is a valid activity, then show a white background
                    //this.suggestionBlock.Background = Brushes.White;
                }
                else
                {
                    // if this is a valid prefix, then show a yellow background
                    //this.suggestionBlock.Background = Brushes.Yellow;
                }
                this.suggestionBlock.Text = activity.Name;
            }
            else
            {
                // if this is not a valid prefix, then show a red background
                //this.suggestionBlock.Background = Brushes.Red;
                this.suggestionBlock.Text = "";
            }
        }
        public ActivityDescriptor ActivityDescriptor
        {
            get
            {
                string text = this.nameBox.Text;
                if (text == null || text == "")
                {
                    return null;
                }
                ActivityDescriptor descriptor = new ActivityDescriptor();
                descriptor.ActivityName = text;
                descriptor.PreferHigherProbability = true;
                descriptor.RequiresPerfectMatch = false;
                return descriptor;
            }
        }
        public Activity Activity
        {
            get
            {
                ActivityDescriptor descriptor = this.ActivityDescriptor;
                return this.database.ResolveDescriptor(descriptor);
            }
        }
        public event NameMatchedSuggestionHandler NameMatchedSuggestion;
        
        TextBox nameBox;
        TextBlock suggestionBlock;
        ActivityDatabase database;
    }

    // Summary:
    //     Represents the method that will handle the ActivityRecommendation.NameMatchedSuggestion
    //      routed event.
    //
    // Parameters:
    //   sender:
    //     The object where the event handler is attached.
    //
    //   e:
    //     The event data.
    public delegate void NameMatchedSuggestionHandler(object sender, TextChangedEventArgs e);

}
