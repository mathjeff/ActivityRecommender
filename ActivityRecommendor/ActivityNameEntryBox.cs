using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace ActivityRecommendation
{
    class ActivityNameEntryBox : TitledControl
    {
        public ActivityNameEntryBox(string startingTitle) : base(startingTitle)
        {
            DisplayGrid content = new DisplayGrid(2, 1);

            this.nameBox = new TextBox();
            this.nameBox.TextWrapping = TextWrapping.Wrap;
            this.nameBox.TextChanged += new TextChangedEventHandler(nameBox_TextChanged);
            this.nameBox.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(nameBox_PreviewKeyDown);
            //this.nameBox.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.nameBox_PreviewTextInput);
            content.AddItem(this.nameBox);

            this.suggestionBlock = new TextBlock();
            this.suggestionBlock.TextWrapping = TextWrapping.Wrap;
            content.AddItem(this.suggestionBlock);

            this.UpdateSuggestions();

            base.SetContent(content);

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
        }
        // update the UI based on a change in the text that the user has typed
        void UpdateSuggestions()
        {
            // default to a white background for the suggestion box
            this.suggestionBlock.Background = Brushes.White;
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
                    this.suggestionBlock.Background = Brushes.White;
                }
                else
                {
                    // if this is a valid prefix, then show a yellow background
                    this.suggestionBlock.Background = Brushes.Yellow;
                }
                this.suggestionBlock.Text = activity.Name;
            }
            else
            {
                // if this is not a valid prefix, then show a red background
                this.suggestionBlock.Background = Brushes.Red;
                this.suggestionBlock.Text = "";
            }
        }
        public ActivityDescriptor ActivityDescriptor
        {
            get
            {
                string prefix = this.nameBox.Text;
                if (prefix == null || prefix == "")
                {
                    return null;
                }
                ActivityDescriptor descriptor = new ActivityDescriptor();
                descriptor.NamePrefix = prefix;
                descriptor.PreferBetterRatings = true;
                descriptor.RequiresPerfectMatch = false;
                return descriptor;
            }
        }
        
        TextBox nameBox;
        TextBlock suggestionBlock;
        ActivityDatabase database;
    }
}
