using System;
using System.Collections.Generic;
using System.ComponentModel;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ActivityNameEntryBox : TitledControl
    {
        public ActivityNameEntryBox(string startingTitle, ReadableActivityDatabase activityDatabase, LayoutStack layoutStack, bool createNewActivity = false) : base(startingTitle)
        {
            // some settings
            this.AutoAcceptAutocomplete = true;
            this.createNewActivity = createNewActivity;
            this.database = activityDatabase;
            this.numAutocompleteRowsToShow = 1;

            // the box the user is typing in
            this.nameBox = new Editor();
            this.nameBox.TextChanged += NameBox_TextChanged;
            this.nameBox_layout = new TextboxLayout(this.nameBox);

            // button for clearing the box's text
            Button xButton = new Button();
            xButton.Text = "X";
            xButton.Clicked += XButton_Clicked;
            this.xButtonLayout = new ButtonLayout(xButton);

            this.nameBoxWithX = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Zero);
            this.nameBoxWithX.AddLayout(this.nameBox_layout);

            // the autocomplete above the text box
            this.autocompleteBlock = new Label();
            this.autocompleteLayout = new TextblockLayout(this.autocompleteBlock);

            // button that gives help with autocomplete
            this.autocomplete_helpLayout = new HelpButtonLayout(new HelpWindowBuilder()
                .AddMessage("This screen explains how you can enter " + startingTitle + ". " +
                "Your input must match an activity that you have previously entered.")
                .AddMessage("The most basic way to input an activity name is to type it in using the letters on the keyboard.")
                .AddMessage("However, while you type the activity name, ActivityRecommender will try to guess which activity you mean, and " +
                "that guess will appear above. If the autocomplete suggestion is what you want, then you can press " +
                "[enter] to use the autocomplete suggestion.")
                .AddMessage("Autocomplete does not require you to type full words but it does require spaces between words.")
                .AddMessage("Autocomplete does not require that you type letters using the correct case but it is more effective if you do.")
                .AddMessage("If autocomplete encounters a misspelled word, autocomplete will ignore the misspelled word.")
                .AddMessage("Consider the following example.")
                .AddMessage("If you have already entered an activity named \"Taking out the Garbage\", " +
                "here are some things you can type that might cause it to become the autocomplete suggestion:")
                .AddMessage("Taking out the")
                .AddMessage("Taking out the MisspelledWord")
                .AddMessage("Taking")
                .AddMessage("out")
                .AddMessage("Garbage")
                .AddMessage("garbage")
                .AddMessage("Taking o t G")
                .AddMessage("T o t G")
                .AddMessage("T")
                .AddMessage("G")
                .AddMessage("t")
                .AddMessage("")
                .AddMessage("Note, of course, that the longer and more unique your text, the more likely that it will be matched with the activity that you intend.")
                .Build()
                , layoutStack);

            // the main layout that contains everything
            LayoutChoice_Set content;
            if (createNewActivity)
            {
                content = nameBoxWithX;
            }
            else
            {
                GridLayout contentWithFeedback = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Get_UnCentered_LayoutScore(1));
                contentWithFeedback.AddLayout(this.responseLayout);
                contentWithFeedback.AddLayout(this.nameBoxWithX);

                content = new LayoutCache(contentWithFeedback);

                this.UpdateFeedback();
            }

            this.updateXButton();

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
                this.UpdateFeedback();

                if (this.NameMatchesSuggestion)
                {
                    if (this.NameMatchedSuggestion != null)
                        this.NameMatchedSuggestion.Invoke(this, new TextChangedEventArgs(oldText, this.nameText));
                }
            }
        }
        public void autoselectRootActivity_if_noCustomActivities()
        {
            if (!this.database.ContainsCustomActivity())
            {
                this.NameText = this.database.GetRootActivity().Name;
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
        bool NameMatchesSuggestion
        {
            get
            {
                return (this.NameText == this.suggestedActivityName);
            }
        }

        // update the UI based on a change in the text that the user has typed
        void UpdateFeedback()
        {
            ActivityDescriptor descriptor = this.WorkInProgressActivityDescriptor;
            string oldText = this.suggestedActivityName;
            if (descriptor == null)
            {
                this.suggestedActivityName = "";
                this.autocompleteBlock.Text = "";
                // hide the Help button if it's there
                this.responseLayout.SubLayout = this.autocompleteLayout;
            }
            else
            {
                IEnumerable<Activity> autocompletes = this.Autocompletes;

                List<String> autocompleteNames = new List<string>();
                foreach (Activity suggestion in autocompletes)
                {
                    autocompleteNames.Add(suggestion.Name);
                }
                if (autocompleteNames.Count > 0)
                {
                    String firstActivity_name = autocompleteNames[0];
                    this.suggestedActivityName = firstActivity_name;

                    if (firstActivity_name == descriptor.ActivityName)
                    {
                        // perfect match, so display nothing
                        this.autocompleteBlock.Text = "";
                    }
                    else
                    {
                        // Update suggestion
                        if (!this.AutoAcceptAutocomplete)
                        {
                            // Not auto-accepting the autocomplete; remind the user that they have to push enter
                            for (int i = 0; i < autocompleteNames.Count; i++)
                            {
                                autocompleteNames[i] = autocompleteNames[i] + "?";
                            }
                        }
                        string suggestionText = String.Join("\n\n", autocompleteNames);
                        this.autocompleteBlock.Text = suggestionText;
                    }
                    // make the autocomplete suggestion appear
                    this.responseLayout.SubLayout = this.autocompleteLayout;
                }
                else
                {
                    // no matches
                    this.suggestedActivityName = "";
                    // show a help button
                    this.responseLayout.SubLayout = this.autocomplete_helpLayout;

                }

            }
            if (oldText == null || oldText == "")
            {
                this.AnnounceChange(true);
            }
        }

        public bool AutoAcceptAutocomplete { get; set; } // Whether to treat a partially entered activity as equivalent to the one recommended by autocomplete

        public bool PreferSuggestibleActivities { get; set; }

        public int NumAutocompleteRowsToShow
        {
            get
            {
                return this.numAutocompleteRowsToShow;
            }
            set
            {
                this.numAutocompleteRowsToShow = value;
            }
        }
        private int numAutocompleteRowsToShow;

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
        public IEnumerable<Activity> Autocompletes
        {
            get
            {
                ActivityDescriptor descriptor = this.WorkInProgressActivityDescriptor;
                if (descriptor == null)
                    return new List<Activity>();
                IEnumerable<Activity> matches = this.database.FindBestMatches(descriptor, this.numAutocompleteRowsToShow);
                return matches;
            }
        }
        public event NameMatchedSuggestionHandler NameMatchedSuggestion;

        ButtonLayout xButtonLayout;
        GridLayout nameBoxWithX;
        TextboxLayout nameBox_layout;
        string nameText;
        Editor nameBox;
        Label autocompleteBlock;
        LayoutChoice_Set autocomplete_helpLayout;
        TextblockLayout autocompleteLayout;
        ContainerLayout responseLayout = new ContainerLayout();
        ReadableActivityDatabase database;
        string suggestedActivityName = "";
        bool createNewActivity;
    }

    public delegate void NameMatchedSuggestionHandler(object sender, TextChangedEventArgs e);

}
