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
            GridLayout contentWithSuggestion = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Get_UnCentered_LayoutScore(1));

            this.nameBox = new TextBox();
            this.nameBox.TextChanged += new TextChangedEventHandler(nameBox_TextChanged);
            this.nameBox.KeyDown += new System.Windows.Input.KeyEventHandler(nameBox_PreviewKeyDown);
            TextboxLayout suggestionLayout = new TextboxLayout(this.nameBox);
            contentWithSuggestion.AddLayout(suggestionLayout);

            this.suggestionBlock = new TextBlock();
            contentWithSuggestion.AddLayout(new TextblockLayout(this.suggestionBlock));

            LayoutUnion content = new LayoutUnion(contentWithSuggestion, suggestionLayout);

            this.UpdateSuggestions();

            base.SetContent(new LayoutCache(content));

        }
        public void AddTextChangedHandler(TextChangedEventHandler h)
        {
            this.nameBox.TextChanged += h;
        }
        void nameBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((this.suggestedActivityName != this.nameBox.Text) && (this.suggestedActivityName != ""))
            {
                // if there is text to fill in, then check whether they pushed a key that indicates a request for an autocomplete
                if (e.Key == Key.Enter || e.Key == Key.Tab)
                {
                    // automatically fill the suggestion text into the box
                    this.nameBox.Text = this.suggestedActivityName;
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
                return (this.NameText == this.suggestedActivityName);
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
            ActivityDescriptor descriptor = this.ActivityDescriptor;
            if (descriptor == null)
            {
                this.suggestedActivityName = "";
                this.suggestionBlock.Text = "";
            }
            else
            {
                Activity activity = this.database.ResolveDescriptor(descriptor);
                if (activity != null)
                {
                    // Also consider using color to prompt users to accept the suggestion
                    this.suggestedActivityName = activity.Name;
                    if (activity.Name == descriptor.ActivityName)
                    {
                        // perfect match, so just display the activity
                        this.suggestionBlock.Text = activity.Name;
                    }
                    else
                    {
                        // Remind the user that they have to accept the suggestion
                        this.suggestionBlock.Text = activity.Name + "?";
                    }
                }
                else
                {
                    this.suggestedActivityName = "";
                    this.suggestionBlock.Text = null;
                }
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
        string suggestedActivityName = "";
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
