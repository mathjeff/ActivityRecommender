using System;
using System.Collections.Generic;
using System.ComponentModel;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class ActivityNameEntryBox : TitledControl
    {
        public ActivityNameEntryBox(string startingTitle, bool matchExisting = true) : base(startingTitle)
        {

            this.nameBox = new Editor();
            this.nameBox.TextChanged += NameBox_TextChanged;
            this.suggestionBlock = new Label();
            LayoutChoice_Set nameLayout = new TextboxLayout(this.nameBox);
            LayoutChoice_Set content;

            if (matchExisting)
            {
                GridLayout contentWithSuggestion = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Get_UnCentered_LayoutScore(1));
                contentWithSuggestion.AddLayout(nameLayout);
                contentWithSuggestion.AddLayout(new TextblockLayout(this.suggestionBlock));
                content = new LayoutCache(new LayoutUnion(contentWithSuggestion, nameLayout));
            }
            else
            {
                content = nameLayout;
            }

            this.UpdateSuggestions();

            base.SetContent(content);

        }

        public void AddTextChangedHandler(EventHandler<TextChangedEventArgs> h)
        {
            this.nameBox.TextChanged += h;
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string oldText = e.OldTextValue;
            string newText = this.nameBox.Text;

            List<String> markers = new List<string> { "\n", "\t" };
            bool addedMarker = false;
            foreach (String marker in markers)
            {
                if (newText.Contains(marker) && !oldText.Contains(marker))
                {
                    addedMarker = true;
                    break;
                }
            }
            if (addedMarker)
            {
                // automatically fill the suggestion text into the box
                this.nameBox.Text = this.suggestedActivityName;
            }
            this.UpdateSuggestions();

            if (this.NameMatchesSuggestion)
            {
                if (this.NameMatchedSuggestion != null)
                    this.NameMatchedSuggestion.Invoke(sender, e);
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
        }
        // update the UI based on a change in the text that the user has typed
        void UpdateSuggestions()
        {
            ActivityDescriptor descriptor = this.ActivityDescriptor;
            string oldText = this.suggestedActivityName;
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
                        this.suggestionBlock.Text = this.suggestedActivityName;
                    }
                    else
                    {
                        // Remind the user that they have to accept the suggestion
                        this.suggestionBlock.Text = this.suggestedActivityName + "?";
                    }
                }
                else
                {
                    this.suggestedActivityName = "";
                    this.suggestionBlock.Text = "";
                }
            }
            if (oldText == null || oldText == "")
            {
                this.AnnounceChange(true);
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
        
        Editor nameBox;
        Label suggestionBlock;
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
