using System;
using System.Collections.Generic;
using System.ComponentModel;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ActivityNameEntryBox : TitledControl
    {
        public ActivityNameEntryBox(string startingTitle, bool createNewActivity = false) : base(startingTitle)
        {
            this.nameBox = new Editor();
            this.nameBox.TextChanged += NameBox_TextChanged;
            this.nameBox_layout = new TextboxLayout(this.nameBox);

            this.AutoAcceptAutocomplete = true;

            Button xButton = new Button();
            xButton.Text = "X";
            xButton.Clicked += XButton_Clicked;
            this.xButtonLayout = new ButtonLayout(xButton);

            this.nameBoxWithX = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Zero);
            this.nameBoxWithX.AddLayout(this.nameBox_layout);

            this.suggestionBlock = new Label();
            LayoutChoice_Set content;
            this.createNewActivity = createNewActivity;


            if (createNewActivity)
            {
                content = nameBoxWithX;
            }
            else
            {
                GridLayout contentWithSuggestion = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Get_UnCentered_LayoutScore(1));
                contentWithSuggestion.AddLayout(new TextblockLayout(this.suggestionBlock));
                contentWithSuggestion.AddLayout(this.nameBoxWithX);

                content = new LayoutCache(contentWithSuggestion);
            }

            this.updateXButton();
            this.UpdateSuggestions();

            base.SetContent(content);
        }

        private void XButton_Clicked(object sender, EventArgs e)
        {
            this.NameText = "";
        }

        public void AddTextChangedHandler(EventHandler<TextChangedEventArgs> h)
        {
            this.nameBox.TextChanged += h;
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string oldText = e.OldTextValue;
            if (oldText == null)
                oldText = "";
            string newText = this.nameBox.Text;

            if (newText != this.nameText)
                this.userEnteredText(oldText, newText);

            this.updateXButton();
        }

        private void updateXButton()
        {
            if (this.NameText != "" && this.NameText != null)
            {
                this.nameBoxWithX.PutLayout(this.xButtonLayout, 1, 0);
            }
            else
            {
                this.nameBoxWithX.PutLayout(null, 1, 0);
            }
        }

        private void userEnteredText(string oldText, string newText)
        {
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
                if (this.createNewActivity)
                {
                    // reject illegal characters
                    this.NameText = oldText;
                }
                else
                {
                    // automatically fill the suggestion text into the box
                    this.NameText = this.suggestedActivityName;
                    this.nameBox.Unfocus();
                }
            }
            else
            {
                this.NameText = newText;
            }
        }
        public string NameText
        {
            get
            {
                return this.nameText;
            }
            set
            {
                string oldText = this.nameText;

                this.nameText = value;
                this.nameBox.Text = value;
                this.UpdateSuggestions();

                if (this.NameMatchesSuggestion)
                {
                    if (this.NameMatchedSuggestion != null)
                        this.NameMatchedSuggestion.Invoke(this, new TextChangedEventArgs(oldText, this.nameText));
                }
            }
        }
        public void Clear()
        {
            this.Set_NameText(null);
        }
        public void Set_NameText(string text)
        {
            this.NameText = text;
        }
        public ReadableActivityDatabase Database
        {
            set
            {
                this.database = value;
            }
        }
        bool NameMatchesSuggestion
        {
            get
            {
                return (this.NameText == this.suggestedActivityName);
            }
        }

        // update the UI based on a change in the text that the user has typed
        void UpdateSuggestions()
        {
            ActivityDescriptor descriptor = this.WorkInProgressActivityDescriptor;
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
                        // perfect match, so display nothing
                        this.suggestionBlock.Text = "";
                    }
                    else
                    {
                        // Update suggestion
                        string suggestionText;
                        if (this.AutoAcceptAutocomplete)
                            suggestionText = this.suggestedActivityName;
                        else
                            suggestionText = this.suggestedActivityName + "?";
                        this.suggestionBlock.Text = suggestionText;
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

        public bool AutoAcceptAutocomplete { get; set; } // Whether to treat a partially entered activity as equivalent to the one recommended by autocomplete

        public bool PreferSuggestibleActivities { get; set; }

        // this is the ActivityDescriptor to be returned to outside callers, which represents the selection that the user made
        public ActivityDescriptor ActivityDescriptor
        {
            get
            {
                ActivityDescriptor activityDescriptor = this.WorkInProgressActivityDescriptor;
                if (activityDescriptor == null)
                    return null;
                if (!this.AutoAcceptAutocomplete)
                {
                    activityDescriptor.RequiresPerfectMatch = true;
                    activityDescriptor.Suggestible = null;
                }
                return activityDescriptor;
            }
        }

        // this is the ActivityDescriptor that models what the user is in the middle of typing
        private ActivityDescriptor WorkInProgressActivityDescriptor
        {
            get
            {
                string text = this.nameText;
                if (text == null || text == "")
                {
                    return null;
                }
                ActivityDescriptor descriptor = new ActivityDescriptor();
                descriptor.ActivityName = text;
                descriptor.PreferMorePopular = true;
                descriptor.RequiresPerfectMatch = this.createNewActivity;
                if (this.PreferSuggestibleActivities && !descriptor.RequiresPerfectMatch)
                    descriptor.Suggestible = true;
                return descriptor;
            }
        }
        public Activity Activity
        {
            get
            {
                ActivityDescriptor descriptor = this.ActivityDescriptor;
                if (descriptor == null)
                    return null;
                return this.database.ResolveDescriptor(descriptor);
            }
        }
        public event NameMatchedSuggestionHandler NameMatchedSuggestion;

        ButtonLayout xButtonLayout;
        GridLayout nameBoxWithX;
        TextboxLayout nameBox_layout;
        string nameText;
        Editor nameBox;
        Label suggestionBlock;
        ReadableActivityDatabase database;
        string suggestedActivityName = "";
        bool createNewActivity;
    }

    public delegate void NameMatchedSuggestionHandler(object sender, TextChangedEventArgs e);

}
